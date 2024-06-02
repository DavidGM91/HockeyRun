using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnObstacle : MonoBehaviour
{
    public Transform origin;

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
    private GameObject btmR;
    [SerializeField]
    private GameObject btmL;
    [SerializeField]
    private GameObject topR;

    public ObstacleType obstacleType;

    public float distance { get { return btmR.transform.position.x - origin.position.x; } }
    public float initialArea { get { return btmR.transform.position.z - origin.position.z; } }
    public float finalArea { get { return btmL.transform.position.z - origin.position.z; } }
    public float initialHeight { get { return btmR.transform.position.y - origin.position.y; } }
    public float finalHeight { get { return topR.transform.position.z - origin.position.z; ; } }  

    private void Start()
    {
        if (origin == null)
        {
            origin = gameObject.transform.Find("Origin").GetComponent<Transform>();
        }
        if(btmR == null)
        {
            btmR = gameObject.transform.Find("EventBtmR").gameObject;
        }
        if (btmL == null)
        {
            btmL = gameObject.transform.Find("EventBtmL").gameObject;
        }
        if (topR == null)
        {
            topR = gameObject.transform.Find("EventTopR").gameObject;
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

    public Vector4 GetPoints()
    {
        return new Vector4(transform.position.x, transform.position.z, transform.localScale.x, transform.localScale.z);
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
                break;
            case MyEvent.checkResult.OutSide:
                Debug.Log("Missed");
                break;
        }
        StartCoroutine(DestroyAfterTime(2));
        return LevelGenerator.ObjectActionOnPlayer.None;
    }

}
