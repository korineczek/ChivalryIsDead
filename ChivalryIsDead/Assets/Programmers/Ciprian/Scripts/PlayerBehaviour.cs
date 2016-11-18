﻿using System;
using System.Collections.Generic;
using UnityEngine;

class PlayerBehaviour : ScorePublisher
{

    public int ScoreChange { get; set; }
    public string ScoreHandle { get; set; }

    private DummyManager dummyManager;

    public GameObject RepGainParticle;
    public GameObject RepLossParticle;

    public PlayerBehaviour(string handle)
    {
        dummyManager = DummyManager.dummyManager;

        switch (handle)
        {
            case "rep":
                dummyManager.ReputationHandler.Subscribe(this); break;
            case "susp":
                Debug.Log("NOT SUPPORTED"); break;
            case "days":
                Debug.Log("NOT SUPPORTED"); break;
            default:
                break;
        }
    }

    public void ChangeRepScore(int score)
    {
        
        if (score < 0)
        {
            RepLossParticle.SetActive(false);

            ScoreChange = dummyManager.GetComboMultiplier(score);
            
            //@@HARDCODED 
            if (dummyManager.GetComboValue() > 8)
                WwiseInterface.Instance.PlayKnightCombatSound(KnightCombatHandle.LoseRepCombo, RepLossParticle);
            else if ( (ScoreChange * -1)  > 500)
                WwiseInterface.Instance.PlayKnightCombatSound(KnightCombatHandle.LoseRepBig, RepLossParticle);
            else
                WwiseInterface.Instance.PlayKnightCombatSound(KnightCombatHandle.LoseRepSmall, RepLossParticle);

            //increase combo
            dummyManager.IncreaseCombo();
            //reset cooldown
            dummyManager.resetCooldown();

            //particle effect
            RepLossParticle.SetActive(true);
        }
        else
        {
            RepGainParticle.SetActive(false);
            //particle effect
            RepGainParticle.SetActive(true);
            Reset();

            WwiseInterface.Instance.PlayKnightCombatSound(KnightCombatHandle.GainRep, RepGainParticle);
            ScoreChange = dummyManager.GetComboMultiplier(score);
        }
    }
    
    //reset combo
    public void Reset()
    {
        dummyManager.ResetCombo();
    }



    public void Invoke()
    {
        dummyManager.ActionPerformed();

        OnChangeScoreEvent(new ScoreEventArgs(ScoreChange));
    }




}
