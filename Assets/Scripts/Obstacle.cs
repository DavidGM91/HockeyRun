using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnObstacle : MonoBehaviour
{
    public Transform origin = null;

    public KeyCode keyQTE;

    [Serializable]
    public enum ObstacleType
    {
        QTE,
        AreaQTE,
        AreaAltura,
        Area
    }

    [SerializeField]
    protected Transform btmR = null;
    [SerializeField]
    protected Transform btmL = null;
    [SerializeField]
    protected Transform topR = null;

    [SerializeField]
    protected Animator animator = null;

    public ObstacleType obstacleType;

    protected void EnsureTransformsAssigned()
    {
        if (origin == null)
        {
            origin = gameObject.transform.Find("Origin");
            if (origin == null)
                throw new System.Exception("No Origin found. This obstacle was likely not set up correctly.");
        }

        if (btmR == null)
        {
            btmR = gameObject.transform.Find("EventBtmR");
            if (btmR == null)
                throw new System.Exception("No EventBtmR found. This obstacle was likely not set up correctly.");
        }

        if (btmL == null)
        {
            btmL = gameObject.transform.Find("EventBtmL");
            if (btmL == null)
                throw new System.Exception("No EventBtmL found. This obstacle was likely not set up correctly.");
        }

        if (topR == null)
        {
            topR = gameObject.transform.Find("EventTopR");
            if (topR == null)
                throw new System.Exception("No EventTopR found. This obstacle was likely not set up correctly.");
        }
    }


    public float distance
    {
        get
        {
            if (btmR == null || origin == null)
            {
                origin = gameObject.transform.Find("Origin");
                btmR = gameObject.transform.Find("EventBtmR");
                if (btmR == null || origin == null)
                    throw new System.Exception("No btmR or origin found. This obstacle was likely not set up correctly.");
            }
            return btmR.position.x - origin.position.x;
        }
    }

    // Property to get the initial area
    public float initialArea
    {
        get
        {
            EnsureTransformsAssigned();
            return -(btmR.position.z - origin.position.z);
        }
    }

    // Property to get the final area
    public float finalArea
    {
        get
        {
            EnsureTransformsAssigned();
            return -(btmL.position.z - origin.position.z);
        }
    }

    // Property to get the initial height
    public float initialHeight
    {
        get
        {
            EnsureTransformsAssigned();
            return (btmR.position.y - origin.position.y);
        }
    }

    // Property to get the final height
    public float finalHeight
    {
        get
        {
            EnsureTransformsAssigned();
            return (topR.position.y - origin.position.y);
        }
    }
    void Start()
    {
        origin = gameObject.transform.Find("Origin");
        btmR = gameObject.transform.Find("EventBtmR");
        btmL = gameObject.transform.Find("EventBtmL");
        topR = gameObject.transform.Find("EventTopR");
        if (animator == null)
        {
            animator = gameObject.GetComponentInChildren<Animator>();
            if(animator != null)
                animator.enabled = false;
        }
    }

    public void Init()
    {
        origin = gameObject.transform.Find("Origin");
        btmR = gameObject.transform.Find("EventBtmR");
        btmL = gameObject.transform.Find("EventBtmL");
        topR = gameObject.transform.Find("EventTopR");
        if (animator == null)
        {
            animator = gameObject.GetComponentInChildren<Animator>();
            if (animator != null)
                animator.enabled = false;
        }
    }
    public void positionYourselfPlease(Vector3 position)
    {
        transform.position = position - origin.localPosition;
    }
    public void rotateYourselfAroundYourOriginPlease(Vector3 rotation)
    {
        transform.RotateAround(origin.position, Vector3.up, rotation.y);
    }
    public void positionYourselfPlease(Vector3 position, Vector3 offset)
    {
        transform.position = position - origin.position + offset;
    }
    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public virtual LevelGenerator.ObjectActionOnPlayer OnEvent(uint id, bool succes, MyEvent.checkResult checkResult)
    {
        switch(checkResult)
        {
            case MyEvent.checkResult.Success:
                //Debug.Log("Correct");
                break;
            case MyEvent.checkResult.Fail:
                //Debug.Log("Wrong");
                if(animator != null)
                {
                    animator.enabled = true;
                    animator.StartPlayback();
                }
                break;
            case MyEvent.checkResult.OutSide:
                //Debug.Log("Missed");
                break;
        }
        StartCoroutine(DestroyAfterTime(2));
        return LevelGenerator.ObjectActionOnPlayer.None;
    }
}
