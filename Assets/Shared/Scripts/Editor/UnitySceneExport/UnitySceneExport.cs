using UnityEngine;
using UnityEditor;

public class StatusMonitor
{
    private float _progress = 0;
    private bool _isComplete = false;

    object _lock = new object();

    public float progress
    {
        get {
            float ret;
            
            lock (_lock) {
                ret = _progress;
            }

            return ret;
        }

        set {
            lock (_lock) {
                _progress = value;
            }
        }
    }

    public bool isComplete
    {
        get {
            bool ret;
            lock (_lock) {
                ret = _isComplete;
            }
            return ret;
        }

        set {
            lock (_lock) {
                _isComplete = value;
            }
        }
    }

    public void Cancel()
    {
        lock (_lock) {
            _progress = 1.0f;
            _isComplete = true;
        }
    }
};

public class UnitySceneExportWindow : EditorWindow
{
    [MenuItem("Window/Unity Scene Exporter")]
    private static void Init()
    {
        GetWindow(typeof(UnitySceneExportWindow));
    }

    string outputPath = "";
    bool copyTextures = true;
    StatusMonitor monitor = null;

    void OnGUI()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Unity Scene Exporter");
        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                outputPath = EditorUtility.OpenFolderPanel("Choose Output Folder", outputPath, "");
            }

            EditorGUILayout.LabelField(outputPath);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        copyTextures = EditorGUILayout.Toggle("Copy Textures", copyTextures);
        
        EditorGUILayout.Space();
        
        if(GUILayout.Button("Export Selected", GUILayout.Width(130)))
        {
            if(monitor == null)
            {
                UnityScene scene = new UnityScene(Selection.gameObjects);

                monitor = new StatusMonitor();
                scene.DoExport(outputPath, copyTextures, monitor);
                //Debug.Log(scene);
            }
        }
        
        if(monitor!= null)
        {
            if(!monitor.isComplete)
            {
                EditorUtility.DisplayProgressBar(
                    "Exporting",
                    "Exporting selected objects from the scene...",
                    monitor.progress);
            }
            else
            {
                monitor = null;
                EditorUtility.ClearProgressBar();
            }
        }
    }

    void OnInspectorUpdate()
    {
        if(monitor!= null)
            Repaint();
    }
}
