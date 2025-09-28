using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChrMdeFmlAdult_General_AnmCtl : AnimatorAPI
{
    // State enumerations
    //-------------------------------------------------------------------------

    // Base state list
    public enum BaseStateList
    {
        Idle01 = 0,
    }
    
    // Idle state lists
    public enum Idle01_StateList
    {
        Idle01 = 0,
    }
    
    // Variables
    //-------------------------------------------------------------------------
    
    public Animator AnimatorController; 

    private Dictionary<BaseStateList, AnimationState> m_stateMappingDictionary = new Dictionary<BaseStateList, AnimationState>()
    {
        { BaseStateList.Idle01, new AnimationState(typeof(Idle01_StateList), "Idle01State") },
    };

    // API functions
    //-------------------------------------------------------------------------

    public override void SetState(int baseState, int state) { AnimatorController.SetInteger("BaseState", baseState); AnimatorController.SetInteger(m_stateMappingDictionary[(BaseStateList)baseState].AnimationStateName, state); }

    public void SetState(Idle01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle01); AnimatorController.SetInteger("Idle01State", (int)state); }
    
#if UNITY_EDITOR
    
    public BaseStateList BaseState       = BaseStateList.Idle01;
    public Idle01_StateList Idle01_State = Idle01_StateList.Idle01;

    public bool RandomizeIdle = false;

    void Update()
    {
        if (Application.isPlaying) return;

        if (RandomizeIdle == true)
        {
            RandomizeBaseState();
        }
        
        switch (BaseState)
        {
            case BaseStateList.Idle01: SetState(Idle01_State); break;
        }
    }
    
    public void RandomizeBaseState()
    {
        int randomIdleIndex = UnityEngine.Random.Range(0, Enum.GetNames(typeof(BaseStateList)).Length);

        if (randomIdleIndex == 3)
        {
            randomIdleIndex = 4;
        }
        BaseState = (BaseStateList)randomIdleIndex;
        RandomizeIdle = false;
    }

#endif
}
