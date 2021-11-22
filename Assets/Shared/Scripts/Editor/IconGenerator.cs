using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class IconGenerator : EditorWindow
{
    [MenuItem("Window/Icon Generator")]
    static void Init()
    {
        IconGenerator window = EditorWindow.GetWindow<IconGenerator>(false, "Icon Generator");
        window.Show();
    }

    [SerializeField]
    public Texture2D fullSizeIcon;

    [SerializeField]
    public List<int> sizes = new List<int>() {
        512, 432, 324, 256, 216, 196, 192, 180,
        172, 167, 162, 152, 144, 128,
        120, 114, 108, 100, 96, 88,
        87, 81, 80, 76, 72, 64, 60,
        58, 57, 55, 50, 48, 40,
        36, 32, 29, 20, 16
    };

    [SerializeField]
    public bool bicubicSampling = true;

    [SerializeField]
    public string outputFilePrefix;

    private void OnSelectionChange()
    {
        this.Repaint();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label("Full-size Icon (1024x1024)", EditorStyles.boldLabel);

        fullSizeIcon = Selection.activeObject as Texture2D;

        GUI.enabled = false;
        EditorGUILayout.ObjectField(fullSizeIcon, typeof(Texture2D), false);
        GUI.enabled = true;
        
        EditorGUILayout.Space();
       
        //sharpeningKernelWidth = EditorGUILayout.IntField("Sharpening Kernel Width", sharpeningKernelWidth);
        //sharpeningStrength = EditorGUILayout.FloatField("Sharpening Strength", sharpeningStrength);

        EditorGUILayout.Space();

        bicubicSampling = EditorGUILayout.Toggle("Bicubic Sampling (Sharper)", bicubicSampling, GUILayout.ExpandWidth(false));

        if (fullSizeIcon)
        {
            var fn = fullSizeIcon.name;
            int i = fn.Length - 1;

            while (i >= 0 && char.IsDigit(fn[i]))
                --i;

            if (i < fn.Length)
                outputFilePrefix = fn.Substring(0, i + 1);
        }
        else
        {
            outputFilePrefix = "Icon-";
        }

        GUI.enabled = false;
        EditorGUILayout.TextField("Output Filename", outputFilePrefix + "#");
        GUI.enabled = true;

        EditorGUILayout.Space();

        if(GUILayout.Button("Generate Icons", GUILayout.ExpandWidth(false)))
        {
            if(fullSizeIcon == null)
                throw new System.Exception("Cannot generate icons - full size icon not specified");
            
            if(fullSizeIcon.width != 1024 || fullSizeIcon.height != 1024)
                throw new System.Exception("Cannot generate icons - full size icon must be 1024x1024");

            var importer = AssetImporter.GetAtPath( AssetDatabase.GetAssetPath(fullSizeIcon) ) as TextureImporter;

            var wasReadable = importer.isReadable;
            importer.isReadable = true;
            importer.SaveAndReimport();

            for(int i = 0; i < sizes.Count; ++i) {
                GenerateIcon(sizes[i]);
            }

            importer.isReadable = wasReadable;
            importer.SaveAndReimport();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    float CubicHermite(float a, float b, float c, float d, float t)
    {
        float aa = -a / 2.0f + (3.0f * b) / 2.0f - (3.0f * c) / 2.0f + d / 2.0f;
        float bb = a - (5.0f * b) / 2.0f + 2.0f * c - d / 2.0f;
        float cc = -a / 2.0f + c / 2.0f;
        float dd = b;
        return aa * t * t * t + bb * t * t + cc * t + dd;
    }

    Color32 GetPixelClamped(Color32[] pixels, int width, int height, int x, int y)
    {
        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, width - 1);
        return pixels[y * width + x];
    }

    static int RoundUpToPowerOfTwo(int x)
    {
        int power = 1;

        while(power < x)
            power <<= 1;

        return power;
    }

    void GenerateIcon(int size)
    {
        string projPath = Path.GetDirectoryName(Application.dataPath);
        string relPath = AssetDatabase.GetAssetPath(fullSizeIcon);
        string oldPath = Path.Combine(projPath, relPath);

        var pixels = fullSizeIcon.GetPixels32();

        var startSize = fullSizeIcon.width;
        var mipSize = RoundUpToPowerOfTwo(size);

        if(mipSize < startSize)
        {
            var step = startSize / mipSize;
            var block = step * step;

            Color32[] tmp = new Color32[mipSize * mipSize];

            for(int y = 0; y < mipSize; ++y)
            {
                for(int x = 0; x < mipSize; ++x)
                {
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    int a = 0;

                    for(int ys = 0; ys < step; ++ys)
                    {
                        for(int xs = 0; xs < step; ++xs)
                        {
                            int yy = y * step + ys;
                            int xx = x * step + xs;
                            var c = pixels[yy * startSize + xx];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            a += c.a;
                        }
                    }

                    r = (r + block / 2) / block;
                    g = (g + block / 2) / block;
                    b = (b + block / 2) / block;
                    a = (a + block / 2) / block;

                    tmp[y * mipSize + x] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                }
            }

            pixels = tmp;
            startSize = mipSize;
        }

        if(startSize != size)
        {
            var tmp = new Color32[size * size];

            for(int y = 0; y < size; ++y)
            {
                for(int x = 0; x < size; ++x)
                {
                    if(bicubicSampling)
                    {
                        float u = (float)(0.5f + x) / size;
                        float v = (float)(0.5f + y) / size;

                        float xx = (u * startSize) - 0.5f;
                        int xint = (int)xx;
                        float xfract = xx - Mathf.Floor(xx);

                        float yy = (v * startSize) - 0.5f;
                        int yint = (int)yy;
                        float yfract = yy - Mathf.Floor(yy);

                        var p00 = GetPixelClamped(pixels, startSize, startSize, xint - 1, yint - 1);
                        var p10 = GetPixelClamped(pixels, startSize, startSize, xint + 0, yint - 1);
                        var p20 = GetPixelClamped(pixels, startSize, startSize, xint + 1, yint - 1);
                        var p30 = GetPixelClamped(pixels, startSize, startSize, xint + 2, yint - 1);

                        var p01 = GetPixelClamped(pixels, startSize, startSize, xint - 1, yint + 0);
                        var p11 = GetPixelClamped(pixels, startSize, startSize, xint + 0, yint + 0);
                        var p21 = GetPixelClamped(pixels, startSize, startSize, xint + 1, yint + 0);
                        var p31 = GetPixelClamped(pixels, startSize, startSize, xint + 2, yint + 0);

                        var p02 = GetPixelClamped(pixels, startSize, startSize, xint - 1, yint + 1);
                        var p12 = GetPixelClamped(pixels, startSize, startSize, xint + 0, yint + 1);
                        var p22 = GetPixelClamped(pixels, startSize, startSize, xint + 1, yint + 1);
                        var p32 = GetPixelClamped(pixels, startSize, startSize, xint + 2, yint + 1);

                        var p03 = GetPixelClamped(pixels, startSize, startSize, xint - 1, yint + 2);
                        var p13 = GetPixelClamped(pixels, startSize, startSize, xint + 0, yint + 2);
                        var p23 = GetPixelClamped(pixels, startSize, startSize, xint + 1, yint + 2);
                        var p33 = GetPixelClamped(pixels, startSize, startSize, xint + 2, yint + 2);

                        float r0 = CubicHermite(p00.r, p10.r, p20.r, p30.r, xfract);
                        float r1 = CubicHermite(p01.r, p11.r, p21.r, p31.r, xfract);
                        float r2 = CubicHermite(p02.r, p12.r, p22.r, p32.r, xfract);
                        float r3 = CubicHermite(p03.r, p13.r, p23.r, p33.r, xfract);
                        float red = CubicHermite(r0, r1, r2, r3, yfract);
                        int r = Mathf.Clamp(Mathf.RoundToInt(red), 0, 255);

                        float g0 = CubicHermite(p00.g, p10.g, p20.g, p30.g, xfract);
                        float g1 = CubicHermite(p01.g, p11.g, p21.g, p31.g, xfract);
                        float g2 = CubicHermite(p02.g, p12.g, p22.g, p32.g, xfract);
                        float g3 = CubicHermite(p03.g, p13.g, p23.g, p33.g, xfract);
                        float grn = CubicHermite(g0, g1, g2, g3, yfract);
                        int g = Mathf.Clamp(Mathf.RoundToInt(grn), 0, 255);

                        float b0 = CubicHermite(p00.b, p10.b, p20.b, p30.b, xfract);
                        float b1 = CubicHermite(p01.b, p11.b, p21.b, p31.b, xfract);
                        float b2 = CubicHermite(p02.b, p12.b, p22.b, p32.b, xfract);
                        float b3 = CubicHermite(p03.b, p13.b, p23.b, p33.b, xfract);
                        float blu = CubicHermite(b0, b1, b2, b3, yfract);
                        int b = Mathf.Clamp(Mathf.RoundToInt(blu), 0, 255);

                        float a0 = CubicHermite(p00.a, p10.a, p20.a, p30.a, xfract);
                        float a1 = CubicHermite(p01.a, p11.a, p21.a, p31.a, xfract);
                        float a2 = CubicHermite(p02.a, p12.a, p22.a, p32.a, xfract);
                        float a3 = CubicHermite(p03.a, p13.a, p23.a, p33.a, xfract);
                        float alp = CubicHermite(a0, a1, a2, a3, yfract);
                        int a = Mathf.Clamp(Mathf.RoundToInt(alp), 0, 255);

                        tmp[y * size + x] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    }
                    else
                    {
                        float u = (0.5f + x) / size * startSize - 0.5f;
                        float v = (0.5f + y) / size * startSize - 0.5f;

                        int xx = (int)u;
                        int yy = (int)v;
                        float u1 = u - xx;
                        float v1 = v - yy;
                        float u0 = 1 - u1;
                        float v0 = 1 - v1;

                        Color32 c00 = GetPixelClamped(pixels, startSize, startSize, xx,     yy);
                        Color32 c10 = GetPixelClamped(pixels, startSize, startSize, xx + 1, yy);
                        Color32 c01 = GetPixelClamped(pixels, startSize, startSize, xx,     yy + 1);
                        Color32 c11 = GetPixelClamped(pixels, startSize, startSize, xx + 1, yy + 1);

                        int r = Mathf.Clamp(Mathf.RoundToInt((c00.r * u0 + c10.r * u1) * v0 + (c01.r * u0 + c11.r * u1) * v1), 0, 255);
                        int g = Mathf.Clamp(Mathf.RoundToInt((c00.g * u0 + c10.g * u1) * v0 + (c01.g * u0 + c11.g * u1) * v1), 0, 255);
                        int b = Mathf.Clamp(Mathf.RoundToInt((c00.b * u0 + c10.b * u1) * v0 + (c01.b * u0 + c11.b * u1) * v1), 0, 255);
                        int a = Mathf.Clamp(Mathf.RoundToInt((c00.a * u0 + c10.a * u1) * v0 + (c01.a * u0 + c11.a * u1) * v1), 0, 255);
                        
                        tmp[y * size + x] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    }
                }
            }

            pixels = tmp;
            startSize = size;
        }

        Texture2D outTex = new Texture2D(size, size, TextureFormat.ARGB32, false, true);
        outTex.SetPixels32(pixels, 0);
        var bytes = outTex.EncodeToPNG();

        string newFilename = string.Format("{0}{1}.png", outputFilePrefix, size);
        string newRelPath = Path.Combine(Path.GetDirectoryName(relPath), newFilename);

        string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newFilename);
        File.WriteAllBytes(newPath, bytes);
        
        AssetDatabase.ImportAsset(newRelPath, ImportAssetOptions.ForceUncompressedImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        var imp = AssetImporter.GetAtPath(newRelPath) as TextureImporter;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.sRGBTexture = false;
        imp.mipmapEnabled = false;
        imp.spritePixelsPerUnit = 1;
        imp.spriteImportMode = SpriteImportMode.Single;

        var textureSettings = new TextureImporterSettings();
        imp.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        textureSettings.spriteExtrude = 0;
        textureSettings.alphaSource = TextureImporterAlphaSource.FromInput;
        textureSettings.alphaIsTransparency = true;
        textureSettings.wrapMode = TextureWrapMode.Clamp;
        textureSettings.npotScale = TextureImporterNPOTScale.None;
        imp.SetTextureSettings(textureSettings);

        AssetDatabase.ImportAsset(newRelPath, ImportAssetOptions.ForceUncompressedImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
    }
}
