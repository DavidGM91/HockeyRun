using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleColisio : SpawnObstacle
{
    public override LevelGenerator.ObjectActionOnPlayer OnEvent(uint id, bool succes, MyEvent.checkResult checkResult)
    {
        switch (checkResult)
        {
            case MyEvent.checkResult.Success:
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.StartPlayback();
                }
                return LevelGenerator.ObjectActionOnPlayer.Kill;
                break;
            case MyEvent.checkResult.Fail:
                break;
            case MyEvent.checkResult.OutSide:
                break;
        }
        StartCoroutine(DestroyAfterTime(2));
        return LevelGenerator.ObjectActionOnPlayer.None;
    }
}