using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChrUsaMleAdult_General_AnmCtl : AnimatorAPI
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
        IdleAim01 = 4,
        IdleBreak01 = 5,
        IdleCopilotSitting01 = 6,
        IdleCrouched01 = 7,
        IdleCrouchedAim01 = 8,
        IdleCrouchedAim02 = 9,
        IdleCrouchedHelping01 = 10,
        IdleCrouchedHelping02 = 11,
        IdleCrouchedHelping03 = 12,
        IdleLyingProne01 = 13,
        IdleLyingWounded01 = 14,
        IdleLyingWounded02 = 15,
        IdlePilotSitting01 = 16,
        IdleSitting01 = 17,
        IdleSittingBlackhawk01 = 18,
        IdleSittingBlackhawk02 = 19,
        IdleStanding01 = 20,
        IdleSteering01 = 21,
        IdleTurret01 = 22,
        IdleWoundedExplosion01 = 23,
        IdleWoundedExplosion02 = 24,
        TalkGurneyWounded01 = 25,
        WalkGurney01 = 26,
        WalkRifle01 = 27,
        RandomIdleState = 50,
    }
    
    // Idle state lists
    public enum Idle01StateList
    {
        Idle01 = 0,
        BrowWipeRt01 = 1,
        FollowMeLf01 = 2,
        HandSmackFaceLf01 = 3,
    }
    
    public enum Idle02StateList
    {
        Idle02 = 0,
    }
    
    public enum Idle03StateList
    {
        Idle03 = 0,
    }
    
    public enum Idle04StateList
    {
        Idle04 = 0,
    }
    
    public enum IdleAim01StateList
    {
        IdleAim01 = 0,
        ShootRifleBurst01 = 1,
    }
    
    public enum IdleBreak01StateList
    {
        IdleBreak01 = 0,
    }
    
    public enum IdleCopilotSitting01StateList
    {
        IdleCopilotSitting01 = 0,
    }
    
    public enum IdleCrouched01StateList
    {
        IdleCrouched01 = 0,
    }
    
    public enum IdleCrouchedAim01StateList
    {
        IdleCrouchedAim01 = 0,
        ShootRifle01 = 1,
        ShootRifle02 = 2, 
    }
    
    public enum IdleCrouchedAim02StateList
    {
        IdleCrouchedAim02 = 0,
        ShootRifle01 = 1,
        ShootRifle02 = 2, 
    }
    
    public enum IdleCrouchedHelping01StateList
    {
        IdleCrouchedHelping01 = 0,
    }
    
    public enum IdleCrouchedHelping02StateList
    {
        IdleCrouchedHelping02 = 0,
    }
    
    public enum IdleCrouchedHelping03StateList
    {
        IdleCrouchedHelping03 = 0,
    }
    
    public enum IdleLyingProne01StateList
    {
        IdleLyingProne01 = 0,
    }
    
    public enum IdleLyingWounded01StateList
    {
        IdleLyingWounded01 = 0,
    }
    
    public enum IdleLyingWounded02StateList
    {
        IdleLyingWounded02 = 0,
    }
    
    public enum IdlePilotSitting01StateList
    {
        IdlePilotSitting01 = 0,
    }
    
    public enum IdleSitting01StateList
    {
        IdleSitting01 = 0,
    }
    
    public enum IdleSittingBlackhawk01StateList
    {
        IdleSittingBlackhawk01 = 0,
    }
    
    public enum IdleSittingBlackhawk02StateList
    {
        IdleSittingBlackhawk02 = 0,
    }
   
    public enum IdleStanding01StateList
    {
        IdleStanding01 = 0,
        DoctorTalking01 = 1,
    }

    public enum IdleSteering01StateList
    {
        IdleSteering01 = 0,
        SteerLf01 = 1,
        SteerLf02 = 2,
        SteerRt01 = 3,
        SteerRt02 = 4,
    }
    
    public enum IdleTurret01StateList
    {
        IdleTurret01 = 0,
    }
    
    public enum IdleWoundedExplosion01StateList
    {
        IdleWoundedExplosion01 = 0,
    }
    
    public enum TalkGurneyWounded01StateList
    {
        TalkGurneyWounded01 = 0,
    }

    public enum IdleWoundedExplosion02StateList
    {
        IdleWoundedExplosion02 = 0,
    }
    
    public enum WalkGurney01StateList
    {
        Default = 0,
        WalkGurneyFront01 = 1,
        WalkGurneyRear01 = 2,
        WalkGurneySide01Rt = 3,
        WalkGurneySide01Lf = 4,
    }
    
    public enum WalkRifle01StateList
    {
        WalkRifle01 = 0,
    }

    // Random idle list
    public enum RandomIdleStateList
    {
        None = 0,
        ConversationTalking01 = 1,
        ConversationTalking02 = 2,
        ConversationTalking03 = 3,
        IdleBreak01 = 4,
        TalkGurneyWounded01 = 5,
        IdleCrouchedHelping01 = 10,
        IdleCrouchedHelping02 = 11,
        IdleCrouchedHelping03 = 12,
        IdleLyingWounded01 = 20,
        IdleLyingWounded02 = 21,
        IdlePilotSitting = 30,
        IdleCopilotSitting = 31,
    }
    
    // Variables
    //-------------------------------------------------------------------------
    
    public Animator AnimatorController;
    
    private Dictionary<BaseStateList, AnimationState> m_stateMappingDictionary = new Dictionary<BaseStateList, AnimationState>()
    {
        { BaseStateList.Idle01,                 new AnimationState(typeof(Idle01StateList ),                "Idle01State")                  },
        { BaseStateList.Idle02,                 new AnimationState(typeof(Idle02StateList ),                "Idle02State")                  },
        { BaseStateList.Idle03,                 new AnimationState(typeof(Idle03StateList ),                "Idle03State")                  },
        { BaseStateList.Idle04,                 new AnimationState(typeof(Idle04StateList ),                "Idle04State")                  },
        { BaseStateList.IdleAim01,              new AnimationState(typeof(IdleAim01StateList ),             "IdleAim01State")               },
        { BaseStateList.IdleBreak01,            new AnimationState(typeof(IdleBreak01StateList ),           "IdleBreak01State")             },
        { BaseStateList.IdleCopilotSitting01,   new AnimationState(typeof(IdleCopilotSitting01StateList ),  "IdleCopilotSitting01State")    },
        { BaseStateList.IdleCrouched01,         new AnimationState(typeof(IdleCrouched01StateList ),        "IdleCrouched01State")          },
        { BaseStateList.IdleCrouchedAim01,      new AnimationState(typeof(IdleCrouchedAim01StateList ),     "IdleCrouchedAim01State")       },
        { BaseStateList.IdleCrouchedAim02,      new AnimationState(typeof(IdleCrouchedAim02StateList ),     "IdleCrouchedAim02State")       },
        { BaseStateList.IdleCrouchedHelping01,  new AnimationState(typeof(IdleCrouchedHelping01StateList ), "IdleCrouchedHelping01State")   },
        { BaseStateList.IdleCrouchedHelping02,  new AnimationState(typeof(IdleCrouchedHelping02StateList ), "IdleCrouchedHelping02State")   },
        { BaseStateList.IdleCrouchedHelping03,  new AnimationState(typeof(IdleCrouchedHelping03StateList ), "IdleCrouchedHelping03State")   },
        { BaseStateList.IdleLyingProne01,       new AnimationState(typeof(IdleLyingProne01StateList ),      "IdleLyingProne01State")        },
        { BaseStateList.IdleLyingWounded01,     new AnimationState(typeof(IdleLyingWounded01StateList ),    "IdleLyingWounded01State")      },
        { BaseStateList.IdleLyingWounded02,     new AnimationState(typeof(IdleLyingWounded02StateList ),    "IdleLyingWounded02State")      },
        { BaseStateList.IdlePilotSitting01,     new AnimationState(typeof(IdlePilotSitting01StateList ),    "IdlePilotSitting01State")      },
        { BaseStateList.IdleSitting01,          new AnimationState(typeof(IdleSitting01StateList ),         "IdleSitting01State")           },
        { BaseStateList.IdleSittingBlackhawk01, new AnimationState(typeof(IdleSittingBlackhawk01StateList ),"IdleSittingBlackhawk01State")  },
        { BaseStateList.IdleSittingBlackhawk02, new AnimationState(typeof(IdleSittingBlackhawk02StateList ),"IdleSittingBlackhawk02State")  },
        { BaseStateList.IdleStanding01,         new AnimationState(typeof(IdleStanding01StateList),         "IdleStanding01State")          },
        { BaseStateList.IdleSteering01,         new AnimationState(typeof(IdleSteering01StateList ),        "IdleSteering01State")          },
        { BaseStateList.IdleTurret01,           new AnimationState(typeof(IdleTurret01StateList ),          "IdleTurret01State")            },
        { BaseStateList.IdleWoundedExplosion01, new AnimationState(typeof(IdleWoundedExplosion01StateList ),"IdleWoundedExplosion01State")  },
        { BaseStateList.IdleWoundedExplosion02, new AnimationState(typeof(IdleWoundedExplosion02StateList ),"IdleWoundedExplosion02State")  },
        { BaseStateList.TalkGurneyWounded01,    new AnimationState(typeof(TalkGurneyWounded01StateList ),   "TalkGurneyWounded01State")     },
        { BaseStateList.WalkGurney01,           new AnimationState(typeof(WalkGurney01StateList ),          "WalkGurney01State")            },
        { BaseStateList.WalkRifle01,            new AnimationState(typeof(WalkRifle01StateList ),           "WalkRifle01State")             },
        { BaseStateList.RandomIdleState,        new AnimationState(typeof(RandomIdleStateList ),            "RandomIdleState")                  },
    };
    
    // API functions
    //-------------------------------------------------------------------------

    public override void SetState(int baseState, int state) { AnimatorController.SetInteger("BaseState", baseState); AnimatorController.SetInteger(m_stateMappingDictionary[(BaseStateList)baseState].AnimationStateName, state); }
    public void SetState(Idle01StateList                    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle01);                 AnimatorController.SetInteger("Idle01State",                (int)state); }
    public void SetState(Idle02StateList                    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle02);                 AnimatorController.SetInteger("Idle02State",                (int)state); }
    public void SetState(Idle03StateList                    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle03);                 AnimatorController.SetInteger("Idle03State",                (int)state); }
    public void SetState(Idle04StateList                    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.Idle04);                 AnimatorController.SetInteger("Idle04State",                (int)state); }
    public void SetState(IdleAim01StateList                 state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleAim01);              AnimatorController.SetInteger("IdleAim01State",             (int)state); }
    public void SetState(IdleBreak01StateList               state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleBreak01);            AnimatorController.SetInteger("IdleBreak01State",           (int)state); }
    public void SetState(IdleCopilotSitting01StateList      state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCopilotSitting01);   AnimatorController.SetInteger("IdleCopilotSitting01State",  (int)state); }
    public void SetState(IdleCrouched01StateList            state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouched01);         AnimatorController.SetInteger("IdleCrouched01State",        (int)state); }
    public void SetState(IdleCrouchedAim01StateList         state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouchedAim01);      AnimatorController.SetInteger("IdleCrouchedAim01State",     (int)state); }
    public void SetState(IdleCrouchedAim02StateList         state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouchedAim02);      AnimatorController.SetInteger("IdleCrouchedAim02State",     (int)state); }
    public void SetState(IdleCrouchedHelping01StateList     state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouchedHelping01);  AnimatorController.SetInteger("IdleCrouchedHelping01State", (int)state); }
    public void SetState(IdleCrouchedHelping02StateList     state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouchedHelping02);  AnimatorController.SetInteger("IdleCrouchedHelping02State", (int)state); }
    public void SetState(IdleCrouchedHelping03StateList     state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleCrouchedHelping03);  AnimatorController.SetInteger("IdleCrouchedHelping03State", (int)state); }
    public void SetState(IdleLyingProne01StateList          state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleLyingProne01);       AnimatorController.SetInteger("IdleLyingProne01State",      (int)state); }
    public void SetState(IdleLyingWounded01StateList        state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleLyingWounded01);     AnimatorController.SetInteger("IdleLyingWounded01State",    (int)state); }
    public void SetState(IdleLyingWounded02StateList        state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleLyingWounded02);     AnimatorController.SetInteger("IdleLyingWounded02State",    (int)state); }
    public void SetState(IdlePilotSitting01StateList        state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdlePilotSitting01);     AnimatorController.SetInteger("IdlePilotSitting01State",    (int)state); }
    public void SetState(IdleSitting01StateList             state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleSitting01);          AnimatorController.SetInteger("IdleSitting01State",         (int)state); }
    public void SetState(IdleSittingBlackhawk01StateList    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleSittingBlackhawk01); AnimatorController.SetInteger("IdleSittingBlackhawk01State",(int)state); }
    public void SetState(IdleSittingBlackhawk02StateList    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleSittingBlackhawk02); AnimatorController.SetInteger("IdleSittingBlackhawk02State",(int)state); }
    public void SetState(IdleStanding01StateList            state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleStanding01);         AnimatorController.SetInteger("IdleStanding01State",        (int)state); }
    public void SetState(IdleSteering01StateList            state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleSteering01);         AnimatorController.SetInteger("IdleSteering01State",        (int)state); }
    public void SetState(IdleTurret01StateList              state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleTurret01);           AnimatorController.SetInteger("IdleTurret01State",          (int)state); }
    public void SetState(IdleWoundedExplosion01StateList    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleWoundedExplosion01); AnimatorController.SetInteger("IdleWoundedExplosion01State",(int)state); }
    public void SetState(IdleWoundedExplosion02StateList    state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.IdleWoundedExplosion02); AnimatorController.SetInteger("IdleWoundedExplosion02State",(int)state); }
    public void SetState(TalkGurneyWounded01StateList       state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.TalkGurneyWounded01);    AnimatorController.SetInteger("TalkGurneyWounded01State",   (int)state); }
    public void SetState(WalkGurney01StateList              state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.WalkGurney01);           AnimatorController.SetInteger("WalkGurney01State",          (int)state); }
    public void SetState(WalkRifle01StateList               state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.WalkRifle01);            AnimatorController.SetInteger("WalkRifle01State",           (int)state); }
    public void SetState(RandomIdleStateList                state) { AnimatorController.SetInteger("BaseState", (int)BaseStateList.RandomIdleState);        AnimatorController.SetInteger("RandomIdleState",            (int)state); }

