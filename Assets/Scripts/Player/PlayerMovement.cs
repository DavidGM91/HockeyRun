using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MyMonoBehaviour
{
    [SerializeField]
    public int lateralSpace = 10;
    public float forwardSpeed = 3; // velocitat a la que anirà cap endavant
    public float lateralSpeed = 4; // velocitat a la que anirà cap els costats
    public float speedIncreasePerSecond = 0.1f; // augment de velocitat per cada segon
    public Transform OGanchor;
    public Transform anchor;

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

    override public void myStart()
    {
        PlayerStart();
        rb = GetComponent<Rigidbody>();
        if (anchor == null)
        {
            anchor = OGanchor;
        }
        lateralDistance = lateralSpace / 2;
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
        forwardDistance = 0;
        lateralDistance = 0;
        distance = 0;
        anchor = OGanchor;
        transform.position = new Vector3(0, 4, 3);
    }
    public void PlayerStart()
    {
        _forwardSpeed = forwardSpeed;
        _lateralSpeed = lateralSpeed;
    }

    public void ChangeAnchor(Vector3 newAnchor, Quaternion rot)
    { 
        anchor.position = newAnchor;
        anchor.rotation = rot;
        ChangeAnchor(anchor);
    }

    public void ChangeAnchor(Transform newAnchor)
    {
        //TODO: Make the player lerp from the positions
        anchor.position = newAnchor.position;
        anchor.rotation = newAnchor.rotation;
        forwardDistance = 0;
        transform.rotation = anchor.rotation;
        myUpdate();
    }

    // Update is called once per frame
    override public void myUpdate()
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

        distance += forwardSpeed * Time.deltaTime;
        forwardDistance += forwardSpeed * Time.deltaTime;

        Vector3 newPos = anchor.position - anchor.right * forwardDistance - anchor.forward * lateralDistance;
        newPos.y = transform.position.y;
        transform.position = newPos;


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
