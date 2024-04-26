using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orchestrator : MonoBehaviour
{
    public GameObject player;
    public GameObject cam;
    public LevelGenerator levelGenerator;

    public float deathHeight = -10;

    private PlayerMovement playerMovement;

    private int puntos = 0;
    private float acumulatedTime = 0;

    private void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player.transform.position.y < deathHeight)
        {
            levelGenerator.Regenerate();
            playerMovement.Restart();
        } 
        else
        {
            acumulatedTime += Time.deltaTime;
            if( acumulatedTime > 1)
            {
                acumulatedTime -= 1;
                puntos++;
            }
        }
    }
}
