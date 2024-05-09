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
        public string name;
        public GameObject obj;
        public Vector3 pos;
        public Quaternion rot;
        public Vector2Int[] allowedSectionsAfter;
        public int[] allowedObstacles;
        public float lenght;
        public Vector3 levelPos;
        public int coinSides = 111; // 000 -> no coin, 001 -> right, 010 -> middle, 100 -> left if(coinSides & 0b001) -> right

        private int sectionWeights = 0;
        public int getSectionWeights()
        {
            return sectionWeights;
        }
        public void setSectionWeights(int weights)
        {
            sectionWeights = weights;
        }
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

    private Vector2Int[] allowedSectionsNext;
    private int allowedSectionsNextMaxWeight = 0;

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

    private int GetRandomSection(Vector2Int[] possibleSections, int maxWeight)
    {
        int randomWeight = Random.Range(0, maxWeight);
        for (int i = 0; i < possibleSections.Length; i++)
        {
            randomWeight -= possibleSections[i].y;
            if (randomWeight <= 0)
            {
                return possibleSections[i].x;
            }
        }
        return -1;
    }

    public void SetLevelRotation(Quaternion rot)
    {
        levelRot = rot;
    }

    private void GenerateNewSection()
    {
        if(currentSection == -1)
        {
            //La primera secci� es la 0 per que no caigui el jugador
            allowedSectionsNext = new Vector2Int[] { new Vector2Int(2, 0) };
        }
        //TODO Fix the random range so it takes into account the allowed sections and add random weights
        int sectionId = GetRandomSection(allowedSectionsNext, allowedSectionsNextMaxWeight);
        if(sectionId == -1)
        {
            Debug.Log("Error: No section found");
            Debug.Log("SectionId: " + sectionId);
            Debug.Log("allowedSectionsNext: " + string.Join(", ", allowedSectionsNext));
            sectionId = 0;
        }
        

        Section newSection = sections[sectionId];
        allowedSectionsNext = newSection.allowedSectionsAfter;
        allowedSectionsNextMaxWeight = newSection.getSectionWeights();
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

        // Encontrar la posici�n aleatoria entre las posiciones v�lidas
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
        foreach (var section in sections)
        {
            int weights = 0;
            foreach (var nextSection in section.allowedSectionsAfter)
            {
                weights += nextSection.y;
            }
            section.setSectionWeights(weights);
        }
        for (int i = 0; i < sectionsCount; i++)
        {
            GenerateNewSection();
        }
        InvokeRepeating("needNewSection", timeBetweenChecks, timeBetweenChecks);
        InvokeRepeating("GenerateCoinsWrapper", 0, 0.1f);
    }
}
