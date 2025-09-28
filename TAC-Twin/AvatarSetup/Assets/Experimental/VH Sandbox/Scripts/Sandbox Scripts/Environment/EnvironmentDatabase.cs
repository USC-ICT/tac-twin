using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for environments available for the VH Sandbox
/// </summary>
public class EnvironmentDatabase : ScriptableObject
{
    [System.Serializable]
    public struct EnvironmentData
    {
        //public TerrainSO terrainSO;
        public Vector3 loadPosition;
    }

    [SerializeField] EnvironmentData[] m_environments;


    public EnvironmentData[] Environments => m_environments;

    public EnvironmentData this[int index] => m_environments[index];

}
