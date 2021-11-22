using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : Object, new()
{
    [System.NonSerialized]
    private static T _that = null;
    public static T that
    {
        get
        {
            if(_that == null)
            {
                _that = (T)FindObjectOfType(typeof(T));

                //if(_that == null)
                //{
                //    //throw new System.Exception("Instaniating: " + typeof(T).Name);
                //    Instantiate(Resources.Load(typeof(T).Name));
                //    _that = (T)FindObjectOfType(typeof(T));
                //    if (typeof(T) == typeof(Player))
                //    {
                //        Debug.Log("CREATING PLAYER");
                //    }
                //}

                if(_that != null)
                    _that.GetType().GetMethod("OnInitialize").Invoke(_that, null);
            }

            return _that;
        }
    }

    public static bool exists {
        get { return _that != null; }
    }

    public void OnDestroy() {
        this.OnTerminate();
        _that = null;
    }

    public virtual void OnInitialize(){}
    public virtual void OnTerminate(){}
}
