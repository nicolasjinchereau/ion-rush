using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class ObjectData
{
    public string name = "";
    public int hidden = 0;
    public Matrix4x4 matrix;
    public string meshID = "";
    public List<string> materialIDs = new List<string>();
}

public class MeshData
{
    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] texcoords;
    public int subMeshCount;
    public int[][] trianges;
}

public class MaterialData
{
    public string texture = "";
    public Color color = Color.white;
    public Color specularColor = Color.white;
    public float shininess = 0;
    public Vector2 tiling;
    public Vector2 offset;
}

public class UnityScene
{
    public Color ambientLight = Color.white;
    public Color lightColor = Color.white;
    public float lightIntensity = 0.0f;
    public Matrix4x4 lightMatrix = Matrix4x4.identity;

    public Dictionary<string, MeshData> meshes = new Dictionary<string, MeshData>();
    public Dictionary<string, MaterialData> materials = new Dictionary<string, MaterialData>();
    public List<ObjectData> objects = new List<ObjectData>();

    public UnityScene(IEnumerable<GameObject> gameObjects)
    {
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        List<Light> lights = new List<Light>();

        foreach(var go in gameObjects)
        {
            renderers.AddRange(go.GetComponentsInChildren<MeshRenderer>());
            lights.AddRange(go.GetComponentsInChildren<Light>());
        }

        ambientLight = RenderSettings.ambientLight;

        foreach(MeshRenderer mr in renderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            string meshID = AssetDatabase.GetAssetPath(mesh).Replace('/', '_').Replace('.', '_') + "_" + mesh.name;
            
            MeshData meshData = new MeshData();
            meshData.vertices = mesh.vertices;
            meshData.normals = mesh.normals;
            meshData.texcoords = mesh.uv;
            meshData.subMeshCount = mesh.subMeshCount;
            meshData.trianges = new int[meshData.subMeshCount][];
            
            for(int i = 0; i < meshData.subMeshCount; ++i)
                meshData.trianges[i] = mesh.GetTriangles(i);

            meshes[meshID] = meshData;

            ObjectData objectData = new ObjectData();
            objectData.name = mr.gameObject.name;
            objectData.hidden = (mr.enabled && mr.gameObject.activeInHierarchy) ? 0 : 1;
            objectData.matrix = mr.transform.localToWorldMatrix;
            objectData.meshID = meshID;

            foreach(Material mat in mr.sharedMaterials)
            {
                MaterialData matData = new MaterialData();
                matData.offset = mat.mainTextureOffset;
                matData.tiling = mat.mainTextureScale;

                if(mat.HasProperty("_MainTex"))
                {
                    Texture tex = mat.GetTexture("_MainTex");
                    if(tex != null)
                        matData.texture = AssetDatabase.GetAssetPath(tex).Remove(0, "Assets/".Length);
                }

                if(mat.HasProperty("_Color"))
                    matData.color = mat.GetColor("_Color");

                if(mat.HasProperty("_SpecColor"))
                    matData.specularColor = mat.GetColor("_SpecColor");

                if(mat.HasProperty("_Shininess"))
                    matData.shininess = mat.GetFloat("_Shininess");

                string fn = Path.GetFileName(matData.texture).Replace('.', '_');
                string matID = AssetDatabase.GetAssetPath(mat).Replace('/', '_').Replace('.', '_') + "_" + fn;
                
                materials[matID] = matData;
                objectData.materialIDs.Add(matID);
            }

            objects.Add(objectData);
        }

        foreach(Light light in lights)
        {
            // use first directional light found as dominant light
            if(light.type == LightType.Directional)
            {
                lightColor = light.color;
                lightIntensity = light.intensity;
                lightMatrix = light.transform.localToWorldMatrix;

                break;
            }
        }
    }

