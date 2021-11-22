using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraRenderMode
{
    Normal,
    Colorful,
    Grayscale,
    Sepia,
}

public enum CameraPostColorMode
{
    None,
    Add,
    Multiply,
    AlphaBlend,
}

public enum RenderEffect
{
    None,
    Wave,
    Pinch,
    Dream,
    Aberration,
    //TemporalBlur
}

public class RenderManager : MonoBehaviour
{
    public static RenderManager that {
        get; private set;
    }
    
    CameraRenderMode _renderMode = CameraRenderMode.Normal;
    CameraPostColorMode _cameraPostColorMode = CameraPostColorMode.AlphaBlend;
    RenderEffect _renderEffect = RenderEffect.None;
    Color _postColor = Color.black;
    float _postColorScale = 1.0f;
    public Material _postMaterial = null;
    public Material _plainMaterial = null;
    Material _downsample2XMaterial = null;
    Material _downsample1XMaterial = null;
    Shader _normalShader = null;
    Shader _waveShader = null;
    Shader _pinchShader = null;
    Shader _dreamShader = null;
    Shader _aberrationShader = null;
    Vector2 _wavePhase = Vector2.zero;
    Vector2 _waveFreq = Vector2.zero;
    Vector2 _waveAmplitude = Vector2.zero;
    float _pinchRadius = 1.0f;
    float _pinchOffset = 0.1f;
    float _pinchAmplitude = 1.0f;
    float _pinchPower = 1.0f;
    float _bloomPower = 0; // 2
    float _bloomStrength = 0; // 0.07f;
    float _noiseStrength = 0; // 1.0f;
    float _blurStrength = 0; // 1.0f;
    Vector2 _aberrationRedOffset = Vector2.zero; 
    Vector2 _aberrationGreenOffset = Vector2.zero;
    Vector2 _aberrationBlueOffset = Vector2.zero;
    bool _dirtyPost = true;
    
    // flickering
    bool _flicker = false;
    bool _lit = true;
    float _nextFlicker;
    float _flickerStart;
    float _flickerLength;
    float _flickerFinish;
    Color _flickerColor = Color.clear;
    
    // fading
    bool isFading = false;
    float fadeStart = 0.0f;
    float fadeLength = 0.0f;
    Color startColor;
    Color goalColor;
    
    // post processing
    int _targetWidth;
    int _targetHeight;
    
    RenderTexture _targetTex_mip2;
    RenderTexture _targetTex_mip4;
    RenderTexture _targetTex_mip5;

    // EFFECTS
    [Header("Post Effects")]
    public bool applyHSV = true;
    public bool applyLevels = true;
    public bool applyMixer = true;
    
    [Header("HSV")]
    [Range(-180.0f, 180.0f)] public float hue = 0.0f;
    [Range(-100.0f, 100.0f)] public float saturation = 0.0f;
    [Range(-100.0f, 100.0f)] public float value = 0.0f;
    
    [Header("Levels")]
    [Range(0.0f, 255.0f)]  public float inputMin = 0.0f;
    [Range(0.0f, 255.0f)] public float inputMax = 255.0f;
    
    [Header("Mixer")]
    [Range(-100.0f, 100.0f)] public float redRed = 100.0f;
    [Range(-100.0f, 100.0f)] public float redGreen = 0.0f;
    [Range(-100.0f, 100.0f)] public float redBlue = 0.0f;
    [Range(-100.0f, 100.0f)] public float redConstant = 0.0f;
    [Range(-100.0f, 100.0f)] public float greenRed = 0.0f;
    [Range(-100.0f, 100.0f)] public float greenGreen = 100.0f;
    [Range(-100.0f, 100.0f)] public float greenBlue = 0.0f;
    [Range(-100.0f, 100.0f)] public float greenConstant = 0.0f;
    [Range(-100.0f, 100.0f)] public float blueRed = 0.0f;
    [Range(-100.0f, 100.0f)] public float blueGreen = 0.0f;
    [Range(-100.0f, 100.0f)] public float blueBlue = 100.0f;
    [Range(-100.0f, 100.0f)] public float blueConstant = 0.0f;
    
    public static CameraRenderMode renderMode
    {
        set {
            if(that._renderMode != value) {
                that._renderMode = value;
                that._dirtyPost = true;
            }
        }
        get { return that._renderMode; }
    }
    
