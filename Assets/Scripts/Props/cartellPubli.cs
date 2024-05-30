using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cartellPubli : MonoBehaviour
{
    public Material mat;
    public GameObject cartell;
    // Start is called before the first frame update
    void Start()
    {
        cartell.GetComponent<Renderer>().material = mat;
        cartell.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(0.5f*Random.Range(0,2),0.25f*Random.Range(0,4));
    }
}
