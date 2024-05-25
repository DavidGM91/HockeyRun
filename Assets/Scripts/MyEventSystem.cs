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

    SortedSet<MyEvent> events = new SortedSet<MyEvent>(new EventDistanceComparer());
    private uint nextID = 1;
    private List<MyEvent> tickingEvents = new List<MyEvent>();

    public void IgnoreEvent(uint ID)
    {
        MyEvent eventToRemove = events.FirstOrDefault(e => e.ID == ID);
        if (eventToRemove != null)
        {
            events.Remove(eventToRemove);
        }
    }

    public uint AddEvent(MyEvent e)
    {
        e.ID = nextId();
        events.Add(e);
        if (debug)
        {
            // Create a 3D marker at the distance marked by the event
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = new Vector3(-e.Distance, 2f, 0f);
            marker.name = "Event " + e.Name;
            if (e is MyQTEEvent)
            {
                marker.GetComponent<Renderer>().material.color = Color.red;
                marker.transform.position = new Vector3(-e.Distance, 2.5f, 1f);
            }
            else if (e is MyQTEAreaEvent)
            {
                marker.GetComponent<Renderer>().material.color = Color.blue;
                marker.transform.position = new Vector3(-e.Distance, 3f, 1f);
            }
            else if (e is MyAreaEvent)
            {
                marker.GetComponent<Renderer>().material.color = Color.green;
                marker.transform.position = new Vector3(-e.Distance, 3.5f, 1f);
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

    public void checkEvents(float distance, float pos)
    {
        uint index;
        MyEvent.checkResult result = MyEvent.checkResult.Success;
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
                    qte.callBack(index, true);
                    _toRemove.Add(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    _toRemove.Add(next);
                }
            }
            else if (next is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)next;
                result = qte.checkEvent(distance, pos);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    _toRemove.Add(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    _toRemove.Add(next);
                }
            }
        }
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
                result = area.checkEvent(distance, pos);
                index = area.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    area.callBack(index, true);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    area.callBack(index, false);
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
                    qte.callBack(index, true);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
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
                result = qte.checkEvent(distance, pos);
                index = qte.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Ticking)
                {
                    tickingEvents.Add(next);
                    events.Remove(next);
                }
            }
            else if (next is MyEvent)
            {
                result = next.checkEvent(distance);
                index = next.ID;
                if (result == MyEvent.checkResult.Success)
                {
                    next.callBack(index, true);
                    events.Remove(next);
                }
                else if (result == MyEvent.checkResult.Fail)
                {
                    next.callBack(index, false);
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
        Fail
    };
    public float Distance;
    public Action<uint, bool> callBack;
    public MyEvent(string name, float distance, Action<uint, bool> callback)
    {
        Name = name;
        Distance = distance;
        callBack = callback;

    }
    public checkResult checkEvent(float distance)
    {
        if (distance >= Distance)
        {
            return checkResult.Success;
        }
        return checkResult.NotYet;
    }
}
public class MyAreaEvent : MyEvent
{
    private float initialAreaPos;
    private float finalAreaPos;
    public MyAreaEvent(string name, float distance, Action<uint, bool> callback, float initialAreaPos, float finalAreaPos) : base(name, distance, callback)
    {
        this.initialAreaPos = initialAreaPos;
        this.finalAreaPos = finalAreaPos;
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
}
public class MyQTEEvent : MyEvent
{
    private KeyCode key;
    private float remainingTime;
    public MyQTEEvent(string name, float distance, Action<uint, bool> callback, KeyCode key, float timeGrace) : base(name, distance, callback)
    {
        this.key = key;
        this.remainingTime = timeGrace;
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
            if (Input.GetKey(key) && remainingTime > 0)
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
}
public class MyQTEAreaEvent : MyEvent
{
    private KeyCode key;
    private float remainingTime;
    private float initialAreaPos;
    private float finalAreaPos;
    public MyQTEAreaEvent(string name, float distance, Action<uint, bool> callback, KeyCode key, float remainingTime, float initialAreaPos, float finalAreaPos) : base(name, distance, callback)
    {
        this.key = key;
        this.remainingTime = remainingTime;
        this.initialAreaPos = initialAreaPos;
        this.finalAreaPos = finalAreaPos;
    }
    public void updateRemainingTime(float deltaTime, float distance)
    {
        if (distance >= Distance && remainingTime > 0)
        {
            remainingTime -= deltaTime;
        }
    }
    public new checkResult checkEvent(float distance, float areaPos)
    {
        if (distance >= Distance)
        {
            if (areaPos >= initialAreaPos && areaPos <= finalAreaPos)
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
                return checkResult.Fail;
            else
                return checkResult.Ticking;
        }
        return checkResult.NotYet;
    }
}


