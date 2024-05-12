using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
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
        public namedValue<Vector2Int>[] allowedSectionsAfter;
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

    [System.Serializable]
    public class namedValue<T>
    {
        [HideInInspector]
        public string name;
        public T value;

        public namedValue(string v, T value)
        {
            name = v;
            this.value = value;
        }

        public static implicit operator T(namedValue<T> d) => d.value;
        public override string ToString() => $"{name}={value}";
    }
    private namedValue<Vector2Int>[] allowedSectionsNext;
    private int allowedSectionsNextMaxWeight = 0;

    private Vector3 nextSectionPos = new Vector3(0, 0, 0);

    private Quaternion levelRot = Quaternion.Euler(0, 0, 0);

    private int currentSection = -1;

    /// <summary>
    /// Elimina totes les seccions actuals, genera de noves i torna totes les posicions a l'inici.
    /// </summary>
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

    /// <summary>
    /// Retorna un element aleatori d'una llista tenint en compte el pes.
    /// </summary>
    /// <param name="possibleSections">Llista de vectors2int on el primer parametre es el valor i el segon el pes d'aquest valor.</param>
    /// <param name="maxWeight">Suma opcional de tots els valors de la llista precalculats.</param>
    /// <returns></returns>
    private int GetRandomSelection(namedValue<Vector2Int>[] possibleSections, int maxWeight = -1)
    {
        if(maxWeight == -1)
        {
            maxWeight = 0;
            foreach (var section in possibleSections)
            {
                maxWeight += section.value.y;
            }
        }
        int randomWeight = Random.Range(0, maxWeight+1);
        if(randomWeight >= maxWeight)
            return possibleSections[possibleSections.Length].value.x;
        for (int i = 0; i < possibleSections.Length; i++)
        {
            randomWeight -= possibleSections[i].value.y;
            if (randomWeight <= 0)
            {
                return possibleSections[i].value.x;
            }
        }
        return possibleSections[0].value.x;
    }

    public void SetLevelRotation(Quaternion rot)
    {
        levelRot = rot;
    }

    /// <summary>
    /// Crea una nova secció amb els seus obstacles i monedes i elimina l'última secció.
    /// </summary>
    private void GenerateNewSection()
    {
        if(currentSection == -1)
        {
            allowedSectionsNext = new namedValue<Vector2Int>[] { new namedValue<Vector2Int>("first", new Vector2Int(0, 1)) };
            allowedSectionsNextMaxWeight = 1;
        }
        int sectionId = GetRandomSelection(allowedSectionsNext, allowedSectionsNextMaxWeight);
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
    
    /// <summary>
    /// Comproba si derrera del jugador hi ha seccions per trucar a la nova.
    /// </summary>
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
    private void OnValidate()
    {
        if (!Application.IsPlaying(gameObject))
        {
            // Editor logic
            foreach(var section in sections)
            {
                foreach(var nextSection in section.allowedSectionsAfter)
                {
                    if(nextSection.value.x >= sections.Length)
                    {
                        Debug.LogError("Value of "+section.name+ " for "+nextSection.ToString()+" is out of range.");
                        break;
                    }
                    nextSection.name = sections[nextSection.value.x].name;
                }

            }
        }
        else
        {
            // Play logic
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Application.IsPlaying(gameObject))
        {
            for (int i = 0; i < sectionsCount; i++)
            {
                GenerateNewSection();
            }
            needNewSection(); //Aquesta primera call es perque Unity no pensi que no s'utilitza la funció i l'elimini en optimitzar ja que els InvokeRepeating no compilan com a call
            InvokeRepeating("needNewSection", timeBetweenChecks, timeBetweenChecks);
            InvokeRepeating("GenerateCoinsWrapper", 0, 0.1f);  //Les monedes s'han de generar per secció totes a l'hora, 
                                                                //ja s'encarrega la resta del codi de garantir que es truca quan cal.
        }
        else
        {
            foreach (var section in sections)
            {
                int weights = 0;
                foreach (var nextSection in section.allowedSectionsAfter)
                {
                    weights += nextSection.value.y;
                }
                section.setSectionWeights(weights);
            }
        }
    }
}
