using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSection : MonoBehaviour
{
    public Transform origin;
    public List<Transform> spawns;

    private void Start(){
        if (origin == null)
        {
            origin = gameObject.transform.Find("Origin").GetComponent<Transform>();
        }
        if (spawns.Count == 0)
        {
            foreach (Transform child in transform) {
                if (child.name == "Spawn"){
                    spawns.Add(child);
                }
            }
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

    public int GetSpawnsCount()
    {
        return spawns.Count;
    }

    public Vector3 GetSpawn(int index)
    {
        return spawns[index].position;
    }
}
