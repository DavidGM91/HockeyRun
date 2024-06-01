using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ShowOnlyOnEditor : MonoBehaviour
{
    [SerializeField]
    private Color gizmoColor = Color.yellow;
    [SerializeField]
    private Renderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if(Application.isEditor)
        {
            meshRenderer.enabled = true;
            meshRenderer.material.SetColor("_BaseColor",gizmoColor);
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    // Update is called once per frame
    void OnValidate()
    {
        if (Application.isEditor)
        {
            meshRenderer.enabled = true;
            meshRenderer.material.SetColor("_BaseColor", gizmoColor);
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }
}
