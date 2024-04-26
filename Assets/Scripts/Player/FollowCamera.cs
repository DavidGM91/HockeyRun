using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
    [SerializeField]
    public Transform player;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.position;   
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Pos = offset + player.position;
        Pos.x = 0;
        transform.position = Pos;
    }
}