#if UNITY_EDITOR

    public BaseStateList                    BaseState                       = BaseStateList.IdleStanding01 ;
    public Idle01StateList                  Idle01State                     = Idle01StateList.Idle01 ;
    public Idle02StateList                  Idle02State                     = Idle02StateList.Idle02 ;
    public Idle03StateList                  Idle03State                     = Idle03StateList.Idle03 ;
    public Idle04StateList                  Idle04State                     = Idle04StateList.Idle04 ;
    public IdleAim01StateList               IdleAim01State                  = IdleAim01StateList.IdleAim01 ;
    public IdleBreak01StateList             IdleBreak01State                = IdleBreak01StateList.IdleBreak01 ;
    public IdleCopilotSitting01StateList    IdleCopilotSitting01State       = IdleCopilotSitting01StateList.IdleCopilotSitting01 ;
    public IdleCrouched01StateList          IdleCrouched01State             = IdleCrouched01StateList.IdleCrouched01 ;
    public IdleCrouchedAim01StateList       IdleCrouchedAim01State          = IdleCrouchedAim01StateList.IdleCrouchedAim01 ;
    public IdleCrouchedAim02StateList       IdleCrouchedAim02State          = IdleCrouchedAim02StateList.IdleCrouchedAim02 ;
    public IdleCrouchedHelping01StateList   IdleCrouchedHelping01State      = IdleCrouchedHelping01StateList.IdleCrouchedHelping01 ;
    public IdleCrouchedHelping02StateList   IdleCrouchedHelping02State      = IdleCrouchedHelping02StateList.IdleCrouchedHelping02 ;
    public IdleCrouchedHelping03StateList   IdleCrouchedHelping03State      = IdleCrouchedHelping03StateList.IdleCrouchedHelping03 ;
    public IdleLyingProne01StateList        IdleLyingProne01State           = IdleLyingProne01StateList.IdleLyingProne01 ;
    public IdleLyingWounded01StateList      IdleLyingWounded01State         = IdleLyingWounded01StateList.IdleLyingWounded01 ;
    public IdleLyingWounded02StateList      IdleLyingWounded02State         = IdleLyingWounded02StateList.IdleLyingWounded02 ;
    public IdlePilotSitting01StateList      IdlePilotSitting01State         = IdlePilotSitting01StateList.IdlePilotSitting01 ;
    public IdleSitting01StateList           IdleSitting01State              = IdleSitting01StateList.IdleSitting01 ;
    public IdleSittingBlackhawk01StateList  IdleSittingBlackhawk01State     = IdleSittingBlackhawk01StateList.IdleSittingBlackhawk01 ;
    public IdleSittingBlackhawk02StateList  IdleSittingBlackhawk02State     = IdleSittingBlackhawk02StateList.IdleSittingBlackhawk02 ;
    public IdleStanding01StateList          IdleStanding01State             = IdleStanding01StateList.IdleStanding01 ;
    public IdleSteering01StateList          IdleSteering01State             = IdleSteering01StateList.IdleSteering01 ;
    public IdleTurret01StateList            IdleTurret01State               = IdleTurret01StateList.IdleTurret01 ;
    public IdleWoundedExplosion01StateList  IdleWoundedExplosion01State     = IdleWoundedExplosion01StateList.IdleWoundedExplosion01 ;
    public IdleWoundedExplosion02StateList  IdleWoundedExplosion02State     = IdleWoundedExplosion02StateList.IdleWoundedExplosion02 ;
    public TalkGurneyWounded01StateList     TalkGurneyWounded01State        = TalkGurneyWounded01StateList.TalkGurneyWounded01 ;
    public WalkGurney01StateList            WalkGurney01State               = WalkGurney01StateList.Default ;
    public WalkRifle01StateList             WalkRifle01State                = WalkRifle01StateList.WalkRifle01 ;
    public RandomIdleStateList              RandomIdleState                 = RandomIdleStateList.None ;
    
    // Update
    //-------------------------------------------------------------------------

    void Update()
    {
        if (Application.isPlaying) return;

        switch (BaseState)
        {
            
            case BaseStateList.Idle01:                  SetState(Idle01State);                  break;
            case BaseStateList.Idle02:                  SetState(Idle02State);                  break;
            case BaseStateList.Idle03:                  SetState(Idle03State);                  break;
            case BaseStateList.Idle04:                  SetState(Idle04State);                  break;
            case BaseStateList.IdleAim01:               SetState(IdleAim01State);               break;
            case BaseStateList.IdleBreak01:             SetState(IdleBreak01State);             break;
            case BaseStateList.IdleCopilotSitting01:    SetState(IdleCopilotSitting01State);    break;
            case BaseStateList.IdleCrouched01:          SetState(IdleCrouched01State);          break;
            case BaseStateList.IdleCrouchedAim01:       SetState(IdleCrouchedAim01State);       break;
            case BaseStateList.IdleCrouchedAim02:       SetState(IdleCrouchedAim02State);       break;
            case BaseStateList.IdleCrouchedHelping01:   SetState(IdleCrouchedHelping01State);   break;
            case BaseStateList.IdleCrouchedHelping02:   SetState(IdleCrouchedHelping02State);   break;
            case BaseStateList.IdleCrouchedHelping03:   SetState(IdleCrouchedHelping03State);   break;
            case BaseStateList.IdleLyingProne01:        SetState(IdleLyingProne01State);        break;
            case BaseStateList.IdleLyingWounded01:      SetState(IdleLyingWounded01State);      break;
            case BaseStateList.IdleLyingWounded02:      SetState(IdleLyingWounded02State);      break;
            case BaseStateList.IdlePilotSitting01:      SetState(IdlePilotSitting01State);      break;
            case BaseStateList.IdleSitting01:           SetState(IdleSitting01State);           break;
            case BaseStateList.IdleSittingBlackhawk01:  SetState(IdleSittingBlackhawk01State);  break;
            case BaseStateList.IdleSittingBlackhawk02:  SetState(IdleSittingBlackhawk02State);  break;
            case BaseStateList.IdleStanding01:          SetState(IdleStanding01State);          break;
            case BaseStateList.IdleSteering01:          SetState(IdleSteering01State);          break;
            case BaseStateList.IdleTurret01:            SetState(IdleTurret01State);            break;
            case BaseStateList.IdleWoundedExplosion01:  SetState(IdleWoundedExplosion01State);  break;
            case BaseStateList.IdleWoundedExplosion02:  SetState(IdleWoundedExplosion02State);  break;
            case BaseStateList.TalkGurneyWounded01:     SetState(TalkGurneyWounded01State);     break;
            case BaseStateList.WalkGurney01:            SetState(WalkGurney01State);            break;
            case BaseStateList.WalkRifle01:             SetState(WalkRifle01State);             break;
            case BaseStateList.RandomIdleState:         SetState(RandomIdleState);              break;
        }
    }

#endif
}
