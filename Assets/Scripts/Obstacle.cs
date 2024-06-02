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
    private Transform btmR = null;
    [SerializeField]
    private Transform btmL = null;
    [SerializeField]
    private Transform topR = null;

    [SerializeField]
    private Animator animator = null;

    public ObstacleType obstacleType;

    public float distance { get { return btmR.position.x - origin.position.x; } }
    public float initialArea { get { return btmR.position.z - origin.position.z; } }
    public float finalArea { get { return btmL.position.z - origin.position.z; } }
    public float initialHeight { get { return btmR.position.y - origin.position.y; } }
    public float finalHeight { get { return topR.position.z - origin.position.z; ; } }  

    private void Start()
    {
        if (origin == null)
        {
            origin = gameObject.transform.Find("Origin");
        }
        if(btmR == null)
        {
            btmR = gameObject.transform.Find("EventBtmR");
        }
        if (btmL == null)
        {
            btmL = gameObject.transform.Find("EventBtmL");
        }
        if (topR == null)
        {
            topR = gameObject.transform.Find("EventTopR");
        }
        if (animator == null)
        {
            animator = gameObject.GetComponentInChildren<Animator>();
            if(animator != null)
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

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public virtual LevelGenerator.ObjectActionOnPlayer OnEvent(uint id, bool succes, MyEvent.checkResult checkResult)
    {
        switch(checkResult)
        {
            case MyEvent.checkResult.Success:
                Debug.Log("Correct");
                break;
            case MyEvent.checkResult.Fail:
                Debug.Log("Wrong");
                if(animator != null)
                {
                    animator.enabled = true;
                    animator.StartPlayback();
                }
                break;
            case MyEvent.checkResult.OutSide:
                Debug.Log("Missed");
                break;
        }
        StartCoroutine(DestroyAfterTime(2));
        return LevelGenerator.ObjectActionOnPlayer.None;
    }

}
