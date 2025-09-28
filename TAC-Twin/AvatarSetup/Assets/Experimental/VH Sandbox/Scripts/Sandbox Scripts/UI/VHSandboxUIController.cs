using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride.UI;
using UnityEngine.EventSystems;

namespace Ride.Examples
{
    /// <summary>
    /// Main UI Controller script for the VH Sandbox Example
    /// Dropdown UI Ride IDs are linked to a specific VH backend system type for better scalability if system types are added/removed
    /// </summary>
    public class VHSandboxUIController : MenuMono
    {
        //Enum for VH Sub-menu tabs
        public enum ConfigMode
        {
            CREATE,
            EDIT,
            SAVE_LOAD
        }

        [Header("Config UI Groups")]
        [SerializeField] CanvasGroup m_configPanelGroup;
        [SerializeField] CanvasGroup m_configContentGroup;

        [Header("Config UI Widgets")]
        [SerializeField] List<RideToggleUGUI> m_menuTabToggles;
        [SerializeField] RideToggleUGUI m_editToggle;
        [SerializeField] RideToggleUGUI m_createToggle;
        [SerializeField] RideToggleUGUI m_saveLoadToggle;
        [SerializeField] RideInputFieldTMPro m_nameInputField;
        [SerializeField] RideInputFieldTMPro m_posXInputField;
        [SerializeField] RideInputFieldTMPro m_posZInputField;
        [SerializeField] RideInputFieldTMPro m_rotYInputField;
        [SerializeField] RideDropdownTMPro m_modelDropDown;
        [SerializeField] RideDropdownTMPro m_characterDropDown;
        [SerializeField] RideDropdownTMPro m_loadCharacterInputField;
        [SerializeField] RideDropdownTMPro m_ASRDropdown;
        [SerializeField] RideDropdownTMPro m_NLPDropdown;
        [SerializeField] RideDropdownTMPro m_NLPSentimentDropdown;
        [SerializeField] RideDropdownTMPro m_NLPEntitiesDropdown;
        [SerializeField] RideDropdownTMPro m_NVBGDropdown;
        [SerializeField] RideDropdownTMPro m_TTSDropdown;
        [SerializeField] RideDropdownTMPro m_voiceDropdown;
        [SerializeField] RideDropdownTMPro m_sensingDropdown;
        [SerializeField] RideToggleUGUI m_randomizeToggle;
        [SerializeField] RideButtonUGUI m_createButton;
        [SerializeField] RideButtonUGUI m_deleteButton;
        [SerializeField] RideButtonUGUI m_saveButton;
        [SerializeField] RideButtonUGUI m_loadButton;
        [SerializeField] RideButtonUGUI m_deleteSaveButton;

        [Header("Conversation Input UI Groups")]
        [SerializeField] CanvasGroup m_inputGroup;
        [SerializeField] CanvasGroup m_webCamGroup;
        [SerializeField] CanvasGroup m_audioGroup;
        [SerializeField] CanvasGroup m_textGroup;

        [Header("Conversation Input UI Widgets")]
        [SerializeField] RideToggleUGUI m_webcamToggle;
        [SerializeField] GameObject m_sensingTextParent;
        [SerializeField] RideRawImage m_webcamRawImage;
        [SerializeField] RideButtonUGUI m_textSubmitButton;
        [SerializeField] RideInputFieldTMPro m_textInputField;
        [SerializeField] RideButtonUGUI m_pushToTalkButton;

        [Header("Output UI Widgets")]
        [SerializeField] RideTextTMPro m_captionsText;

        [field: SerializeField] public ConfigMode configMode { get; protected set; } = ConfigMode.CREATE;
        [field: SerializeField] public bool inTextInput { get; protected set; } = false;
        [field: SerializeField] public bool randomizationEnabled { get; protected set; } = false;
        [field: SerializeField] public bool pushToTalkDown { get; protected set; } = false;

        public RideID modelDropdownID => m_modelDropDown.id;
        public RideID characterDropdownID => m_characterDropDown.id;
        public RideID loadCharacterDropdownID => m_loadCharacterInputField.id;
        public RideID nlpDropdownID => m_NLPDropdown.id;
        public RideID nlpEntitiesDropdownID => m_NLPEntitiesDropdown.id;
        public RideID nlpSentimentDropdownID => m_NLPSentimentDropdown.id;
        public RideID nvbgDropdownID => m_NVBGDropdown.id;
        public RideID ttsDropdownID => m_TTSDropdown.id;
        public RideID voiceDropdownID => m_voiceDropdown.id;
        public RideID asrDropdownID => m_ASRDropdown.id;
        public RideID sensingDropdownID => m_sensingDropdown.id;

