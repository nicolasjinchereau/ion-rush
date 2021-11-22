using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct TweenPoint
{
    public Vector3 pos;
    public Quaternion rot;

    public void Apply(Transform xf) {
        xf.position = pos;
        xf.rotation = rot;
    }
}

public enum TweenPathSmoothing
{
    None,
    Subdivision,
    CatmullRom
}

public class TweenPath : MonoBehaviour, IEnumerable<TweenPoint>
{
    public bool showHandles = false;
    public bool showLines = true;
    public int subdivisions = 0;
    public bool showSubdivisions = false;
    public bool alignPointsToPath = true;
    public List<TweenPoint> points = new List<TweenPoint>();
    public float totalLength = 0;
    public float speed = 1.0f;
    public TweenPathSmoothing smoothing = TweenPathSmoothing.Subdivision;

    public void Awake()
    {
        UpdatePoints();

        foreach (Transform c in transform)
            c.gameObject.SetActive(false);
    }
    
    public GameObject AddPoint()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if(go.GetComponent<Collider>() != null)
            DestroyImmediate(go.GetComponent<Collider>());

        go.transform.localScale = Vector3.one * 0.25f;
        go.name = "Waypoint " + transform.childCount.ToString();
        go.GetComponent<Renderer>().sharedMaterial = Resources.Load("TweenMaterial") as Material;

        if (transform.childCount == 0)
        {
            go.transform.position = transform.position;
        }
        else if (transform.childCount == 1)
        {
            go.transform.position = transform.GetChild(0).position + Vector3.forward * 5.0f;
        }
        else if (transform.childCount > 1)
        {
            Vector3 a = transform.GetChild(transform.childCount - 1).position;
            Vector3 b = transform.GetChild(transform.childCount - 2).position;

            go.transform.position = a + (a - b).normalized * 5.0f;
        }

        go.transform.parent = this.transform;

        CenterPivot();

