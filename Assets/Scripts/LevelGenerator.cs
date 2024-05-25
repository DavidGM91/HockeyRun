using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class LevelGenerator : MonoBehaviour
{
    public GameObject player;
    public GameObject levelCamera;
    public Section firstSection;
    public CoinPool coinPool;
    public MyEventSystem eventSystem;
    public int sectionsCount = 10;
    public int sectionsBehind = 2;
    public float timeBetweenChecks = 2.0f;
    public int minStraightSectionsBetweenRotations = 15;

    public float timeToQTE = 2.0f;

    [System.Serializable]
    public enum SectionType
    {
        recte,
        final,
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


    private Dictionary<uint, Transform> rightRotations = new();
    private Dictionary<uint, Transform> leftRotations = new();

    private PlayerMovement playerMovement = null;

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
    private Vector3 nextSectionPos2 = new Vector3(0, 0, 0);

    private Quaternion levelRot = Quaternion.Euler(0, 0, 0);

    private int currentSection = -1;

    private int sectionsWithCoins = 0;

    private float nextCoinPos = 0;


    private float distance = 0;

    private uint justRotatedRight = 0;
    private uint justRotatedLeft = 0;

    private int gracePeriodNoRots = 10;

    private bool bifurcateCopy = false;


    /// <summary>
    /// Elimina totes les seccions actuals, genera de noves i torna totes les posicions a l'inici.
    /// </summary>
    public void Regenerate()
    {
        foreach (var section in levelSections)
        {
            Destroy(section.Value);
        }
        levelSections.Clear();
        currentSection = -1;
        nextSectionPos = new Vector3(0, 0, 0);
        levelRot = Quaternion.Euler(0, 0, 0);
        distance = 0;
        gracePeriodNoRots = minStraightSectionsBetweenRotations;
        bifurcateCopy = false;
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
    /// Crea una nova secció amb els seus obstacles i monedes.
    /// </summary>
    private void GenerateNewSection()
    {
        //SECCIONS
        //Primera secció forcem first
        bool isFirst = false;
        if(currentSection == -1)
        {
            isFirst = true;
            allowedSectionsNext = new namedValue<Vector2Int>[] { new namedValue<Vector2Int>("first", new Vector2Int(0, 1)) };
            allowedSectionsNextMaxWeight = 1;
        }
        //Escollim la següent secció
        int sectionId = GetRandomSelection(allowedSectionsNext, allowedSectionsNextMaxWeight);
        Section newSection = sections[sectionId];

        //Si encara estem copiant la bifurcació o seguim en periode de gracia forcem a ser un final la secció aleatori(No té en compte el pes aleatori en ser un cas limit).
        if (newSection.type != SectionType.recte && newSection.type != SectionType.final && (gracePeriodNoRots > 0 || bifurcateCopy))
        {
            bool finalFound = false;
            foreach (Section _section in sections)
            {
                if(_section.type == SectionType.final && !finalFound)
                {
                    if (finalFound)
                    {
                        if (Random.Range(0, 2) == 0)
                            newSection = _section;
                    }
                    else
                    {
                        finalFound = true;
                        newSection = _section;
                    }
                }
            }
        }
        gracePeriodNoRots--;

        //Preparem les que poden venir després
        allowedSectionsNext = newSection.allowedSectionsAfter;
        allowedSectionsNextMaxWeight = newSection.getSectionWeights();


        currentSection++;

        distance += newSection.lenght;

        //Event de neteja
        MyEvent myEvent = new MyEvent("Section Destroy", distance + sectionsBehind * newSection.lenght, SectionEvent);
        uint id = eventSystem.AddEvent(myEvent);
        uint id2 = 0;

        //Creació i snapping de la secció
        if (newSection.obj != null)
            levelSections.Add(id,Instantiate(newSection.obj));

        SpawnSection section = levelSections[id].GetComponent<SpawnSection>();
        section.positionYourselfPlease(nextSectionPos);
        section.rotateYourselfAroundYourOriginPlease(levelRot.eulerAngles);
        nextSectionPos = section.GetSpawn(0);

        if(isFirst)
        {
            playerMovement.ChangeAnchor(section.GetSpawn(0), section.transform.rotation);
        }

        if (bifurcateCopy)
        {
            id2 = eventSystem.AddEvent(myEvent);
            levelSections.Add(id2, Instantiate(newSection.obj));
            SpawnSection section2 = levelSections[id2].GetComponent<SpawnSection>();
            section2.positionYourselfPlease(nextSectionPos2);
            section2.rotateYourselfAroundYourOriginPlease((levelRot* Quaternion.Euler(0, 180, 0)).eulerAngles);
            nextSectionPos2 = section2.GetSpawn(0);
        }

        if(justRotatedRight != 0 && justRotatedLeft == 0)
        {
            rightRotations.Add(justRotatedRight, levelSections[id].transform);
            justRotatedRight = 0;
        }
        if(justRotatedLeft != 0 && justRotatedRight == 0)
        {
            leftRotations.Add(justRotatedLeft, levelSections[id].transform);
            justRotatedLeft = 0;
        }
        if(justRotatedRight != 0 && justRotatedLeft != 0)
        {
            rightRotations.Add(justRotatedRight, levelSections[id].transform);
            leftRotations.Add(justRotatedLeft, levelSections[id2].transform);
            justRotatedRight = 0;
            justRotatedLeft = 0;
        }

        //Events de girs
        if (newSection.type == SectionType.bifurcacio)
        {
            //Assumim que gira a la esquerra per generar el nivell
            AddLevelRotation(90);
            bifurcateCopy = true;
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.lenght*2, TurnRightSectionEvent, KeyCode.D, timeToQTE);
            uint rEventId = eventSystem.AddEvent(myRQTEEvent);
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.lenght*2, TurnLeftSectionEvent, KeyCode.A, timeToQTE);
            bifur.Add(new Tuple<uint,uint>(rEventId,eventSystem.AddEvent(myLQTEEvent)));
            justRotatedRight = bifur.Last().Item1;
            justRotatedLeft = bifur.Last().Item2;
            nextSectionPos2 = section.GetSpawn(1);
        }
        else if (newSection.type == SectionType.dreta)
         {
            AddLevelRotation(90);
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.lenght, TurnRightSectionEvent, KeyCode.W, timeToQTE);
            justRotatedRight = eventSystem.AddEvent(myRQTEEvent);
        }
        else if (newSection.type == SectionType.esquerra)
        {
            AddLevelRotation(-90);
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.lenght, TurnLeftSectionEvent, KeyCode.D, timeToQTE);
            justRotatedLeft = eventSystem.AddEvent(myLQTEEvent);
        }

        //OBSTACLES
        int coinsNext = GenerateObstacles(sectionId, levelSections[id].GetComponent<BoxCollider>());

        //MONEDES
        if (sectionsWithCoins > 0)
        {
            sectionsWithCoins--;
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
        Debug.Log("TurnRightSectionEvent "+id+" "+success);
        bool isBifur = false;
        for(int i = 0; i < bifur.Count; i++)
        {
            if (bifur[i].Item1 == id)
            {
                isBifur = true;
                if (success)
                {
                    playerMovement.ChangeAnchor(rightRotations[id].position, rightRotations[id].rotation);
                    rightRotations.Remove(id);
                    leftRotations.Remove(bifur[i].Item2);
                    bifurcateCopy = false;
                }
                else
                {
                    //Això indica que el jugador ha fallat la bifurcació cap a la dreta, que es truca sempre abans que la de l'esquerra.(crec)
                    bifur[i] = new Tuple<uint, uint>(0, bifur[i].Item2);
                }
                break;
            }
        }
        if (!isBifur)
        {
            if (success)
            {
                playerMovement.ChangeAnchor(rightRotations[id].position, rightRotations[id].rotation);
            }
            else
            {
                //TODO: OH NO EL JUGADOR CAU!
            }
        }
    }
    public void TurnLeftSectionEvent(uint id, bool success)
    {
        Debug.Log("TurnLeftSectionEvent "+id+" "+success);
        bool isBifur = false;
        for (int i = 0; i < bifur.Count; i++)
        {
            if (bifur[i].Item2 == id)
            {
                isBifur = true;
                if (success)
                {
                    playerMovement.ChangeAnchor(leftRotations[id].position, leftRotations[id].rotation);
                    rightRotations.Remove(bifur[i].Item1);
                    leftRotations.Remove(id);
                    bifurcateCopy = false;
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
                playerMovement.ChangeAnchor(leftRotations[id].position, leftRotations[id].rotation);
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
            playerMovement = player.GetComponent<PlayerMovement>();
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
