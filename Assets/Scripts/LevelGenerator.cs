using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject levelCamera;
    public int sectionsCount = 10;
    public int sectionsBehind = 2;
    public float timeBetweenChecks = 2.0f;
    public GameObject prefabCoin;
    [System.Serializable]
    public class Section
    {
        public GameObject obj;
        public string name;
        public Vector3 pos;
        public Quaternion rot;
        public int[] allowedSectionsAfter;
        public int[] allowedObstacles;
        public float lenght;
        public Vector3 levelPos;
        public int coinSides = 111; // 000 -> no coin, 001 -> right, 010 -> middle, 100 -> left if(coinSides & 0b001) -> right
    }
    [SerializeField]
    public Section[] sections;

    [System.Serializable]
    public enum ObstacleType
    {
        lowerObstacle,
        upperObstacle,
        middleObstacle,
        rightObstacle,
        leftObstacle
    }
    [System.Serializable]
    public class obstacle
    {
        public GameObject obj;
        public Vector3 scale;
        public Vector3 pos;
        public Quaternion rot;
        public string name;
        public ObstacleType type;
        public bool isDeadly = false;
    }
    [SerializeField]
    public List<obstacle> obstacles = new();

    private List<GameObject> levelSections = new();

    private int[] allowedSectionsNext;

    private Vector3 nextSectionPos = new Vector3(0, 0, 0);

    private Quaternion levelRot = Quaternion.Euler(0, 0, 0);

    private int currentSection = -1;

    public void Regenerate()
    {
        foreach(GameObject section in levelSections)
        {
            Destroy(section);
        }
        levelSections.Clear();
        currentSection = -1;
        nextSectionPos = new Vector3(0, 0, 0);
        levelRot = Quaternion.Euler(0, 0, 0);
        for (int i = 0; i < sectionsCount; i++)
        {
            GenerateNewSection();
        }
    }

    private void GenerateNewSection()
    {
        if(currentSection == -1)
        {
            //La primera seccio es la 0 per que no caigui el jugador
            allowedSectionsNext = new int[] { 0 };
        }
        int sectionId = Random.Range(0, allowedSectionsNext.Length);
        Section newSection = sections[sectionId];
        allowedSectionsNext = newSection.allowedSectionsAfter;
        currentSection++;
        levelSections.Add(Instantiate(newSection.obj, nextSectionPos+ levelRot * newSection.pos, levelRot * newSection.rot));
        GenerateObstacles(sectionId, nextSectionPos);

        //COINS
       // GenerateCoinsWrapper(newSection.coinSides);

        nextSectionPos += levelRot * (new Vector3(0, 0, newSection.lenght)+newSection.pos);
        nextSectionPos.x = 0;
        nextSectionPos.y = 0;

        if(levelSections.Count > sectionsCount)
        {
            Destroy(levelSections[0]);
            levelSections.RemoveAt(0);
        }
    }

    private void GenerateObstacles(int sectionId, Vector3 pos)
    {
        
    }
    
    private void GenerateCoinsWrapper()
    {
        Section newSection;
        //GenerateCoins(newSection.coinSides);
    }
    private void GenerateCoins(int coinSides)
    {      

        if (coinSides == 0) return; 

        Vector3[] coinSpawnPositions = new Vector3[3];
        coinSpawnPositions[0] = new Vector3(-1.5f, 0, 0); 
        coinSpawnPositions[1] = new Vector3(0, 0, 0);
        coinSpawnPositions[2] = new Vector3(1.5f, 0, 0); 

        int validPositionsCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if ((coinSides & (1 << i)) != 0)
            {
                validPositionsCount++;
            }
        }

        if (validPositionsCount == 0) return;

        // index aleatori per selecciona una de les posicions
        int randomIndex = Random.Range(0, validPositionsCount);

        // Encontrar la posición aleatoria entre las posiciones válidas
        int validIndex = -1;
        for (int i = 0; i < 3; i++)
        {
            if ((coinSides & (1 << i)) != 0)
            {
                validIndex++;
                if (validIndex == randomIndex)
                {
                    Instantiate(prefabCoin, transform.TransformPoint(coinSpawnPositions[i]), Quaternion.identity);
                    break;
                }
            }
        }
    }

    private void needNewSection()
    {
        Transform playerPos = player.GetComponent<Transform>();

        Bounds lastSectionPos = levelSections[sectionsBehind].GetComponent<Renderer>().bounds;
        Vector3 playerForward = playerPos.forward;
        Vector3 playerPosition = playerPos.position;

        while (Vector3.Dot(lastSectionPos.center - playerPosition, playerForward) < 0)
        {
            GenerateNewSection();
            lastSectionPos = levelSections[sectionsBehind].GetComponent<Renderer>().bounds;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < sectionsCount; i++)
        {
            GenerateNewSection();
        }
        InvokeRepeating("needNewSection", timeBetweenChecks, timeBetweenChecks);
        InvokeRepeating("GenerateCoinsWrapper", 0, 0.1f);
    }
}
