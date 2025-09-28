using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride.IO;
using Ride.Conversation;
using System;
using Ride.TextToSpeech;
using Ride.SpeechRecognition;
using Ride.NLP;
using Ride.Sensing;
using Ride.VirtualHumans;
using VHAssets;

namespace Ride.Examples
{
    /// <summary>
    /// Scenario controller script for VH Sandbox
    /// </summary>
    public class VHSandboxExample : RideBaseMinimal
    {
        [System.Serializable]
        public struct SystemInfo
        {
            public RideID id;
            public Type type;
        }

        [Header("Scenario References")]
        public VHSandboxUIController configUIController;
        public ConversationUIDisplay conversationUIDisplay;
        public VirtualHumanSystemMono vhSystem;
        public EnvironmentSwapper environmentSwapper;
        public PlayerInputController player;

        [Header("VH Backends")]
        [SerializeField] SystemFactoryMono m_systemFactory;

        [Header("Keybinds")]
        [SerializeField] RideKeyCode m_toggleMenuKey = RideKeyCode.Tab;
        [SerializeField] RideKeyCode m_pushToTalkKey = RideKeyCode.V;
        [SerializeField] RideKeyCode m_toggleCursorKey = RideKeyCode.F12;
        [SerializeField] RideKeyCode m_toggleWebcamKey = RideKeyCode.F;
        [SerializeField] RideKeyCode m_cycleMenuTabKey = RideKeyCode.T;
        [SerializeField] RideKeyCode m_createVHKey = RideKeyCode.F2;

        [Header("Scenario Settings")]
        [SerializeField] string m_namePrefix = "VirtualHuman_";
        [SerializeField] bool m_cursorEnabledOnStartup = true;

        [SerializeField] List<SystemCollection> m_systemCollectionList = new List<SystemCollection>();
        Dictionary<SystemCollection, SystemInfo> m_systemInfoLookup = new Dictionary<SystemCollection, SystemInfo>();
        Dictionary<RideID, Type> m_typeLookup = new Dictionary<RideID, Type>();
        Dictionary<RideID, System.Action<RideID, int>> m_callbackLookup = new Dictionary<RideID, System.Action<RideID, int>>();

        #region  Scenario Init
        protected override void Start()
        {
            base.Start();

            m_debugMenu.AddMenu("Avatars", OnGUIAvatars);
            m_debugMenu.SetMenu(m_debugMenu.GetMenuCount() - 1);
            m_debugMenu.ShowMenu(false);

            BindSystemCollections();
            InitializeUI();

            SetRandomCharacterName();

            HandleConfigModeChanged(configUIController.configMode);

            configUIController.DisableUserInteration();

            if (vhSystem.ConversationManager.ConversationCount == 0)
            {
                configUIController.SetConfigEditTabInteraction(false);
                configUIController.SetSaveButtonInteraction(false);
            }

            player ??= GameObject.FindAnyObjectByType<PlayerInputController>();


            Globals.api.worldStateSystem.AddListener<WorldState.TerrainLoadedEvent>(WorldState.WorldEvent.terrainLoaded, UpdateTerrainAndAgents);

            StartCoroutine(LateStart(2)); 
        }

        IEnumerator LateStart(int frameDelay)
        {
            for (int i = 0; i < frameDelay; i++)
            {
                yield return null;
            }

            if (m_cursorEnabledOnStartup)
            {
                player.Active = false; // If we dont wait 2 frames, then cursor stays on the screen even if the player is inactive
            }
        }


        protected override void Update()
        {
            base.Update();

            if (configUIController.inTextInput)
            {
                player.Active = false;
                return;
            }
            if (Globals.api.inputSystem.GetKeyDown(m_toggleCursorKey))
            {
                player.Active = !player.Active;
            }

            if (Globals.api.inputSystem.GetKeyDown(RideKeyCode.Return))
            {
                configUIController.ManualTextInputSelection();
            }

            if (Globals.api.inputSystem.GetKeyDown(m_cycleMenuTabKey))
            {
                configUIController.CycleCurrentMenuTab();
            }

            if (Globals.api.inputSystem.GetKeyDown(m_toggleWebcamKey))
            {
                configUIController.ManualWebCamToggle();
            }

            if (Globals.api.inputSystem.GetKeyDown(m_createVHKey))
            {
                if (configUIController.configMode == VHSandboxUIController.ConfigMode.CREATE)
                    configUIController.OnCharacterCreate();
            }

            if (Globals.api.inputSystem.GetKeyDown(m_toggleMenuKey))
            {
                if (configUIController.configEnabled)
                    configUIController.DisableConfig();
                else
                    configUIController.EnableConfig();
            }

            if (Globals.api.inputSystem.GetKeyDown(m_pushToTalkKey))
            {
                configUIController.ManualPushToTalkStart();
            }
            if (Globals.api.inputSystem.GetKeyUp(m_pushToTalkKey))
            {
                configUIController.ManualPushToTalkStop();
            }

            if (player.Active)
            {
                SetHeightAboveTerrain(player.transform);
            }

        }

