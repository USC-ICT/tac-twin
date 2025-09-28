using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ChrMdeMleAdult_General_AnmCtl : AnimatorAPI
{
    // State enumerations
    //-------------------------------------------------------------------------

    // Base state list
    public enum BaseStateList
    {
        Idle01 = 0,
        Idle02 = 1,
        Idle03 = 2,
        Idle05 = 4,
        Idle06 = 5,
        Idle07 = 6,
        Idle08 = 7,
        Idle09 = 8,
        Idle10 = 9,
        IdleCellPhone01 = 10,
        IdleConversation01 = 11,
    }
    
    // Idle state lists
    public enum Idle01_StateList
    {
        Idle01 = 0,
        BrowWipeRt01 = 1,
        HandsUp01 = 2, 
        HeadRaise01 = 3,
        HeadScratchLf01 = 4,
        HeadShake01 = 5,
        HeadTurnNodRt01 = 6,
        IndicateHereBt = 7,
        MeBt01 = 8,
        MeLf01 = 9,
        Shrug01 = 10,
    }
    
    public enum Idle02_StateList
    {
        Idle02 = 0,
        HeadShake01 = 1,
    }
    
    public enum Idle03_StateList
    {
        Idle03 = 0,
        AngryPoint01 = 1,
        Disgusted01 = 2,
        Disgusted02 = 3,
        FistRaiseRt01 = 4,
        HandBehindHeadLf01 = 5,
        HandHeadShakeLf01 = 6,
        HandsFaceBt01 = 7,
        HandsWtfBt01 = 8,
        HeadTurnRt01 = 9,
        LookingLoop01 = 10,
        PuffChest01 = 11,
    }
    
    public enum Idle05_StateList
    {
        Idle05 = 0,
    }
    
    public enum Idle06_StateList
    {
        Idle06 = 0,
    }
    
    public enum Idle07_StateList
    {
        Idle07 = 0,
    }
    
    public enum Idle08_StateList
    {
        Idle08 = 0,
    }
    
    public enum Idle09_StateList
    {
        Idle09 = 0,
    }
    
    public enum Idle10_StateList
    {
        Idle10 = 0,
    }
    
    public enum IdleCellPhone01_StateList
    {
        IdleCellPhone01 = 0,
    }
    
    public enum IdleConversation01_StateList
    {
        IdleConversation01 = 0,
    }
    
    // Variables
    //-------------------------------------------------------------------------
    
    public Animator AnimatorController; 

    private Dictionary<BaseStateList, AnimationState> m_stateMappingDictionary = new Dictionary<BaseStateList, AnimationState>()
    {
        { BaseStateList.Idle01, new AnimationState(typeof(Idle01_StateList), "Idle01State") },
        { BaseStateList.Idle02, new AnimationState(typeof(Idle02_StateList), "Idle02State") },
        { BaseStateList.Idle03, new AnimationState(typeof(Idle03_StateList), "Idle03State") },
        { BaseStateList.Idle05, new AnimationState(typeof(Idle05_StateList), "Idle05State") },
        { BaseStateList.Idle06, new AnimationState(typeof(Idle06_StateList), "Idle06State") },
        { BaseStateList.Idle07, new AnimationState(typeof(Idle07_StateList), "Idle07State") },
        { BaseStateList.Idle08, new AnimationState(typeof(Idle08_StateList), "Idle08State") },
        { BaseStateList.Idle09, new AnimationState(typeof(Idle09_StateList), "Idle09State") },
        { BaseStateList.Idle10, new AnimationState(typeof(Idle10_StateList), "Idle10State") },

        { BaseStateList.IdleCellPhone01,    new AnimationState(typeof(IdleCellPhone01_StateList),    "IdleCellPhone01State") },
        { BaseStateList.IdleConversation01, new AnimationState(typeof(IdleConversation01_StateList), "IdleConversation01State") },
    };
    
    // API functions
    //-------------------------------------------------------------------------

    public override void SetState(int baseState, int state) { AnimatorController.SetInteger("BaseState", baseState); AnimatorController.SetInteger(m_stateMappingDictionary[(BaseStateList)baseState].AnimationStateName, state); }

    public void SetState(Idle01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle01); AnimatorController.SetInteger("Idle01State", (int)state); }
    public void SetState(Idle02_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle02); AnimatorController.SetInteger("Idle02State", (int)state); }
    public void SetState(Idle03_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle03); AnimatorController.SetInteger("Idle03State", (int)state); }
    public void SetState(Idle05_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle05); AnimatorController.SetInteger("Idle05State", (int)state); }
    public void SetState(Idle06_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle06); AnimatorController.SetInteger("Idle06State", (int)state); }
    public void SetState(Idle07_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle07); AnimatorController.SetInteger("Idle07State", (int)state); }
    public void SetState(Idle08_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle08); AnimatorController.SetInteger("Idle08State", (int)state); }
    public void SetState(Idle09_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle09); AnimatorController.SetInteger("Idle09State", (int)state); }
    public void SetState(Idle10_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle10); AnimatorController.SetInteger("Idle10State", (int)state); }

    public void SetState(IdleCellPhone01_StateList state)    { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCellPhone01);    AnimatorController.SetInteger("IdleCellPhone01State",    (int)state); }
    public void SetState(IdleConversation01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleConversation01); AnimatorController.SetInteger("IdleConversation01State", (int)state); }

#if UNITY_EDITOR

    public BaseStateList    BaseState    = BaseStateList.Idle01;
    public Idle01_StateList Idle01_State = Idle01_StateList.Idle01;
    public Idle02_StateList Idle02_State = Idle02_StateList.Idle02;
    public Idle03_StateList Idle03_State = Idle03_StateList.Idle03;
    public Idle05_StateList Idle05_State = Idle05_StateList.Idle05;
    public Idle06_StateList Idle06_State = Idle06_StateList.Idle06;
    public Idle07_StateList Idle07_State = Idle07_StateList.Idle07;
    public Idle08_StateList Idle08_State = Idle08_StateList.Idle08;
    public Idle09_StateList Idle09_State = Idle09_StateList.Idle09;
    public Idle10_StateList Idle10_State = Idle10_StateList.Idle10;
    
    public IdleCellPhone01_StateList    IdleCellPhone01_State    = IdleCellPhone01_StateList.IdleCellPhone01;
    public IdleConversation01_StateList IdleConversation01_State = IdleConversation01_StateList.IdleConversation01;
    
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
            case BaseStateList.Idle02: SetState(Idle02_State); break;
            case BaseStateList.Idle03: SetState(Idle03_State); break;
            case BaseStateList.Idle05: SetState(Idle05_State); break;
            case BaseStateList.Idle06: SetState(Idle06_State); break;
            case BaseStateList.Idle07: SetState(Idle07_State); break;
            case BaseStateList.Idle08: SetState(Idle08_State); break;
            case BaseStateList.Idle09: SetState(Idle09_State); break;
            case BaseStateList.Idle10: SetState(Idle10_State); break;
            
            case BaseStateList.IdleCellPhone01:    SetState(IdleCellPhone01_State);    break;
            case BaseStateList.IdleConversation01: SetState(IdleConversation01_State); break;
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
