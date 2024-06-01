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
    [SerializeField]
    private Transform anchor = null;
    private bool isPlayer = false;

    private PlayerMovement playerMovement = null;

    private float elapsed = 0;
    private float rotationTime = 2.0f;
    private Quaternion originalRot;
    private Quaternion toRotate;


    public void Focus(Transform obj,bool isPlayer = false)
    {
        if(isPlayer && playerMovement == null)
        {
            playerMovement = obj.GetComponent<PlayerMovement>();
            anchor = playerMovement.anchor;
        }
        this.isPlayer = isPlayer;
        this.objective = obj;
        transform.parent = obj;
        toRotate = Quaternion.LookRotation(objective.position - transform.position);
        originalRot = transform.rotation;
    }

    public void AdjustCamera(Vector3 offset, float dist) {
        transform.localPosition = offset * dist;
        Focus(this.objective,isPlayer);
    }

    // Start is called before the first frame update
    void Start()
    { 
        AdjustCamera(offset, dist);
        Focus(this.objective, isPlayer);
    }

    // Update is called once per frame
    void Update()
    {     
        if(isPlayer)
        {
            transform.localPosition = offset + new Vector3(0,0,-playerMovement.lateralSpace/2 + playerMovement.lateralDistance);
            transform.LookAt((anchor.position - anchor.right * playerMovement.forwardDistance - anchor.forward * playerMovement.lateralSpace/2));
        }
        if(elapsed < rotationTime) { 
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationTime); // valor normalitzat
            transform.rotation = Quaternion.Slerp(originalRot, toRotate, t);
            if (elapsed >= rotationTime)
            {
                transform.LookAt(objective.position);
            }
        }
    }
}
