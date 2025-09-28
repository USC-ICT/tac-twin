using System.Collections;
using System.Collections.Generic;
using Ride;

/// <summary>
/// Wrapper class for a list of IRideSystems using unique string names as identifiers
/// </summary>
[System.Serializable]
public class SystemCollection
{

    List<string> m_systemNames;
    List<IRideSystem> m_systems;

    public IRideSystem DefaultSystem => (m_systems != null && m_systems.Count > 0) ? m_systems[0] : null;

    public IRideSystem this[int index] => m_systems?[index];
    public IRideSystem this[string systemName] => m_systems?[m_systemNames.IndexOf(systemName)];
    public string this[IRideSystem system] => m_systemNames?[m_systems.IndexOf(system)];
    public bool SystemExists(string systemName) => m_systemNames?.Contains(systemName) == true;

    public List<string> SystemNames() => m_systemNames;

    public int IndexOf(IRideSystem system) => m_systems.IndexOf(system);
    public int IndexOf(string systemName) => m_systemNames.IndexOf(systemName);

    public SystemCollection()
    {
        m_systems = new List<IRideSystem>();
        m_systemNames = new List<string>();
    }

    public bool AddSystem(string systemName, IRideSystem system)
    {
        if (m_systemNames.Contains(systemName) || m_systems.Contains(system)) return false;

        m_systemNames.Add(systemName);
        m_systems.Add(system);

        return true;
    }
}

/// <summary>
/// Interface for a system that can create system collections and instances of other IRideSystem implementations
/// </summary>/
public interface IISystemFactory : IRideSystem
{
    SystemCollection CreateSystemCollection<T>() where T : IRideSystem;
    SystemCollection GetSystemCollection<T>() where T : class, IRideSystem;
    bool TryGetSystemCollection<T>(out SystemCollection systemCollection) where T : class, IRideSystem;
    SystemCollection this[System.Type type] { get; }

    IRideSystem CreateSystemInstance(System.Type type, string systemObjectName = "");
    T CreateSystemInstance<T>(string systemObjectName = "") where T : class, IRideSystem;
}