        protected virtual void OnEnable()
        {
            configUIController.TextSubmitted += SendQuestion;

            configUIController.WebCamToggled += SetVideoInput;

            configUIController.PushToTalkStarted += StartAudioInput;
            configUIController.PushToTalkStopped += StopAudioInput;

            configUIController.CreateAttempt += CreateVHCharacter;

            configUIController.ConfigModeChanged += HandleConfigModeChanged;

            configUIController.DeleteAttempt += DeleteVHCharacter;

            configUIController.SaveAttempt += SaveVHConfig;
            configUIController.LoadAttempt += LoadVHConfig;
            configUIController.DeleteSaveAttempt += DeleteVHConfig;

            configUIController.TransformValueChanged += HandleTransformValuesChanged;
        }

        protected virtual void OnDisable()
        {
            configUIController.TextSubmitted -= SendQuestion;
            configUIController.WebCamToggled -= SetVideoInput;

            configUIController.PushToTalkStarted -= StartAudioInput;
            configUIController.PushToTalkStopped -= StopAudioInput;

            configUIController.CreateAttempt -= CreateVHCharacter;

            configUIController.ConfigModeChanged -= HandleConfigModeChanged;

            configUIController.DeleteAttempt -= DeleteVHCharacter;

            configUIController.SaveAttempt -= SaveVHConfig;
            configUIController.LoadAttempt -= LoadVHConfig;
            configUIController.DeleteSaveAttempt -= DeleteVHConfig;

            configUIController.TransformValueChanged -= HandleTransformValuesChanged;


        }

        //Creating a system collection for each VH backend, and binding a UI dropdown id to it locally so we can iterate over the interfaces
        public void BindSystemCollections()
        {
            BindSystemCollection<INonverbalGeneratorSystem>(configUIController.nvbgDropdownID);
            BindSystemCollection<ISpeechRecognitionSystem>(configUIController.asrDropdownID);
            BindSystemCollection<ILipsyncedTextToSpeechSystem>(configUIController.ttsDropdownID);
            //BindSystemCollection<INLPQnASystem>(configUIController.nlpDropdownID);
            BindSystemCollection<ISensingSystem>(configUIController.sensingDropdownID);
            BindSystemCollection<INlpSentimentSystem>(configUIController.nlpSentimentDropdownID);
            BindSystemCollection<INLPEntitiesSystem>(configUIController.nlpEntitiesDropdownID);
        }

        void BindSystemCollection<T>(RideID rideID) where T : class, IRideSystem
        {
            SystemCollection systemCollection = m_systemFactory.CreateSystemCollection<T>();
            m_systemCollectionList.Add(systemCollection);
            m_typeLookup.Add(rideID, typeof(T));
            m_systemInfoLookup.Add(systemCollection, new SystemInfo()
            {
                id = rideID,
                type = typeof(T)
            });
        }

        void InitializeUI()
        {
            configUIController.FillDropdownText(configUIController.modelDropdownID, vhSystem.CharacterTemplates);
            configUIController.FillDropdownText(configUIController.characterDropdownID, vhSystem.ConversationManager.RegisteredCharacters);

            m_callbackLookup.TryAdd(configUIController.characterDropdownID, HandleCharacterPicked);

            foreach (var systemCollection in m_systemCollectionList)
            {
                configUIController.FillDropdownText(m_systemInfoLookup[systemCollection].id, systemCollection.SystemNames());
                m_callbackLookup.Add(m_systemInfoLookup[systemCollection].id, HandleBackendChanged); //Lookup dictionary for callback events
            }

            configUIController.FillDropdownText
                (
                configUIController.voiceDropdownID,
                new List<string>(((ILipsyncedTextToSpeechSystem)m_systemFactory.GetSystemCollection<ILipsyncedTextToSpeechSystem>().DefaultSystem).GetAvailableVoices())
                ); // Adding voice list from the TTS system

            m_callbackLookup.Add(configUIController.voiceDropdownID, HandleVoiceChanged);

            configUIController.SetWebcamTexture(vhSystem.ConversationManager.sensingProcessor.RenderMaterial);

            
            m_callbackLookup[configUIController.ttsDropdownID] += HandleTTSBackendChanged;

            UpdateLoadedCharacterListUI();
        }