    public void DoExport(string outputPath, bool copyTextures, StatusMonitor monitor = null)
    {
        if(string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
        {
            Debug.LogError("cannot export scene. no output path specified.");

            if(monitor != null)
                monitor.Cancel();

            return;
        }
        
        string projectFolder = Path.GetDirectoryName(Application.dataPath);

        Thread worker = new Thread(()=>
        {
            try
            {
                MemoryStream ms = new MemoryStream(4096);
                BinaryWriter br = new BinaryWriter(ms);

                int totalItems = meshes.Count + materials.Count + objects.Count;
                int finishedItems = 0;

                Debug.Log("writing scene settings...");
                br.Write(ambientLight.r);
                br.Write(ambientLight.g);
                br.Write(ambientLight.b);

                br.Write(lightColor.r);
                br.Write(lightColor.g);
                br.Write(lightColor.b);

                br.Write(lightIntensity);

                // light is backward in 3ds max
                Matrix4x4 preFlip = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 180, 0), Vector3.one);
                Matrix4x4 lm = unityToMaxTransform * lightMatrix * preFlip;
                br.Write(lm.m00);
                br.Write(lm.m01);
                br.Write(lm.m02);
                br.Write(lm.m03);

                br.Write(lm.m10);
                br.Write(lm.m11);
                br.Write(lm.m12);
                br.Write(lm.m13);

                br.Write(lm.m20);
                br.Write(lm.m21);
                br.Write(lm.m22);
                br.Write(lm.m23);

                br.Write(lm.m30);
                br.Write(lm.m31);
                br.Write(lm.m32);
                br.Write(lm.m33);

                Debug.Log("writing " + meshes.Count + " meshes...");
                br.Write(meshes.Count);
                
                foreach(var mesh in meshes)
                {
                    WriteMesh(br, mesh.Key, mesh.Value);

                    if(monitor != null)
                        monitor.progress = (float)++finishedItems / (float)totalItems;
                }

                Debug.Log("writing " + materials.Count + " materials...");
                br.Write(materials.Count);

                foreach(var mat in materials)
                {
                    if(!string.IsNullOrEmpty(mat.Value.texture))
                    {
                        string srcFile = projectFolder + "/Assets/" + mat.Value.texture;
                        string destFile = outputPath + "/" + mat.Value.texture;

                        if(copyTextures)
                        {
                            bool shouldCopy = true;

                            if(File.Exists(destFile))
                            {
                                DateTime lastWriteSrc = File.GetLastWriteTime(srcFile);
                                DateTime lastWriteDest = File.GetLastWriteTime(destFile);
                                
                                if(lastWriteDest >= lastWriteSrc)
                                    shouldCopy = false;
                            }

                            if(shouldCopy)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                                File.Copy(srcFile, destFile, true);
                            }
                        }
                    }

                    WriteMaterial(br, mat.Key, mat.Value);

                    if(monitor != null)
                        monitor.progress = (float)++finishedItems / (float)totalItems;
                }

                Debug.Log("writing " + objects.Count + " objects...");
                br.Write(objects.Count);

                foreach(var obj in objects)
                {
                    WriteObject(br, obj);

                    if(monitor != null)
                        monitor.progress = (float)++finishedItems / (float)totalItems;
                }

                string outputFile = outputPath + "/scene.uscene";

                Debug.Log("writing file: " + outputFile);

                FileStream fs = File.OpenWrite(outputFile);
                ms.WriteTo(fs);
                ms.Close();
                fs.Close();

                Debug.Log("export complete.");

                if(monitor != null)
                {
                    monitor.progress = 1.0f;
                    monitor.isComplete = true;
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);

                if(monitor != null)
                    monitor.Cancel();
            }
        });
        
        worker.Start();
    }

    private void WriteMesh(BinaryWriter writer, string id, MeshData mesh)
    {
        int vertexCount = mesh.vertices.Length;
        int normalCount = mesh.normals.Length;
        int texcoordCount = mesh.texcoords.Length;
        int subMeshCount = mesh.subMeshCount;

        writer.Write(id.Length);
        writer.Write(Encoding.UTF8.GetBytes(id));

        writer.Write(vertexCount);
        
        for(int i = 0; i < vertexCount; ++i)
        {
            Vector3 v = mesh.vertices[i];
            writer.Write(v.x);
            writer.Write(v.y);
            writer.Write(v.z);
        }

        writer.Write(normalCount);
        
        for(int i = 0; i < vertexCount; ++i)
        {
            Vector3 n = mesh.normals[i];
            writer.Write(n.x);
            writer.Write(n.y);
            writer.Write(n.z);
        }

        writer.Write(texcoordCount);

        for(int i = 0; i < texcoordCount; ++i)
        {
            writer.Write(mesh.texcoords[i].x);
            writer.Write(mesh.texcoords[i].y);
        }

        writer.Write(subMeshCount);

        for(int sm = 0; sm < subMeshCount; ++sm)
        {
            int[] indices = mesh.trianges[sm];
            int indexCount = indices.Length;

            writer.Write(indexCount);

            for(int i = 0; i < indexCount; ++i)
                writer.Write(indices[i]);
        }
    }

    private void WriteMaterial(BinaryWriter writer, string id, MaterialData mat)
    {
        writer.Write(id.Length);
        writer.Write(Encoding.UTF8.GetBytes(id));

        writer.Write(mat.texture.Length);
        writer.Write(Encoding.UTF8.GetBytes(mat.texture));

        writer.Write(mat.color.r);
        writer.Write(mat.color.g);
        writer.Write(mat.color.b);
        writer.Write(mat.color.a);

        writer.Write(mat.specularColor.r);
        writer.Write(mat.specularColor.g);
        writer.Write(mat.specularColor.b);
        writer.Write(mat.specularColor.a);

        Debug.Log(id + " shininess: " + mat.shininess);

        writer.Write(mat.shininess);

        writer.Write(mat.tiling.x);
        writer.Write(mat.tiling.y);

        writer.Write(mat.offset.x);
        writer.Write(mat.offset.y);
    }

    public Matrix4x4 unityToMaxTransform
    {
        get {
            Matrix4x4 rot = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one);
            Matrix4x4 scale  = Matrix4x4.Scale(new Vector3(1, 1, -1));
            return rot * scale;
        }
    }

    private void WriteObject(BinaryWriter writer, ObjectData obj)
    {
        writer.Write(obj.name.Length);
        writer.Write(Encoding.UTF8.GetBytes(obj.name));

        writer.Write(obj.hidden);

        Matrix4x4 m = unityToMaxTransform * obj.matrix;

        writer.Write(m.m00);
        writer.Write(m.m01);
        writer.Write(m.m02);
        writer.Write(m.m03);

        writer.Write(m.m10);
        writer.Write(m.m11);
        writer.Write(m.m12);
        writer.Write(m.m13);

        writer.Write(m.m20);
        writer.Write(m.m21);
        writer.Write(m.m22);
        writer.Write(m.m23);

        writer.Write(m.m30);
        writer.Write(m.m31);
        writer.Write(m.m32);
        writer.Write(m.m33);

        writer.Write(obj.meshID.Length);
        writer.Write(Encoding.UTF8.GetBytes(obj.meshID));

        int materialCount = obj.materialIDs.Count;

        writer.Write(materialCount);

        for(int i = 0; i < materialCount; ++i)
        {
            string id = obj.materialIDs[i];
            writer.Write(id.Length);
            writer.Write(Encoding.UTF8.GetBytes(id));
        }
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder(256);

        stringBuilder.Append("Meshes:\n");

        foreach(var m in meshes)
            stringBuilder.Append("  " + m.Key + "\n");
        
        stringBuilder.Append("Materials:\n");

        foreach(var m in materials)
        {
            stringBuilder.Append("  " + m.Key + "\n");
            stringBuilder.Append("    " + m.Value.texture + "\n");
        }

        stringBuilder.Append("Objects:\n");

        foreach(var o in objects)
            stringBuilder.Append("  " + o.name + "\n");

        return stringBuilder.ToString();
    }
}
