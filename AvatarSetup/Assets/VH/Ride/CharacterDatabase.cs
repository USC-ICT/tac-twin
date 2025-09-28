using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHAssets;

namespace Ride.VirtualHumans
{

    [CreateAssetMenu()]
    public class CharacterDatabase : ScriptableObject
    {

        [SerializeField] MecanimCharacter[] m_characters;
        Dictionary<string, MecanimCharacter> m_lookup;

        public MecanimCharacter this[int index] => m_characters[index];
        public MecanimCharacter this[string name] => m_lookup[name];
        public List<string> names => new List<string>(m_lookup.Keys);
        public bool CharacterExists(string name)=> m_lookup.ContainsKey(name);

        public void BuildLookup()
        {
            m_lookup = new Dictionary<string, MecanimCharacter>();

            foreach (var character in m_characters)
            {
                m_lookup.Add(character.name, character);
            }
        }
    }
}