        #endregion

        #region Sandbox Functionality
        //Creates context with the option of adding default systems
        ConversationContext CreateCharacterContext(ICharacter character, bool defaultSystems = true)
        {
            ConversationContext conversationContext = new ConversationContext()
            {
                voiceIndex = 0,
                character = character,
            };

            if (defaultSystems)
            {
                foreach (SystemCollection systemCollection in m_systemCollectionList)
                {
                    Type systemType = m_systemInfoLookup[systemCollection].type;
                    RideID iD = m_systemInfoLookup[systemCollection].id;


                    conversationContext.SetBackend
                        (systemType,
                        m_systemFactory.CreateSystemInstance(systemType),
                        systemCollection[systemCollection.DefaultSystem]);
                }
            }


            return conversationContext;
        }

        void RemoveCharacterContext(string character)
        {
            ConversationContext context = vhSystem.ConversationManager[character].context;

            if (m_systemFactory.useInstancing)
                foreach (SystemCollection systemCollection in m_systemCollectionList)
                {
                    Type systemType = m_systemInfoLookup[systemCollection].type;

                    Destroy((context.GetBackend(systemType) as Component)?.gameObject);
                }
        }

        

        public void UpdateConversationUI(ConversationHandler.ConversationState state)
        {
            configUIController.SetInputText(vhSystem.GetConversationHandler().latestResponseData.inputQuestion);
            conversationUIDisplay.UpdateUIDisplay(vhSystem.GetConversationHandler());
        }

        //Fills created VH character dropdown list
        public void UpdateLoadedCharacterListUI()
        {
            List<string> savedVHNames = new();
            foreach (VHConfigSerializer.VirtualHumanConfig config in vhSystem.Serializer.GetSavedVHConfigs().configData)
            {
                savedVHNames.Add(config.vhName);
            }
            configUIController.FillDropdownText(configUIController.loadCharacterDropdownID, savedVHNames);

            if (savedVHNames.Count == 0)
            {
                configUIController.SetLoadButtonInteraction(false);
            }
            else
                configUIController.SetLoadButtonInteraction(true);
        }

        //When a VH character is selected in the dropdown, updated backend dropdowns to match
        void HandleCharacterPicked(RideID id, int option)
        {
            ConversationContext context = GetCurrentCharacterConversationContext();

            foreach (SystemCollection systemCollection in m_systemCollectionList)
            {
                if (systemCollection.DefaultSystem == null) continue;

                configUIController.SetDropdownOption(
                    m_systemInfoLookup[systemCollection].id,
                    systemCollection.IndexOf(context.GetBackendName(m_systemInfoLookup[systemCollection].type)));
            }

            configUIController.FillDropdownText(configUIController.voiceDropdownID, new List<string>(context.GetBackend<ILipsyncedTextToSpeechSystem>().GetAvailableVoices()));

            configUIController.SetDropdownOption(
                configUIController.voiceDropdownID,
                context.voiceIndex);


            if (vhSystem.GetConversationHandler().context == context &&
            (vhSystem.GetConversationHandler().state != ConversationHandler.ConversationState.IDLE &&
            vhSystem.GetConversationHandler().state != ConversationHandler.ConversationState.INACTIVE))
                configUIController.SetDeleteButtonInteraction(false);
            else
                configUIController.SetDeleteButtonInteraction(true);

            configUIController.SetTransformValues(
                new Vector3(
                    context.character.transform.position.x,
                    context.character.transform.eulerAngles.y,
                    context.character.transform.position.z
                ));

        }