        public event System.Action PushToTalkStarted;
        public event System.Action PushToTalkStopped;
        public event System.Action<bool> WebCamToggled;
        public event System.Action<string> TextSubmitted;
        public event System.Action<RideID, int> DropdownOptionSelected;

        public event System.Action<ConfigMode> ConfigModeChanged;
        public event System.Action<string> CreateAttempt;
        public event System.Action<string> DeleteAttempt;

        public event System.Action<Vector3> TransformValueChanged;

        public event System.Action SaveAttempt;
        public event System.Action LoadAttempt;
        public event System.Action DeleteSaveAttempt;

        Dictionary<RideID, RideDropdownTMPro> m_dropDownLookUp = new Dictionary<RideID, RideDropdownTMPro>();
        EventTrigger m_audioInputEventTrigger;


        protected virtual void Awake()
        {
            RegisterWidgets();
            InitalizeUIEvents();
        }

        #region UI Initalization & Release

        void RegisterWidgets()
        {
            AddDropDown(m_characterDropDown);
            AddDropDown(m_modelDropDown);
            AddDropDown(m_loadCharacterInputField);

            AddDropDown(m_NVBGDropdown);
            AddDropDown(m_TTSDropdown);
            AddDropDown(m_voiceDropdown);
            AddDropDown(m_ASRDropdown);
            AddDropDown(m_NLPDropdown);
            AddDropDown(m_sensingDropdown);
            AddDropDown(m_NLPEntitiesDropdown);
            AddDropDown(m_NLPSentimentDropdown);

            AddInputField(m_nameInputField);
            AddInputField(m_textInputField);
            AddInputField(m_posXInputField);
            AddInputField(m_posZInputField);
            AddInputField(m_rotYInputField);
        }

        //Common input field event registration
        void AddInputField(RideInputFieldTMPro inputField)
        {
            inputField.m_inputField.onSelect.AddListener((_) => OnTextInputStart());
            inputField.m_inputField.onDeselect.AddListener((_) => OnTextInputEnd());
            inputField.m_inputField.onSubmit.AddListener((_) => OnTextInputEnd());
        }

        //Registering dropdown ids along with common events
        void AddDropDown(RideDropdownTMPro dropdown)
        {
            if (!m_dropDownLookUp.TryAdd(dropdown.id, dropdown))
                return;

            dropdown.ClearItems();

            dropdown.m_dropdown.onValueChanged.AddListener((int option) => OnDropdownOptionSelect(dropdown.id, option));
        }

        void InitalizeUIEvents()
        {
            //Menu tabs
            m_createToggle.AddOnValueChanged((isCreate) => { if (isCreate) SetConfigMode(ConfigMode.CREATE); });
            m_editToggle.AddOnValueChanged((isEdit) => { if (isEdit) SetConfigMode(ConfigMode.EDIT); });
            m_saveLoadToggle.AddOnValueChanged((isSaveLoad) => { if (isSaveLoad) SetConfigMode(ConfigMode.SAVE_LOAD); });

            //Push-to-talk button needs an event trigger for press and hold functionality
            m_audioInputEventTrigger = m_pushToTalkButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry onPress = new EventTrigger.Entry();
            onPress.eventID = EventTriggerType.PointerDown;
            onPress.callback.AddListener((eventData) => { OnPushToTalkStart(); });

            EventTrigger.Entry onRelease = new EventTrigger.Entry();
            onRelease.eventID = EventTriggerType.PointerUp;
            onRelease.callback.AddListener((eventData) => { OnPushToTalkStop(); });

            m_audioInputEventTrigger.triggers.Add(onPress);
            m_audioInputEventTrigger.triggers.Add(onRelease);

            //User text input
            m_textSubmitButton.onClick.AddListener(OnTextSubmit);
            m_textInputField.m_inputField.onSubmit.AddListener(_ => OnTextSubmit());

            //Webcam
            m_webcamToggle.AddOnValueChanged(OnWebCamToggled);
            m_webcamRawImage.Show(false);
            m_sensingTextParent.SetActive(false);

            //Misc
            m_randomizeToggle.AddOnValueChanged(OnRandomizationToggled);

            m_nameInputField.m_inputField.onEndEdit.AddListener(CheckCharacterNameInput);

            //Menu buttons
            m_createButton.onClick.AddListener(OnCharacterCreate);
            m_deleteButton.onClick.AddListener(OnCharacterDelete);
            m_saveButton.onClick.AddListener(OnSaveAttempt);
            m_loadButton.onClick.AddListener(OnLoadAttempt);
            m_deleteSaveButton.onClick.AddListener(OnDeleteSaveAttempt);

            //Position/Rotation input
            m_posXInputField.m_inputField.onEndEdit.AddListener(OnTransformValueChanged);
            m_posZInputField.m_inputField.onEndEdit.AddListener(OnTransformValueChanged);
            m_rotYInputField.m_inputField.onEndEdit.AddListener(OnTransformValueChanged);
        }

