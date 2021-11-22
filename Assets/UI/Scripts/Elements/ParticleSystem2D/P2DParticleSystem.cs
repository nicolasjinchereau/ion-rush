using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public enum P2DEmitterType
{
    Point,
    Ellipse,
    Rectangle
}

[RequireComponent(typeof(CanvasRenderer))]
public class P2DParticleSystem : MaskableGraphic
{
    public bool emit = false;
    public int rate = 10;
    public int maxParticles = 1000;
    public P2DEmitterType emitterType;
    public Vector2 particleSize = new Vector2(100, 100);
    public Sprite sprite;

    public float life = 1.0f;
    public float lifeVariation = 0.0f;

    public float startSpeed = 100.0f;
    public float speedVariation = 0.0f;
    public AnimationCurve speedScale = AnimationCurve.Constant(0, 1, 1);

    public bool angleRelativeToVelocity = false;
    public float startAngle = 0;
    public float angleVariation = 0.0f;

    public float startScale = 1.0f;
    public float scaleVariation = 0.0f;
    public AnimationCurve scaleScale = AnimationCurve.Constant(0, 1, 1);

    public float startAngularVelocity = 0;
    public float angularVelocityVariation = 0.0f;
    public AnimationCurve angularVelocityScale = AnimationCurve.Constant(0, 1, 1);

    public Vector2 startForce = Vector2.zero;
    public float forceVariation = 0.0f;
    public AnimationCurve forceScale = AnimationCurve.Constant(0, 1, 1);

    public float startAlpha = 1.0f;
    public float alphaVariation = 0.0f;
    public AnimationCurve alphaScale = AnimationCurve.Constant(0, 1, 1);

    protected P2DEmitter _emitter = new P2DPointEmitter();
    protected bool _lastEmitValue = false;
    protected float _particlesToSpawn = 0.0f;
    protected float _lastSpawn = 0;

    protected List<P2DParticle> _particles = new List<P2DParticle>();
    protected List<Vector3> _vertices = new List<Vector3>();
    protected List<Vector2> _texcoords = new List<Vector2>();
    protected List<Color> _colors = new List<Color>();
    
    public int particleCount {
        get { return _particles.Count; }
    }

    public bool alive {
        get { return _particles.Count > 0; }
    }

    public override Texture mainTexture
    {
        get
        {
            if (sprite == null) {
                if (material != null && material.mainTexture != null) {
                    return material.mainTexture;
                }

                return s_WhiteTexture;
            }

            return sprite.texture;
        }
    }

    public P2DParticleSystem() {
        useLegacyMeshGeneration = false;
    }

    public void Emit(int count)
    {
        Emit(count, Vector2.zero);
    }

    public void Emit(int count, Vector2 position)
    {
        if (_emitter == null || _emitter.type != emitterType)
        {
            switch (emitterType)
            {
                case P2DEmitterType.Point:
                    _emitter = new P2DPointEmitter();
                    break;

                case P2DEmitterType.Ellipse:
                    _emitter = new P2DEllipseEmitter();
                    break;

                case P2DEmitterType.Rectangle:
                    _emitter = new P2DRectangleEmitter();
                    break;

                default:
                    throw new Exception("Invalid emitter type");
            }
        }
        
        count = Mathf.Min(maxParticles - _particles.Count, count);

        for(int i = 0; i < count; ++i)
            _particles.Add(_emitter.Emit(this, position));
    }

    protected override void OnEnable()
    {
        _lastSpawn = Time.time;
        _particlesToSpawn = 0.0f;
    }
    
