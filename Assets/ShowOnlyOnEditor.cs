using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShowOnlyOnEditor : MonoBehaviour
{
    [SerializeField]
    private Color gizmoColor = Color.yellow;
    // Start is called before the first frame update
    void Start()
    {
        if(Application.isEditor)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<MeshRenderer>().material.color = gizmoColor;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void OnValidate()
    {
        if (Application.isEditor)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<MeshRenderer>().material.color = gizmoColor;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
