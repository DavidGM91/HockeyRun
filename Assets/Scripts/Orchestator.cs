using System.Collections;
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
    public GameObject GameOver;

    public AudioSource music;
    public AudioSource gameOverAudio;


    public Vector3 playerCamOffset = new Vector3(6, 3, 0);

    public Transform inici;
    public Vector3 iniciCamOffset = new Vector3(0, 6, -12);

    public Vector3 customizeCamOffset = new Vector3(-3, 0.5f, 3);


    public KeyCode OpenMenu;

    public float deathHeight = -5;

    private bool ismenu = true;

    public bool isHit = false;

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
        playerMovement.PlayAnim(PlayerMovement.EAnims.Forward);
        music.GetComponent<AudioSource>();

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
        GameOver.SetActive(false);
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

    public void Restart()
    {
        //Desactivem primer els scripts a reiniciar
        playerMovement.enabled = false;
        levelGenerator.enabled = false;
        coinPool.enabled = false;
        eS.enabled = false;

        //Els reiniciem
        eS.Restart();
        coinPool.Restart();
        levelGenerator.Regenerate();
        playerMovement.Restart();
        puntos = 0;

        //Reactivem els scripts
        eS.enabled = true;
        coinPool.enabled = true;
        levelGenerator.enabled = true;
        playerMovement.enabled = true;

        playerMovement.StopAnim(PlayerMovement.EAnims.GameOver);
        playerMovement.PlayAnim(PlayerMovement.EAnims.Forward);

        music.Play();     
    }

    public void Kill()
    {  
        playerMovement.enabled = false;
        levelGenerator.enabled = false;
        coinPool.enabled = false;
        eS.enabled = false;

        playerMovement.PlayAnim(PlayerMovement.EAnims.GameOver);

        playerMovement.StopAnim(PlayerMovement.EAnims.Forward);

        gameOverAudio.Play();
        GameOver.SetActive(true);

        if (_enems == null)
        {
            _enems = enem.GetComponent<Enemics>();
        }
        _enems.GetKill();
        music.Stop();
    }


    public GameObject enem;
    private Enemics _enems;

    private float lastHit = 0;

    public void HideGameOver()
    {
        GameOver.SetActive(false);
    }

    public void Hit()
    {
        if(isHit && Time.time-lastHit > 2)
        {
            Kill();
        }
        lastHit = Time.time;
        isHit = true;
        if (_enems == null)
        {
            _enems = enem.GetComponent<Enemics>();
        }
        _enems.GetClose();
    }

    public void UnHit()
    {
        if (isHit)
        {
            isHit = false;
            _enems.RetreatClose();
        }
    }
    void Update()
    {
        eS.UpdateTimes(Time.deltaTime, playerMovement.distance);
        eS.checkEvents(playerMovement.distance, playerMovement.lateralDistance, playerMovement.transform.position.y);
        if (player.transform.position.y < deathHeight)
        {
            Kill();
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
