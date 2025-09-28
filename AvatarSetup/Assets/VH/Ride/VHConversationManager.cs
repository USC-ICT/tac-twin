using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHAssets;
using Ride.Conversation;
using Ride.Audio;
using Ride.Sensing;

namespace Ride.VirtualHumans
{
    /// <summary>
    /// Manages conversation handlers of multiple VH characters.
    /// Starts/Stops mirroring and listening behaviors
    /// </summary>
    public class VHConversationManager : MonoBehaviour
    {
        [Header("References")]
        public MicrophoneAudioSystem microphoneAudio;
        public SensingProcessor sensingProcessor;
        public ConversationResponsePlayer responsePlayer;

        [Header("Settings")]
        public float microphoneThreshold = 0.001f;
        public bool enableMirroring = false;

        [field: Header("State")]
        [field: SerializeField] public ConversationHandler currentConversation { get; private set; }

        Dictionary<string, ConversationHandler> m_conversations = new Dictionary<string, ConversationHandler>();

        public List<string> RegisteredCharacters => new List<string>(m_conversations.Keys);
        public ConversationHandler this[string characterName]
        {
            get
            {
                return m_conversations[characterName];
            }

            set
            {
                m_conversations[characterName] = value;
            }
        }


        public int ConversationCount => m_conversations.Count;

        private void OnEnable()
        {
            if (enableMirroring)
                sensingProcessor.onEmotionProcessed += StartMirroring;
        }

        private void OnDisable()
        {
            sensingProcessor.onEmotionProcessed -= StartMirroring;
        }

        public void StartListening(bool withListeningBehavior = false)
        {
            currentConversation.StartListening();
            if (withListeningBehavior)
            {
                StartCoroutine(PlayCharacterListeningBehavior(currentConversation));
            }
        }

        public void StartListening(ConversationHandler conversation, bool withListeningBehavior = false)
        {
            conversation.StartListening();
            if (withListeningBehavior)
            {
                StartCoroutine(PlayCharacterListeningBehavior(conversation));
            }
        }

        public void StopListening(string characterName)
        {
            m_conversations[characterName].StopListening();
        }

        public IEnumerator PlayConversationResponse(ConversationHandler conversation)
        {
            StopMirroring();
            sensingProcessor.onEmotionProcessed -= StartMirroring;

            ICharacter character = conversation.context.character;

            yield return responsePlayer.PlayResponse(character, conversation.latestResponseData);

            conversation.CompleteResponse();

            if (enableMirroring)
                sensingProcessor.onEmotionProcessed += StartMirroring;
        }

        public IEnumerator PlayConversationResponse(ICharacter character, AudioSpeechFile audioSpeechFile)
        {
            yield return responsePlayer.PlayResponse(character, audioSpeechFile);
        }

        public IEnumerator PlayCharacterListeningBehavior(ConversationHandler conversation)
        {
            ListeningController listeningController = conversation.context.character.GetComponent<ListeningController>();

            microphoneAudio.StartRecording();
            listeningController.StartListening(microphoneAudio, microphoneThreshold);

            while (conversation.state == ConversationHandler.ConversationState.LISTENING)
            {
                yield return null;
            }

            listeningController.StopListening();
            microphoneAudio.StopRecording();
        }

        public void CreateCharacterConversation(ICharacter character, ConversationContext context = null)
        {
            context ??= new ConversationContext()
            {
                character = character,
                voiceIndex = 0
            };

            ConversationHandler conversation = new ConversationHandler(context);

            m_conversations.Add(character.CharacterName, conversation);
        }

        public void RemoveCharacterConversation(ICharacter character)
        {
            m_conversations.Remove(character.CharacterName);
        }

        public void SetCurrentConversation(ICharacter character) => SetCurrentConversation(character.CharacterName);
        public void SetCurrentConversation(string characterName)
        {
            currentConversation?.EndConversation();
            if (m_conversations.ContainsKey(characterName) == false) { Debug.LogWarning($"VHConversationManager.cs::Unregistered character name - {characterName}"); return; }
            
            currentConversation = m_conversations[characterName];


            sensingProcessor.SetSensingSystems(currentConversation.context.GetBackend<ISensingSystem>());

            currentConversation.BeginConversation();
        }

        void StartMirroring()
        {
            if (currentConversation?.context?.character == null) { return; }
            currentConversation?.context?.character.GetComponent<MirroringController>()?.MirrorEmotion(sensingProcessor.emotion);
        }

        void StopMirroring()
        {
            currentConversation?.context?.character.GetComponent<MirroringController>()?.StopMirroring();
        }
    }
}
