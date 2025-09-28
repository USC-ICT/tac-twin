using System;
using System.Collections;
using System.Collections.Generic;
using VHAssets;

namespace Ride.Conversation
{
    /// <summary>
    /// Type-determined data lookup container for VH backends
    /// </summary>
    [Serializable]
    public class ConversationContext
    {
        [Serializable]
        public struct ContextSystem
        {
            public string systemName;
            public IRideSystem rideSystem;
        }

        public int voiceIndex;
        public ICharacter character;

        Dictionary<Type, ContextSystem> m_typeLookup;

        public ConversationContext()
        {
            m_typeLookup = new Dictionary<Type, ContextSystem>();
        }

        public T GetBackend<T>() where T : class, IRideSystem
        {
            return m_typeLookup[typeof(T)].rideSystem as T;
        }

        public IRideSystem GetBackend(Type type)
        {
            if (!m_typeLookup.ContainsKey(type)) return null;

            return m_typeLookup[type].rideSystem;
        }

        public string GetBackendName<T>()
        {
            return m_typeLookup[typeof(T)].systemName;
        }

        public string GetBackendName(Type type)
        {
            return m_typeLookup[type].systemName;
        }

        public void SetBackend<T>(T backend, string backendName) where T : class, IRideSystem
        {
            SetBackend(typeof(T), backend, backendName);
        }

        public void SetBackend(Type type, IRideSystem backend, string backendName)
        {
            ContextSystem contextSystem = new ContextSystem()
            {
                rideSystem = backend,
                systemName = backendName
            };

#if UNITY_2021_1_OR_NEWER
            if (!m_typeLookup.TryAdd(type, contextSystem))
                m_typeLookup[type] = contextSystem;
#else
            if (!m_typeLookup.ContainsKey(type))
                m_typeLookup.Add(type, contextSystem);
            else
                m_typeLookup[type] = contextSystem;
#endif
        }
    }
}