        //Add/remove callbacks based on the config menu tab
        void HandleConfigModeChanged(VHSandboxUIController.ConfigMode configMode)
        {
            switch (configUIController.configMode)
            {
                case VHSandboxUIController.ConfigMode.CREATE:
                    configUIController.DropdownOptionSelected -= HandleDropdownOptionSelected;
                    configUIController.DropdownOptionSelected += HandleTTSBackendChanged;
                    configUIController.SetDropdownsToDefault();

                    break;

                case VHSandboxUIController.ConfigMode.EDIT:
                    HandleCharacterPicked
                    (
                        configUIController.characterDropdownID,
                        configUIController.GetDropdownOptionSelected(configUIController.characterDropdownID)
                    );

                    configUIController.DropdownOptionSelected += HandleDropdownOptionSelected;
                    configUIController.DropdownOptionSelected -= HandleTTSBackendChanged;
                    break;
                case VHSandboxUIController.ConfigMode.SAVE_LOAD:
                    configUIController.DropdownOptionSelected -= HandleDropdownOptionSelected;
                    break;
            }
        }

        //Update VH backend change from the UI
        void HandleBackendChanged(RideID id, int option)
        {
            ConversationContext context = GetCurrentCharacterConversationContext();
            Type type = m_typeLookup[id];

            Destroy((context.GetBackend(type) as Component).gameObject);
            string systemName = configUIController.GetDropdownOptionText(id);
            context.SetBackend(type, m_systemFactory.CreateSystemInstance(type, systemName), systemName);
        }

        void HandleDropdownOptionSelected(RideID id, int option)
        {
            m_callbackLookup[id].Invoke(id, option);
        }

        //Specfific case for TTS as voice index also needs to change
        void HandleTTSBackendChanged(RideID id, int option)
        {
            if (id != configUIController.ttsDropdownID) return;

            configUIController.FillDropdownText(configUIController.voiceDropdownID,

            new List<string>(((ILipsyncedTextToSpeechSystem)m_systemFactory.GetSystemCollection<ILipsyncedTextToSpeechSystem>()[configUIController.GetDropdownOptionSelected(id)]).GetAvailableVoices()));

            configUIController.SetDropdownOption(
                configUIController.voiceDropdownID,
                0);


            if (configUIController.configMode == VHSandboxUIController.ConfigMode.EDIT)
                GetCurrentCharacterConversationContext().voiceIndex = 0;
        }

        void HandleVoiceChanged(RideID id, int option) =>
        GetCurrentCharacterConversationContext().voiceIndex = configUIController.GetDropdownOptionSelected(configUIController.voiceDropdownID);

        //When position/rotation is adjusted
        void HandleTransformValuesChanged(Vector3 value)
        {
            Transform transform = GetCurrentCharacterConversationContext().character.transform;

            Vector3 temp = transform.eulerAngles;
            temp.y = value.y;
            transform.eulerAngles = temp;

            value.y = transform.position.y;
            transform.position = value;

            SetHeightAboveTerrain(transform);
        }

        ConversationContext GetCurrentCharacterConversationContext()
        {
            return vhSystem.GetConversationContext(configUIController.GetDropdownOptionText(configUIController.characterDropdownID));
        }

        //Sensing start/stop
        void SetVideoInput(bool enabled)
        {
            if (enabled)
                vhSystem.ConversationManager.sensingProcessor?.StartProcessing();
            else
                vhSystem.ConversationManager.sensingProcessor?.StopProcessing();
        }

        //Push to talk start
        void StartAudioInput()
        {
            vhSystem.ConversationManager.StartListening(true);

            if (vhSystem.GetConversationHandler().state != ConversationHandler.ConversationState.LISTENING) return;

            configUIController.SetTextInteraction(false);
            configUIController.SetCaptionText("Listening...");
        }

        //Push to talk stop
        void StopAudioInput()
        {
            configUIController.SetDeleteButtonInteraction(false);
            configUIController.DisableUserInteration();
            configUIController.SetCaptionText("Processing...");

            StartCoroutine(PushToTalkCoolDownRoutine());
        }

