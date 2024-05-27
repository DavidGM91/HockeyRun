using TMPro;
using UnityEditor;
using UnityEngine;

public class Orchestrator : MonoBehaviour
{
    public GameObject player;
    public Transform inici;
    public GameObject cam;
    public LevelGenerator levelGenerator;
    public CoinPool coinPool;

    public MyEventSystem eS;

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
        eS.UpdateTimes(Time.deltaTime, playerMovement.distance);
        eS.checkEvents(playerMovement.distance, playerMovement.lateralDistance);
        foreach (MyMonoBehaviour script in scripts)
        {
            if(script.pausable)
            {
                if (!ismenu)
                    script.myUpdate();
            }
            else
            {
                script.myUpdate();
            }
        }
        if (player.transform.position.y < deathHeight)
        {
            playerMovement.Restart();
            levelGenerator.Regenerate();
            eS.Restart();
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
            player.GetComponent<Rigidbody>().useGravity = !player.GetComponent<Rigidbody>().useGravity;
        }
     
    }
}
public class MyMonoBehaviour : MonoBehaviour
{
    public bool pausable = true;
    public virtual void myStart() { }
    public virtual void myUpdate() { }
}
