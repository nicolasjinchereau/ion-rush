using UnityEngine;
using System.Collections;

public class OccludedArea : MonoBehaviour
{
    public GameObject targetObject;
    public GameObject[] targetObjectsToLighten;
    
    Color darkColor = new Color(0.45f, 0.45f, 0.45f, 1.0f);

    private bool inside = false;
    private Color color;

    void Awake()
    {
        color = targetObject.GetComponent<Renderer>().material.GetColor("_Color");
        color.a = 1.0f;
        targetObject.GetComponent<Renderer>().material.SetColor("_Color", color);

        foreach(GameObject obj in targetObjectsToLighten)
            obj.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.white, darkColor, color.a));
    }

    void Update()
    {
        if(inside)
        {
            if(color.a > 0.0f)
            {
                color.a = Mathf.Max(color.a - Time.deltaTime * 4.0f, 0.0f);
                targetObject.GetComponent<Renderer>().material.SetColor("_Color", color);

                foreach(GameObject obj in targetObjectsToLighten)
                    obj.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.white, darkColor, color.a));
            }
        }
        else
        {
            if(color.a < 1.0f)
            {
                color.a = Mathf.Min(color.a + Time.deltaTime * 4.0f, 1.0f);
                targetObject.GetComponent<Renderer>().material.SetColor("_Color", color);

                foreach(GameObject obj in targetObjectsToLighten)
                    obj.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.white, darkColor, color.a));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            inside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            inside = false;
        }
    }
}
