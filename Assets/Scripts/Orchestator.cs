using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Orchestrator : MonoBehaviour
{
    public GameObject player;
    
    public GameObject cam;
    public LevelGenerator levelGenerator;
    public CoinPool coinPool;

    public MyEventSystem eS;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI creditsText;



    public GameObject menu;
    public GameObject customizationMenu;
    public GameObject creditsPanel;
    public GameObject instructionsPanel;

    public Vector3 playerCamOffset = new Vector3(6, 3, 0);

    public Transform inici;
    public Vector3 iniciCamOffset = new Vector3(0, 6, -12);

    public Vector3 customizeCamOffset = new Vector3(-3, 0.5f, 3);


    public KeyCode OpenMenu;

    public float deathHeight = -10;

    private bool ismenu = true;

    private PlayerMovement playerMovement = null;

    private int puntos = 0;
    private float acumulatedTime = 0;

    public void Play()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(false);
        cam.GetComponent<FollowCamera>().Focus(player.transform, true);
        cam.GetComponent<FollowCamera>().AdjustCamera(playerCamOffset, 0.5f);
        HideMenu();
    }
    public void ShowCustomization()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(true);
        cam.GetComponent<FollowCamera>().Focus(player.transform);
        cam.GetComponent<FollowCamera>().AdjustCamera(customizeCamOffset, 1);
        //TODO Camera shows player front
        menu.SetActive(false);
        customizationMenu.SetActive(true);
    }
    public void HideCustomization()
    {
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.setIdle(false);
        cam.GetComponent<FollowCamera>().AdjustCamera(playerCamOffset, 1);
        //TODO Camera shows player front
        menu.SetActive(true);
        customizationMenu.SetActive(false);
    }
    public void ShowMenu()
    {
        //Time.timeScale = 0;
        playerMovement.enabled = false;
        scoreText.enabled = false;
        ismenu = true;
        menu.SetActive(true);
        instructionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
    public void HideMenu()
    {
        //Time.timeScale = 1;
        scoreText.enabled = true;
        playerMovement.enabled = true;
        ismenu = false;
        menu.SetActive(false);
    }
    public void IncrementScoreWithCoins(int score)
    {
        puntos += score;
        //scoreText.text = "Score: " + puntos;
    }
    // Update is called once per frame

    public void ShowInstructions()
    {
        menu.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        instructionsPanel.SetActive(false);
        menu.SetActive(true);
       
    }

    public void ShowCredits()
    {
        menu.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
        menu.SetActive(true);
    }

    public void BackMenu()
    {
        menu.SetActive(true);
        creditsPanel.SetActive(false) ;
        instructionsPanel.SetActive(false);

    }
    void Update()
    {
        eS.UpdateTimes(Time.deltaTime, playerMovement.distance);
        eS.checkEvents(playerMovement.distance, playerMovement.lateralDistance, playerMovement.transform.position.y);
        if (player.transform.position.y < deathHeight)
        {
            eS.Restart();
            coinPool.Restart();
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
            player.GetComponent<Rigidbody>().useGravity = !player.GetComponent<Rigidbody>().useGravity;
        }
    }
    void Start()
    {
        cam.GetComponent<FollowCamera>().Focus(inici);
        //Time.timeScale = 0;
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.PlayerStart();
        playerMovement.setIdle(true);
        ShowMenu();
    }
    public void stopGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        # endif
    }
}
