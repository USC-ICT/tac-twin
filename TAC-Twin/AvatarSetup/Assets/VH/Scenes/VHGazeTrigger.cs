using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride;
using Ride.IO;
using VHAssets;

public class VHGazeTrigger : RideMonoBehaviour
{
    public ICharacter m_character;
    public float m_stopGazeTime = 2.0f;

    float m_stopGazeStartTime = 0;

    public event System.Action<ICharacter> onGazeTriggered;

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

            //m_character.Gaze(other.gameObject.name);
            //((MecanimCharacter)m_character).SetGazeWeights(0.5);
            StartCoroutine(GazeAtObject(other));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsCorrectTarget(other))
        {
            //Debug.LogFormat("OnTriggerExit() - {0}", other.gameObject.name);

            m_character.StopGaze(m_stopGazeTime);
            m_stopGazeStartTime = Time.time;
        }
    }

    IEnumerator GazeAtObject(Collider other)
    {
        // sometimes if you go in/out of the trigger zone too quickly, it doesn't gaze because StopGaze is still running.
        // in this case, we wait until it's finished
        // no ICharacter api allows you to check, so we monitor it ourselves

        while (Time.time - m_stopGazeStartTime < m_stopGazeTime + 0.1f)
        {
            yield return new WaitForEndOfFrame();
        }

        ((MecanimCharacter)m_character).SetGazeTargetWithSpeed(other.gameObject, 100, 200, 100);
        onGazeTriggered?.Invoke(m_character);
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
