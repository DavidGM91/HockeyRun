using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    public bool debug = false;

    public int lateralSpace = 10;
    public float forwardSpeed = 3; // velocitat a la que anirà cap endavant
    public float lateralSpeed = 4; // velocitat a la que anirà cap els costats
    public float speedIncreasePerSecond = 0.1f; // augment de velocitat per cada segon

    public Transform firstAnchor;
    public Transform secondAnchor;
    public Transform startPos;

    private Transform anchor = null;
    private Transform oldAnchor = null;
    private Vector3 oldPos;

    public float distance
    {
        get { return prevDistance + forwardDistance; }
    }
    public float prevDistance = 0;
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

    public enum EAnims
    {
        JumpStart,
        JumpEnd,
        Forward,
        PushLeft,
        PushRight,
        RotateLeft,
        RotateRight,
        Slide,
        Hit,
        GameOver
    }
    [HideInInspector]
    public EAnims anim;

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

    private Tuple<Vector3,Quaternion> RotationBezier(float t)
    {
        // Per si de cas ens assegurem que t estigui entre 0 i 1
        t = Mathf.Clamp01(t);

        Vector3 p0 = oldPos;
        Vector3 p2 = anchor.position - anchor.forward * lateralDistance;
        Vector3 p1 = Vector3.Lerp(p0, p2, 0.5f) - oldAnchor.right * 2;

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
            anchor = new GameObject().transform;
            oldAnchor = new GameObject().transform;
            anchor.position = firstAnchor.position;
            anchor.rotation = firstAnchor.rotation;
            oldAnchor.position = secondAnchor.position;
            oldAnchor.rotation = secondAnchor.rotation;
        }
        lateralDistance = lateralSpace / 2;
        forwardDistance = -startPos.position.x;
        transform.position = startPos.position;
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
                prevDistance += forwardDistance;
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
    private void PlayAnim(EAnims _anim)
    {
        anim = _anim;
        animator.SetTrigger(EAnims.GetName(typeof(EAnims), _anim));
    }
    private void StopAnim(EAnims _anim)
    {
        anim = EAnims.Forward;
        animator.ResetTrigger(Enum.GetName(typeof(EAnims), _anim));
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
        prevDistance = 0;
        lateralDistance = lateralSpace / 2;
        anchor.position = firstAnchor.position;
        anchor.rotation = firstAnchor.rotation;
        oldAnchor.position = secondAnchor.position;
        oldAnchor.rotation = secondAnchor.rotation;
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
            lerpDuration = _lerpDuration;
        }

        oldAnchor.position = anchor.position;
        oldAnchor.rotation = anchor.rotation;

        anchor.position = newAnchor;
        anchor.rotation = rot;

        oldPos = transform.position;
        lerpTime = lerpDuration * (Vector3.Distance(oldPos, anchor.position - anchor.forward * lateralDistance)/forwardSpeed);
    }
    public void ChangeAnchor(Transform newAnchor, float _lerpDuration = -1)
    {
        ChangeAnchor(newAnchor.position, newAnchor.rotation, _lerpDuration);
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