        //Adding a cooldown of 1.5 seconds so that spamming the button won't cause issues, and gives time for ASR to "catch up" on speech processing.
        IEnumerator PushToTalkCoolDownRoutine(float cooldown = 1.5f)
        {
            configUIController.SetAudioInteraction(false);

            if (string.IsNullOrEmpty(vhSystem.GetConversationHandler().latestResponseData.inputQuestion))
                yield return new WaitForSeconds(cooldown);

            vhSystem.GetConversationHandler().StopListening();

            if (vhSystem.GetConversationHandler().state == ConversationHandler.ConversationState.IDLE)
            {
                configUIController.SetDeleteButtonInteraction(true);
                configUIController.EnableUserInteraction();
                configUIController.SetCaptionText(string.Empty);
                yield break;
            }
        }

    
        public void SendQuestion(string question)
        {
            vhSystem.GetConversationHandler().AskQuestion(question);

            if (vhSystem.GetConversationHandler().state != ConversationHandler.ConversationState.PROCESSING_QUESTION) return;

            configUIController.SetCaptionText("Processing...");
            configUIController.DisableUserInteration();
            configUIController.SetDeleteButtonInteraction(false);
        }

        void OnResponseReady(ConversationHandler conversation) => StartCoroutine(PlayVHResponse(conversation));
        IEnumerator PlayVHResponse(ConversationHandler conversation)
        {
            configUIController.SetCaptionText(conversation.latestResponseData.outputResponse);

            yield return vhSystem.ConversationManager.PlayConversationResponse(conversation);

            configUIController.EnableUserInteraction();

            if (conversation.context == GetCurrentCharacterConversationContext())
                configUIController.SetDeleteButtonInteraction(true);

            configUIController.SetInputText(string.Empty);

            if (configUIController.pushToTalkDown)
                StartAudioInput();
        }

        #endregion

        #region VH Creation
        public void CreateVHCharacter(string characterName)
        {
            if (configUIController.randomizationEnabled)
            {
                configUIController.RandomizeDropdown(configUIController.modelDropdownID, vhSystem.CharacterTemplates.Count - 1);

                configUIController.RandomizeDropdown(configUIController.asrDropdownID);
                configUIController.RandomizeDropdown(configUIController.ttsDropdownID);
                configUIController.RandomizeDropdown(configUIController.nlpDropdownID);
                configUIController.RandomizeDropdown(configUIController.nvbgDropdownID);
                configUIController.RandomizeDropdown(configUIController.sensingDropdownID);

                HandleTTSBackendChanged(configUIController.ttsDropdownID, configUIController.GetDropdownOptionSelected(configUIController.ttsDropdownID));
                configUIController.RandomizeDropdown(configUIController.voiceDropdownID);
            }

            string modelTemplate = configUIController.GetDropdownOptionText(configUIController.modelDropdownID);
            CreateVHCharacter(characterName, modelTemplate);
        }

        public void CreateVHCharacter(string characterName, string modelTemplate)
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                configUIController.SetCaptionText("Character name is empty!");
                return;
            }

            if (vhSystem.ConversationManager.RegisteredCharacters.Contains(characterName))
            {
                configUIController.SetCaptionText("A character named " + characterName + " already exists!");
                return;
            }

            if (vhSystem.ConversationManager.ConversationCount == 0)
            {
                configUIController.SetConfigEditTabInteraction(true);
                configUIController.SetSaveButtonInteraction(true);
            }

            ICharacter character = vhSystem.CreateVHCharacterInView(characterName, modelTemplate, player.transform.position, player.transform.forward);
            character.GetComponentInChildren<VHGazeTrigger>().onGazeTriggered += SetCurrentConversation;

            ConversationContext context = CreateCharacterContext(character, false);
            
            //Set the correct backend system config for the created VH
            foreach (SystemCollection systemCollection in m_systemCollectionList)
            {
                Type systemType = m_systemInfoLookup[systemCollection].type;
                RideID iD = m_systemInfoLookup[systemCollection].id;

                string systemName = configUIController.GetDropdownOptionText(iD);

                context.SetBackend
                    (systemType,
                    m_systemFactory.CreateSystemInstance(systemType, systemName),
                    systemName);
            }

            vhSystem.ConversationManager[character.name] = new ConversationHandler(context);

            context.voiceIndex = configUIController.GetDropdownOptionSelected(configUIController.voiceDropdownID);

            configUIController.FillDropdownText(configUIController.characterDropdownID, new List<string>(vhSystem.ConversationManager.RegisteredCharacters));

