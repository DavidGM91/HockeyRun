using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class LevelGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject levelCamera;
    public CoinPool coinPool;
    public MyEventSystem eventSystem;
    public int sectionsCount = 10;
    public int sectionsBehind = 2;
    public float timeBetweenChecks = 2.0f;

    public float timeToQTE = 2.0f;

    [System.Serializable]
    public enum SectionType
    {
        recte,
        dreta,
        esquerra,
        bifurcacio
    }

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
        public int coinSides = 0b111111; // 000 -> no coin, 001 -> right, 010 -> middle, 100 -> left if(coinSides & 0b001) -> right

        public SectionType type;

        [HideInInspector]
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
    private Dictionary<uint, GameObject> levelSections = new();
    private List<Tuple<uint, uint>> bifur = new();

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

    private int sectionsWithCoins = 0;

    private float nextCoinPos = 0;

    private float distance = 0;

    /// <summary>
    /// Elimina totes les seccions actuals, genera de noves i torna totes les posicions a l'inici.
    /// </summary>
    public void Regenerate()
    {
        foreach (var section in levelSections)
        {
            Destroy(section.Value);
            eventSystem.IgnoreEvent(section.Key);
        }
        levelSections.Clear();
        currentSection = -1;
        nextSectionPos = new Vector3(0, 0, 0);
        levelRot = Quaternion.Euler(0, 0, 0);
        distance = 0;
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
            return possibleSections[possibleSections.Length-1].value.x;
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

    public void AddLevelRotation(float rot)
    {
        Quaternion rotation = Quaternion.Euler(0, rot, 0);
        levelRot *= rotation;
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

        distance += newSection.lenght;

        MyEvent myEvent = new MyEvent("Section Destroy", distance + sectionsBehind * newSection.lenght, SectionEvent);
        uint id = eventSystem.AddEvent(myEvent);

        if (newSection.obj != null)
            levelSections.Add(id,Instantiate(newSection.obj, nextSectionPos+ levelRot * newSection.pos, levelRot * newSection.rot));



        int coinsNext = GenerateObstacles(sectionId, levelSections[id].GetComponent<BoxCollider>());

        if(newSection.type == SectionType.bifurcacio)
        {
            //TODO: Generar doble seccions a les bifurcacions
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.lenght, TurnRightSectionEvent, KeyCode.W, timeToQTE);
            uint rEventId = eventSystem.AddEvent(myRQTEEvent);
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.lenght, TurnLeftSectionEvent, KeyCode.D, timeToQTE);
            bifur.Add(new Tuple<uint,uint>(rEventId,eventSystem.AddEvent(myLQTEEvent)));
        }
        else if (newSection.type == SectionType.dreta)
        {
            AddLevelRotation(90);
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.lenght, TurnRightSectionEvent, KeyCode.W, timeToQTE);
            eventSystem.AddEvent(myRQTEEvent);
        }
        else if (newSection.type == SectionType.esquerra)
        {
            AddLevelRotation(-90);
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.lenght, TurnLeftSectionEvent, KeyCode.D, timeToQTE);
            eventSystem.AddEvent(myLQTEEvent);
        }

        nextSectionPos += levelRot * (new Vector3(0, 0, newSection.lenght)+newSection.pos);
        

        if(sectionsWithCoins > 0)
        {
            sectionsWithCoins--;
            //COINS
            GenerateCoins(coinsNext, levelSections[id].GetComponent<BoxCollider>());
        }
        else
        {
            if (Random.Range(0, 4) == 0)
            {
                sectionsWithCoins = Random.Range(3, 4);
            }
        }
    }
    public void TurnRightSectionEvent(uint id, bool success)
    {
        bool isBifur = false;
        for(int i = 0; i < bifur.Count; i++)
        {
            if (bifur[i].Item1 == id)
            {
                isBifur = true;
                if (success)
                {
                    //TODO: Rotar el jugador cap a la dreta
                }
                else
                {
                    //Això indica que el jugador ha fallat la bifurcació cap a la dreta, que es truca sempre abans que la de l'esquerra.
                    bifur[i] = new Tuple<uint, uint>(0, bifur[i].Item2);
                }
                break;
            }
        }
        if (!isBifur)
        {
            if (success)
            {
                //TODO: Rotar el jugador cap a la dreta
            }
            else
            {
                //TODO: OH NO EL JUGADOR CAU!
            }
        }
    }
    public void TurnLeftSectionEvent(uint id, bool success)
    {
        bool isBifur = false;
        for (int i = 0; i < bifur.Count; i++)
        {
            if (bifur[i].Item2 == id)
            {
                isBifur = true;
                if (success)
                {
                    //TODO: Rotar el jugador cap a l'esquerra
                }
                else
                {
                    if (bifur[i].Item1 == 0)
                    {
                        //Això indica que el jugador ha fallat ambdues bifurcacions, per tant ha de caure.
                        //TODO: OH NO EL JUGADOR CAU!
                    }
                }
                break;
            }
        }
        if (!isBifur)
        {
            if (success)
            {
                //TODO: Rotar el jugador cap a l'esquerra
            }
            else
            {
                //TODO: OH NO EL JUGADOR CAU!
            }
        }
    }
    public void SectionEvent(uint id, bool success)
    {
        if (success)
        {
            GenerateNewSection();
            Destroy(levelSections[id]);
            levelSections.Remove(id);
        }
    }

    private int GenerateObstacles(int sectionId, BoxCollider section)
    {
        //TODO Set nextCoinPos
        return 0b111111;
    }
    
    private void GenerateCoins(int coinsNext, BoxCollider section)
    {   
        int finalCoinSide = 0;
        List<int> possibleCoinSide = new List<int>();
        for (int i = 0; i < 6; ++i)
        {
            if (((coinsNext >> i) & 1) == 1) possibleCoinSide.Add(i); // guardo els bits a 1
        }

        if (possibleCoinSide.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleCoinSide.Count);
            finalCoinSide = possibleCoinSide[randomIndex]; // 0 = izquierda 1 = medio 2 = derecha 3 = izquierda flotante...
        }

        nextCoinPos = -2;
        Transform sectionPos = section.transform;

        // 5 monedes per seccio
        for (int i = 0; i < 5; ++i)
        {
            GameObject coin = coinPool.RequestCoin();
            if (coin != null)
            {
                Vector3 coinPosition = new Vector3();
                switch (finalCoinSide)
                {
                    case 0: // izquierda
                        coinPosition = new Vector3(nextCoinPos, 0, 0);
                        break;
                    case 1: // medio
                        coinPosition = new Vector3(nextCoinPos, 0, 2);
                        break;
                    case 2: // derecha
                        coinPosition = new Vector3(nextCoinPos, 0, 3.5f);
                        break;
                    case 3: // izquierda flotante
                        if (i == 0) coinPosition = new Vector3(nextCoinPos, 1, 0);
                        else if (i == 1) coinPosition = new Vector3(nextCoinPos, 1.8f, 0);
                        else if (i == 2) coinPosition = new Vector3(nextCoinPos, 2, 0);
                        else if (i == 3) coinPosition = new Vector3(nextCoinPos, 1.8f, 0);
                        else coinPosition = new Vector3(nextCoinPos, 1, 0);
                        break;
                    case 4: // medio flotante
                        if (i == 0) coinPosition = new Vector3(nextCoinPos, 1, 2);
                        else if (i == 1) coinPosition = new Vector3(nextCoinPos, 1.8f, 2);
                        else if (i == 2) coinPosition = new Vector3(nextCoinPos, 2, 2);
                        else if (i == 3) coinPosition = new Vector3(nextCoinPos, 1.8f, 2);
                        else coinPosition = new Vector3(nextCoinPos, 1, 2);
                        break;
                    case 5: // derecha flotante
                        if (i == 0) coinPosition = new Vector3(nextCoinPos, 1, 4);
                        else if (i == 1) coinPosition = new Vector3(nextCoinPos, 1.8f, 4);
                        else if (i == 2) coinPosition = new Vector3(nextCoinPos, 2, 4);
                        else if (i == 3) coinPosition = new Vector3(nextCoinPos, 1.8f, 4);
                        else coinPosition = new Vector3(nextCoinPos, 1, 4);

                        break;
                }
                coin.transform.position = sectionPos.position + sectionPos.rotation * coinPosition + new Vector3(0,1,0);
                //coin.transform.SetParent(sectionPos, true);
                nextCoinPos += 1f; // distancia entre monedas
            }
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
                        //Debug.LogError("Value of "+section.name+ " for "+nextSection.ToString()+" is out of range.");
                        break;
                    }
                    nextSection.name = sections[nextSection.value.x].name;
                }

            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Application.IsPlaying(gameObject))
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

            for (int i = 0; i < sectionsCount; i++)
            {
                GenerateNewSection();
            }
            //Les monedes s'han de generar per secció totes a l'hora, 
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
