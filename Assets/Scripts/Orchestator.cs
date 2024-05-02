using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Orchestrator : MonoBehaviour
{
    public GameObject player;
    public Transform inici;
    public GameObject cam;
    public LevelGenerator levelGenerator;
    public TextMeshProUGUI scoreText;
    public GameObject menu;

    public KeyCode rotateCamera;

    public float deathHeight = -10;

    private bool isInici = false;

    private PlayerMovement playerMovement;

    private int puntos = 0;
    private float acumulatedTime = 0;

    public void ShowMenu()
    {
        menu.SetActive(true);
    }

    public void HideMenu()
    {
        menu.SetActive(false);
    }
    private void Start()
    {
        playerMovement = player.GetComponent<PlayerMovement>();
        levelGenerator.menu = false;
        ShowMenu();
        player.SetActive(false);
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
            HideMenu();
            levelGenerator.menu = true;
            player.SetActive(true);
            /*
            if (isInici)
            {
                cam.GetComponent<FollowCamera>().Focus(player.GetComponent<Transform>());
            }
            else
            {
                cam.GetComponent<FollowCamera>().Focus(inici);
            }
            */
        }
     
    }
}
