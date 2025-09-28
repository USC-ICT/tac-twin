using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChrMdeMleChild_General_AnmCtl : AnimatorAPI
{
    // State enumerations
    //-------------------------------------------------------------------------

    // Base state list
    public enum BaseStateList
    {
        Idle01 = 0,
        Idle02 = 1,
        Idle03 = 2,
        Idle04 = 3,
        MarketIED_WaveAtMarines01 = 4,
        Run01 = 5,
        Run02 = 6,
        Walk01 = 7,
        Walk02 = 8,
    }
    
    // Idle state lists
    public enum Idle01_StateList
    {
        Idle01 = 0,
        HeadTurnLf02 = 1, 
        HeadTurnRt02 = 2, 
    }
    
    public enum Idle02_StateList
    {
        Idle02 = 0,
    }
    
    public enum Idle03_StateList
    {
        Idle03 = 0,
    }
    
    public enum Idle04_StateList
    {
        Idle04 = 0,
        Wave01 = 1, 
    }
    
    public enum MarketIED_WaveAtMarines01_StateList
    {
        MarketIED_WaveAtMarines01 = 0,
    }
    
    public enum Run01_StateList
    {
        Run01 = 0,
    }
    
    public enum Run02_StateList
    {
        Run02 = 0,
    }
    
    public enum Walk01_StateList
    {
        Walk01 = 0,
    }
    
    public enum Walk02_StateList
    {
        Walk02 = 0,
    }
    
    // Variables
    //-------------------------------------------------------------------------
    
    public Animator AnimatorController; 

    private Dictionary<BaseStateList, AnimationState> m_stateMappingDictionary = new Dictionary<BaseStateList, AnimationState>()
    {
        { BaseStateList.Idle01, new AnimationState(typeof(Idle01_StateList), "Idle01State") },
        { BaseStateList.Idle02, new AnimationState(typeof(Idle02_StateList), "Idle02State") },
        { BaseStateList.Idle03, new AnimationState(typeof(Idle03_StateList), "Idle03State") },
        { BaseStateList.Idle04, new AnimationState(typeof(Idle04_StateList), "Idle04State") },
        { BaseStateList.MarketIED_WaveAtMarines01, new AnimationState(typeof(MarketIED_WaveAtMarines01_StateList), "MarketIED_WaveAtMarines01State") },
        { BaseStateList.Run01,  new AnimationState(typeof(Run01_StateList),  "Run01State")  },
        { BaseStateList.Run02,  new AnimationState(typeof(Run02_StateList),  "Run02State")  },
        { BaseStateList.Walk01, new AnimationState(typeof(Walk01_StateList), "Walk01State") },
        { BaseStateList.Walk02, new AnimationState(typeof(Walk02_StateList), "Walk02State") },
    };
    
    // API functions
    //-------------------------------------------------------------------------

    public override void SetState(int baseState, int state) { AnimatorController.SetInteger("BaseState", baseState); AnimatorController.SetInteger(m_stateMappingDictionary[(BaseStateList)baseState].AnimationStateName, state); }

    public void SetState(Idle01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle01); AnimatorController.SetInteger("Idle01State", (int)state); }
    public void SetState(Idle02_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle02); AnimatorController.SetInteger("Idle02State", (int)state); }
    public void SetState(Idle03_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle03); AnimatorController.SetInteger("Idle03State", (int)state); }
    public void SetState(Idle04_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle04); AnimatorController.SetInteger("Idle04State", (int)state); }
    public void SetState(MarketIED_WaveAtMarines01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.MarketIED_WaveAtMarines01); AnimatorController.SetInteger("MarketIED_WaveAtMarines01State",  (int)state); }
    public void SetState(Run01_StateList  state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Run01);  AnimatorController.SetInteger("Run01State",  (int)state); }
    public void SetState(Run02_StateList  state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Run02);  AnimatorController.SetInteger("Run02State",  (int)state); }
    public void SetState(Walk01_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Walk01); AnimatorController.SetInteger("Walk01State", (int)state); }
    public void SetState(Walk02_StateList state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Walk02); AnimatorController.SetInteger("Walk02State", (int)state); }

#if UNITY_EDITOR

    public BaseStateList    BaseState    = BaseStateList.Idle01;
    public Idle01_StateList Idle01_State = Idle01_StateList.Idle01;
    public Idle02_StateList Idle02_State = Idle02_StateList.Idle02;
    public Idle03_StateList Idle03_State = Idle03_StateList.Idle03;
    public Idle04_StateList Idle04_State = Idle04_StateList.Idle04;
    public MarketIED_WaveAtMarines01_StateList MarketIED_WaveAtMarines01_State = MarketIED_WaveAtMarines01_StateList.MarketIED_WaveAtMarines01;
    public Run01_StateList  Run01_State  = Run01_StateList.Run01;
    public Run02_StateList  Run02_State  = Run02_StateList.Run02;
    public Walk01_StateList Walk01_State = Walk01_StateList.Walk01;
    public Walk02_StateList Walk02_State = Walk02_StateList.Walk02;

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
            case BaseStateList.Idle04: SetState(Idle04_State); break;
            case BaseStateList.MarketIED_WaveAtMarines01: SetState(MarketIED_WaveAtMarines01_State); break;
            case BaseStateList.Run01:  SetState(Run01_State); break;
            case BaseStateList.Run02:  SetState(Run02_State); break;
            case BaseStateList.Walk01: SetState(Walk01_State); break;
            case BaseStateList.Walk02: SetState(Walk02_State); break;
        }
    }
    
    public void RandomizeBaseState()
    {
        int randomIdleIndex = UnityEngine.Random.Range(0, Enum.GetNames(typeof(BaseStateList)).Length - 4);
        BaseState = (BaseStateList)randomIdleIndex;
        RandomizeIdle = false;
    }

#endif
}
