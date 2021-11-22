using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallWithWindow : MonoBehaviour
{
    public Transform wall;
    public Transform boneBL;
    public Transform boneBR;
    public Transform boneTL;
    public Transform boneTR;

    [Space]
    public Vector2 windowPos = new Vector2(0.25f, 0.25f);
    public Vector2 windowSize = new Vector2(0.5f, 0.5f);
    public bool useMeshCollider = false;
    public bool generateBackSideMesh = true;

    private void Awake()
    {
        UpdateWindow();
    }

    private void OnValidate()
    {
        UpdateWindow();
    }

    private void UpdateWindow()
    {
        if (!wall)
            return;

        var meshRenderer = wall.GetComponent<MeshRenderer>();
        if(meshRenderer.sharedMaterial == null)
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));


        Mesh mesh = new Mesh();
        mesh.name = "Generated Wall Mesh";

        Vector3[] vertices = new Vector3[]
        {
            // front
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(windowPos.x, windowPos.y + windowSize.y, 0),
            new Vector3(windowPos.x + windowSize.x, windowPos.y + windowSize.y, 0),
            new Vector3(windowPos.x, windowPos.y, 0),
            new Vector3(windowPos.x + windowSize.x, windowPos.y, 0),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),

            // top
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),

            // back
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(windowPos.x, windowPos.y + windowSize.y, 1),
            new Vector3(windowPos.x + windowSize.x, windowPos.y + windowSize.y, 1),
            new Vector3(windowPos.x, windowPos.y, 1),
            new Vector3(windowPos.x + windowSize.x, windowPos.y, 1),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
        };
        mesh.SetVertices(vertices, 0, generateBackSideMesh ? 20 : 12);

        Vector3[] normals = new Vector3[]
        {
            // front
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back,

            // top
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,

            // back
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
        };
        mesh.SetNormals(normals, 0, generateBackSideMesh ? 20 : 12);

        Vector2[] uv = new Vector2[]
        {
            // front
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(windowPos.x, windowPos.y + windowSize.y),
            new Vector2(windowPos.x + windowSize.x, windowPos.y + windowSize.y),
            new Vector2(windowPos.x, windowPos.y),
            new Vector2(windowPos.x + windowSize.x, windowPos.y),
            new Vector2(0, 0),
            new Vector2(1, 0),

            // top
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0.98f),
            new Vector2(1, 0.98f),

            // back
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(windowPos.x, windowPos.y + windowSize.y),
            new Vector2(windowPos.x + windowSize.x, windowPos.y + windowSize.y),
            new Vector2(windowPos.x, windowPos.y),
            new Vector2(windowPos.x + windowSize.x, windowPos.y),
            new Vector2(0, 0),
            new Vector2(1, 0),
        };
        mesh.SetUVs(0, uv, 0, generateBackSideMesh ? 20 : 12);

        int[] tris = new int[]
        {
            // front
            0, 1, 2,
            2, 1, 3,
            4, 5, 6,
            6, 5, 7,
            6, 0, 4,
            4, 0, 2,
            3, 1, 7,
            5, 3, 7,

            // top
            8, 11, 9,
            8, 10, 11,

            // back
            14, 13, 12,
            15, 13, 14,
            18, 17, 16,
            19, 17, 18,
            16, 12, 18,
            14, 12, 16,
            19, 13, 15,
            19, 15, 17,
        };
        mesh.SetTriangles(tris, 0, generateBackSideMesh ? 54 : 30, 0);

        var meshFilter = wall.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var meshCollider = wall.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = useMeshCollider ? mesh : null;
        meshCollider.enabled = useMeshCollider;

        var wallScale = wall.localScale;
        var depth = 0.1f;

        boneBL.localPosition = new Vector3(-windowPos.x * wallScale.x, windowPos.y * wallScale.y, depth);
        boneBR.localPosition = new Vector3(-(windowPos.x + windowSize.x) * wallScale.x, windowPos.y * wallScale.y, depth);
        boneTL.localPosition = new Vector3(-windowPos.x * wallScale.x, (windowPos.y + windowSize.y) * wallScale.y, depth);
        boneTR.localPosition = new Vector3(-(windowPos.x + windowSize.x) * wallScale.x, (windowPos.y + windowSize.y) * wallScale.y, depth);
    }
}
