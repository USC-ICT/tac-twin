using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride.Conversation;
using VHAssets;

namespace Ride.VirtualHumans
{
    /// <summary>
    /// Responsible for most VH functionality such as VH creation, save/loading and conversations.
    /// Can also act as a modern wrapper around the legacy MecanimManager
    /// </summary>
    public class VirtualHumanSystemMono : RideSystemMonoBehaviour, IVirtualHumanSystem
    {
        [Header("References")]
        public CharacterDatabase vhCharacterDatabase;

        [Header("Settings")]
        public LayerMask floorLayerMask;
        public float spawnRadius = 1.0f;

        [Header("VH Components")]
        [SerializeField] VHConversationManager m_conversationManager;
        [SerializeField] VHConfigSerializer m_vhSerializer;
        [SerializeField] MecanimManager m_mecanimManager;
        [SerializeField] Cutscene m_cutscene;

        public VHConfigSerializer Serializer => m_vhSerializer;
        public VHConversationManager ConversationManager => m_conversationManager;

        Dictionary<string, string> m_characterTemplateLookup = new Dictionary<string, string>();
        GameObjectSpawner m_spawner;

        public string LookupVHCharacterTemplate(string name) => m_characterTemplateLookup[name];
        public List<string> CharacterTemplates => vhCharacterDatabase.names;

        public override void SystemAwake()
        {
            base.SystemAwake();

            m_vhSerializer = new VHConfigSerializer();
            m_spawner = new GameObjectSpawner(spawnRadius, floorLayerMask);
            vhCharacterDatabase.BuildLookup();
        }

        public override void SystemInit()
        {
            base.SystemInit();
            FindCharactersInScene();
        }

        public override void SystemShutdown()
        {
            m_vhSerializer.SaveData();
            base.SystemShutdown();
        }

        public void SaveVHConfig(VHConfigSerializer.VirtualHumanConfig config)
        {
            m_vhSerializer.UpdateVHConfig(config);
        }

        public ICharacter CreateVHCharacterInView(
            string characterName,
            string characterTemplate,
            Vector3 viewerPos,
            Vector3 viewerForward,
            ConversationContext context = null)
        {
#if UNITY_2021_1_OR_NEWER
            if (!m_characterTemplateLookup.TryAdd(characterName, characterTemplate))
                return null;
#else
            if (!m_characterTemplateLookup.ContainsKey(characterName))
                m_characterTemplateLookup.Add(characterName, characterTemplate);
            else
                return null;
#endif

            if (!vhCharacterDatabase.CharacterExists(characterTemplate))
                return null;

            MecanimCharacter characterBase = vhCharacterDatabase[characterTemplate];

            GameObject spawnedObject = m_spawner.SpawnGameObjectInView(characterName, characterBase.gameObject, viewerPos, viewerForward); spawnedObject.name = characterName;

            MecanimCharacter character = spawnedObject.GetComponent<MecanimCharacter>();

            RegisterVHCharacter(character, context);

            return character;
        }

        public ICharacter CreateVHCharacterAtPostion(
            string characterName,
            string characterTemplate,
            Vector3 position,
            Vector3 forward = default,
            ConversationContext context = null)
        {
#if UNITY_2021_1_OR_NEWER
            if (!m_characterTemplateLookup.TryAdd(characterName, characterTemplate))
                return null;
#else
            if (!m_characterTemplateLookup.ContainsKey(characterName))
                m_characterTemplateLookup.Add(characterName, characterTemplate);
            else
                return null;
#endif

            if (!vhCharacterDatabase.CharacterExists(characterTemplate))
                return null;

            MecanimCharacter characterBase = vhCharacterDatabase[characterTemplate];

            GameObject spawnedObject = m_spawner.SpawnGameObject(characterName, characterBase.gameObject, position, forward);
            spawnedObject.name = characterName;

            MecanimCharacter character = spawnedObject.GetComponent<MecanimCharacter>();

            RegisterVHCharacter(character, context);

            return character;
        }

        public void DeleteVHCharacter(string characterName) => DeleteVHCharacter(m_mecanimManager.GetCharacter(characterName) as MecanimCharacter);
        public void DeleteVHCharacter(MecanimCharacter character)
        {
            RemoveVHCharacter(character);
            m_spawner.DeleteGameObject(character.gameObject);
        }

        public void RegisterVHCharacter(MecanimCharacter character, ConversationContext context = null)
        {
            m_conversationManager.CreateCharacterConversation(character, context);
            m_mecanimManager?.AddCharacter(character as MecanimCharacter);

            BMLEventHandler bmEvtHandler = character.GetComponent<BMLEventHandler>();

            bmEvtHandler.m_CutscenePrefab = m_cutscene;
            bmEvtHandler.m_CharacterController = m_mecanimManager;

            Debug.Log("Character Registered:" + character.name);
        }

        public void RemoveVHCharacter(MecanimCharacter character)
        {
            m_conversationManager.RemoveCharacterConversation(character);
            m_mecanimManager?.RemoveCharacter(character.CharacterName);

            m_characterTemplateLookup.Remove(character.CharacterName);

            Debug.Log("Character Removed:" + character);
        }

        public void SaveVHCharacterConfig(ICharacter character, List<VHConfigSerializer.VHSystemID> systemIDs)
        {
            m_vhSerializer.UpdateVHConfig(new VHConfigSerializer.VirtualHumanConfig()
            {
                vhName = character.name,
                position = character.transform.position,
                forward = character.transform.forward,
                modelTemplate = LookupVHCharacterTemplate(character.name),
                systemIDs = systemIDs,
                voiceIndex = GetConversationContext(character.name).voiceIndex
            });
        }

        public VHConfigSerializer.VirtualHumanConfig LoadCharacterConfig(string name)
        {
            return m_vhSerializer[name];
        }


        public ConversationContext GetConversationContext(string virutalHumanName) => GetConversationHandler(virutalHumanName).context;

        public ConversationContext GetConversationContext() => GetConversationHandler().context;

        public ConversationHandler GetConversationHandler(string virutalHumanName) => m_conversationManager[virutalHumanName];

        public ConversationHandler GetConversationHandler() => m_conversationManager.currentConversation;

        void FindCharactersInScene()
        {
            foreach (MecanimCharacter character in FindObjectsByType<MecanimCharacter>(FindObjectsSortMode.None))
            {
                if (character.name == "all") continue;
                RegisterVHCharacter(character);
            }
        }
    }
}
