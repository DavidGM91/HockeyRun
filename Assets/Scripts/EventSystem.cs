using System;
using System.Collections.Generic;
using UnityEngine;

public class Event
{
    protected string Name;
    public uint ID;
    public enum checkResult
    {
        NotYet,
        Success,
        Fail
    };
    protected float Distance;
    public Action<uint, bool> callBack;
    public Event(string name, float distance, Action<uint,bool> callback)
    {
        Name = name;
        Distance = distance;
        callBack = callback;

    }
    public checkResult checkEvent(float distance)
    {
        if(distance >= Distance)
        {
            return checkResult.Success;
        }
        return checkResult.NotYet;
    }
}
public class AreaEvent : Event
{
    private float initialAreaPos;
    private float finalAreaPos;
    public AreaEvent(string name, float distance, Action<uint, bool> callback, float initialAreaPos, float finalAreaPos) : base(name, distance, callback)
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
public class QTEEvent : Event
{
    private KeyCode key;
    private float remainingTime;
    public QTEEvent(string name, float distance, Action<uint, bool> callback, float timeGrace) : base(name, distance, callback)
    {
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
public class QTEAreaEvent : Event
{
    private KeyCode key;
    private float remainingTime;
    private float initialAreaPos;
    private float finalAreaPos;
    public QTEAreaEvent(string name, float distance, Action<uint, bool> callback, float remainingTime, float initialAreaPos, float finalAreaPos) : base(name, distance,callback)
    {
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


public class EventSystem : MonoBehaviour
{
    private Queue<Event> events = new Queue<Event>();
    private uint nextID = 0;

    public uint AddEvent(Event e)
    {
        e.ID = nextId();
        events.Enqueue(e);
        return e.ID;
    }
    public void UpdateTimes(float deltaTime, float distance)
    {
        foreach (Event e in events)
        {
            if (e is QTEEvent)
            {
                QTEEvent qte = (QTEEvent)e;
                qte.updateRemainingTime(deltaTime, distance);
            }
            else if (e is QTEAreaEvent)
            {
                QTEAreaEvent qte = (QTEAreaEvent)e;
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
            nextID = 0;
        }
        return ID;
    }

    public void checkEvents(float distance, float pos)
    {
        uint index;
        Event next = events.Peek();
        Event.checkResult result = Event.checkResult.Success;
        while (result != Event.checkResult.NotYet)
        {
            if (next is Event)
            {
                result = next.checkEvent(distance);
                index = next.ID;
                if (result == AreaEvent.checkResult.Success)
                {
                    next.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == AreaEvent.checkResult.Fail)
                {
                    next.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is AreaEvent)
            {
                AreaEvent area = (AreaEvent)next;
                result = area.checkEvent(distance, pos);
                index = area.ID;
                if (result == AreaEvent.checkResult.Success)
                {
                    area.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == AreaEvent.checkResult.Fail)
                {
                    area.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is QTEEvent)
            {
                QTEEvent qte = (QTEEvent)next;
                result = qte.checkEvent(distance);
                index = qte.ID;
                if (result == QTEEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == QTEEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    events.Dequeue();
                }
            }
            else if (next is QTEAreaEvent)
            {
                QTEAreaEvent qte = (QTEAreaEvent)next;
                result = qte.checkEvent(distance, pos);
                index = qte.ID;
                if (result == QTEAreaEvent.checkResult.Success)
                {
                    qte.callBack(index, true);
                    events.Dequeue();
                }
                else if (result == QTEAreaEvent.checkResult.Fail)
                {
                    qte.callBack(index, false);
                    events.Dequeue();
                }
            }
        }
    }
}
