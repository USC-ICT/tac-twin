using System.Collections.Generic;
using UnityEngine;

public class BloodDecalController : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_bloodDecals = new(); //order is light, moderate, severe
    private int m_currentlyActiveDecal = 0;

    public void SetActiveDecal(int index)
    {
        foreach(var decal in m_bloodDecals)
            decal.SetActive(false);
        m_currentlyActiveDecal = index;
        m_bloodDecals[m_currentlyActiveDecal].SetActive(true);
    }
}
