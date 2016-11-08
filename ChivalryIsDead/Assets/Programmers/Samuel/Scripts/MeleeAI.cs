﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MeleeAI : MonsterAI
{

    [Header("Melee Specific Values")]
    public float chargeSpeedMultiplier = 3f;

    public float attackLength = 1f;
    public float attackAngleWidth = 0.6f;

    public float attackDamage = 5f;

    private float normSpeed;

    public override float GetBaseAttackDamage()
    {
        return attackDamage;
    }

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
        Collider[] Colliders = new Collider[0];
        Colliders = Physics.OverlapSphere(transform.position, attackLength);
        for (int i = 0; i < Colliders.Length; i++)
        {
            if (Colliders[i].tag == "Player")
            {
                Debug.Log("UH WEE");
                Vector3 vectorToCollider = (Colliders[i].transform.position - transform.position).normalized;
                Debug.Log(Vector3.Dot(vectorToCollider, transform.forward));
                if (Vector3.Dot(vectorToCollider, transform.forward) > attackAngleWidth)
                {
                    Rigidbody body = Colliders[i].transform.GetComponent<Rigidbody>();
                    if (body)
                        body.AddExplosionForce(100000, transform.position, attackLength);

                    //@@HARDCODED
                    base.targetObject.GetComponent<PlayerActionController>().PlayerAttacked(this);
                    Debug.Log("Hit player");
                }
            }
        }
        
        Debug.Log("Attacking");

    }

    public void Charge()
    {
        UpdateNavMeshPathDelayed();
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
        if (RangeCheckNavMesh())
            UpdateNavMeshPathDelayed();
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
        if (state == State.Charge)
            return;

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
        Debug.Log(name + "  Collided with something");
        if(state == State.Charge)
        {
            ChargeToAttack();
        }
    }

    public override void KillThis()
    {
        Debug.Log(transform.name + " : Has died");
        this.enabled = false;
    }
}