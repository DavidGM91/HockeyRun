using System;
using System.Collections.Generic;
using UnityEngine;

public class MyEventSystem : MonoBehaviour
{
    [SerializeField]
    private Queue<MyEvent> events = new Queue<MyEvent>();
    private uint nextID = 1;
    private List<uint> ignoredEvents = new List<uint>();

    public void IgnoreEvent(uint ID)
    {
        ignoredEvents.Add(ID);
    }
    public uint AddEvent(MyEvent e)
    {
        e.ID = nextId();
        events.Enqueue(e);
        return e.ID;
    }
    public void UpdateTimes(float deltaTime, float distance)
    {
        foreach (MyEvent e in events)
        {
            if(ignoredEvents.Contains(e.ID))
            {
                continue;
            }
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
    }
    private uint nextId()
    {
        uint ID = nextID;
        nextID++;
        if (nextID > uint.MaxValue)
        {
            nextID = 1;
        }
        return ID;
    }

    public void checkEvents(float distance, float pos)
    {
        uint index;
        MyEvent.checkResult result = MyEvent.checkResult.Success;
        while (result != MyEvent.checkResult.NotYet)
        {
            MyEvent next = events.Peek();
            if(ignoredEvents.Contains(next.ID))
            {
                events.Dequeue();
                continue;
            }
            //Debug.Log("Checking event: " + next.ID +"at "+next.Distance + " vs "+ distance);
            if (next is MyEvent)
            {
                result = next.checkEvent(distance);
                index = next.ID;
                if (result == MyAreaEvent.checkResult.Success)
                {
                    next.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == MyAreaEvent.checkResult.Fail)
                {
                    next.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is MyAreaEvent)
            {
                MyAreaEvent area = (MyAreaEvent)next;
                result = area.checkEvent(distance, pos);
                index = area.ID;
                if (result == MyAreaEvent.checkResult.Success)
                {
                    area.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == MyAreaEvent.checkResult.Fail)
                {
                    area.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is MyQTEEvent)
            {
                MyQTEEvent qte = (MyQTEEvent)next;
                result = qte.checkEvent(distance);
                index = qte.ID;
                if (result == MyQTEEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == MyQTEEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is MyQTEAreaEvent)
            {
                MyQTEAreaEvent qte = (MyQTEAreaEvent)next;
                result = qte.checkEvent(distance, pos);
                index = qte.ID;
                if (result == MyQTEAreaEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == MyQTEAreaEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    events.Dequeue();
                }
            }
        }
    }
}


public class MyEvent
{
    protected string Name;
    public uint ID;
    public enum checkResult
    {
        NotYet,
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
            remainingTime -= deltaTime;
        }
    }
    public new checkResult checkEvent(float distance)
    {
        if (distance >= Distance)
        {
            if (Input.GetKeyDown(key) && remainingTime > 0)
            {
                return checkResult.Success;
            }
            return checkResult.Fail;
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
                if (Input.GetKeyDown(key) && remainingTime > 0)
                {
                    return checkResult.Success;
                }
                return checkResult.Fail;
            }
            return checkResult.Fail;
        }
        return checkResult.NotYet;
    }
}


