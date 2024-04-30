using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Orchestrator : MonoBehaviour
{
    public GameObject player;
    public Transform inici;
    public GameObject cam;
    public LevelGenerator levelGenerator;
    public TextMeshProUGUI scoreText;

    public KeyCode rotateCamera;

    public float deathHeight = -10;

    private bool isInici = false;

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
        if (player.transform.position.y < deathHeight)
        {
            levelGenerator.Regenerate();
            playerMovement.Restart();
            puntos = 0;
        }
        else
        {
            acumulatedTime += Time.deltaTime;
            if (acumulatedTime > 1)
            {
                acumulatedTime -= 1;
                puntos++;
            }
            scoreText.text = "Score: " + puntos;
        }


        //DEBUG TODO REMOVE
        if (Input.GetKey(rotateCamera))
        {
            if (isInici)
            {
                cam.GetComponent<FollowCamera>().Focus(player.GetComponent<Transform>());
            }
            else
            {
                cam.GetComponent<FollowCamera>().Focus(inici);
            }
        }
    }
}
