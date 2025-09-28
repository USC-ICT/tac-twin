using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride;
using Ride.IO;
using VHAssets;

public class VHSpeechTrigger : RideMonoBehaviour
{
    public ICharacter m_character;

    //float m_greetingResetTime = 5;  // in seconds, when to reset the character to greet again
    float m_greetingStartTime = 0;


    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsCorrectTarget(other))
        {
            //Debug.LogFormat("OnTriggerEnter() - {0}", other.gameObject.name);

            var vhDemo = GameObject.FindAnyObjectByType<VHDemo>();

#if false
            if (Time.time - m_greetingStartTime > m_greetingResetTime)
                vhDemo.ResetGreeting();
            else
                m_greetingStartTime = Time.time;

            vhDemo.PlayGreeting(other.gameObject);
#endif
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsCorrectTarget(other))
        {
            //Debug.LogFormat("OnTriggerExit() - {0}", other.gameObject.name);

            m_greetingStartTime = Time.time;
        }
    }

    bool IsCorrectTarget(Collider other)
    {
        //if (other.gameObject.GetComponent<Camera>() != null ||
        //    (other.gameObject.transform.parent != null &&
        //     other.gameObject.transform.parent.GetComponent<PlayerInputControllerDataMono>() != null))
        //    return true;

        return false;
    }
}