            SetRandomCharacterName();
        }

        public void DeleteVHCharacter(string characterName)
        {
            RemoveCharacterContext(characterName);

            vhSystem.DeleteVHCharacter(characterName);

            configUIController.FillDropdownText(configUIController.characterDropdownID, new List<string>(vhSystem.ConversationManager.RegisteredCharacters));
            configUIController.SetDropdownOption(configUIController.characterDropdownID, 0);

            if (vhSystem.ConversationManager.ConversationCount == 0)
            {
                configUIController.ManualSwapConfigTab();
                configUIController.SetConfigEditTabInteraction(false);
                configUIController.SetSaveButtonInteraction(false);
            }
        }

        //Adding/removing callbacks when we swap the character we are talking to
        public void SetCurrentConversation(ICharacter character)
        {
            if (vhSystem.GetConversationHandler()?.context?.character != null)
            {
                vhSystem.GetConversationHandler().ResponseReady -= OnResponseReady;
                vhSystem.GetConversationHandler().SpeechRecognized -= configUIController.SetInputText;
                vhSystem.GetConversationHandler().ConversationStateChange -= UpdateConversationUI;
            }

            vhSystem.ConversationManager.SetCurrentConversation(character);

            vhSystem.GetConversationHandler().ResponseReady += OnResponseReady;
            vhSystem.GetConversationHandler().SpeechRecognized += configUIController.SetInputText;
            vhSystem.GetConversationHandler().ConversationStateChange += UpdateConversationUI;

            configUIController.EnableUserInteraction();
            UpdateConversationUI(vhSystem.GetConversationHandler().state);
        }



        #endregion

        void SetRandomCharacterName()
        {
            string randCharacter = string.Empty;
            int count = 0;
            do
            {
                randCharacter = m_namePrefix + (vhSystem.ConversationManager.ConversationCount + count);
                count++;

            } while (vhSystem.ConversationManager.RegisteredCharacters.Contains(randCharacter));
            configUIController.SetCharacterNameText(m_namePrefix + vhSystem.ConversationManager.ConversationCount);
        }

        #region  VH Saving
        public void SaveVHConfig(string name)
        {
            ConversationContext context = vhSystem.GetConversationContext(name);
            ICharacter character = context.character;

            List<VHConfigSerializer.VHSystemID> systemIDs = new();

            foreach (SystemCollection systemCollection in m_systemCollectionList)
            {
                if (systemCollection.DefaultSystem == null) continue;

                systemIDs.Add(new()
                {
                    systemType = m_systemInfoLookup[systemCollection].type.ToString(),
                    systemName = context.GetBackendName(m_systemInfoLookup[systemCollection].type)
                });
            }

            vhSystem.SaveVHCharacterConfig(character, systemIDs);

            UpdateLoadedCharacterListUI();
        }

        public void LoadVHConfig(int index)
        {
            VHConfigSerializer.VirtualHumanConfig config = vhSystem.Serializer[index];

            MecanimCharacter character;
            ConversationContext context;

            if (vhSystem.ConversationManager.RegisteredCharacters.Contains(config.vhName)) //if character is already created, then overrwrite it's current config
            {
                context = vhSystem.GetConversationContext(config.vhName);
                character = (MecanimCharacter)context.character;

                character.transform.position = config.position;
                character.transform.forward = config.forward;

                SetHeightAboveTerrain(character.transform);

                if (m_systemFactory.useInstancing)
                    foreach (SystemCollection systemCollection in m_systemCollectionList)
                    {
                        Type systemType = m_systemInfoLookup[systemCollection].type;

                        Destroy((context.GetBackend(systemType) as Component)?.gameObject);
                    }
            }
            else    //Creating a new VH character with the loaded config
            {

                if (vhSystem.ConversationManager.ConversationCount == 0)
                {
                    configUIController.SetConfigEditTabInteraction(true);
                    configUIController.SetSaveButtonInteraction(true);
                }

                character = vhSystem.CreateVHCharacterAtPostion(config.vhName, config.modelTemplate, config.position, config.forward) as MecanimCharacter;
                character.transform.position = config.position;
                character.transform.forward = config.forward;
                character.GetComponentInChildren<VHGazeTrigger>().onGazeTriggered += SetCurrentConversation;
                context = CreateCharacterContext(character, false);
                vhSystem.ConversationManager[config.vhName] = new ConversationHandler(context);
            }

            SetHeightAboveTerrain(character.transform);

            List<VHConfigSerializer.VHSystemID> systemIDs = config.systemIDs;

            foreach (VHConfigSerializer.VHSystemID systemID in config.systemIDs)
            {
                Type systemType = Type.GetType(systemID.systemType);
                string systemName = systemID.systemName;

                context.SetBackend
                    (systemType,
                    m_systemFactory.CreateSystemInstance(systemType, systemName),
                    systemName);
            }

            context.voiceIndex = config.voiceIndex;

            configUIController.FillDropdownText(configUIController.characterDropdownID, new List<string>(vhSystem.ConversationManager.RegisteredCharacters));
        }

        public void DeleteVHConfig(string name)
        {
            vhSystem.Serializer.DeleteVHConfig(name);
            UpdateLoadedCharacterListUI();
        }

        void SaveVHConfig()
        {
            string name = configUIController.GetDropdownOptionText(configUIController.characterDropdownID);

            if (name == string.Empty) return;

            SaveVHConfig(name);
        }

        void LoadVHConfig()
        {
            int index = configUIController.GetDropdownOptionSelected(configUIController.loadCharacterDropdownID);

            LoadVHConfig(index);
        }

        void DeleteVHConfig()
        {
            string name = configUIController.GetDropdownOptionText(configUIController.loadCharacterDropdownID);

            DeleteVHConfig(name);
        }
        #endregion

        //When terrain updates, update y position of all created characters
        void UpdateTerrainAndAgents(WorldState.WorldEventMarker simulationEvent, WorldState.TerrainLoadedEvent eventData)
        {
            //((Terrain.TerrainMono)environmentSwapper.m_terrain).terrainRoot.transform.position = environmentSwapper.terrainCenter;

            Physics.SyncTransforms();

            foreach (ICharacter character in MecanimManager.Get().GetControlledCharacters())
            {
                SetHeightAboveTerrain(character.transform);
            }
            SetHeightAboveTerrain(player.transform);
        }
        

        void SetHeightAboveTerrain(Transform target)
        {
            Vector3 targetPos = target.position;
            float height = Globals.api.terrainSystem.GetTerrainHeight(targetPos);
            targetPos.y = height;
            target.position = targetPos;
        }

        #region  Debug
        void OnGUIAvatars()
        {
            if (m_debugMenu.Button("Hide"))
                m_debugMenu.ToggleMenu();

            GUILayout.Space(5);

            using (new GUILayout.HorizontalScope())
            {
                m_debugMenu.Label("Speak:", 70);

                if (m_debugMenu.Button("Female"))
                {
                    // speak female line
                    AudioSpeechFile audioSpeechFile = GameObject.Find("F_ADMC_admc_intro_010").GetComponent<AudioSpeechFile>();
                    StartCoroutine(PlayPrerecordedAudio(audioSpeechFile));

                }

                if (m_debugMenu.Button("Male"))
                {
                    // speak male line
                    AudioSpeechFile audioSpeechFile = GameObject.Find("M_ADMC_admc_intro_010").GetComponent<AudioSpeechFile>();
                    StartCoroutine(PlayPrerecordedAudio(audioSpeechFile));
                }

                GUILayout.Space(5);

                m_debugMenu.Label("Speak with TTS:", 70);

                if (m_debugMenu.Button("Speak"))
                {
                    string textInput = (GameObject.Find("F_ADMC_admc_intro_010").GetComponent<AudioSpeechFile>().UtteranceText)
                        .Replace('\n', ' ')
                        .Replace('\t', ' ')
                        .Replace('\r', ' ');

                    vhSystem.GetConversationHandler().LoadTextToSpeech(textInput);
                    vhSystem.GetConversationHandler().LoadNVBG(textInput);
                }

            }
        }

        IEnumerator PlayPrerecordedAudio(AudioSpeechFile audioSpeechFile)
        {
            string nvbgResult = "";

            vhSystem.GetConversationContext().GetBackend<INonverbalGeneratorSystem>().GetNonverbalBehavior(
                "Kevin",
                audioSpeechFile.UtteranceText,
                (string result) =>
                {
                    nvbgResult = result;
                }
            );

            while (nvbgResult == "") yield return null;
            audioSpeechFile.ConvertedXml = nvbgResult;

            yield return vhSystem.ConversationManager.PlayConversationResponse(vhSystem.GetConversationContext().character, audioSpeechFile);
        }
        #endregion
    }
    
}
