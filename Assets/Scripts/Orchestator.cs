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
    public CoinPool coinPool;
    public TextMeshProUGUI scoreText;

    public GameObject menu;
    public GameObject customizationMenu;

    public MyMonoBehaviour[] scripts;


    public KeyCode OpenMenu;

    public float deathHeight = -10;

    private bool ismenu = true;

    private PlayerMovement playerMovement = null;

    private int puntos = 0;
    private float acumulatedTime = 0;

    private Vector3 playerOffset = new Vector3(0, 6, -12);

    public void Play()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(false);
        cam.GetComponent<FollowCamera>().Focus(player.transform);
        cam.GetComponent<FollowCamera>().AdjustCamera(playerOffset, 0.5f);
        HideMenu();
    }

    public void ShowCustomization()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(true);
        cam.GetComponent<FollowCamera>().Focus(player.transform);
        cam.GetComponent<FollowCamera>().AdjustCamera(new Vector3(0, 0.5f, 3), 1);
        //TODO Camera shows player front
        menu.SetActive(false);
        customizationMenu.SetActive(true);
    }
    public void HideCustomization()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(false);
        cam.GetComponent<FollowCamera>().AdjustCamera(playerOffset, 1);
        //TODO Camera shows player front
        menu.SetActive(true);
        customizationMenu.SetActive(false);
    }

    public void ShowMenu()
    {
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
        scoreText.enabled = true;
        playerMovement.enabled = true;
        ismenu = false;
        menu.SetActive(false);
    }
    private void Start()
    {
        foreach (MyMonoBehaviour script in scripts)
        {
            script.myStart();
        }
        cam.GetComponent<FollowCamera>().Focus(inici);
        Time.timeScale = 0;
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.PlayerStart();
        playerMovement.setIdle(true);
        ShowMenu();
    }

    public void IncrementScoreWithCoins(int score)
    {
        puntos += score;
        //scoreText.text = "Score: " + puntos;
    }
    // Update is called once per frame
    void Update()
    {

        foreach (MyMonoBehaviour script in scripts)
        {
            script.myUpdate();
        }
        if (player.transform.position.y < deathHeight)
        {
            levelGenerator.Regenerate();
            playerMovement.Restart();
            coinPool.Restart();
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

        if(Input.GetKeyDown(KeyCode.Q))
        {
            levelGenerator.AddLevelRotation(90);
        }

        if (Input.GetKey(OpenMenu))
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

        if (Input.GetKeyDown(KeyCode.G))
        {
            //setGodMode
        }
     
    }

}
public class MyMonoBehaviour : MonoBehaviour
{
    public virtual void myStart() { }
    public virtual void myUpdate() { }
}