    public static CameraPostColorMode cameraPostColorMode
    {
        set {
            if(that._cameraPostColorMode != value) {
                that._cameraPostColorMode = value;
                that._dirtyPost = true;
            }
        }
        get { return that._cameraPostColorMode; }
    }
    
    public static Color postColor
    {
        get { return that._postColor; }
        set {
            that._postColor = value;
            that._dirtyPost = true;
        }
    }
    
    public static float postColorScale
    {
        get { return that._postColorScale; }
        set {
            that._postColorScale = value;
            that._dirtyPost = true;
        }
    }
    
    public static RenderEffect renderEffect {
        get { return that._renderEffect; }
        set {
            that._renderEffect = value;
            if (value == RenderEffect.None)
                that._postMaterial.shader = that._normalShader;
            else if (value == RenderEffect.Wave)
                that._postMaterial.shader = that._waveShader;
            else if (value == RenderEffect.Pinch)
                that._postMaterial.shader = that._pinchShader;
            else if (value == RenderEffect.Dream)
                that._postMaterial.shader = that._dreamShader;
            else if (value == RenderEffect.Aberration)
                that._postMaterial.shader = that._aberrationShader;

            that._dirtyPost = true;
        }
    }
    
    public static Vector2 wavePhase {
        get { return that._wavePhase; }
        set { that._wavePhase = value; }
    }
    
    public static Vector2 waveFreq {
        get { return that._waveFreq; }
        set { that._waveFreq = value; }
    }

    public static Vector2 waveAmplitude {
        get { return that._waveAmplitude; }
        set { that._waveAmplitude = value; }
    }

    public static float pinchRadius
    {
        get { return that._pinchRadius; }
        set { that._pinchRadius = value; }
    }

    public static float pinchOffset
    {
        get { return that._pinchOffset; }
        set { that._pinchOffset = value; }
    }

    public static float pinchAmplitude
    {
        get { return that._pinchAmplitude; }
        set { that._pinchAmplitude = value; }
    }

    public static float pinchPower
    {
        get { return that._pinchPower; }
        set { that._pinchPower = value; }
    }

    public static float bloomPower {
        get { return that._bloomPower; }
        set { that._bloomPower = value; }
    }

    public static float bloomStrength {
        get { return that._bloomStrength; }
        set { that._bloomStrength = value; }
    }
    
    public static float noiseStrength {
        get { return that._noiseStrength; }
        set { that._noiseStrength = value; }
    }
    
    public static float blurStrength {
        get { return that._blurStrength; }
        set { that._blurStrength = value; }
    }

    // fraction of screen height
    public static Vector2 aberrationRedOffset {
        get { return that._aberrationRedOffset; }
        set { that._aberrationRedOffset = value; }
    }

    // fraction of screen height
    public static Vector2 aberrationGreenOffset {
        get { return that._aberrationGreenOffset; }
        set { that._aberrationGreenOffset = value; }
    }

    // fraction of screen height
    public static Vector2 aberrationBlueOffset {
        get { return that._aberrationBlueOffset; }
        set { that._aberrationBlueOffset = value; }
    }

    public static void Flicker(float length) {
        that._doFlicker(length);
    }
    
    public static void StopFlicker() {
        that._flicker = false;
        that._lit = true;
        that._flickerColor = Color.clear;
        that._dirtyPost = true;
    }
    
    public static void FadeToColor(Color color, float duration) {
        that._fadeToColor(color, duration);
    }
    
    public static void StopFade() {
        that.isFading = false;
        that._dirtyPost = true;
    }
    
    private void _doFlicker(float length)
    {
        _flicker = true;
        _lit = true;
        _flickerStart = Time.time;
        _flickerLength = length;
        _flickerFinish = _flickerStart + _flickerLength;
        _nextFlicker = _flickerStart;
    }
    
    private void _fadeToColor(Color color, float duration)
    {
        fadeStart = Time.time;
        fadeLength = duration;
        startColor = _postColor;
        goalColor = color;
        isFading = true;
    }

