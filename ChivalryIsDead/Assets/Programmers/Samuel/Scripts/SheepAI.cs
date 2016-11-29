﻿using UnityEngine;
using System.Collections;
using System;

public class SheepAI : MonsterAI {

    [Header("Sheep Specific Variables")]
    public float aggroTime = 10;

    public override void Attack() { }

    public override void Idle()
    {
        if (aggroed)
        {
            targetPoint = -targetObject.forward * 2 + targetObject.position;
            if (RangeCheck())
                IdleToMove();

            aggroTimerCheck();
        }
        else
        {
            rotateTowardsTarget(targetObject.position);
        }
    }

    private bool aggroTimerCheck()
    {
        if (t1 > aggroTime)
        {
            aggroed = false;
            return true;
        }
        return false;
    }

    public override void Init()
    {
        //Set the state of the sheep to patrolling, this will make the sheep go to a point
        //instead of a target
        patrolling = true;
        //Set the sheep to not aggro when in aggro range
        aggro = false;
    }

    public override void KillThis() { }

    public override void Move()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        targetPoint = -targetObject.forward * 2 + targetObject.position;
        if (RangeCheckNavMesh())
            UpdateNavMeshPathDelayed();
        else
            MoveToIdle();

        if (aggroTimerCheck())
            MoveToIdle();
    }

    public override void Taunt()
    {
        if (state == State.Death)
            return;

        Debug.Log("Sheep taunted");
        if(state != State.Move)
            ToMove();

        aggroed = true;
        ResetTimer();
    }

    public void MoveToIdle()
    {
        //Debug.Log("MoveToIdle");
        StopNavMeshAgent();
        state = State.Idle;
        stateFunc = Idle;
        anim.SetFloat("Speed", 0);
    }

    public override void Utility() {}

    public override void EnterUtilityState()
    {
        ToIdle();
    }

    public override int GetAttackReputation()
    {
        throw new NotImplementedException();
    }

    public override int GetObjectiveAttackReputation()
    {
        throw new NotImplementedException();
    }

    public override void MoveEvent()
    {
        //Called every time AI goes into move state
    }

    public override void HitThis() { }

    public override void Turn() { }
}
