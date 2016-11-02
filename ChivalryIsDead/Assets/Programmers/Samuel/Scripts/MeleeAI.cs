﻿using UnityEngine;
using System.Collections;
using System;

public class MeleeAI : MonsterAI
{

    [Header("Melee Specific Values")]
    public float chargeSpeedMultiplier = 3f;

    private float normSpeed;

    public override void Init()
    {
        normSpeed = agent.speed;
    }

    public override void Attack()
    {
        rotateTowardsTarget();
        if (t1 > attackTime)
        {
            if (RangeCheck())
            {
                AttackToMove();
                return;
            }

            MeleeAttack();
            ResetTimer();
        }
    }

    public void MeleeAttack()
    {

        //Physi

        //Vector3 vectorToCollider = (collider.transform.position - player.transform.position).Normalize();
        // 180 degree arc, change 0 to 0.5 for a 90 degree "pie"
        //if (Vector3.Dot(vectorToCollider, player.transform.forward)) > 0)
        //{
            //Damage the enemy
        //}
        Debug.Log("Attacking");

    }

    public void Charge()
    {
        GotoNextPoint();
    }

    public override void Idle()
    {

        if (agent.hasPath)
        {
            IdleToMove();
        }

    }

    public override void Move()
    {
        bool b = RangeCheckNavMesh();
        if (b)
            GotoNextPoint();
        else
            MoveToAttack();
    }

    public override void Taunt()
    {
        Debug.Log("TAUNT");
        ToCharge();
    }

    public void ToCharge()
    {
        Debug.Log("ToCharge");
        agent.speed = normSpeed * chargeSpeedMultiplier;
        state = State.Charge;
        stateFunc = Charge;
    }

    public void ChargeToAttack()
    {
        Debug.Log("ChargeToAttack");
        StopNavMeshAgent();
        agent.speed = normSpeed;
        agent.velocity = Vector3.zero;
        state = State.Attack;
        stateFunc = Attack;
    }

    void OnCollisionEnter()
    {
        if(state == State.Charge)
        {
            ChargeToAttack();
        }
    }
}