    private void Awake()
    {
        that = this;

        _targetWidth = Mathf.RoundToInt(Screen.width);
        _targetHeight = Mathf.RoundToInt(Screen.height);
        
        _targetTex_mip2 = new RenderTexture(_targetWidth / 4, _targetHeight / 4, 0);
        _targetTex_mip2.useMipMap = false;
        _targetTex_mip2.autoGenerateMips = false;
        _targetTex_mip2.filterMode = FilterMode.Bilinear;
        _targetTex_mip2.anisoLevel = 0;
        _targetTex_mip2.wrapMode = TextureWrapMode.Clamp;
        
        _targetTex_mip4 = new RenderTexture(_targetWidth / 16, _targetHeight / 16, 0);
        _targetTex_mip4.useMipMap = false;
        _targetTex_mip4.autoGenerateMips = false;
        _targetTex_mip4.filterMode = FilterMode.Bilinear;
        _targetTex_mip4.anisoLevel = 0;
        _targetTex_mip4.wrapMode = TextureWrapMode.Clamp;
        
        _targetTex_mip5 = new RenderTexture(_targetWidth / 32, _targetHeight / 32, 0);
        _targetTex_mip5.useMipMap = false;
        _targetTex_mip5.autoGenerateMips = false;
        _targetTex_mip5.filterMode = FilterMode.Bilinear;
        _targetTex_mip5.anisoLevel = 0;
        _targetTex_mip5.wrapMode = TextureWrapMode.Clamp;

        _normalShader = Shader.Find("Custom/CameraShader");
        _waveShader = Shader.Find("Custom/CameraShaderWave");
        _pinchShader = Shader.Find("Custom/CameraShaderPinch");
        _dreamShader = Shader.Find("Custom/CameraShaderDream");
        _aberrationShader = Shader.Find("Custom/CameraShaderAberration");

        _postMaterial = new Material(_normalShader);
        _downsample2XMaterial = new Material(Shader.Find("Custom/DownsampleX2"));
        _downsample1XMaterial = new Material(Shader.Find("Custom/DownsampleX1"));
        
        _dirtyPost = true;
    }

    private void _updateBlurMips()
    {
        _targetWidth = Screen.width;
        _targetHeight = Screen.height;
        
        _targetTex_mip2 = new RenderTexture(_targetWidth / 4, _targetHeight / 4, 0);
        _targetTex_mip2.useMipMap = false;
        _targetTex_mip2.autoGenerateMips = false;
        _targetTex_mip2.filterMode = FilterMode.Bilinear;
        _targetTex_mip2.anisoLevel = 0;
        _targetTex_mip2.wrapMode = TextureWrapMode.Clamp;
        
        _targetTex_mip4 = new RenderTexture(_targetWidth / 16, _targetHeight / 16, 0);
        _targetTex_mip4.useMipMap = false;
        _targetTex_mip4.autoGenerateMips = false;
        _targetTex_mip4.filterMode = FilterMode.Bilinear;
        _targetTex_mip4.anisoLevel = 0;
        _targetTex_mip4.wrapMode = TextureWrapMode.Clamp;
        
        _targetTex_mip5 = new RenderTexture(_targetWidth / 32, _targetHeight / 32, 0);
        _targetTex_mip5.useMipMap = false;
        _targetTex_mip5.autoGenerateMips = false;
        _targetTex_mip5.filterMode = FilterMode.Bilinear;
        _targetTex_mip5.anisoLevel = 0;
        _targetTex_mip5.wrapMode = TextureWrapMode.Clamp;

        _dirtyPost = true;
    }
        
    void OnValidate() {
        _dirtyPost = true;
    }
    