    public void Update()
    {
        int particleCount = _particles.Count;
        int numVerts = particleCount * 4;

        _vertices.Clear();
        _texcoords.Clear();
        _colors.Clear();

        if(_vertices.Capacity < numVerts)  _vertices.Capacity = numVerts;
        if(_texcoords.Capacity < numVerts) _texcoords.Capacity = numVerts;
        if(_colors.Capacity < numVerts)    _colors.Capacity = numVerts;

        float hw = particleSize.x * 0.5f;
        float hh = particleSize.y * 0.5f;

        for(int i = 0; i < _particles.Count; ++i)
        {
            P2DParticle p = _particles[i];

            if(Time.time >= p.birth + p.life)
            {
                _particles.RemoveAt(i--);
                continue;
            }
            
            float t = Mathf.Clamp01((Time.time - p.birth) / p.life);
            p.velocity += startForce * (forceScale.Evaluate(t) * Time.deltaTime);
            p.position += p.velocity * (speedScale.Evaluate(t) * Time.deltaTime);
            p.angle += p.angularVelocity * (angularVelocityScale.Evaluate(t) * Time.deltaTime);

            float p_scale = p.scale * scaleScale.Evaluate(t);
            float p_alpha = p.alpha * alphaScale.Evaluate(t);
            
            Matrix4x4 xf = Matrix4x4.TRS(new Vector3(p.position.x, p.position.y, 0),
                                         Quaternion.Euler(0, 0, p.angle),
                                         new Vector3(p_scale, p_scale, 1));

            Color col = this.color;
            col.a *= p_alpha;

            _vertices.Add(xf.MultiplyPoint(new Vector3(-hw,  hh, 0)));
            _vertices.Add(xf.MultiplyPoint(new Vector3(-hw, -hh, 0)));
            _vertices.Add(xf.MultiplyPoint(new Vector3( hw,  hh, 0)));
            _vertices.Add(xf.MultiplyPoint(new Vector3( hw, -hh, 0)));

            _texcoords.Add(new Vector2(0.0f, 1.0f));
            _texcoords.Add(new Vector2(0.0f, 0.0f));
            _texcoords.Add(new Vector2(1.0f, 1.0f));
            _texcoords.Add(new Vector2(1.0f, 0.0f));

            _colors.Add(col);
            _colors.Add(col);
            _colors.Add(col);
            _colors.Add(col);
        }

        if(emit && !_lastEmitValue)
        {
            _lastSpawn = Time.time;
            _particlesToSpawn = 0.0f;
        }

        _lastEmitValue = emit;

        float timeSinceSpawn = Time.time - _lastSpawn;
        float particleDelay = rate > 0 ? (1.0f / rate) : 0.0f;

        if(emit && rate > 0 && timeSinceSpawn >= particleDelay)
        {
            _particlesToSpawn += particleDelay > 0 ? timeSinceSpawn / particleDelay : 0.0f;
            int pcount = (int)_particlesToSpawn;
            _particlesToSpawn -= (float)pcount;

            Emit(pcount);

            _lastSpawn = Time.time;
        }

        if(_particles.Count != 0 || particleCount != 0)
            SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        for(int v = 0; v < _vertices.Count; v += 4)
        {
            int i0 = v + 0;
            int i1 = v + 1;
            int i2 = v + 2;
            int i3 = v + 3;

            vh.AddVert(_vertices[i0], _colors[i0], _texcoords[i0]);
            vh.AddVert(_vertices[i1], _colors[i1], _texcoords[i1]);
            vh.AddVert(_vertices[i2], _colors[i2], _texcoords[i2]);
            vh.AddVert(_vertices[i3], _colors[i3], _texcoords[i3]);
            vh.AddTriangle(v + 0, v + 1, v + 2);
            vh.AddTriangle(v + 2, v + 1, v + 3);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var mtx = UnityEditor.Handles.matrix;
        
        try
        {
            if (emitterType == P2DEmitterType.Ellipse)
            {
                var sz = rectTransform.sizeDelta * 0.5f;
                UnityEditor.Handles.matrix = rectTransform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(sz.x, sz.y, 1));
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, transform.forward, 1.0f);
            }
            else if (emitterType == P2DEmitterType.Rectangle)
            {
                var sz = rectTransform.sizeDelta;
                UnityEditor.Handles.matrix = rectTransform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(sz.x, sz.y, 0));
                UnityEditor.Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
        finally
        {
            UnityEditor.Handles.matrix = mtx;
        }
    }

    private void OnDrawGizmosSelected()
    {
        var mtx = UnityEditor.Handles.matrix;
        var col = UnityEditor.Handles.color;

        try
        {
            if (emitterType == P2DEmitterType.Ellipse)
            {
                var sz = rectTransform.sizeDelta * 0.5f;
                UnityEditor.Handles.matrix = rectTransform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(sz.x, sz.y, 1));
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, transform.forward, 1.0f);
            }
            else if (emitterType == P2DEmitterType.Rectangle)
            {
                var sz = rectTransform.sizeDelta;
                UnityEditor.Handles.matrix = rectTransform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(sz.x, sz.y, 0.1f));
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
        finally
        {
            UnityEditor.Handles.matrix = mtx;
            UnityEditor.Handles.color = col;
        }
    }
#endif
}
