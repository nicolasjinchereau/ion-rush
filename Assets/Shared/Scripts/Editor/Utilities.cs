using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using JsonFx;
using System.Text;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Utilities : ScriptableObject
{
    [MenuItem("Utilities/Align Camera With Scene View")]
    public static void AlignSelectedCameraWithSceneView()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Align Camera With Scene View",
            "The transform of the selected camera will be updated to match the one from the scene view.\n\n" +
            "Do you wish to proceed?",
            "Align", "Cancel");

        if (ok)
        {
            var sceneViewCam = SceneView.lastActiveSceneView.camera;
            var selectedCam = Selection.activeGameObject.GetComponent<Camera>();

            if (sceneViewCam != null && selectedCam != null)
            {
                selectedCam.transform.position = sceneViewCam.transform.position;
                selectedCam.transform.rotation = sceneViewCam.transform.rotation;
            }
        }
    }

    [MenuItem("GameObject/Custom/Convert To Static", false)]
    public static void ConvertToStaticDestructive()
    {
        bool ok = EditorUtility.DisplayDialog(
            "Convert To Static",
            "The selected game objects and their children will be stripped of all components except renderers. " +
            "Game objects with no components will be removed, and all remaining game objects will be set to static.\n\n" +
            "Do you wish to proceed?",
            "Convert", "Cancel");

        if (ok)
        {
            foreach (var go in Selection.gameObjects)
            {
                if (go)
                {
                    Undo.RegisterFullObjectHierarchyUndo(go, "Convert To Static");
                    DestroyEmpty(go);
                }
            }
        }
    }

    static void DestroyEmpty(GameObject go)
    {
        if (PrefabUtility.IsPartOfNonAssetPrefabInstance(go) && PrefabUtility.IsOutermostPrefabInstanceRoot(go))
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        if (!go.activeSelf)
        {
            DestroyImmediate(go);
            return;
        }

        var children = go.transform.Cast<Transform>().Select(t => t.gameObject).ToArray();
        foreach (var child in children) {
            DestroyEmpty(child);
        }

        foreach (var comp in go.GetComponents<Component>())
        {
            var type = comp.GetType();
            if (!typeof(Renderer).IsAssignableFrom(type) && !typeof(MeshFilter).IsAssignableFrom(type) && !typeof(Transform).IsAssignableFrom(type)) {
                DestroyImmediate(comp);
            }
        }

        foreach (var comp in go.GetComponents<Component>())
        {
            var type = comp.GetType();
            if (type == typeof(ParticleSystemRenderer)) {
                DestroyImmediate(comp);
            }
        }

        bool keepHierarchy = true;

        var components = go.GetComponents<Component>();
        if (components.Length == 1 && go.transform.parent != null && (go.transform.childCount == 0 || !keepHierarchy)) // transform only
        {
            var children2 = go.transform.Cast<Transform>().Select(t => t.gameObject).ToArray();
            foreach (var child in children2) {
                child.transform.SetParent(go.transform.parent, true);
            }

            DestroyImmediate(go);
        }
        else
        {
            go.isStatic = true;
        }
    }

    [MenuItem("Utilities/Generate Gradient Texture")]
    public static void GenerateGradientTexture()
    {
        int width = 128;
        int height = 128;
        
        Color[] pixels = new Color[width * height];

        for (int y = 0; y != height; ++y)
        {
            for (int x = 0; x != width; ++x)
            {
                var center = new Vector2(0.5f, 0.5f);
                var pos = new Vector2(x / (float)(width - 1), y / (float)(height - 1));
                var d = Vector2.Distance(center, pos) / 0.5f;
                var t = Mathf.Clamp01(1.0f - d);
                var a = Mathf.Pow(t * (2 - t), 3);
                pixels[y * width + x] = new Color(1, 1, 1, a);
            }
        }
        
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        tex.SetPixels(pixels);
        tex.Apply(false, false);

        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Props/Wavetable/Gradient3.png", bytes);
        AssetDatabase.Refresh();
    }

    [MenuItem("Utilities/Count Gears and Coins")]
    public static void CountGears()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        int gearCount = GameObject.FindGameObjectsWithTag("CollectableGear").Length;
        int coinCount = GameObject.FindGameObjectsWithTag("Coin").Length;
        Debug.Log($"{sceneName}: {gearCount} Gears, {coinCount} Coins");
    }
}
