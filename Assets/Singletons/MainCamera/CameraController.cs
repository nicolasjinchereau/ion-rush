using UnityEngine;

public struct CameraView
{
    public Vector3 position;
    public Quaternion rotation;
    public float fieldOfView;
}

public class CameraController : MonoBehaviour
{
    private static CameraController _that = null;
    public static CameraController that {
        get {
            if(_that == null)
                _that = Object.FindObjectOfType<CameraController>();
            
            return _that;
        }
    }
    
    public Camera cam;
    public Vector3 offset = new Vector3(0, 8.1f, -4.11f);
    public float minX = 0.0f;
    public float maxX = 73.0f;
    public float camAngle = 52.0f;
    public float defaultFOV = 68.0f;
    public DepthTextureMode depthTextureMode = DepthTextureMode.None;

    float moveStartTime;
    float moveLength = 1.0f;
    CameraView startView = new CameraView();
    CameraView goalView = new CameraView();
    Curve.Function moveFunc = Curve.SmoothStepInSteep;
    
    private State _state = State.Idle;
    
    public enum State
    {
        Idle,
        PlayerFollow,
        ShowView,
    }

    public State state {
        get { return _state; }
    }

    public const float DefaultCameraShakeAmplitude = 0.03f;

    bool cameraShakeEnabled = false;
    float nextCameraShakeUpdate = 0;
    Vector3 cameraShakeOffset = Vector3.zero;
    float cameraShakeAmplitude = DefaultCameraShakeAmplitude;
    float camereShakeUpdateInterval = 0.03f;
    float cameraImpactAmplitude = 0;
    float cameraImpactTime = 0;
    float cameraImpactDuration = 0;

    public float CameraShakeAmplitude
    {
        get { return cameraShakeAmplitude; }
        set { cameraShakeAmplitude = value; }
    }

    public bool CameraShakeEnabled
    {
        set
        {
            cameraShakeEnabled = value;

            if (!cameraShakeEnabled)
            {
                nextCameraShakeUpdate = 0;
                cameraShakeOffset = Vector3.zero;
                cam.transform.localPosition = Vector3.zero;
                cameraImpactAmplitude = 0;
                cameraImpactTime = 0;
                cameraImpactDuration = 0;
            }
        }

        get
        {
            return cameraShakeEnabled;
        }
    }

    public void DoImpactShake(float amplitude = 0.08f, float duration = 1.5f)
    {
        if (cameraShakeEnabled)
        {
            cameraImpactAmplitude = amplitude;
            cameraImpactDuration = duration;
            cameraImpactTime = Time.time;
        }
    }

    void Awake()
    {
        _that = this;

        cam.eventMask = 0;
        cam.depthTextureMode = depthTextureMode;
        
        moveStartTime = Time.time;
        startView.position = transform.position;
        startView.rotation = transform.rotation;
        startView.fieldOfView = defaultFOV;
        
        float camLimitMargin = 5.622f;
        
        var levelStart = GameObject.Find("LevelStart");
        if(levelStart) minX = levelStart.transform.position.x + camLimitMargin;

        var levelEnd = GameObject.Find("LevelEnd");
        if(levelEnd)   maxX = levelEnd.transform.position.x - camLimitMargin;
    }
    
    void OnDestroy()
    {
        _that = null;
    }
    
    Vector3 playerFollowPos
    {
        get
        {
            Vector3 ret = transform.position;
            
            if(Player.exists)
            {
                float camX = Mathf.Clamp(Player.position.x, minX, maxX);
                ret = Vector3.right * camX + offset;
            }
            
            return ret;
        }
    }
    
    Quaternion playerFollowRot {
        get { return Quaternion.Euler(camAngle, 0, 0); }
    }

    public void SetIdle()
    {
        _state = State.Idle;
    }

    public void SetTransform(Transform t)
    {
        _state = State.Idle;
        transform.position = t.position;
        transform.rotation = t.rotation;
    }

    public CameraView GetCurrentView()
    {
        return new CameraView() {
            position = transform.position,
            rotation = transform.rotation,
            fieldOfView = cam.fieldOfView
        };
    }

    public CameraView GetStartView()
    {
        if (_state != State.ShowView)
            throw new System.Exception("ShowView has not been called");

        return startView;
    }

    public CameraView GetGoalView()
    {
        if (_state != State.ShowView)
            throw new System.Exception("ShowView has not been called");

        return goalView;
    }

    public void ShowView(CameraView goal, float length, Curve.Function func = null)
    {
        _state = State.ShowView;
        moveStartTime = Time.time;
        moveLength = length;
        startView.position = transform.position;
        startView.rotation = transform.rotation;
        startView.fieldOfView = cam.fieldOfView;
        goalView = goal;

        if (length < 0.0001f)
        {
            transform.position = goalView.position;
            transform.rotation = goalView.rotation;
            cam.fieldOfView = goalView.fieldOfView;
        }

        moveFunc = func != null ? func : Curve.SmoothStepInSteep;
    }

    public void ShowView(Camera goal, float length, Curve.Function func = null)
    {
        var view = new CameraView() {
            position = goal.transform.position,
            rotation = goal.transform.rotation,
            fieldOfView = goal.fieldOfView
        };

        ShowView(view, length, func);
    }
    
    public void FollowPlayer(float length = 1.0f)
    {
        _state = State.PlayerFollow;
        moveStartTime = Time.time;
        moveLength = length;
        startView.position = transform.position;
        startView.rotation = transform.rotation;
        startView.fieldOfView = cam.fieldOfView;
        moveFunc = Curve.SmoothStepInSteep;
    }

    void UpdateCameraShake()
    {
        if (cameraShakeEnabled)
        {
            var now = Time.time;
            if (now >= nextCameraShakeUpdate)
            {
                nextCameraShakeUpdate = now + camereShakeUpdateInterval;

                var shake = Random.insideUnitSphere;
                var shakeNormal = shake.normalized;
                cameraShakeOffset = Vector3.Lerp(shake, shakeNormal, 0.5f);
            }

            var impactElapsedLocalTime = Mathf.Clamp01((now - cameraImpactTime) / cameraImpactDuration);
            var impactFactor = Curve.OutQuad(impactElapsedLocalTime);
            cam.transform.localPosition = cameraShakeOffset * (cameraShakeAmplitude + cameraImpactAmplitude * impactFactor);
        }
    }

    void Update()
    {
        UpdateCameraShake();

        float t = moveLength > 0 ? moveFunc((Time.time - moveStartTime) / moveLength) : 1.0f;
        
        switch(_state)
        {
        case State.Idle:
            
            break;

        case State.PlayerFollow:
            if(t < 1.0f)
            {
                transform.position = Vector3.Lerp(startView.position, playerFollowPos, t);
                transform.rotation = Quaternion.Slerp(startView.rotation, playerFollowRot, t);
                cam.fieldOfView = Mathf.Lerp(startView.fieldOfView, defaultFOV, t);
            }
            else
            {
                transform.position = playerFollowPos;
                transform.rotation = playerFollowRot;
                cam.fieldOfView = defaultFOV;
            }
            break;
                
        case State.ShowView:
            if(t < 1.0f)
            {
                transform.position = Vector3.Lerp(startView.position, goalView.position, t);
                transform.rotation = Quaternion.Slerp(startView.rotation, goalView.rotation, t);
                cam.fieldOfView = Mathf.Lerp(startView.fieldOfView, goalView.fieldOfView, t);
            }
            else
            {
                transform.position = goalView.position;
                transform.rotation = goalView.rotation;
                cam.fieldOfView = goalView.fieldOfView;
            }
            break;
        }
    }
}
