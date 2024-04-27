using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    public int lateralSpace = 5;
    public float forwardSpeed = 3; // velocitat a la que anira cap endavant
    public float lateralSpeed = 4; // velocitat a la que anira cap endavant
    public float speedIncreasePerSecond = 0.1f; // augment de velocitat per cada segon

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
        JumpStart = 0,
        JumpLand = 1,
        PushLeft = 2,
        PushRight = 3,
        RotateLeft = 4,
        RotateRight = 5,
        Slide = 6,
        Hit = 7,
        GameOver = 8
    }

    private float _forwardSpeed = 3; // velocitat a la que anira cap endavant
    private float _lateralSpeed = 4; // velocitat a la que anira cap endavant


    private float jumpTimeCounter = 0;
    // Start is called before the first frame update

    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter = 0;
    private bool isJumping = false;

    private void PlayAnim(EAnims anim)
    {
        animator.SetTrigger(EAnims.GetName(typeof(EAnims), anim));
    }
    private void StopAnim(EAnims anim)
    {
        animator.ResetTrigger(Enum.GetName(typeof(EAnims), anim));
    }

    public void Restart()
    {
        forwardSpeed = _forwardSpeed;
        lateralSpeed = _lateralSpeed;
        this.gameObject.transform.position = new Vector3(0, 0, 0);
    }
    void Start()
    {
        _forwardSpeed = forwardSpeed;
        _lateralSpeed = lateralSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);

        forwardSpeed += speedIncreasePerSecond * Time.deltaTime;
        if (Input.GetKey(leftKey))
        {
            //Comprovar que no es surt dels limits
            if (this.gameObject.transform.position.x > (lateralSpace * -1))
            {
                transform.Translate(Vector3.left * Time.deltaTime * lateralSpeed);
            }
        }

        if (Input.GetKey(rightKey))
        {
            //Comprovar que no es surt dels limits
            if (this.gameObject.transform.position.x < lateralSpace)
            {
                transform.Translate(Vector3.right * Time.deltaTime * lateralSpeed);
            }
        }

        //Lògica de salt

        if (coyoteTimeCounter != 0 && checkGround())
        {
            coyoteTimeCounter = 0;
            if(isJumping)
            {
                isJumping = false;
                PlayAnim(EAnims.JumpLand);
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
