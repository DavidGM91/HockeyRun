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

    public ObstacleType obstacleType;

    public float distance { get { return 0; } }
    public float initialArea { get { return 0; } }
    public float finalArea { get { return 0; } }
    public float initialHeight { get { return 0; } }
    public float finalHeight { get { return 0; } }  

    private void Start()
    {
        if (origin == null)
        {
            origin = gameObject.transform.Find("Origin").GetComponent<Transform>();
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
        return LevelGenerator.ObjectActionOnPlayer.None;
    }

}
