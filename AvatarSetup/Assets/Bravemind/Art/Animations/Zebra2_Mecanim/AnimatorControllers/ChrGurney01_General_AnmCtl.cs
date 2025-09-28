using UnityEngine;
using System.Collections;

public class ChrGurney01_General_AnmCtl : AnimatorAPI
{

    // Variables
    //-------------------------------------------------------------------------
    
    public Animator AnimatorController;
    
    // API functions
    //-------------------------------------------------------------------------

    public override void StartMoving() { AnimatorController.SetBool("Rolling", true); }
    public override void StopMoving()  { AnimatorController.SetBool("Rolling", false); }
    
#if UNITY_EDITOR
    
    public bool Rolling = false ; 
    
    // Update
    //--------------------------------------------------------------------------
    void Update()
    {
        if (Application.isPlaying) return;

        // Set animator controller inputs
        if (Rolling == true) StartMoving();
        else                 StopMoving();
    }

#endif
}
