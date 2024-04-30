using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FollowCamera : MonoBehaviour {

    [SerializeField]
    private Camera cam;
    private Transform transCamera;
    [SerializeField]
    private Transform objective;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float dist;

    private bool rotating;
    private float elapsed = 0;
    private Quaternion originalRot;
    private Quaternion toRotate;


    public void Focus(Transform obj)
    {
        this.objective = obj;
        toRotate = Quaternion.LookRotation(objective.position - transCamera.position);
        originalRot = transCamera.rotation;
        rotating = true;
    }

    public void AdjustCamera(Vector3 offset, float dist) {
        this.offset = offset * dist;
        Focus(this.objective);
    }

    // Start is called before the first frame update
    void Start()
    {
        transCamera = cam.GetComponent<Transform>();
        AdjustCamera(offset, dist);
        Focus(this.objective);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Pos = offset + objective.position;
        Pos.x = 0;
        transCamera.position = Pos;
        if(rotating) { 
            elapsed += Time.deltaTime;
            transCamera.rotation = Quaternion.Slerp(originalRot, toRotate, elapsed);
            if(elapsed > 1)
            {
                rotating = false;
            }
        }
    }
}
