using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    private bool ismenu = true;

    private PlayerMovement playerMovement;

    private int puntos = 0;
    private float acumulatedTime = 0;

    public void Play()
    {
        playerMovement.setIdle(false);
        cam.GetComponent<FollowCamera>().Focus(player.transform);
        HideMenu();
    }
   
    public void ShowMenu()

    {
        cam.GetComponent<FollowCamera>().Focus(inici);
        Time.timeScale = 0;
        playerMovement.enabled = false;
        scoreText.enabled = false;
        ismenu = true;
        menu.SetActive(true);
    }

    public void HideMenu()
    {
        cam.GetComponent<FollowCamera>().Focus(player.GetComponent<Transform>());
        Time.timeScale = 1;
        playerMovement.enabled = true;
        ismenu = false;
        menu.SetActive(false);
    }
    private void Start()
    {
        Time.timeScale = 0;
        playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.PlayerStart();
        playerMovement.setIdle(true);
        ShowMenu();
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
            if (ismenu)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
     
    }
}
