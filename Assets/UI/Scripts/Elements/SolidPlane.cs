using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SolidPlane : MaskableGraphic
{
    protected SolidPlane() {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

        var color32 = color;
        var uvRect = new Rect(0f, 0f, 1f, 1f);
        vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uvRect.xMin, uvRect.yMin));
        vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uvRect.xMin, uvRect.yMax));
        vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uvRect.xMax, uvRect.yMax));
        vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uvRect.xMax, uvRect.yMin));
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