    private void OnPreRender()
    {
        if(_flicker)
        {
            if(Time.time < _flickerFinish)
            {
                if(Time.time >= _nextFlicker)
                {
                    float t = Mathf.Clamp01((Time.time - _flickerStart) / _flickerLength);
                    t = Curve.ArcQuad(t);

                    _lit = !_lit;
                    _nextFlicker = Time.time + UnityEngine.Random.Range(0.01f, 0.13f);
                    _flickerColor = new Color(0, 0, 0, _lit ? 0.2f * t : 0.7f * t);
                }
            }
            else
            {
                _flickerColor = new Color(0, 0, 0, 0);
                _flicker = false;
            }

            _dirtyPost = true;
        }

        if(isFading)
        {
            float elapsed = Time.time - fadeStart;

            if(elapsed < fadeLength)
            {
                _postColor = Color.Lerp(startColor, goalColor, Mathf.Clamp01(elapsed / fadeLength));
            }
            else
            {
                _postColor = goalColor;
                isFading = false;
            }

            _dirtyPost = true;
        }
        
        if(_dirtyPost)
        {
            UpdatePostEffects();
            _dirtyPost = false;
        }

        if (_renderEffect == RenderEffect.Wave)
        {
            _postMaterial.SetVector("_WavePhaseFreq", new Vector4(_wavePhase.x, _wavePhase.y, _waveFreq.x, _waveFreq.y));
            _postMaterial.SetVector("_WaveAmplitude", new Vector4(_waveAmplitude.x, _waveAmplitude.y, 0, 0));
        }
        else if (_renderEffect == RenderEffect.Pinch)
        {
            _postMaterial.SetFloat("_PinchRadius", _pinchRadius);
            _postMaterial.SetFloat("_PinchOffset", _pinchOffset);
            _postMaterial.SetFloat("_PinchAmplitude", _pinchAmplitude);
            _postMaterial.SetFloat("_PinchPower", _pinchPower);
        }
        else if (_renderEffect == RenderEffect.Dream)
        {
            _postMaterial.SetFloat("_Power", _bloomPower);
            _postMaterial.SetFloat("_Strength", _bloomStrength);
            _postMaterial.SetFloat("_NoiseStrength", _noiseStrength);
            _postMaterial.SetFloat("_BlurStrength", _blurStrength);
        }
        else if (_renderEffect == RenderEffect.Aberration)
        {
            _postMaterial.SetVector("_RedOffset", _aberrationRedOffset);
            _postMaterial.SetVector("_GreenOffset", _aberrationGreenOffset);
            _postMaterial.SetVector("_BlueOffset", _aberrationBlueOffset);
        }
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_renderEffect == RenderEffect.Dream)
        {
            if (_targetWidth != Screen.width || _targetHeight != Screen.height)
            {
                _updateBlurMips();
            }

            Graphics.Blit(src, _targetTex_mip2, _downsample2XMaterial);
            Graphics.Blit(_targetTex_mip2, _targetTex_mip4, _downsample2XMaterial);
            Graphics.Blit(_targetTex_mip4, _targetTex_mip5, _downsample1XMaterial);
            _postMaterial.SetTexture("_MainTexMip2", _targetTex_mip2);
            _postMaterial.SetTexture("_MainTexMip5", _targetTex_mip5);

            Graphics.Blit(src, dest, _postMaterial);
        }
        else
        {
            Graphics.Blit(src, dest, _postMaterial);
        }
    }

    Matrix4x4 GetLevelMatrix()
    {
        float offset = -(inputMin / 255.0f);
        float scale = 1.0f / ((inputMax - inputMin) / 255.0f);
        Matrix4x4 lvl = Matrix4x4.zero;
        lvl.SetRow(0, new Vector4(scale, 0, 0, offset * scale));
        lvl.SetRow(1, new Vector4(0, scale, 0, offset * scale));
        lvl.SetRow(2, new Vector4(0, 0, scale, offset * scale));
        lvl.SetRow(3, new Vector4(0, 0, 0, 1));
        return lvl;
    }
    
    Matrix4x4 GetHSVMatrix()
    {
        float h = hue * Mathf.Deg2Rad;
        float s = 1.0f + saturation * 0.01f;
        float v = 1.0f + value * 0.01f;
        
        float vsu = v * s * Mathf.Cos(h);
        float vsw = v * s * Mathf.Sin(h);
        
        var ret = Matrix4x4.zero;
        
        ret.SetRow(0, new Vector4(
            0.299f * v + 0.701f * vsu + 0.168f * vsw,
            0.587f * v - 0.587f * vsu + 0.330f * vsw,
            0.114f * v - 0.114f * vsu - 0.497f * vsw,
            0));
        
        ret.SetRow(1, new Vector4(
            0.299f * v - 0.299f * vsu - 0.328f * vsw,
            0.587f * v + 0.413f * vsu + 0.035f * vsw,
            0.114f * v - 0.114f * vsu + 0.292f * vsw,
            0));
        
        ret.SetRow(2, new Vector4(
            0.299f * v - 0.300f * vsu + 1.250f * vsw,
            0.587f * v - 0.588f * vsu - 1.050f * vsw,
            0.114f * v + 0.886f * vsu - 0.203f * vsw,
            0));
        
        ret.SetRow(3, new Vector4(0, 0, 0, 1));
        
        return ret;
    }
    
    Matrix4x4 GetChannelMixerMatrix()
    {
        var ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(redRed * 0.01f, redGreen * 0.01f, redBlue * 0.01f, redConstant * 0.01f));
        ret.SetRow(1, new Vector4(greenRed * 0.01f, greenGreen * 0.01f, greenBlue * 0.01f, greenConstant * 0.01f));
        ret.SetRow(2, new Vector4(blueRed * 0.01f, blueGreen * 0.01f, blueBlue * 0.01f, blueConstant * 0.01f));
        ret.SetRow(3, new Vector4(0, 0, 0, 1));
        return ret;
    }
    
    Matrix4x4 GetAddMatrix(Color c)
    {
        var ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(1.0f, 0.0f, 0.0f, c.r));
        ret.SetRow(1, new Vector4(0.0f, 1.0f, 0.0f, c.g));
        ret.SetRow(2, new Vector4(0.0f, 0.0f, 1.0f, c.b));
        ret.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        return ret;
    }
    
    Matrix4x4 GetMultiplyMatrix(Color c)
    {
        Matrix4x4 ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(c.r,  0.0f, 0.0f, 0.0f));
        ret.SetRow(1, new Vector4(0.0f, c.g,  0.0f, 0.0f));
        ret.SetRow(2, new Vector4(0.0f, 0.0f, c.b,  0.0f));
        ret.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, c.a));
        return ret;
    }
    
    Matrix4x4 GetAlphaBlendMatrix(Color c)
    {
        float inva = (1.0f - c.a);
        var ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(inva, 0.0f, 0.0f, c.r * c.a));
        ret.SetRow(1, new Vector4(0.0f, inva, 0.0f, c.g * c.a));
        ret.SetRow(2, new Vector4(0.0f, 0.0f, inva, c.b * c.a));
        ret.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        return ret;
    }
    
    Matrix4x4 GetGrayscaleMatrix()
    {
        Matrix4x4 ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(0.299f, 0.587f, 0.114f, 0));
        ret.SetRow(1, new Vector4(0.299f, 0.587f, 0.114f, 0));
        ret.SetRow(2, new Vector4(0.299f, 0.587f, 0.114f, 0));
        ret.SetRow(3, new Vector4(0, 0, 0, 1));
        return ret;
    }
    
    Matrix4x4 GetSepiaMatrix()
    {
        Matrix4x4 ret = Matrix4x4.zero;
        ret.SetRow(0, new Vector4(0.393f, 0.769f, 0.189f, 0));
        ret.SetRow(1, new Vector4(0.349f, 0.686f, 0.168f, 0));
        ret.SetRow(2, new Vector4(0.272f, 0.534f, 0.131f, 0));
        ret.SetRow(3, new Vector4(0, 0, 0, 1));
        return ret;
    }
    
    void UpdatePostEffects()
    {
        Matrix4x4 mix = Matrix4x4.identity;
        
        if(_renderMode == CameraRenderMode.Colorful) {
            if(applyHSV)    mix *= GetHSVMatrix();
            if(applyMixer)  mix *= GetChannelMixerMatrix();
            if(applyLevels) mix *= GetLevelMatrix();
        }
        else if(_renderMode == CameraRenderMode.Grayscale) {
            mix *= GetGrayscaleMatrix();
        }
        else if(_renderMode == CameraRenderMode.Sepia) {
            mix *= GetSepiaMatrix();
        }
        else if(_renderMode == CameraRenderMode.Normal) {
            // ...
        }
        
        if(_cameraPostColorMode == CameraPostColorMode.Add) {
            Color pc = _postColor * _postColorScale;
            pc.a = Mathf.Max(pc.a, _flickerColor.a);
            mix *= GetAddMatrix(pc);
        }
        else if(_cameraPostColorMode == CameraPostColorMode.Multiply) {
            Color pc = _postColor * _postColorScale;
            pc.a = Mathf.Max(pc.a, _flickerColor.a);
            mix *= GetMultiplyMatrix(pc);
        }
        else if(_cameraPostColorMode == CameraPostColorMode.AlphaBlend) {
            Color pc = _postColor * _postColorScale;
            pc.a = Mathf.Max(pc.a, _flickerColor.a);
            mix *= GetAlphaBlendMatrix(pc);
        }
        
        _postMaterial.SetMatrix("_Mix", mix);
    }
}
