using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride;
using System;
using System.Linq;

/// <summary>
/// Unity implementation of an ISystemFactory. Uses specifed interface types as lookup keys
/// </summary>
public class SystemFactoryMono : RideSystemMonoBehaviour, IISystemFactory
{
    public bool useInstancing = true;
    Dictionary<Type, SystemCollection> m_systemLookup = new Dictionary<Type, SystemCollection>();

    //Constraining types to any derivations of IRideSystem
    public SystemCollection CreateSystemCollection<T>() where T : IRideSystem
    {
        System.Type newType = typeof(T);
        Debug.Log(newType);

#if UNITY_2021_1_OR_NEWER
        m_systemLookup.TryAdd(newType, new SystemCollection());
#else
        if (!m_systemLookup.ContainsKey(newType))
            m_systemLookup.Add(newType, new SystemCollection());
#endif

        foreach (T rideSystem in GetComponentsInChildren<T>(false))
        {
            string nameID = (rideSystem as Component).name;

            if (!m_systemLookup[newType].AddSystem(nameID, rideSystem))
            {
                Debug.Log("System already added!");
                continue;
            }
        }

        return m_systemLookup[newType];
    }

    public SystemCollection this[Type type] => m_systemLookup?[type];

    public bool TryGetSystemCollection<T>(out SystemCollection systemCollection) where T : class, IRideSystem
    {
        System.Type queryType = typeof(T);

        return m_systemLookup.TryGetValue(queryType, out systemCollection);
    }

    public SystemCollection GetSystemCollection<T>() where T : class, IRideSystem
    {
        return m_systemLookup[typeof(T)];
    }

    public T CreateSystemInstance<T>(string systemObjectName = "") where T : class, IRideSystem
    {
        return CreateSystemInstance(typeof(T), systemObjectName) as T;
    }


    public IRideSystem CreateSystemInstance(Type type, string systemObjectName = "")
    {
        if (!m_systemLookup.TryGetValue(type, out SystemCollection systemCollection))
            return null;

        IRideSystem system;

        if (systemObjectName == "" || !systemCollection.SystemExists(systemObjectName))
            system = systemCollection.DefaultSystem;
        else
            system = systemCollection[systemObjectName];

        if (system == null) return null;

        if(!useInstancing)
        {
            return system;
        }

        //Creating instance assuming all IRideSystem implementations are monobehaviors
        IRideSystem instance = Instantiate(system as Component, transform) as IRideSystem;


        return instance;
    }
}
