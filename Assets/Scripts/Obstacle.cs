using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public BoxCollider GetCollider()
    {
        return GetComponent<BoxCollider>();
    }

}
