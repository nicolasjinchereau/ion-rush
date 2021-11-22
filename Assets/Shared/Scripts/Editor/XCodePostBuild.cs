using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Reflection;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public static class PBXProjectExtensions
{
    public static void AddExternalFrameworkToProject(this PBXProject project, string targetGuid, string path, string framework)
    {
        string fileGuid = project.AddFile(path + "/" + framework, "Frameworks/" + framework, PBXSourceTree.Source);
        project.AddFileToBuildWithFlags(targetGuid, fileGuid, null);
    }
    
    public static void AddCompilerFlagForFile(this PBXProject project, string targetGuid, string fileGuid, string flag)
    {
        List<string> flags = project.GetCompileFlagsForFile(targetGuid, fileGuid);
        flags.Add(flag);
        project.SetCompileFlagsForFile(targetGuid, fileGuid, flags);
    }

    // public static void AddCopyFilesBuildPhase(this PBXProject project)
    // {
    //     BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        
    //     //PBXProjectData
    //     object obj = project.GetType().GetField("m_Data", bindFlags).GetValue(project);
        
    //     var copyFilesBuildPhase = PBXCopyFilesBuildPhaseData.Create("Embed Frameworks", "10");
    //     copyFilesBuildPhase.files.Add("Affdex.framework");
    //     copyFiles.AddEntry(copyFilesBuildPhase);
    //     nativeTargets[mainTarget].phases.AddGUID(copyFilesBuildPhase.guid);
    // }
}

public class XCodePostBuild : ScriptableObject
{
    [PostProcessBuild(int.MaxValue)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if(target != BuildTarget.iOS)
            return;
        
        try
        {
            string projDir = workspaceDir + "/Build/ios";
            string projFile = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            
            var proj = new PBXProject();
            proj.ReadFromFile(projFile);
            string targetName = proj.GetUnityMainTargetGuid();
            string targetGuid = proj.TargetGuidByName(targetName);
            
            proj.AddFrameworkToProject(targetGuid, "WebKit.framework", false);
            proj.AddFrameworkToProject(targetGuid, "CoreTelephony.framework", false);
            proj.AddFrameworkToProject(targetGuid, "EventKit.framework", false);
            proj.AddFrameworkToProject(targetGuid, "EventKitUI.framework", false);
            proj.AddFrameworkToProject(targetGuid, "MessageUI.framework", false);
            proj.AddFrameworkToProject(targetGuid, "Social.framework", false);
            proj.AddFrameworkToProject(targetGuid, "StoreKit.framework", false);
            proj.AddFrameworkToProject(targetGuid, "CoreMedia.framework", false);
            proj.AddFrameworkToProject(targetGuid, "MediaPlayer.framework", false);
            proj.AddFrameworkToProject(targetGuid, "UIKit.framework", false);
            proj.AddFrameworkToProject(targetGuid, "AudioToolbox.framework", false);
            proj.AddFrameworkToProject(targetGuid, "AdSupport.framework", false);
            proj.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");
            
            // string adcGuid = proj.FindFileGuidByProjectPath("Libraries/Plugins/iOS/UnityADC.mm");
            // proj.AddCompilerFlagForFile(targetGuid, adcGuid, "-fno-objc-arc");

            string[] gccOptimizationLevels = {
                "0",   // -O0 - None
                "1",   // -O1 - Fast
                "2",   // -O2 - Faster
                "3",   // -O3 - Fastest
                "s",   // -Os - Fastest, Smallest
                "fast" // -Ofast - Fastest, Aggressive Optimizations
            };
            
            string releaseConfigGuid = proj.BuildConfigByName(targetGuid, "Release");
            proj.SetBuildPropertyForConfig(releaseConfigGuid, "GCC_OPTIMIZATION_LEVEL", gccOptimizationLevels[5]);
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            proj.WriteToFile(projFile);
            
		// update Info.plist file
			string plist = workspaceDir + "/Build/ios/Info.plist";
			var sr = new StreamReader(plist);
			var txt = sr.ReadToEnd();
			sr.Close();
			
			var doc = new XmlDocument();
			doc.LoadXml(txt);

			XmlNode dict = doc.SelectSingleNode("/plist/dict");
            
			string[][] usageDescriptions = new string[][]{
				new string[] { "NSCalendarsUsageDescription", "This is not actually being used." },
                new string[] { "NSCameraUsageDescription", "This is not actually being used." }
			};
			
			foreach(string[] desc in usageDescriptions)
			{
				XmlNode key = doc.SelectSingleNode("/plist/dict/key[text()='" + desc[0] + "']");
				XmlNode val = null;
				
				if(key != null)
				{
					val = key.NextSibling;
				}
				else
				{
					key = doc.CreateNode(XmlNodeType.Element, "key", "");
					key.InnerText = desc[0];
					dict.AppendChild(key);
					
					val = doc.CreateNode(XmlNodeType.Element, "string", "");
					dict.AppendChild(val);
				}
				
				val.InnerText = desc[1];
			}
			
			{
				XmlNode key = doc.SelectSingleNode("/plist/dict/key[text()='ITSAppUsesNonExemptEncryption']");
				
				if(key == null)
				{
					key = doc.CreateNode(XmlNodeType.Element, "key", "");
					key.InnerText = "ITSAppUsesNonExemptEncryption";
					dict.AppendChild(key);
					
					XmlNode val = doc.CreateNode(XmlNodeType.Element, "false", "");
					dict.AppendChild(val);
				}
			}
			
			if(doc.DocumentType != null)
			{
				var name = doc.DocumentType.Name;
				var publicId = doc.DocumentType.PublicId;
				var systemId = doc.DocumentType.SystemId;
				var parent = doc.DocumentType.ParentNode;
				var newDoctype = doc.CreateDocumentType(name, publicId, systemId, null);
				parent.ReplaceChild(newDoctype, doc.DocumentType);
			}
            
			doc.Save(plist);
			doc = null;
            
            Debug.Log("XCode post-build Complete.");
        }
        catch(Exception ex) {
            Debug.LogError("XCode post-build failed: " + ex.ToString());
        }
    }
    
    public static void AddStringChild(XmlDocument doc, XmlNode array, string value)
	{
		foreach(XmlNode n in array)
		{
			if(n.Name == "string" && n.InnerText == value)
				return;
		}

		XmlNode node = doc.CreateNode(XmlNodeType.Element, "string", "");
		node.InnerText = value;
		array.AppendChild(node);
	}
    
    public static string projectDir {
        get { return new DirectoryInfo(Application.dataPath).Parent.FullName; }
    }

    public static string workspaceDir {
        get { return new DirectoryInfo(Application.dataPath).Parent.Parent.FullName; }
    }
}

#endif
