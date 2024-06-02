using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyEventSystem : MonoBehaviour
{
    public bool debug = true;
    public class EventDistanceComparer : IComparer<MyEvent>
    {
        public int Compare(MyEvent x, MyEvent y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }

    [SerializeField]
    private GameObject eventMapCam;
    [SerializeField]
    private GameObject eventMap;

    SortedSet<MyEvent> events = new SortedSet<MyEvent>(new EventDistanceComparer());
    private uint nextID = 1;
    private List<MyEvent> tickingEvents = new List<MyEvent>();
    private GameObject playerMarker;
    private GameObject levelMarker;

    private bool guard = false;
    
    private Dictionary<uint, GameObject> pilotesQueSonDeBones = new Dictionary<uint, GameObject>();

    private void Start()
    {
        if (debug)
        {
            eventMap.SetActive(true);
            eventMapCam.SetActive(true);
            playerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerMarker.name = "Player";
            playerMarker.GetComponent<Renderer>().material.color = Color.black;
            levelMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            levelMarker.name = "Level";
            levelMarker.GetComponent<Renderer>().material.color = Color.white;

            eventMapCam.transform.parent = playerMarker.transform;
            eventMapCam.transform.localPosition = new Vector3(0, 10, 0);
            eventMapCam.transform.LookAt(playerMarker.transform);
            eventMapCam.transform.localPosition = new Vector3(-10, 10, 0);
        }
        else
        {
            eventMap.SetActive(false);
            eventMapCam.SetActive(false);
        }
    }
    public void DebugLevelMarker(Vector3 pos)
    {
        if (debug && levelMarker != null)
        {
            levelMarker.transform.position = pos;
        }
    }
    public void IgnoreEvent(uint ID)
    {
        MyEvent eventToRemove = events.FirstOrDefault(e => e.ID == ID);
        if (eventToRemove != null)
        {
            events.Remove(eventToRemove);
        }
        tickingEvents.RemoveAll(e => e.ID == ID);
    }
    public uint AddEvent(MyEvent e)
    {
        while (guard)
        {            
            System.Threading.Thread.Sleep(1);
        }
        if (e.ID != 0)
        {
            if (e is MyQTEEvent)
            {
                MyQTEEvent copy = new(e as MyQTEEvent);
                e = copy;
            }
            else if (e is MyQTEAreaEvent)
            {
                MyQTEEvent copy = new(e as MyQTEEvent);
                e = copy;
            }
            else if (e is MyAreaEvent)
            {
                MyQTEEvent copy = new(e as MyQTEEvent);
                e = copy;
            }
            else
            {
                MyEvent copy = new(e);
                e = copy;
            }
            e.ID = nextId();
        }
        e.ID = nextId();
        float oldDistance = e.Distance;
        while (!events.Add(e))
        {
            e.Distance += 0.05f;
            if(e.Distance == oldDistance)
            {
                e.Distance += 0.1f;
            }
        }
        if (debug)
        {
            // Create a 3D marker at the distance marked by the event
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pilotesQueSonDeBones.Add(e.ID, marker);
            marker.transform.position = new Vector3(-e.Distance, 2f, 0f);
            marker.name = e.ID+"#Event: " + e.Name +" at "+e.Distance;
            if (e is MyQTEEvent)
            {
                marker.GetComponent<Renderer>().material.color = Color.red;
                marker.transform.position = new Vector3(-e.Distance, 2.5f,-0.5f);
            }
            else if (e is MyQTEAreaEvent)
            {
                marker.GetComponent<Renderer>().material.color = Color.blue;
                marker.transform.position = new Vector3(-e.Distance, 3f, -1.0f);
            }
            else if (e is MyAreaEvent)
            {
                MyAreaEvent myAreaEvent = (MyAreaEvent)e;
                marker.GetComponent<Renderer>().material.color = Color.green;
                marker.transform.position = new Vector3(-e.Distance, 3.5f, -myAreaEvent.initialAreaPos);
            }
            else if (e is MyHeightAreaEvent)
            {
                MyHeightAreaEvent myHeightAreaEvent = (MyHeightAreaEvent)e;
                marker.GetComponent<Renderer>().material.color = Color.magenta;
                marker.transform.position = new Vector3(-e.Distance, myHeightAreaEvent.initialHeight, -myHeightAreaEvent.initialAreaPos);
            }
            else
            {
                marker.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        return e.ID;
    }
    public void Restart()
    {
        events.Clear();
        tickingEvents.Clear();
        pilotesQueSonDeBones.Clear();
        nextID = 1;
    }
    public void UpdateTimes(float deltaTime, float distance)
    {
        foreach (MyEvent e in events)
        {
            if (e is MyQTEEvent)
            {
                MyQTEEvent qte = (MyQTEEvent)e;
                qte.updateRemainingTime(deltaTime, distance);
            }
            else if (e is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)e;
                qte.updateRemainingTime(deltaTime, distance);
            }
        }
        for (int i = 0; i < tickingEvents.Count; i++)
        {
            MyEvent next = tickingEvents[i];
            if (next is MyQTEEvent)
            {
                MyQTEEvent qte = (MyQTEEvent)next;
                qte.updateRemainingTime(deltaTime, distance);
            }
            else if (next is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)next;
                qte.updateRemainingTime(deltaTime, distance);
            }
        }
    }
    private uint nextId()
    {
        uint ID = nextID;
        nextID++;
        if (nextID >= uint.MaxValue)
        {
            nextID = 1;
        }
        return ID;
    }
    public void checkEvents(float distance, float lateral, float altura)
    {
        if (debug)
        {
            playerMarker.transform.position = new Vector3(-distance, altura, -lateral);
            eventMapCam.transform.position = new Vector3(eventMapCam.transform.position.x, eventMapCam.transform.position.y, -3);
            List<uint> mandonguilles = new List<uint>();
            foreach (var pilota in pilotesQueSonDeBones)
            {
                bool found = false;
                foreach (MyEvent nextEvent in tickingEvents)
                {
                    if (nextEvent.ID == pilota.Key)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    foreach (MyEvent eventItem in events)
                    {
                        if (eventItem.ID == pilota.Key)
                        {
                            found = true;
                        }
                    }
                }
                if (!found)
                {
                    Destroy(pilota.Value);
                    mandonguilles.Add(pilota.Key);
                }
            }
            foreach(uint mandonguilla in mandonguilles)
            {
                pilotesQueSonDeBones.Remove(mandonguilla);
            }
        }

        uint index;
        MyEvent.checkResult result = MyEvent.checkResult.Success;
        guard = true;
        List<MyEvent> _toRemove = new List<MyEvent>();
        foreach (MyEvent next in tickingEvents)
        {
            if (next is MyQTEEvent)
            {
                MyQTEEvent qte = (MyQTEEvent)next;
                result = qte.checkEvent(distance);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true,result);
                    _toRemove.Add(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false,result);
                    _toRemove.Add(next);
                }
            }
            else if (next is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)next;
                result = qte.checkEvent(distance, lateral,altura);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true, result);
                    _toRemove.Add(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false, result);
                    _toRemove.Add(next);
                }
                else if (result == MyEvent.checkResult.OutSide)
                {
                    qte.callBack(index, true, result);
                    _toRemove.Add(next);
                }
            }
        }
        guard = false;
        for (int i = _toRemove.Count - 1; i >= 0; i--)
        {
            tickingEvents.Remove(_toRemove[i]);
        }
        while (result != MyEvent.checkResult.NotYet && events.Count > 0)
        {
            MyEvent next = events.Min;
            
            if (next is MyAreaEvent)
            {
                MyAreaEvent area = (MyAreaEvent)next;
                result = area.checkEvent(distance, lateral);
                index = area.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    area.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    area.callBack(index, false, result);
                    events.Remove(next);
                }
            }
            else if (next is MyQTEEvent)
            {
                MyQTEEvent qte = (MyQTEEvent)next;
                result = qte.checkEvent(distance);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Ticking)
                {
                    tickingEvents.Add(next);
                    events.Remove(next);
                }
            }
            else if (next is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)next;
                result = qte.checkEvent(distance, lateral,altura);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.OutSide)
                {
                    qte.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Ticking)
                {
                    tickingEvents.Add(next);
                    events.Remove(next);
                }
            }
            else if (next is MyHeightAreaEvent)
            {
                MyHeightAreaEvent area = (MyHeightAreaEvent)next;
                result = area.checkEvent(distance, lateral, altura);
                index = area.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    area.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    area.callBack(index, false, result);
                    events.Remove(next);
                }
            }   
            else if (next is MyEvent)
            {
                result = next.checkEvent(distance);
                index = next.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    next.callBack(index, true, result);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    next.callBack(index, false, result);
                    events.Remove(next);
                }
            }
        }
    }
}
public class MyEvent
{
    public string Name;
    public uint ID;
    public enum checkResult
    {
        NotYet,
        Ticking,
        Success,
        OutSide,
        Fail
    };
    public float Distance;
    public Action<uint, bool, checkResult> callBack;
    public MyEvent(string name, float distance, Action<uint, bool,checkResult> callback)
    {
        Name = name;
        Distance = distance;
        callBack = callback;

    }
    public MyEvent(MyEvent e)
    {
        Name = e.Name;
        Distance = e.Distance;
        callBack = e.callBack;
    }
    public checkResult checkEvent(float distance)
    {
        if (distance >= Distance)
        {
            return checkResult.Success;
        }
        return checkResult.NotYet;
    }
    public void copyFrom(MyEvent e)
    {
        Name = e.Name;
        Distance = e.Distance;
        callBack = e.callBack;
    }
}
public class MyAreaEvent : MyEvent
{
    public float initialAreaPos;
    public float finalAreaPos;
    public MyAreaEvent(string name, float distance, Action<uint, bool,MyEvent.checkResult> callback, float initialAreaPos, float finalAreaPos) : base(name, distance, callback)
    {
        this.initialAreaPos = initialAreaPos;
        this.finalAreaPos = finalAreaPos;
    }
    public MyAreaEvent(MyAreaEvent e) : base(e)
    {
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
    }
    public new checkResult checkEvent(float distance, float areaPos)
    {
        if (distance >= Distance)
        {
            if (areaPos >= initialAreaPos && areaPos <= finalAreaPos)
            {
                return checkResult.Success;
            }
            return checkResult.Fail;
        }
        return checkResult.NotYet;
    }
    public void copyFrom(MyAreaEvent e)
    {
        base.copyFrom(e);
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
    }
}
public class MyHeightAreaEvent : MyEvent
{
    public float initialAreaPos;
    public float finalAreaPos;
    public float initialHeight;
    public float finalHeight;
    public MyHeightAreaEvent(string name, float distance, Action<uint, bool, MyEvent.checkResult> callback, float initialAreaPos, float finalAreaPos,float initialHeight, float finalHeight) : base(name, distance, callback)
    {
        this.initialAreaPos = initialAreaPos;
        this.finalAreaPos = finalAreaPos;
        this.initialHeight = initialHeight;
        this.finalHeight = finalHeight;
    }
    public MyHeightAreaEvent(MyHeightAreaEvent e) : base(e)
    {
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
        initialHeight = e.initialHeight;
        finalHeight = e.finalHeight;
    }
    public new checkResult checkEvent(float distance, float areaPos, float altura)
    {
        if (distance >= Distance)
        {
            if (areaPos >= initialAreaPos && areaPos <= finalAreaPos && altura >= initialHeight && altura <= finalHeight)
            {
                return checkResult.Success;
            }
            return checkResult.Fail;
        }
        return checkResult.NotYet;
    }
    public void copyFrom(MyHeightAreaEvent e)
    {
        base.copyFrom(e);
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
    }
}
public class MyQTEEvent : MyEvent
{
    private KeyCode key;
    private float remainingTime;
    public MyQTEEvent(string name, float distance, Action<uint, bool, MyEvent.checkResult> callback, KeyCode key, float timeGrace) : base(name, distance, callback)
    {
        this.key = key;
        this.remainingTime = timeGrace;
    }
    public MyQTEEvent(MyQTEEvent e) : base(e)
    {
        key = e.key;
        remainingTime = e.remainingTime;
    }
    public void updateRemainingTime(float deltaTime, float distance)
    {
        if (distance >= Distance && remainingTime > 0)
        {
            //Debug.Log("Remaining time: " + remainingTime);
            remainingTime -= deltaTime;
        }
    }
    public new checkResult checkEvent(float distance)
     {
        if (distance >= Distance)
        {
            bool b = Input.GetKey(key);
            if (b && remainingTime > 0)
            {
                return checkResult.Success;
            }
            if(remainingTime <= 0)
                return checkResult.Fail;
            else
                return checkResult.Ticking;
        }
        return checkResult.NotYet;
    }
    public void copyFrom(MyQTEEvent e)
    {
        base.copyFrom(e);
        key = e.key;
        remainingTime = e.remainingTime;
    }
}
public class MyQTEAreaEvent : MyEvent
{
    private KeyCode key;
    private float remainingTime;
    private float initialAreaPos;
    private float finalAreaPos;
    private float initialHeight;
    private float finalHeight;
    public MyQTEAreaEvent(string name, float distance, Action<uint, bool, MyEvent.checkResult> callback, KeyCode key, float remainingTime, float initialAreaPos, float finalAreaPos, float initialHeight, float finalHeight) : base(name, distance, callback)
    {
        this.key = key;
        this.remainingTime = remainingTime;
        this.initialAreaPos = initialAreaPos;
        this.finalAreaPos = finalAreaPos;
        this.initialHeight = initialHeight;
        this.finalHeight = finalHeight;
    }
    public MyQTEAreaEvent(MyQTEAreaEvent e) : base(e)
    {
        key = e.key;
        remainingTime = e.remainingTime;
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
    }
    public void updateRemainingTime(float deltaTime, float distance)
    {
        if (distance >= Distance && remainingTime > 0)
        {
            remainingTime -= deltaTime;
        }
    }
    public new checkResult checkEvent(float distance, float areaPos, float height)
    {
        if (distance >= Distance)
        {
            if (areaPos >= initialAreaPos && areaPos <= finalAreaPos && height >= initialHeight && height <= finalHeight)
            {
                if (Input.GetKey(key) && remainingTime > 0)
                {
                    return checkResult.Success;
                }
                if (remainingTime <= 0)
                    return checkResult.Fail;
                else
                    return checkResult.Ticking;
            }
            if (remainingTime <= 0)
                return checkResult.OutSide;
            else
                return checkResult.Ticking;
        }
        return checkResult.NotYet;
    }
    public void copyFrom(MyQTEAreaEvent e)
    {
        base.copyFrom(e);
        key = e.key;
        remainingTime = e.remainingTime;
        initialAreaPos = e.initialAreaPos;
        finalAreaPos = e.finalAreaPos;
    }
}



