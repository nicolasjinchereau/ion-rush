using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(TweenPath))]
[CanEditMultipleObjects]
public class TweenPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.Space();

            if(targets.Length == 1)
            {
                GUI.changed = false;

                TweenPath tp = targets[0] as TweenPath;

                if (tp != null)
                {
                    EditorGUILayout.Space();

                    string text = EditorGUILayout.TextField("Subdivisions", tp.subdivisions.ToString());
                    int subdivs = 0;
                    int.TryParse(text, out subdivs);
                    tp.subdivisions = Mathf.Clamp(subdivs, 0, 32);

                    tp.smoothing = (TweenPathSmoothing)EditorGUILayout.EnumPopup(tp.smoothing);

                    tp.showSubdivisions = GUILayout.Toggle(tp.showSubdivisions, "Show Subdivisions");

                    EditorGUILayout.Space();

                    tp.alignPointsToPath = GUILayout.Toggle(tp.alignPointsToPath, "Align Points To Path");

                    EditorGUILayout.Space();

                    GUILayout.Label("Waypoints: " + tp.transform.childCount.ToString());

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    {
                        if(GUILayout.Button("Remove", GUILayout.Width(75)))
                        {
                            tp.RemovePoint();
                        }

                        if(GUILayout.Button("Add", GUILayout.Width(75)))
                        {
                            tp.AddPoint();
                        }
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    if(GUILayout.Button("Align Points to Average Path", GUILayout.Width(200)))
                    {
                        int count = tp.transform.childCount;
                        if(count >= 3)
                        {
                            for(int i = 0; i < count; ++i)
                            {
                                if(i == 0)
                                {
                                    Transform c1 = tp.transform.GetChild(i);
                                    Transform c2 = tp.transform.GetChild(i + 1);

                                    c1.rotation = Quaternion.LookRotation(c2.position - c1.position);
                                }
                                else if(i == (count - 1))
                                {
                                    Transform c1 = tp.transform.GetChild(i - 1);
                                    Transform c2 = tp.transform.GetChild(i);

                                    c2.rotation = Quaternion.LookRotation(c2.position - c1.position);
                                }
                                else
                                {
                                    Transform c1 = tp.transform.GetChild(i - 1);
                                    Transform c2 = tp.transform.GetChild(i);
                                    Transform c3 = tp.transform.GetChild(i + 1);

                                    c2.rotation = Quaternion.LookRotation(c3.position - c1.position);
                                }
                            }
                        }
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button("Align Points to Next Point", GUILayout.Width(200)))
                    {
                        int count = tp.transform.childCount;
                        if(count >= 2)
                        {
                            int i = 0;

                            for( ; i < count - 1; ++i)
                            {
                                Transform c1 = tp.transform.GetChild(i);
                                Transform c2 = tp.transform.GetChild(i + 1);
                                c1.rotation = Quaternion.LookRotation(c2.position - c1.position);
                            }

                            Transform c1b = tp.transform.GetChild(i - 1);
                            Transform c2b = tp.transform.GetChild(i);
                            c2b.rotation = Quaternion.LookRotation(c2b.position - c1b.position);
                        }
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button("Make Waypoints Planar (Maximum)", GUILayout.Width(200)))
                    {
                        MakePlanarMaximum(tp.transform);
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button("Make Waypoints Planar (Average)", GUILayout.Width(200)))
                    {
                        MakePlanarAverage(tp.transform);
                    }

                    EditorGUILayout.Space();

                    if(GUILayout.Button("Make Waypoints Planar (Minimum)", GUILayout.Width(200)))
                    {
                        MakePlanarMinimum(tp.transform);
                    }

                    EditorGUILayout.Space();

                    bool showHandles = tp.showHandles;
                    bool showLines = tp.showLines;

                    tp.showHandles = EditorGUILayout.Toggle("Show Handles", tp.showHandles);
                    tp.showLines = EditorGUILayout.Toggle("Show Lines", tp.showLines);

                    if(showHandles != tp.showHandles
                    || showLines != tp.showLines)
                    {
                        SceneView.RepaintAll();
                    }

                    EditorGUILayout.Space();

                    var lookatTarget = EditorGUILayout.ObjectField("Point Look Target", null, typeof(Transform), true) as Transform;
                    if (lookatTarget)
                    {
                        foreach (Transform tf in tp.transform)
                            tf.LookAt(lookatTarget);

                        tp.UpdatePoints();
                    }
                }

                if(GUI.changed)
                {
                    UnityEditor.Undo.RecordObject(tp, "Update TweenPath Points");
                    tp.UpdatePoints();
                }
            }
            else if(targets.Length > 1)
            {
                GUI.changed = false;

                if(GUILayout.Button("Align Points to Average Path", GUILayout.Width(200)))
                {
                    Undo.RegisterUndo(targets, "Align Points to Average Path");

                    foreach(Object tpObj in targets)
                    {
                        TweenPath tp = (TweenPath)tpObj;

                        int count = tp.transform.childCount;
                        if(count >= 3)
                        {
                            for(int i = 0; i < count; ++i)
                            {
                                if(i == 0)
                                {
                                    Transform c1 = tp.transform.GetChild(i);
                                    Transform c2 = tp.transform.GetChild(i + 1);

                                    c1.rotation = Quaternion.LookRotation(c2.position - c1.position);
                                }
                                else if(i == (count - 1))
                                {
                                    Transform c1 = tp.transform.GetChild(i - 1);
                                    Transform c2 = tp.transform.GetChild(i);

                                    c2.rotation = Quaternion.LookRotation(c2.position - c1.position);
                                }
                                else
                                {
                                    Transform c1 = tp.transform.GetChild(i - 1);
                                    Transform c2 = tp.transform.GetChild(i);
                                    Transform c3 = tp.transform.GetChild(i + 1);

                                    c2.rotation = Quaternion.LookRotation(c3.position - c1.position);
                                }
                            }
                        }

                        UnityEditor.Undo.RecordObject(tp, "Update TweenPath");
                    }
                }

                EditorGUILayout.Space();

                if(GUILayout.Button("Align Points to Next Point", GUILayout.Width(200)))
                {
                    Undo.RecordObjects(targets, "Align Waypoint Directions");

                    foreach(Object tpObj in targets)
                    {
                        TweenPath tp = (TweenPath)tpObj;

                        int count = tp.transform.childCount;
                        if(count >= 2)
                        {
                            int i = 0;

                            for( ; i < count - 1; ++i)
                            {
                                Transform c1 = tp.transform.GetChild(i);
                                Transform c2 = tp.transform.GetChild(i + 1);
                                c1.rotation = Quaternion.LookRotation(c2.position - c1.position);
                            }

                            Transform c1b = tp.transform.GetChild(i - 1);
                            Transform c2b = tp.transform.GetChild(i);
                            c2b.rotation = Quaternion.LookRotation(c2b.position - c1b.position);
                        }
                    }
                }

                int showHandles = 0;
                int showLines = 0;

                foreach(Object tpObj in targets)
                {
                    TweenPath tp = (TweenPath)tpObj;
                    showHandles += tp.showHandles ? 1 : 0;
                    showLines += tp.showLines ? 1 : 0;
                }

                bool showMixedHandles = (showHandles > 0) && (showHandles < targets.Length);
                bool showMixedLines = (showLines > 0) && (showLines < targets.Length);
                bool bShowHandles = showHandles >= (targets.Length / 2);
                bool bShowLines = showLines >= (targets.Length / 2);

                bool updateScene = false;

                EditorGUI.BeginChangeCheck();

                EditorGUI.showMixedValue = showMixedHandles;
                EditorGUILayout.Toggle("Show Handles", bShowHandles);

                if(EditorGUI.EndChangeCheck())
                {
                    bShowHandles = !bShowHandles;
                    updateScene = true;

                    foreach(Object tpObj in targets)
                    {
                        TweenPath tp = (TweenPath)tpObj;
                        UnityEditor.Undo.RecordObject(tp, "Update TweenPath Handles");

                        if (tp != null)
                        {
                            tp.showHandles = bShowHandles;
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();

                EditorGUI.showMixedValue = showMixedLines;
                EditorGUILayout.Toggle("Show Lines", bShowLines);

                if(EditorGUI.EndChangeCheck())
                {
                    bShowLines = !bShowLines;
                    updateScene = true;

                    foreach(Object tpObj in targets)
                    {
                        TweenPath tp = (TweenPath)tpObj;
                        UnityEditor.Undo.RecordObject(tp, "Update TweenPath Lines");

                        if (tp != null)
                        {
                            tp.showLines = bShowLines;
                        }
                    }
                }

                if(updateScene)
                {
                    SceneView.RepaintAll();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    bool GetMouseButtonDown(int button)
    {
        return (Event.current.type == EventType.MouseDown) && (Event.current.button == button);
    }

    bool GetMouseButtonUp(int button)
    {
        return (Event.current.type == EventType.MouseUp) && (Event.current.button == button);
    }

    bool didUndo = false;

    void OnSceneGUI()
    {
        TweenPath path = target as TweenPath;

        bool mouseUp = GetMouseButtonUp(0);
        bool didMove = false;

        if(path.showHandles)
        {
            if(Tools.current == Tool.Move)
            {
                if(Tools.pivotRotation == PivotRotation.Global)
                {
                    if(!path.showSubdivisions)
                    {
                        int ct = path.transform.childCount;

                        for(int i = 0; i < ct; ++i)
                        {
                            Transform xf = path.transform.GetChild(i);

                            Vector3 newPos = Handles.PositionHandle(xf.position, Quaternion.identity);

                            if((newPos - xf.position).magnitude > 0)
                                didMove = true;

                            if(!didUndo && didMove)
                            {
                                didUndo = true;
                                Undo.RegisterUndo(xf, "Moved Tween Point");
                            }

                            xf.position = newPos;
                        }
                    }
                    else
                    {
                        int ct = path.points.Count;

                        for(int i = 0; i < ct; ++i)
                        {
                            Handles.PositionHandle(path.points[i].pos, Quaternion.identity);
                        }
                    }
                }
                else if(Tools.pivotRotation == PivotRotation.Local)
                {
                    if(!path.showSubdivisions)
                    {
                        int ct = path.transform.childCount;

                        for(int i = 0; i < ct; ++i)
                        {
                            Transform xf = path.transform.GetChild(i);

                            Vector3 newPos = Handles.PositionHandle(xf.position, xf.rotation);

                            if((newPos - xf.position).magnitude > 0)
                                didMove = true;

                            if(!didUndo && didMove)
                            {
                                didUndo = true;
                                Undo.RegisterUndo(xf, "Moved Tween Point");
                            }

                            xf.position = newPos;
                        }
                    }
                    else
                    {
                        int ct = path.points.Count;

                        for(int i = 0; i < ct; ++i)
                        {
                            Handles.PositionHandle(path.points[i].pos, path.points[i].rot);
                        }
                    }
                }
            }
            else if(Tools.current == Tool.Rotate)
            {
                if(Tools.pivotRotation == PivotRotation.Global)
                {
                    if(!path.showSubdivisions)
                    {
                        int ct = path.transform.childCount;

                        for(int i = 0; i < ct; ++i)
                        {
                            Transform xf = path.transform.GetChild(i);

                            Handles.PositionHandle(xf.position, Quaternion.identity);
                            Quaternion offRot = Handles.RotationHandle(Quaternion.identity, xf.position);

                            if(Quaternion.Angle(offRot, Quaternion.identity) > 0.1f)
                            {
                                didMove = true;
                            }

                            if(!didUndo && didMove)
                            {
                                didUndo = true;
                                Undo.RegisterUndo(xf, "Moved Tween Point");
                            }

                            xf.rotation = offRot * xf.rotation;
                        }
                    }
                    else
                    {
                        int ct = path.points.Count;

                        for(int i = 0; i < ct; ++i)
                        {
                            Handles.PositionHandle(path.points[i].pos, Quaternion.identity);
                            Handles.RotationHandle(Quaternion.identity, path.points[i].pos);
                        }
                    }
                }
                else if(Tools.pivotRotation == PivotRotation.Local)
                {
                    if(!path.showSubdivisions)
                    {
                        int ct = path.transform.childCount;

                        for(int i = 0; i < ct; ++i)
                        {
                            Transform xf = path.transform.GetChild(i);

                            Handles.PositionHandle(xf.position, xf.rotation);
                            Quaternion newRot = Handles.RotationHandle(xf.rotation, xf.position);

                            if(Quaternion.Angle(newRot, xf.rotation) > 0.1f)
                                didMove = true;

                            if(!didUndo && didMove)
                            {
                                didUndo = true;
                                Undo.RegisterUndo(xf, "Moved Tween Point");
                            }

                            xf.rotation = newRot;
                        }
                    }
                    else
                    {
                        int ct = path.points.Count;

                        for(int i = 0; i < ct; ++i)
                        {
                            Handles.PositionHandle(path.points[i].pos, path.points[i].rot);
                            Handles.RotationHandle(path.points[i].rot, path.points[i].pos);
                        }
                    }
                }
            }
        }

        bool pathIsEmpty = path.points.Count == 0 && path.transform.childCount > 0;

        if(pathIsEmpty || didMove)
        {
            UnityEditor.Undo.RecordObject(path, "Update TweenPath Points");
            path.UpdatePoints();
            SceneView.RepaintAll();
        }

        if(mouseUp)
            didUndo = false;
    }
    
    void MakePlanarMaximum(Transform tp)
    {
        UnityEditor.Undo.RecordObject(tp, "Make Waypoints Planar (Maximum)");

        int count = tp.childCount;
        if(count > 0)
        {
            float maxY = tp.GetChild(0).position.y;

            for(int i = 1; i < count; ++i)
            {
                maxY = Mathf.Max(maxY, tp.GetChild(i).position.y);
            }

            for(int i = 0; i < count; ++i)
            {
                Vector3 pos = tp.GetChild(i).position;
                pos.y = maxY;
                tp.GetChild(i).position = pos;
            }
        }
    }

    void MakePlanarAverage(Transform tp)
    {
        UnityEditor.Undo.RecordObject(tp, "Make Waypoints Planar (Average)");

        int count = tp.childCount;
        if(count > 0)
        {
            float avgY = 0;

            for(int i = 0; i < count; ++i)
            {
                avgY += tp.GetChild(i).position.y;
            }

            avgY /= (float)count;

            for(int i = 0; i < count; ++i)
            {
                Vector3 pos = tp.GetChild(i).position;
                pos.y = avgY;
                tp.GetChild(i).position = pos;
            }
        }
    }

    void MakePlanarMinimum(Transform tp)
    {
        UnityEditor.Undo.RecordObject(tp, "Make Waypoints Planar (Minimum)");

        int count = tp.childCount;
        if(count > 0)
        {
            float minY = tp.GetChild(0).position.y;

            for(int i = 1; i < count; ++i)
            {
                minY = Mathf.Min(minY, tp.GetChild(i).position.y);
            }

            for(int i = 0; i < count; ++i)
            {
                Vector3 pos = tp.GetChild(i).position;
                pos.y = minY;
                tp.GetChild(i).position = pos;
            }
        }
    }
}