        //Validate character name text field
        void CheckCharacterNameInput(string input)
        {
            m_createButton.isInteractable = !(input == string.Empty);
        }
        #endregion

        //Getters/Setters for input widget state + interaction
        #region  UI Updating

        public void SetConfigMode(ConfigMode newConfigMode)
        {
            if (configMode == newConfigMode) return;
            configMode = newConfigMode;
            ConfigModeChanged?.Invoke(configMode);
        }

        public void SetDropdownsToDefault()
        {
            foreach (var dropdown in m_dropDownLookUp.Values)
            {
                dropdown.selection = 0;
            }
        }

        public void FillDropdownText(RideID id, List<string> options)
        {
            if (!m_dropDownLookUp.TryGetValue(id, out RideDropdownTMPro dropdown))
                return;

            dropdown.ClearItems();
            dropdown.m_dropdown.AddOptions(options);
        }

        public int GetDropdownOptionSelected(RideID id)
        {
            if (!m_dropDownLookUp.TryGetValue(id, out RideDropdownTMPro dropdown))
                return 0;

            return dropdown.selection;
        }

        public string GetDropdownOptionText(RideID id)
        {
            if (!m_dropDownLookUp.TryGetValue(id, out RideDropdownTMPro dropdown) ||
            dropdown.numItems == 0)
                return string.Empty;



            return dropdown.m_dropdown.options[dropdown.selection].text;
        }

        public bool SetDropdownOption(RideID id, int option)
        {
            if (!m_dropDownLookUp.TryGetValue(id, out RideDropdownTMPro dropdown) || dropdown.m_dropdown.options.Count < option)
                return false;

            dropdown.m_dropdown.SetValueWithoutNotify(option);
            dropdown.m_dropdown.RefreshShownValue();

            return true;
        }

        public void SetDeleteButtonInteraction(bool interactable) => m_deleteButton.isInteractable = interactable;

        public void SetInputText(string text) => m_textInputField.text = text;

        public void SetCaptionText(string text) => m_captionsText.text = text;

        public void SetCharacterNameText(string text) => m_nameInputField.text = text;

        public void SetTransformValues(Vector3 value)
        {
            m_posXInputField.text = value.x.ToString();
            m_posZInputField.text = value.z.ToString();
            m_rotYInputField.text = value.y.ToString();
        }

        public void DisableUserInteration() => SetUserInteration(false);

        public void EnableUserInteraction() => SetUserInteration(true);

        public void SetUserInteration(bool interactable)
        {
            SetAudioInteraction(interactable);
            SetTextInteraction(interactable);
        }

        public void SetTextInteraction(bool interactable)
        {
            m_textInputField.isInteractable = interactable;
            m_textSubmitButton.isInteractable = interactable;
        }

        public void SetAudioInteraction(bool interactable)
        {
            m_pushToTalkButton.isInteractable = interactable;
            m_audioInputEventTrigger.enabled = interactable;
        }

        public void SetSaveButtonInteraction(bool interactable)
        {
            m_saveButton.isInteractable = interactable;
        }

        public void SetLoadButtonInteraction(bool interactable)
        {
            m_loadButton.isInteractable = interactable;
            m_deleteSaveButton.isInteractable = interactable;
        }

        public void DisableConfig()
        {
            m_configPanelGroup.alpha = 0;
            m_configPanelGroup.interactable = false;
        }

        public void EnableConfig()
        {
            m_configPanelGroup.alpha = 1;
            m_configPanelGroup.interactable = true;
        }

        public bool configEnabled => m_configPanelGroup.interactable;

        public void SetWebcamTexture(Texture texture)
        {
            m_webcamRawImage.texture = texture;
        }

        public void SetWebcamTexture(Material material)
        {
            m_webcamRawImage.texture = material.mainTexture;
            m_webcamRawImage.m_image.material = material;
        }

