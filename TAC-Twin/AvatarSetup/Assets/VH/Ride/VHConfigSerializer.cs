using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ride.VirtualHumans
{
    /// <summary>
    /// Handles JSON serilization/deserialization of VH configs
    /// </summary>
    [System.Serializable]
    public class VHConfigSerializer
    {
        public const string FILE_NAME = "VHConfigData.json";

        public readonly string savePath;

        [System.Serializable]
        public struct VHSystemID
        {
            public string systemType;
            public string systemName;
        }

        [System.Serializable]
        public struct VirtualHumanConfig
        {
            public string vhName;
            public string modelTemplate;
            public Ride.RideVector3 position;
            public Ride.RideVector3 forward;

            public List<VHSystemID> systemIDs;
            public int voiceIndex;
        }

        [System.Serializable]
        public class VirtualHumanConfigData
        {
            public List<VirtualHumanConfig> configData = new List<VirtualHumanConfig>();

            public void Add(VirtualHumanConfig config) => configData.Add(config);
            public void Remove(VirtualHumanConfig config) => configData.Remove(config);

            public int Count => configData.Count;

            public VirtualHumanConfig this[int index]
            {
                get => configData[index];
                set => configData[index] = value;
            }

        }


#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        string m_serializedJSON;

#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        VirtualHumanConfigData m_data;

        Dictionary<string, int> m_lookup = new Dictionary<string, int>();

        public VHConfigSerializer(string savePath = "")
        {
            if (savePath != "")
            {
                this.savePath = savePath;
                return;
            }

            this.savePath = Application.persistentDataPath + "/" + FILE_NAME;

            LoadData();
        }

        public void UpdateVHConfig(VirtualHumanConfig config)
        {
            if (m_lookup.ContainsKey(config.vhName))
            {
                m_data[m_lookup[config.vhName]] = config;
                return;
            }

            m_lookup.Add(config.vhName, m_data.Count);
            m_data.Add(config);
        }

        public void DeleteVHConfig(VirtualHumanConfig config)
        {
            if (!m_lookup.ContainsKey(config.vhName))
            {
                return;
            }

            m_lookup.Remove(config.vhName);
            m_data.Remove(config);
        }

        public void DeleteVHConfig(string configName)
        {
            if (!m_lookup.ContainsKey(configName))
            {
                return;
            }

            DeleteVHConfig(m_data[m_lookup[configName]]);

        }

        public VirtualHumanConfigData GetSavedVHConfigs() => m_data;

        public VirtualHumanConfig this[int index] => m_data[index];
        public VirtualHumanConfig this[string name] => m_data[m_lookup[name]];

        public int SavedVHConfigCount => m_data.Count;


        public void SaveData()
        {
            m_serializedJSON = JsonUtility.ToJson(m_data);

            File.WriteAllText(savePath, m_serializedJSON);
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    m_serializedJSON = File.ReadAllText(savePath);
                    m_data = JsonUtility.FromJson<VirtualHumanConfigData>(m_serializedJSON);
                    m_lookup.Clear();

                    for (int i = 0; i < m_data.Count; i++)
                        m_lookup.Add(m_data[i].vhName, i);

                    Debug.Log("Load success.");
                }
                else
                {
                    m_data = new VirtualHumanConfigData();
                    Debug.Log("Load failed, no saved config exists");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

    }
}