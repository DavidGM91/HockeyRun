using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemics : MonoBehaviour
{
    public Vector3 offsetFar;
    public Vector3 offsetClose;
    public Vector3 offsetKill;

    private Vector3 offset;

    private float retreatClose = 0;
    private float lerpClose = 0;
    private float lerpKill = 0;

    public void SetPlayer(Customization custom)
    {
        foreach (var child in gameObject.GetComponentsInChildren<Enemy>())
        {
            child.player = custom;
        }
    }

    public void GetClose()
    {
        foreach (var child in gameObject.GetComponentsInChildren<Enemy>())
        {
            child.myStart();
        }
        lerpClose = 2;
        transform.localPosition = offsetFar;
        offset = transform.localPosition;
    }
    public void GetKill()
    {
        lerpKill = 2;
        offset = transform.localPosition;
    }

    public void RetreatClose()
    {
        retreatClose = 5;
        offset = transform.localPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        offset = offsetFar;
    }

    // Update is called once per frame
    void Update()
    {
        if(lerpClose > 0)
        {
            lerpClose -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(offset, offsetClose,1- lerpClose/2);
            if(lerpClose < 0)
            {
                transform.localPosition = offsetClose;
                lerpClose = 0;
            }
        }
        else if(retreatClose > 0)
        {
            retreatClose -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(offset, offsetFar,1- retreatClose/5);
            if(retreatClose < 0)
            {
                transform.localPosition = offsetFar;
                retreatClose = 0;
            }
        }
        else if(lerpKill > 0)
        {
            lerpKill -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(offset, offsetKill, 1- lerpKill / 2);
            if(lerpKill < 0)
            {
                transform.localPosition = offsetKill;
                lerpKill = 0;
            }
        }
    }
}
