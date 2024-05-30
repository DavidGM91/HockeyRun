using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using static UnityEditor.PlayerSettings; //What is this for?

public class FollowCamera : MonoBehaviour {
    [SerializeField]
    private Transform objective;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float dist;

    private bool rotating;
    private float elapsed = 0;
    private float rotationTime = 2.0f;
    private Quaternion originalRot;
    private Quaternion toRotate;


    public void Focus(Transform obj)
    {
        this.objective = obj;
        toRotate = Quaternion.LookRotation(objective.position - transform.position);
        originalRot = transform.rotation;
        rotating = true;
    }

    public void AdjustCamera(Vector3 offset, float dist) {
        this.offset = offset * dist;
        Vector3 Pos = (offset + objective.position);
        Pos.x = 0;
        transform.position = Pos;
        Focus(this.objective);
    }

    // Start is called before the first frame update
    void Start()
    {     
        AdjustCamera(offset, dist);
        Focus(this.objective);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Pos = offset + objective.position;
        Pos.x = 2;
        transform.position = Pos;
               
        if(rotating) { 
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationTime); // valor normalitzat
            transform.rotation = Quaternion.Slerp(originalRot, toRotate, t);
            if(elapsed >= rotationTime)
            {
                rotating = false;
            }
        }
    }
}
