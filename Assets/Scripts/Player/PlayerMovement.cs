using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    public int lateralSpace = 10;
    public float forwardSpeed = 3; // velocitat a la que anirà cap endavant
    public float lateralSpeed = 4; // velocitat a la que anirà cap els costats
    public float speedIncreasePerSecond = 0.1f; // augment de velocitat per cada segon
    public Transform firstAnchor;
    public Transform secondAnchor;
    public Transform startPos;

    private Transform anchor = null;
    private Transform oldAnchor = null;

    public float distance = 5.5f;
    public float lateralDistance = 0;
    private float forwardDistance = 0;

    [SerializeField]
    public Animator animator;

    [SerializeField]
    public KeyCode upKey = KeyCode.W;
    public KeyCode bendKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [SerializeField]
    Rigidbody rb;   // per tractar fisiques

    [SerializeField]
    public float jumpForce = 800f;
    public float jumpTime = 2.0f;

    [SerializeField]
    LayerMask groundMask;

    private enum EAnims
    {
        JumpStart ,
        JumpEnd,
        PushLeft,
        PushRight,
        RotateLeft,
        RotateRight,
        Slide,
        Hit,
        GameOver
    }

    private float _forwardSpeed = -1; // Copia de la velocitat a la que anirà cap endavant
    private float _lateralSpeed = -1; // Copia de la velocitat a la que anirà cap els costats

    private float saveForwardSpeed = -1;


    private float jumpTimeCounter = 0;
    // Start is called before the first frame update

    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter = 0;
    private bool isJumping = false;

    private float lerpTime = 0;
    public float lerpDuration = 1;

    private Quaternion originRot;
    private Quaternion targetRot;

    private Tuple<Vector3,Quaternion> RotationBezier(float t)
    {
        // Per si de cas ens assegurem que t estigui entre 0 i 1
        t = Mathf.Clamp01(t);

        Vector3 p0 = oldAnchor.position - oldAnchor.forward * lateralDistance;
        Vector3 p1 = oldAnchor.position + oldAnchor.right * 2;
        Vector3 p2 = anchor.position - anchor.forward * lateralDistance;

        // Fem un càlcul de Bezier amb els punts i el t
        Vector3 result = Mathf.Pow(1 - t, 2) * p0 +
                         2 * (1 - t) * t * p1 +
                         Mathf.Pow(t, 2) * p2;
        
        return new Tuple<Vector3, Quaternion>(result, Quaternion.Lerp(oldAnchor.rotation, anchor.rotation, t));
    }

    void Start()
    {
        PlayerStart();
        rb = GetComponent<Rigidbody>();
        if (anchor == null)
        {
            anchor = firstAnchor;
            oldAnchor = firstAnchor;
        }
        lateralDistance = lateralSpace / 2;
        distance = -startPos.position.x;
        transform.position = startPos.position;
        forwardDistance = distance;
    }
    void Update()
    {
        // Adjust lateral distance based on input
        if (Input.GetKey(rightKey))
        {
            lateralDistance -= lateralSpeed * Time.deltaTime;
            lateralDistance = Mathf.Clamp(lateralDistance, 0f, lateralSpace);
        }
        if (Input.GetKey(leftKey))
        {
            lateralDistance += lateralSpeed * Time.deltaTime;
            lateralDistance = Mathf.Clamp(lateralDistance, 0f, lateralSpace);
        }

        if (lerpTime <= 0)
        {
            distance += forwardSpeed * Time.deltaTime;
            forwardDistance += forwardSpeed * Time.deltaTime;

            Vector3 newPos = anchor.position - anchor.right * forwardDistance - anchor.forward * lateralDistance;
            newPos.y = transform.position.y;
            transform.position = newPos;
        }
        else
        {
            lerpTime -= Time.deltaTime;
            float t = 1 - lerpTime / lerpDuration;
            Tuple<Vector3, Quaternion> result = RotationBezier(t);
            transform.position = new Vector3(result.Item1.x, transform.position.y, result.Item1.z);
            transform.rotation = result.Item2;
            if (lerpTime <= 0)
            {
                forwardDistance = 0;
            }
        }

        // Jump logic
        if (coyoteTimeCounter != 0 && checkGround())
        {
            coyoteTimeCounter = 0;
            if (isJumping)
            {
                isJumping = false;
                PlayAnim(EAnims.JumpEnd);
            }
        }
        else if (!checkGround())
        {
            coyoteTimeCounter += Time.deltaTime;
        }

        if (Input.GetKey(upKey))
        {
            if (Input.GetKeyDown(upKey))
            {
                StartJump();
            }
            Jump();
        }
        else if (Input.GetKeyUp(upKey))
        {
            jumpTimeCounter = 0;
            coyoteTimeCounter = coyoteTime;
        }
    }
    private void PlayAnim(EAnims anim)
    {
        animator.SetTrigger(EAnims.GetName(typeof(EAnims), anim));
    }
    private void StopAnim(EAnims anim)
    {
        animator.ResetTrigger(Enum.GetName(typeof(EAnims), anim));
    }
    public void setIdle(bool idle)
    {
        if(idle)
        {
            animator.SetBool("Idle", true);
            if (forwardSpeed != 0)
            {
                saveForwardSpeed = forwardSpeed;
            }
            else if(saveForwardSpeed != -1)
            {
                saveForwardSpeed = _forwardSpeed;
            }
            else
            {
                saveForwardSpeed = 3;
            }
            forwardSpeed = 0;
            this.enabled = false;
        }
        else
        {
            forwardSpeed = saveForwardSpeed;
            animator.SetBool("Idle", false);
            this.enabled = true;
        }
    }
    
    public void Restart()
    {
        forwardSpeed = _forwardSpeed;
        lateralSpeed = _lateralSpeed;
        forwardDistance = -startPos.position.x;
        lateralDistance = lateralSpace / 2;
        distance = -startPos.position.x;
        anchor = firstAnchor;
        transform.position = startPos.position;
        transform.rotation = startPos.rotation;
    }
    public void PlayerStart()
    {
        _forwardSpeed = forwardSpeed;
        _lateralSpeed = lateralSpeed;
    }
    public void ChangeAnchor(Vector3 newAnchor, Quaternion rot, float _lerpDuration = -1)
    {
        if (_lerpDuration != -1)
        {
            lerpTime = _lerpDuration;
        }
        else
        {
            lerpTime = lerpDuration;
        }

        oldAnchor.position = anchor.position;
        oldAnchor.rotation = anchor.rotation;

        anchor.position = newAnchor;
        anchor.rotation = rot;
    }
    public void ChangeAnchor(Transform newAnchor, float _lerpDuration = -1)
    {
        if(_lerpDuration != -1)
        {
            lerpTime = _lerpDuration;
        }
        else
        {
            lerpTime = lerpDuration;
        }

        oldAnchor.position = anchor.position;
        oldAnchor.rotation = anchor.rotation;

        anchor.position = newAnchor.position;
        anchor.rotation = newAnchor.rotation;
    }
    // Update is called once per frame
    bool checkGround()
    {
        float height = GetComponent<Collider>().bounds.size.y;
        return Physics.Raycast(transform.position, Vector3.down, (height / 2) + 0.1f, groundMask);
    }
    void StartJump()
    {
        if (coyoteTimeCounter < coyoteTime)
        {
            isJumping = true;
            PlayAnim(EAnims.JumpStart);
            jumpTimeCounter = jumpTime;
            coyoteTimeCounter = coyoteTime;
        }
    }
    void Jump()
    {
        if (jumpTimeCounter > 0)
        {
            rb.AddForce(Vector3.up * (jumpForce * (jumpTimeCounter*2) * Time.deltaTime));
            jumpTimeCounter -= Time.deltaTime;
        }
    }
}
