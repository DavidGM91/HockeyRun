using System;
using System.Collections;
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
    public int minStraightSectionsBetweenRotations = 15;

    public float timeToQTE = 2.0f;

    [SerializeField]
    private Orchestrator orchestrator;

    [System.Serializable]
    public enum SectionType
    {
        recte,
        final,
        dreta,
        esquerra,
        bifurcacio
    }

    public enum ObjectActionOnPlayer
    {
        Kill,
        Hit,
        UnHit,
        None
    }

    [System.Serializable]
    public class Section
    {
        public string name;
        public GameObject obj;
        public Vector3 pos;
        public Quaternion rot;
        public namedValue<Vector2Int>[] allowedSectionsAfter;
        public namedValue<Vector2Int>[] allowedObstacles;
        public Vector3 levelPos;
        public int coinSides = 0b111111; // 000 -> no coin, 001 -> right, 010 -> middle, 100 -> left if(coinSides & 0b001) -> right
        [HideInInspector]
        public uint associatedID = 0;
        [HideInInspector]
        public float length = -1;

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

        [HideInInspector]
        private int obstaclesWeights = 0;
        public int getObstaclesWeights()
        {
            if(obstaclesWeights == 0)
            {
                foreach (var obstacle in allowedObstacles)
                {
                    obstaclesWeights += obstacle.value.y;
                }
            }
            return obstaclesWeights;
        }
    }
    [SerializeField]
    public Section[] sections;

    [System.Serializable]
    public class obstacle
    {
        public string name;
        public GameObject obj;
        public int coinsBloqued; // 0 = izquierda 1 = medio 2 = derecha 3 = izquierda flotante , 4 centr..
        public bool isDeadly = false;
    }
    [SerializeField]
    public obstacle[] obstacles;
    private Dictionary<uint, GameObject> levelSections = new();
    private List<Tuple<uint, uint>> bifur = new();
    private Dictionary<uint, Coin> coinEventList = new();
    private Dictionary<uint, SpawnObstacle> obsEventList = new();
    private Dictionary<uint, Transform> rightRotations = new();
    private Dictionary<uint, Transform> leftRotations = new();
    private List<uint> rightBifurSects = new();
    private List<uint> leftBifurSects = new();
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
        justRotatedLeft = 0;
        justRotatedRight = 0;
        rightRotations.Clear();
        leftRotations.Clear();
        bifur.Clear();
        coinEventList.Clear();
        obsEventList.Clear();
        for (int i = 0; i < sectionsCount; i++)
        {
            GenerateNewSection();
        }
        foreach (var section in obsEventList)
        {
            Destroy(section.Value.gameObject);
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
        if (maxWeight == 0)
            return -1;
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
        if(currentSection == -1)
        {
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
        GameObject _newSec = null;
        //Creació de la secció
        if (newSection.obj != null)
            _newSec = Instantiate(newSection.obj);

        //Snapping a les ancores
        SpawnSection section = _newSec.GetComponent<SpawnSection>();
        SpawnSection section2 = null;
        section.positionYourselfPlease(nextSectionPos);
        section.rotateYourselfAroundYourOriginPlease(levelRot.eulerAngles);
        nextSectionPos = section.GetSpawn(0);

        //OBSTACLES
        int coinsNext = GenerateObstacles(sectionId, section.origin.position);

        //MONEDES
        if (sectionsWithCoins > 0)
        {
            sectionsWithCoins--;
            GenerateCoins(coinsNext, section.origin.position);
        }
        else
        {
            //Probablilitat de que la següent secció tingui monedes
            if (Random.Range(0, 4) == 0)
            {
                //Numero de seccions consecutives amb monedes
                sectionsWithCoins = Random.Range(3, 4);
            }
        }

        //Càlcul de la distància
        if (newSection.length == -1 && (newSection.type == SectionType.recte || newSection.type == SectionType.final))
        {
            newSection.length = Vector3.Distance(section.origin.position, section.spawns[0].position);
        }
        else if (newSection.length == -1)
        {
            newSection.length = 0;
        }
        distance += newSection.length;

        //Creació d'event de neteja
        MyEvent myEvent = new MyEvent("Section Destroy", distance + sectionsBehind * ((newSection.length > 0)? newSection.length :10), SectionEvent);
        uint id = eventSystem.AddEvent(myEvent);
        uint id2 = 0;
        levelSections.Add(id, _newSec);
        _newSec.name = "#" + id + _newSec.name;

        //Moviment de les marques //TODO: DEBUG REMOVE
        eventSystem.DebugLevelMarker(new Vector3(-distance, 0, 0));

        //Copia de seccions durant la bifurcació
        if (bifurcateCopy)
        {
            rightBifurSects.Add(id);
            myEvent = new MyEvent("Section Destroy", distance -10, SectionEvent);
            id2 = eventSystem.AddEvent(myEvent);
            leftBifurSects.Add(id2);
            levelSections.Add(id2, Instantiate(newSection.obj));
            section2 = levelSections[id2].GetComponent<SpawnSection>();
            levelSections[id2].name = "#" + id2 + levelSections[id2].name;
            section2.positionYourselfPlease(nextSectionPos2);
            section2.rotateYourselfAroundYourOriginPlease((levelRot* Quaternion.Euler(0, 180, 0)).eulerAngles);
            nextSectionPos2 = section2.GetSpawn(0);
        }

        //Guardem les ancles de les seccions després de rotar
        if(justRotatedRight != 0 && justRotatedLeft == 0)
        {
            rightRotations.Add(justRotatedRight, section.origin);
            justRotatedRight = 0;
        }
        if(justRotatedLeft != 0 && justRotatedRight == 0)
        {
            leftRotations.Add(justRotatedLeft, section.origin);
            justRotatedLeft = 0;
        }
        if(justRotatedRight != 0 && justRotatedLeft != 0)
        {
            rightRotations.Add(justRotatedRight, section.origin);
            leftRotations.Add(justRotatedLeft, section2.origin);
            justRotatedRight = 0;
            justRotatedLeft = 0;
        }

        //Gestió de girs i events
        if (newSection.type == SectionType.bifurcacio)
        {
            AddLevelRotation(90);
            bifurcateCopy = true;
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.length*2, TurnRightSectionEvent, KeyCode.D, timeToQTE);
            uint rEventId = eventSystem.AddEvent(myRQTEEvent);
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.length*2, TurnLeftSectionEvent, KeyCode.A, timeToQTE);
            bifur.Add(new Tuple<uint,uint>(rEventId,eventSystem.AddEvent(myLQTEEvent)));
            justRotatedRight = bifur.Last().Item1;
            justRotatedLeft = bifur.Last().Item2;
            nextSectionPos2 = section.GetSpawn(1);
        }
        else if (newSection.type == SectionType.dreta)
         {
            AddLevelRotation(90);
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myRQTEEvent = new MyQTEEvent("Turn Right", distance - newSection.length, TurnRightSectionEvent, KeyCode.D, timeToQTE);
            justRotatedRight = eventSystem.AddEvent(myRQTEEvent);
        }
        else if (newSection.type == SectionType.esquerra)
        {
            AddLevelRotation(-90);
            gracePeriodNoRots = minStraightSectionsBetweenRotations;
            MyQTEEvent myLQTEEvent = new MyQTEEvent("Turn Left", distance - newSection.length, TurnLeftSectionEvent, KeyCode.A, timeToQTE);
            justRotatedLeft = eventSystem.AddEvent(myLQTEEvent);
        }
    }
    IEnumerator waitAndDelete(List<uint> _secs,float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var sect in _secs)
        {
            Destroy(levelSections[sect]);
            levelSections.Remove(sect);
            eventSystem.IgnoreEvent(sect);
        }
    }
    public void TurnRightSectionEvent(uint id, bool success, MyEvent.checkResult result)
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
                    playerMovement.ChangeAnchor(rightRotations[id]);
                    rightRotations.Remove(id);
                    leftRotations.Remove(bifur[i].Item2);
                    bifurcateCopy = false;
                    waitAndDelete(leftBifurSects, 1.0f);
                    leftBifurSects.Clear();
                    rightBifurSects.Clear();
                    
                    if (bifur[i].Item2 != 0)
                    {
                        eventSystem.IgnoreEvent(bifur[i].Item2);
                    }
                    bifur.RemoveAt(i);
                }
                else
                {
                    //Això indica que el jugador ha fallat la bifurcació cap a la dreta, que es truca sempre abans que la de l'esquerra.(crec)
                    //Per tant indico amb un 0 que ha fallat la bifurcació cap a la dreta.
                    bifur[i] = new Tuple<uint, uint>(0, bifur[i].Item2);
                }
                break;
            }
        }
        if (!isBifur)
        {
            if (success)
            {
                playerMovement.ChangeAnchor(rightRotations[id]);
                rightRotations.Remove(id);
            }
        }
    }
    public void TurnLeftSectionEvent(uint id, bool success, MyEvent.checkResult result)
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
                    playerMovement.ChangeAnchor(rightRotations[id]);
                    rightRotations.Remove(bifur[i].Item1);
                    leftRotations.Remove(id);
                    bifurcateCopy = false;
                    waitAndDelete(rightBifurSects, 5.0f);
                    rightBifurSects.Clear();
                    leftBifurSects.Clear();
                    nextSectionPos = nextSectionPos2;
                    
                    //levelRot *= Quaternion.Euler(0, 180, 0);
                    if (bifur[i].Item1 != 0)
                    {
                        eventSystem.IgnoreEvent(bifur[i].Item1);
                    }
                    bifur.RemoveAt(i);
                }
                else
                {
                    if (bifur[i].Item1 == 0)
                    {
                        bifur.RemoveAt(i);
                    }
                }
                break;
            }
        }
        if (!isBifur)
        {
            if (success)
            {
                playerMovement.ChangeAnchor(leftRotations[id]);
                leftRotations.Remove(id);
            }
        }
    }
    public void SectionEvent(uint id, bool success, MyEvent.checkResult result)
    {
        if (success)
        {
            GenerateNewSection();
            Destroy(levelSections[id]);
            levelSections.Remove(id);
        }
    }
    public void ObjectEvent(uint id, bool success, MyEvent.checkResult result)
    {
        var obj = obsEventList[id];
        ObjectActionOnPlayer action = ObjectActionOnPlayer.None;
        if (obj != null)
        {
            action = obj.OnEvent(id, success, result);
        }
        switch (action)
        {
            case ObjectActionOnPlayer.Kill:
                orchestrator.Kill();
                break;
            case ObjectActionOnPlayer.Hit:
                orchestrator.Hit();
                break;
            case ObjectActionOnPlayer.None:
                break;
            case ObjectActionOnPlayer.UnHit:
                orchestrator.UnHit();
                break;
        }
    }
    private int GenerateObstacles(int sectionId, Vector3 anchor)
    {
        int obstacleId = GetRandomSelection(sections[sectionId].allowedObstacles, sections[sectionId].getObstaclesWeights());
        if(obstacleId != -1)
        {
            //Crear obstacle
            GameObject obstacle = Instantiate(obstacles[obstacleId].obj);
            SpawnObstacle obs = obstacle.GetComponent<SpawnObstacle>();
            obs.Init();
            
            uint eventID = 0;
            //Event
            if (obs.obstacleType == SpawnObstacle.ObstacleType.QTE)
            {
                MyQTEEvent myEvent = new MyQTEEvent("QTE",distance - obs.distance, ObjectEvent, obs.keyQTE, timeToQTE);
                eventID = eventSystem.AddEvent(myEvent);
                obsEventList.Add(eventID, obs);
            }
            else if(obs.obstacleType == SpawnObstacle.ObstacleType.AreaQTE)
            {
                MyQTEAreaEvent myEvent = new MyQTEAreaEvent("AreaQTE",distance - obs.distance, ObjectEvent, obs.keyQTE, timeToQTE, obs.initialArea ,obs.finalArea,obs.initialHeight,obs.finalHeight);
                eventID = eventSystem.AddEvent(myEvent);
                obsEventList.Add(eventID, obs);
            }
            else if(obs.obstacleType == SpawnObstacle.ObstacleType.AreaAltura)
            {
                MyHeightAreaEvent myEvent = new MyHeightAreaEvent("AreaAltura", distance - obs.distance, ObjectEvent, obs.initialArea, obs.finalArea, obs.initialHeight, obs.finalHeight);
                eventID = eventSystem.AddEvent(myEvent);
                obsEventList.Add(eventID, obs);
            }
            else if(obs.obstacleType == SpawnObstacle.ObstacleType.Area)
            {
                MyAreaEvent myEvent = new MyAreaEvent("Area", distance - obs.distance, ObjectEvent, obs.initialArea, obs.finalArea);
                eventID = eventSystem.AddEvent(myEvent);
                obsEventList.Add(eventID, obs);
            }
            obstacle.transform.name = "#" + eventID + " Obstacle "+obs.obstacleType + obstacle.transform.name;

            obs.positionYourselfPlease(anchor);
            obs.rotateYourselfAroundYourOriginPlease(levelRot.eulerAngles);
            //Retornem les monedes que poden aparèixer a la següent secció com a les permeses per la secció i l'inversa de les que bloqueja l'obstacle.
            return sections[sectionId].coinSides & ~obstacles[obstacleId].coinsBloqued;
        }


        //Retornem les monedes que poden aparèixer a la següent secció com a les permeses per la secció ja que no hi ha obstacle.
        return sections[sectionId].coinSides;
    }
    private void GenerateCoins(int coinsNext, Vector3 section)
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
            finalCoinSide = possibleCoinSide[randomIndex]; // 1 = izquierda 2 = medio 2 = derecha 3 = izquierda flotante...
        }
        else
        {
            return;
        }

        nextCoinPos = 0;

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
                        coinPosition = new Vector3(-nextCoinPos, 1, -5.5f);
                        break;
                    case 1: // medio
                        coinPosition = new Vector3(-nextCoinPos, 1, -3.5f);
                        break;
                    case 2: // derecha
                        coinPosition = new Vector3(-nextCoinPos, 1, -1.5f);
                        break;
                    case 3: // izquierda flotante
                        if (i == 0) coinPosition = new Vector3(-nextCoinPos, 2, -5.5f);
                        else if (i == 1) coinPosition = new Vector3(-nextCoinPos, 2.8f, -5.5f);
                        else if (i == 2) coinPosition = new Vector3(-nextCoinPos, 3, -5.5f);
                        else if (i == 3) coinPosition = new Vector3(-nextCoinPos, 2.8f, -5.5f);
                        else coinPosition = new Vector3(-nextCoinPos, 2, -5.5f);
                        break;
                    case 4: // medio flotante
                        if (i == 0) coinPosition = new Vector3(-nextCoinPos, 2, -3.5f);
                        else if (i == 1) coinPosition = new Vector3(-nextCoinPos, 2.8f, -3.5f);
                        else if (i == 2) coinPosition = new Vector3(-nextCoinPos, 3, -3.5f);
                        else if (i == 3) coinPosition = new Vector3(-nextCoinPos, 2.8f, -3.5f);
                        else coinPosition = new Vector3(-nextCoinPos, 2, -3.5f);
                        break;
                    case 5: // derecha flotante
                        if (i == 0) coinPosition = new Vector3(-nextCoinPos, 2, -1.5f);
                        else if (i == 1) coinPosition = new Vector3(-nextCoinPos, 2.8f, -1.5f);
                        else if (i == 2) coinPosition = new Vector3(-nextCoinPos, 3, -1.5f);
                        else if (i == 3) coinPosition = new Vector3(-nextCoinPos, 2.8f, -1.5f);
                        else coinPosition = new Vector3(-nextCoinPos, 2, -1.5f);

                        break;
                }
                coin.transform.position = section + coinPosition;
                coin.transform.RotateAround(section, Vector3.up, levelRot.eulerAngles.y);
                //coin.transform.SetParent(sectionPos, true);
                MyHeightAreaEvent coinEve = new MyHeightAreaEvent("coinevent", distance + nextCoinPos, coinCollectorEv, -coinPosition.z - 0.8f, -coinPosition.z + 0.8f, coinPosition.y - 1.5f, coinPosition.y + 1.5f);
                uint eventID = eventSystem.AddEvent(coinEve);
                coin.transform.name = "#" + eventID + "Coin";
                coinEventList.Add(eventID, coin.GetComponent<Coin>());
                nextCoinPos += 1f; // distancia entre monedas
            }
        }
    }
    public void coinCollectorEv(uint coinId, bool success, MyEvent.checkResult result)
    {
        coinEventList[coinId].coinCollectorEvent(coinId, success);
        coinEventList.Remove(coinId);
    }
    private void OnValidate()
    {
        if (!Application.IsPlaying(gameObject))
        {
            // Editor logic
            foreach(var section in sections)
            {
                foreach (var nextSection in section.allowedSectionsAfter)
                {
                    if (nextSection.value.x >= sections.Length)
                    {
                        break;
                    }
                    nextSection.name = sections[nextSection.value.x].name;
                }
                foreach (var nextSection in section.allowedObstacles)
                {
                    if (nextSection.value.x >= obstacles.Length)
                    {
                        break;
                    }
                    nextSection.name = obstacles[nextSection.value.x].name;
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
        orchestrator = FindObjectOfType<Orchestrator>();
    }
}