        public void SetConfigCreateTabInteraction(bool interactable)
        {
            m_createToggle.isInteractable = interactable;
        }

        public void SetConfigEditTabInteraction(bool interactable)
        {
            m_editToggle.isInteractable = interactable;
        }

        public void RandomizeDropdown(RideID id, params int[] skips)
        {
            RideDropdownTMPro dropdown = m_dropDownLookUp[id];
            int current = GetDropdownOptionSelected(id);

            List<int> indices = new();
            for (int i = 0; i < dropdown.numItems; i++)
            {
                if (i == current) continue;
                bool skip = false;
                for (int j = 0; j < skips.Length; j++)
                {
                    if (i == skips[j])
                    {
                        skip = true;
                        break;
                    }
                }
                if (skip) continue;

                indices.Add(i);
            }

            int randIndex = Random.Range(0, indices.Count);

            SetDropdownOption(id, indices.Count > 0 ? indices[randIndex] : 0);
        }

        public void CycleCurrentMenuTab()
        {
            bool activateNext = false;

            foreach (RideToggleUGUI toggle in m_menuTabToggles)
            {
                if (toggle.isOn)
                    activateNext = true;
                else if (activateNext)
                {
                    toggle.isOn = true;
                    activateNext = false;
                    break;
                }
            }

            if (activateNext)
            {
                m_menuTabToggles[0].isOn = true;
            }
        }
        #endregion

        //Trigger UI functionality without user input but retain UI state transitioning
        #region  Manual UI Commands

        public void ManualPushToTalkStart()
        {
            if (!m_audioInputEventTrigger.enabled) return;

            m_pushToTalkButton.m_button.OnPointerDown(new PointerEventData(EventSystem.current));
            OnPushToTalkStart();

        }

        public void ManualPushToTalkStop()
        {
            if (!m_audioInputEventTrigger.enabled) return;

            m_pushToTalkButton.m_button.OnPointerUp(new PointerEventData(EventSystem.current));
            OnPushToTalkStop();
        }

        public void ManualTextInputSelection()
        {
            if (!m_textInputField.isInteractable) return;

            m_textInputField.m_inputField.Select();
        }

        public void ManualWebCamToggle()
        {
            if (!m_webcamToggle.isInteractable) return;

            OnWebCamToggled(!m_webcamToggle.isOn);
            m_webcamToggle.m_toggle.SetIsOnWithoutNotify(!m_webcamToggle.isOn);
        }

        public void ManualSwapConfigTab()
        {
            m_createToggle.isOn = m_editToggle.isOn;
        }

        #endregion

        #region  UI Events & Callbacks

        public void OnPushToTalkStart()
        {
            pushToTalkDown = true;
            PushToTalkStarted?.Invoke();
            Debug.Log("Push to talk start");
        }

        public void OnPushToTalkStop()
        {
            pushToTalkDown = false;
            PushToTalkStopped?.Invoke();
            Debug.Log("Push to talk stop");
        }

        public void OnTextSubmit()
        {
            if (m_textInputField.text == string.Empty) return;

            TextSubmitted?.Invoke(m_textInputField.text);
        }

        public void OnWebCamToggled(bool toggled)
        {
            WebCamToggled?.Invoke(toggled);
            m_webcamRawImage.Show(toggled);
            m_sensingTextParent.SetActive(toggled);
        }

        public void OnRandomizationToggled(bool toggled)
        {
            randomizationEnabled = toggled;
        }

        public void OnDropdownOptionSelect(RideID id, int option)
        {
            DropdownOptionSelected?.Invoke(id, option);
        }

        public void OnCharacterCreate()
        {
            CreateAttempt?.Invoke(m_nameInputField.text);
        }

        public void OnCharacterDelete()
        {
            DeleteAttempt?.Invoke(GetDropdownOptionText(characterDropdownID));
        }

        public void OnTextInputStart() => inTextInput = true;
        public void OnTextInputEnd() => inTextInput = false;

        public void OnSaveAttempt()
        {
            SaveAttempt?.Invoke();
        }

        public void OnLoadAttempt()
        {
            LoadAttempt?.Invoke();
        }

        public void OnDeleteSaveAttempt()
        {
            DeleteSaveAttempt?.Invoke();
        }

        public void OnTransformValueChanged(string text)
        {
            Vector3 value = new Vector3(
                float.Parse(m_posXInputField.text),
                float.Parse(m_rotYInputField.text),
                float.Parse(m_posZInputField.text)
                );

            TransformValueChanged?.Invoke(value);
        }

        #endregion
    }
}