        return go;
    }

    public void RemovePoint()
    {
        if (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }

        CenterPivot();
    }

    public int waypointCount
    {
        get
        {
            return transform.childCount;
        }
    }

    public void UpdatePoints()
    {
        if (smoothing == TweenPathSmoothing.Subdivision)
            UpdatePointsSubdivision();
        else if (smoothing == TweenPathSmoothing.CatmullRom)
            UpdatePointsCatmullRom();
    }

    public void UpdatePointsCatmullRom()
    {
        List<TweenPoint> newPoints = new List<TweenPoint>(transform.childCount * subdivisions);
        
        if (transform.childCount >= 2)
        {
            List<TweenPoint> points = new List<TweenPoint>(transform.childCount);

            foreach (Transform t in transform)
            {
                points.Add(new TweenPoint() {
                    pos = t.position,
                    rot = t.rotation
                });
            }
            
            float stepSize = 1.0f / subdivisions;

            for (int p = 0; p < points.Count - 1; ++p)
            {
                var p1 = points[Mathf.Clamp(p - 1, 0, points.Count - 1)];
                var p2 = points[Mathf.Clamp(p, 0, points.Count - 1)];
                var p3 = points[Mathf.Clamp(p + 1, 0, points.Count - 1)];
                var p4 = points[Mathf.Clamp(p + 2, 0, points.Count - 1)];

                var f1 = p1.pos + p1.rot * Vector3.forward * 0.05f;
                var f2 = p2.pos + p2.rot * Vector3.forward * 0.05f;
                var f3 = p3.pos + p3.rot * Vector3.forward * 0.05f;
                var f4 = p4.pos + p4.rot * Vector3.forward * 0.05f;

                var u1 = p1.pos + p1.rot * Vector3.up * 0.05f;
                var u2 = p2.pos + p2.rot * Vector3.up * 0.05f;
                var u3 = p3.pos + p3.rot * Vector3.up * 0.05f;
                var u4 = p4.pos + p4.rot * Vector3.up * 0.05f;

                for (float t = 0; t < 1.0f; t += stepSize)
                {
                    var pos = CatmullRom(t, p1.pos, p2.pos, p3.pos, p4.pos);
                    var forward = CatmullRom(t, f1, f2, f3, f4);
                    var up = CatmullRom(t, u1, u2, u3, u4);
                    var rot = Quaternion.LookRotation(forward - pos, up - pos);

                    newPoints.Add(new TweenPoint() {
                        pos = pos,
                        rot = rot
                    });
                }
            }

            newPoints.Add(new TweenPoint() {
                pos = points[points.Count - 1].pos,
                rot = points[points.Count - 1].rot
            });
        }

        this.points = newPoints;

        UpdateTotalLength();

        if (alignPointsToPath)
            AlignPointsToPath();
    }

    public void UpdatePointsSubdivision()
    {
        List<TweenPoint> points = new List<TweenPoint>(transform.childCount);

        foreach(Transform t in transform)
        {
            points.Add(new TweenPoint(){
                pos = t.position,
                rot = t.rotation
            });
        }

        for(int s = 0; s < subdivisions; ++s)
        {
            List<TweenPoint> newPoints = new List<TweenPoint>(points.Count * 2);

            for(int p = 0; p < points.Count - 1; ++p)
            {
                TweenPoint newPt = new TweenPoint(){
                    pos = Vector3.Lerp(points[p].pos, points[p + 1].pos, 0.5f),
                    rot = Quaternion.Slerp(points[p].rot, points[p + 1].rot, 0.5f)
                };

                newPoints.Add(points[p]);
                newPoints.Add(newPt);
            }

            newPoints.Add(points[points.Count - 1]);

            points = newPoints;
            newPoints = new List<TweenPoint>(points);

            for(int p = 1; p < points.Count - 1; ++p)
            {
                int prev = Mathf.Max(p - 1, 0);
                int next = Mathf.Min(p + 1, points.Count - 1);

                TweenPoint tpt = new TweenPoint();

                tpt.pos = (points[prev].pos + points[p].pos + points[next].pos) / 3.0f;

                Quaternion q1 = Quaternion.Slerp(points[prev].rot, points[p].rot, 0.5f);
                Quaternion q2 = Quaternion.Slerp(points[p].rot, points[next].rot, 0.5f);
                tpt.rot = Quaternion.Slerp(q1, q2, 0.5f);

                newPoints[p] = tpt;
            }

            points = newPoints;
        }

        this.points = points;

        UpdateTotalLength();

        if (alignPointsToPath)
            AlignPointsToPath();
    }

    void UpdateTotalLength()
    {
        totalLength = 0;

        for (int i = 0; i < points.Count - 1; ++i) {
            totalLength += Vector3.Distance(points[i].pos, points[i + 1].pos);
        }
    }

    void AlignPointsToPath()
    {
        int i = 0;
        for (; i < points.Count - 1; ++i)
        {
            TweenPoint tp = points[i];
            tp.rot = Quaternion.LookRotation(points[i + 1].pos - points[i].pos);
            points[i] = tp;
        }

        TweenPoint tp1 = points[i];
        tp1.rot = Quaternion.LookRotation(points[i].pos - points[i - 1].pos);
        points[i] = tp1;
    }

    public Transform GetWaypoint(int index) {
        return transform.GetChild(index);
    }

    public IEnumerator MoveObject(Transform xf) {
        return MoveObject(xf, this.speed);
    }
    
    public IEnumerator MoveObject(Transform xf, float movementSpeed)
    {
        var en = Traverse(movementSpeed);
        while (en.MoveNext())
        {
            en.Current.Apply(xf);
            yield return null;
        }
    }
    
    public IEnumerator<TweenPoint> Traverse(float movementSpeed)
    {
        int _index = 0;
        float _dist = 0;

        yield return new TweenPoint(){
            pos = points[0].pos,
            rot = points[0].rot
        };

        while(_index != points.Count - 1)
        {
            _dist += movementSpeed * Time.deltaTime;

            float length = Vector3.Distance(points[_index + 1].pos, points[_index].pos);

            while(_dist >= length && ++_index < points.Count - 1)
            {
                _dist -= length;
                length = (points[_index + 1].pos - points[_index].pos).magnitude;
            }

            if(_index < points.Count - 1)
            {
                float t = _dist / length;

                yield return new TweenPoint(){
                    pos = Vector3.Lerp(points[_index].pos, points[_index + 1].pos, t),
                    rot = Quaternion.Slerp(points[_index].rot, points[_index + 1].rot, t)
                };
            }
            else
            {
                yield return new TweenPoint(){
                    pos = points[_index].pos,
                    rot = points[_index].rot
                };
            }
        }
    }

    public IEnumerator<TweenPoint> GetEnumerator() {
        return Traverse(this.speed);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return Traverse(this.speed);
    }
    
    public class Iterator
    {
        private TweenPath _path;
        private int _index;
        private float _dist;
        private Vector3 _pos;
        private Quaternion _rot;

        public Vector3 position {
            get { return _pos; }
        }

        public Quaternion rotation {
            get { return _rot; }
        }

        public Iterator(TweenPath path)
        {
            if(path.points.Count < 2)
                throw new System.Exception("Cannot Create an Iterator from an Empty TweenPath");

            _path = path;
            _dist = 0;
            _index = 0;
            _pos = _path.points[_index].pos;
            _rot = _path.points[_index].rot;
        }

        public bool Advance(float dist)
        {
            if(_index == _path.points.Count - 1)
                return false;

            _dist += dist;

            float length = Vector3.Distance(_path.points[_index + 1].pos, _path.points[_index].pos);

            while(_dist >= length && ++_index < _path.points.Count - 1)
            {
                _dist -= length;
                length = (_path.points[_index + 1].pos - _path.points[_index].pos).magnitude;
            }

            if(_index < _path.points.Count - 1)
            {
                float t = _dist / length;
                _pos = Vector3.Lerp(_path.points[_index].pos, _path.points[_index + 1].pos, t);
                _rot = Quaternion.Slerp(_path.points[_index].rot, _path.points[_index + 1].rot, t);
            }
            else
            {
                _pos = _path.points[_index].pos;
                _rot = _path.points[_index].rot;
            }

            return true;
        }

        public void Apply(Transform xf)
        {
            xf.position = _pos;
            xf.rotation = _rot;
        }

        public bool Apply(Transform xf, float advanceBy)
        {
            xf.position = _pos;
            xf.rotation = _rot;
            return Advance(advanceBy);
        }
    }

    public Iterator GetIterator()
    {
        return new Iterator(this);
    }

    public float pathLength
    {
        get
        {
            float length = 0;

            int pointCount = points.Count;

            for(int i = 1; i < pointCount; ++i) {
                length += Vector3.Distance(points[i - 1].pos, points[i].pos);
            }

            return length;
        }
    }

    public float GetLengthBetweenPoints(int a, int b)
    {
        int pointCount = transform.childCount;

        if(b <= a)
        {
            Debug.Log("GetLengthBetweenPoints: Points A is smaller than B");
            return 0;
        }

        if(b >= pointCount)
        {
            Debug.Log("GetLengthBetweenPoints: Point B is out of range");
            return 0;
        }

        float dist = 0;

        for(int i = a; i < b; ++i)
        {
            dist += (transform.GetChild(i + 1).position - transform.GetChild(i).position).magnitude;
        }

        return dist;
    }

    public Transform destination
    {
        get
        {
            Transform t = null;

            if(transform.childCount > 0)
                t = transform.GetChild(transform.childCount - 1);

            return t;
        }
    }

    public void CenterPivot()
    {
        if (transform.childCount == 0)
            return;

        Vector3 min = transform.GetChild(0).position;

        for(int i = 1; i < transform.childCount; ++i)
        {
            min = Vector3.Min(min, transform.GetChild(i).position);
        }

        if((transform.position - min).sqrMagnitude > 0.001f)
        {
            Transform[] tr = new Transform[transform.childCount];

            int i = 0;
            foreach (Transform t in transform)
            {
                tr[i++] = t;
            }

            foreach (Transform t in tr)
            {
                t.parent = null;
            }

            transform.position = min;

            foreach (Transform t in tr)
            {
                t.parent = transform;
            }
        }
    }

    public Vector3 SamplePosition(float t)
    {
        float d = Mathf.Clamp01(t) * totalLength;

        float segStart = 0;

        int pointCount = points.Count;

        for(int i = 1; i < pointCount; ++i)
        {
            float segLength = (points[i].pos - points[i - 1].pos).magnitude;
            float segEnd = segStart + segLength;

            if(d >= segStart && d < segEnd)
            {
                float lt = (d - segStart) / segLength;
                return Vector3.Lerp(points[i - 1].pos, points[i].pos, lt);
            }

            segStart = segEnd;
        }

        return points[pointCount - 1].pos;
    }

    public Quaternion SampleRotation(float t)
    {
        float d = Mathf.Clamp01(t) * totalLength;

        float segStart = 0;

        int pointCount = points.Count;

        for(int i = 1; i < pointCount; ++i)
        {
            float segLength = (points[i].pos - points[i - 1].pos).magnitude;
            float segEnd = segStart + segLength;

            if(d >= segStart && d < segEnd)
            {
                float lt = (d - segStart) / segLength;
                return Quaternion.Slerp(points[i - 1].rot, points[i].rot, lt);
            }

            segStart = segEnd;
        }

        return points[pointCount - 1].rot;
    }

    public Vector3 CatmullRom(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return (
               (b * 2) +
               (-a + c) * t +
               (a * 2 - b * 5 + c * 4 - d) * t * t +
               (-a + b * 3 - c * 3 + d) * t * t * t
               ) * 0.5f;
    }

#if UNITY_EDITOR

    public void OnDrawGizmos()
    {
        if(showLines)
        {
            for(int i = 0; i < points.Count - 1; ++i)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(points[i].pos, points[i + 1].pos);
            }
        }
    }
    #endif
}
