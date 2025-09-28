using Ride;
using Ride.Examples;
using Ride.IO;
using Ride.NLP;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using VHAssets;

public class VHDemo : RideBaseMinimal
{
#if false
    public SpeechRecognizerDictationRecognizer m_speechRecognizerSystem;
    public ExternalProcessTTSRelay m_externalProcessTTSRelay;
    public ExternalProcessElsender m_externalProcessElsender;
    public Ride.NLP.AzureQnAMakerSystem m_azureQnA;
    public Ride.NLP.AzureTASentimentSystem m_azureSentiment;
    public Ride.NLP.AzureTAEntitiesSystem m_azureEntities;
    

    #region NVBG Backends

    private enum NvbgBackend
    {
        Local = 1,
        Remote = 2,
        External = 3,
    }

    private NvbgBackend m_nvbgBackend = NvbgBackend.External;

    public ExternalProcessNVBG m_nvbgExternal;
    public NonverbalBehaviorGeneratorSystem m_nvbgLocal;
    public RestfulNonverbalBehaviorGeneratorSystem m_nvbgRemote;

    private INonverbalGeneratorSystem m_nvbg => m_nvbgBackend switch
    {
        NvbgBackend.Local => m_nvbgLocal,
        NvbgBackend.Remote => m_nvbgRemote,
        NvbgBackend.External => m_nvbgExternal,
        _ => throw new InvalidOperationException(),
    };
    #endregion

    public bool m_useChatGPT;
    public OpenAIChatGPTSystem m_openAIChatGPT;

    string m_characterName = "Kevin";

    string m_speechRecognizerResultType = "";
    string m_speechRecognizerResult = "";

    string m_nlpInput = "Hello how are you?";
    string m_nlpResult = "";
    string m_nlpSentiment = "";
    string m_nlpEntities = "";

    string m_nvbgInput = "Hello how are you?";

    string m_ttsInput = "Hello how are you?";

    string m_fullInput = "Hello how are you?";
    string m_fullAnswer = "";
    bool m_fullPath = false;  // run through the full code path:  recording -> reply and play utterance

    string m_nonverbalBehaviorResult = "";

    int m_ttsVoiceIdx = 0;
    string m_textToSpeechLipsyncSchedule = "";
    string m_textToSpeechAudioFilePath = "";

    UnityWebRequest m_wwwAudio = null;

    Vector2 m_postureScrollPosition;
    static readonly string [] m_postures =
    {
        "IdleSittingUpright01",
        "IdleStandingUpright01",
    };
    Vector2 m_animScrollPosition;
    static readonly string [] m_animations =
    {
        "IdleSittingUpright01_CenterSmLf01",
        "IdleSittingUpright01_CoughMedLf01",
        "IdleSittingUpright01_FrontSmRt01",
        "IdleSittingUpright01_LeftMedLf01",
        "IdleStandingUpright01_Entrance01",
        "IdleStandingUpright01_Exit01",
        "IdleStandingUpright01_YouLf01",
        "IdleStandingUpright01_MeLf01",
    };

    bool m_greetingPlayed = false;


    protected override void Start()
    {
        base.Start();

        AddDebugMenu("VHDemo", OnGUIVHDemo);
        AddDebugMenu("VHDemo2", OnGUIVHDemo2);
        AddDebugMenu("VHDemo3", OnGUIVHDemo3);
        m_debugMenu.ShowMenu(true);
        m_debugMenu.SetMenu(m_debugMenu.GetNumberMenus() - 3);


        m_speechRecognizerSystem.OnStartRecording += () =>
        {
            Debug.LogFormat("VHDemo OnStartRecording received");

            m_speechRecognizerResult = "";
        };
        m_speechRecognizerSystem.OnHypothesis += (hypothesis) =>
        {
            Debug.LogFormat("VHDemo OnHypothesis - {0}", hypothesis);

            m_speechRecognizerResultType = "hyp: ";
            m_speechRecognizerResult = hypothesis;

            m_fullInput = hypothesis;
        };
        m_speechRecognizerSystem.OnStopRecording += (result, success) =>
        {
            Debug.LogFormat("VHDemo OnStopRecording - success: {0} - result: {1}", success, result);

            m_speechRecognizerResultType = "result: ";
            m_speechRecognizerResult = result;

            m_fullInput = result;

            if (m_fullPath)
            {
                m_fullPath = false;

                if (!m_useChatGPT)
                {
                    m_azureQnA.AskQuestion(m_fullInput, (response) =>
                    {
                        Ride.NLP.NLPQnAAnswer answer = (Ride.NLP.NLPQnAAnswer)response;
                        if (answer == null)
                            m_fullAnswer = "Sorry, I can't answer that.";
                        else
                            m_fullAnswer = answer.m_Answers[0]; // For now, always pick the first answer

                        var voices = m_externalProcessTTSRelay.GetAvailableVoices();
                        string voice = voices[m_ttsVoiceIdx];
                        AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
                        StartCoroutine(SendTTSAndNvbgAndPlayAudio(audioSource, m_characterName, voice, m_fullAnswer));
                    });
                }
                else
                {
                    m_openAIChatGPT.AskQuestion(m_fullInput, (response) =>
                    {
                        string answer = ((NLPQnAAnswer)response).m_Answers[0];
                        m_fullAnswer = answer;

                        Debug.Log(answer);

                        var voices = m_externalProcessTTSRelay.GetAvailableVoices();
                        string voice = voices[m_ttsVoiceIdx];
                        AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
                        StartCoroutine(SendTTSAndNvbgAndPlayAudio(audioSource, m_characterName, voice, answer));

                        m_greetingPlayed = true;
                    });
                }
            }
        };


        var configSystem = Globals.api.systemAccessSystem.GetSystem<Ride.RideConfigSystem>();
        m_azureQnA.m_uri = string.Format("{0}/qnamaker/knowledgebases/{1}/generateAnswer", configSystem.config.azureQnA.endpoint, configSystem.config.azureQnA.kbId);
        m_azureQnA.m_authorizationKey = "EndpointKey " + configSystem.config.azureQnA.endpointKey;
        m_azureSentiment.m_uri = string.Format("{0}/text/analytics/v3.0-preview.1/sentiment", configSystem.config.azureTA.endpoint);
        m_azureSentiment.m_authorizationKey = configSystem.config.azureTA.endpointKey;
        m_azureEntities.m_uri = string.Format("{0}/text/analytics/v3.0/entities/recognition/general", configSystem.config.azureTA.endpoint);
        m_azureEntities.m_authorizationKey = configSystem.config.azureTA.endpointKey;
        m_openAIChatGPT.m_uri = configSystem.config.openAIChatGPT.endpoint;
        m_openAIChatGPT.m_authorizationKey = configSystem.config.openAIChatGPT.endpointKey;


        StartCoroutine(StartProcesses());
    }


    protected override void Update()
    {
        base.Update();

        if (Globals.api.inputSystem.GetKeyDown(RideKeyCode.Space, RideInputLayer.System))
        {
            if (m_speechRecognizerSystem.IsRecording)
            {
                m_speechRecognizerSystem.StopRecording();
            }
            else
            {
                m_fullPath = true;
                m_speechRecognizerSystem.StartRecording();
            }
        }

        if (Globals.api.inputSystem.GetKeyDown(RideKeyCode.F12, RideInputLayer.System))
        {
            PlayerInputController controller = GameObject.FindObjectOfType<PlayerInputController>();
            controller.Active = !controller.Active;
        }
    }


    void OnGUIVHDemo()
    {
        var configSystem = Globals.api.systemAccessSystem.GetSystem<Ride.RideConfigSystem>();

        GUILayout.Space(10);

        DrawGUILabel("VH Services");

        using (new GUILayout.HorizontalScope())
        {
            void drawToggle(NvbgBackend backend, INonverbalGeneratorSystem nvbg)
            {
                var isSelected = m_nvbgBackend == backend;
                var name = Enum.GetName(typeof(NvbgBackend), backend);
                var text = $"{name}:{(nvbg.ProcessLoaded ? "ON" : (isSelected ? "<color=red>OFF</color>" : "OFF"))}";
                if (DrawGUIToggle(isSelected, text))
                {
                    m_nvbgBackend = backend;
                }
            }
            DrawGUILabel("NVBG:");
            drawToggle(NvbgBackend.Local, m_nvbgLocal);
            drawToggle(NvbgBackend.External, m_nvbgExternal);
            drawToggle(NvbgBackend.Remote, m_nvbgRemote);
            DrawGUILabel("");

            if (DrawGUIButton("Run")) { m_nvbg.StartProcess(m_characterName); }
            if (DrawGUIButton("Close")) { m_nvbg.StopProcess(); }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (m_externalProcessTTSRelay.ProcessLoaded) DrawGUILabel("TTSRelay: ON");
            else                                         DrawGUILabel("<color=red>TTSRelay: OFF</color>");

            if (DrawGUIButton("Run")) { m_externalProcessTTSRelay.StartProcess(); }
            if (DrawGUIButton("Close")) { m_externalProcessTTSRelay.StopProcess(); }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (m_externalProcessElsender.ProcessLoaded) DrawGUILabel("Elsender: ON");
            else                                         DrawGUILabel("<color=red>Elsender: OFF</color>");

            if (DrawGUIButton("Run")) { m_externalProcessElsender.StartProcess(); }
            if (DrawGUIButton("Close")) { m_externalProcessElsender.StopProcess(); }
        }

        using (new GUILayout.HorizontalScope())
        {
            if (DrawGUIButton("Run All"))
            {
                StartCoroutine(StartProcesses());
            }

            if (DrawGUIButton("Close All"))
            {
                m_nvbgLocal.StopProcess();
                m_nvbgRemote.StopProcess();
                m_nvbgExternal.StopProcess();
                m_externalProcessTTSRelay.StopProcess();
                m_externalProcessElsender.StopProcess();
            }
        }

        GUILayout.Space(10);

        DrawGUILabel("ASR (Local DictationRecognizer)");

        DrawGUILabel(string.Format("PhraseRecogntionSystem.isSupported: {0}", m_speechRecognizerSystem.PhraseRecognitionSystemIsSupported));
        DrawGUILabel(string.Format("PhraseRecogntionSystem.status: {0}", m_speechRecognizerSystem.PhraseRecognitionSystemStatus));

        if (m_speechRecognizerSystem.SpeechRecognizerStatus == "Running")
            DrawGUILabel(string.Format("<color=lime>Dictation.status: {0}</color>", m_speechRecognizerSystem.SpeechRecognizerStatus));
        else
            DrawGUILabel(string.Format("Dictation.status: {0}", m_speechRecognizerSystem.SpeechRecognizerStatus));
        DrawGUILabel(string.Format("Dictation.AutoSilenceTimeoutSeconds: {0}", m_speechRecognizerSystem.AutoSilenceTimeoutSeconds));
        DrawGUILabel(string.Format("Dictation.InitialSilenceTimeoutSeconds: {0}", m_speechRecognizerSystem.InitialSilenceTimeoutSeconds));

        if (m_speechRecognizerSystem.IsRecording)
        {
            if (DrawGUIButton("Stop"))
            {
                m_speechRecognizerSystem.StopRecording();
            }
        }
        else
        {
            if (DrawGUIButton("Start"))
            {
                m_speechRecognizerSystem.StartRecording();
            }
        }

        DrawGUILabel(string.Format("{0}{1}", m_speechRecognizerResultType, m_speechRecognizerResult));

        GUILayout.Space(20);

        DrawGUILabel("NLP - Azure QnA Maker");

        using (new GUILayout.HorizontalScope())
        {
            DrawGUILabel("Endpoint:");
            string endpoint = DrawGUITextField(configSystem.config.azureQnA.endpoint, 250);
            if (endpoint != configSystem.config.azureQnA.endpoint)
            {
                configSystem.SetQnAMaker(endpoint.Trim(), configSystem.config.azureQnA.endpointKey, configSystem.config.azureQnA.kbId);
                configSystem.Save();

                m_azureQnA.m_uri = string.Format("{0}/qnamaker/knowledgebases/{1}/generateAnswer", configSystem.config.azureQnA.endpoint, configSystem.config.azureQnA.kbId);
                m_azureQnA.m_authorizationKey = "EndpointKey " + configSystem.config.azureQnA.endpointKey;
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            DrawGUILabel("Key:");
            string key = DrawGUITextField(configSystem.config.azureQnA.endpointKey, 250);
            if (key != configSystem.config.azureQnA.endpointKey)
            {
                configSystem.SetQnAMaker(configSystem.config.azureQnA.endpoint, key.Trim(), configSystem.config.azureQnA.kbId);
                configSystem.Save();

                m_azureQnA.m_uri = string.Format("{0}/qnamaker/knowledgebases/{1}/generateAnswer", configSystem.config.azureQnA.endpoint, configSystem.config.azureQnA.kbId);
                m_azureQnA.m_authorizationKey = "EndpointKey " + configSystem.config.azureQnA.endpointKey;
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            DrawGUILabel("kbId:");
            string kbId = DrawGUITextField(configSystem.config.azureQnA.kbId, 250);
            if (kbId != configSystem.config.azureQnA.kbId)
            {
                configSystem.SetQnAMaker(configSystem.config.azureQnA.endpoint, configSystem.config.azureQnA.endpointKey, kbId.Trim());
                configSystem.Save();

                m_azureQnA.m_uri = string.Format("{0}/qnamaker/knowledgebases/{1}/generateAnswer", configSystem.config.azureQnA.endpoint, configSystem.config.azureQnA.kbId);
                m_azureQnA.m_authorizationKey = "EndpointKey " + configSystem.config.azureQnA.endpointKey;
            }
        }

        m_nlpInput = DrawGUITextField(m_nlpInput);

        if (DrawGUIButton("Send"))
        {
            m_azureQnA.AskQuestion(m_nlpInput, (response) =>
            {
                Ride.NLP.NLPQnAAnswer answer = (Ride.NLP.NLPQnAAnswer)response;
                if (answer == null)
                    m_nlpResult = "Sorry, I can't answer that.";
                else
                    m_nlpResult = answer.m_Answers[0]; // For now, always pick the first answer
            });

            //TSS.Globals.api.serviceSystem.GetService<TSS.NLP.INLPSentimentService>(m_azureSentimentID).AnalyzeSentiment(m_nlpInput, (response) =>
            //{
            //    TSS.NLP.NLPSentimentResponse output = (TSS.NLP.NLPSentimentResponse)response;
            //    m_nlpSentiment = string.Format(
            //        "Text Analytics\n"  +
            //        "Sentiment: \t{0}\n" +
            //        "Positive: \t\t{1}\n" +
            //        "Neutral: \t\t{2}\n" +
            //        "Negative: \t\t{3}\n", output.mainSentiment, output.positiveScore, output.neutralScore, output.negativeScore);
            //    ;
            //});

            //TSS.Globals.api.serviceSystem.GetService<TSS.NLP.INLPEntitiesService>(m_azureEntitiesID).AnalyzeEntities(m_nlpInput, (response) =>
            //{
            //    TSS.NLP.NLPEntitiesResponse output = (TSS.NLP.NLPEntitiesResponse)response;
            //
            //    string s = "";
            //    foreach (var e in output.entities) 
            //        s += string.Format("Text: {0}, Category: {1}\n", e.text, e.category);
            //
            //    m_nlpEntities = s;
            //});
        }

        DrawGUILabel(string.Format("result: {0}", m_nlpResult));
        DrawGUILabel(string.Format("sentiment: {0}", m_nlpSentiment));
        DrawGUILabel(string.Format("entities: {0}", m_nlpEntities));

        GUILayout.Space(20);

        DrawGUILabel("NVBG (local)");

        m_nvbgInput = DrawGUITextField(m_nvbgInput);

        if (DrawGUIButton("Send"))
        {
            m_nvbg.GetNonverbalBehavior(m_characterName, m_nvbgInput, (result) =>
            {
                m_nonverbalBehaviorResult = result;
            });
        }

        DrawGUILabel(string.Format("result: {0}...", m_nonverbalBehaviorResult.Substring(0, Math.Min(300, m_nonverbalBehaviorResult.Length))));

        GUILayout.Space(20);

        DrawGUILabel("TTS (Local TTSRelay)");

        m_ttsInput = DrawGUITextField(m_ttsInput);

        m_ttsVoiceIdx = DrawGUISelectionGrid(m_ttsVoiceIdx, m_externalProcessTTSRelay.GetAvailableVoices(), 1);

        if (DrawGUIButton("Send"))
        {
            var voices = m_externalProcessTTSRelay.GetAvailableVoices();
            string voice = voices[m_ttsVoiceIdx];
            AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
            StartCoroutine(SendTTSAndPlayAudioNoCharacter(audioSource, m_characterName, voice, m_ttsInput));
        }

        DrawGUILabel(string.Format("Sound Path: {0}", m_textToSpeechAudioFilePath));
    }

    void OnGUIVHDemo2()
    {
        GUILayout.Space(10);

        DrawGUILabel("Full Q&A");

        GUILayout.Space(10);

        if (m_speechRecognizerSystem.SpeechRecognizerStatus == "Running")
            DrawGUILabel(string.Format("<color=lime>Dictation.status: {0}</color>", m_speechRecognizerSystem.SpeechRecognizerStatus));
        else
            DrawGUILabel(string.Format("Dictation.status: {0}", m_speechRecognizerSystem.SpeechRecognizerStatus));

        if (m_speechRecognizerSystem.IsRecording)
        {
            if (DrawGUIButton("Stop"))
            {
                m_speechRecognizerSystem.StopRecording();
            }
        }
        else
        {
            if (DrawGUIButton("Start"))
            {
                m_speechRecognizerSystem.StartRecording();
            }
        }

        m_fullInput = DrawGUITextField(m_fullInput);

        GUILayout.Space(10);

        if (DrawGUIButton("Send"))
        {
            if (!m_useChatGPT)
            {
                m_azureQnA.AskQuestion(m_fullInput, (response) =>
                {
                    Ride.NLP.NLPQnAAnswer answer = (Ride.NLP.NLPQnAAnswer)response;
                    if (answer == null)
                        m_fullAnswer = "Sorry, I can't answer that.";
                    else
                        m_fullAnswer = answer.m_Answers[0]; // For now, always pick the first answer

                    var voices = m_externalProcessTTSRelay.GetAvailableVoices();
                    string voice = voices[m_ttsVoiceIdx];
                    AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
                    StartCoroutine(SendTTSAndNvbgAndPlayAudio(audioSource, m_characterName, voice, m_fullAnswer));
                });
            }
            else
            {
                m_openAIChatGPT.AskQuestion(m_fullInput, (response) =>
                {
                    string answer = ((NLPQnAAnswer)response).m_Answers[0];
                    m_fullAnswer = answer;

                    Debug.Log(answer);

                    var voices = m_externalProcessTTSRelay.GetAvailableVoices();
                    string voice = voices[m_ttsVoiceIdx];
                    AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
                    StartCoroutine(SendTTSAndNvbgAndPlayAudio(audioSource, m_characterName, voice, answer));

                    m_greetingPlayed = true;
                });
            }
        }

        m_fullAnswer = DrawGUITextField(m_fullAnswer);
    }

    void OnGUIVHDemo3()
    {
        GUILayout.Space(10);

        DrawGUILabel("Animations");

        GUILayout.Space(10);

        DrawGUILabel("Idle");
        m_postureScrollPosition = GUILayout.BeginScrollView(m_postureScrollPosition, GUILayout.Height(90));
        foreach (var posture in m_postures)
        {
            if (DrawGUIButton(posture))
            {
                var characterObj = GameObject.Find(m_characterName);
                ICharacter character = characterObj.GetComponent<ICharacter>();
                character.PlayPosture(posture, 0);
            }
        }
        GUILayout.EndScrollView();

        DrawGUILabel("Animation");
        m_animScrollPosition = GUILayout.BeginScrollView(m_animScrollPosition, GUILayout.Height(150));
        foreach (var animation in m_animations)
        {
            if (DrawGUIButton(animation))
            {
                var characterObj = GameObject.Find(m_characterName);
                ICharacter character = characterObj.GetComponent<ICharacter>();
                character.PlayAnim(animation);
            }
        }
        GUILayout.EndScrollView();

        DrawGUISpace();

        DrawGUILabel("Expressions");
        if (DrawGUIButton("Nod"))
        {
            float amount = 0.5f;
            float numTimes = 2.0f;
            float duration = 2.0f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.Nod(amount, numTimes, duration);
        }

        if (DrawGUIButton("Shake"))
        {
            float amount = 0.5f;
            float numTimes = 2.0f;
            float duration = 1.0f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.Shake(amount, numTimes, duration);
        }

        if (DrawGUIButton("Open"))
        {
            string viseme = "open";
            float weight = 1.0f;
            float totalTime = 2.0f;
            float blendTime = 0.1f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.PlayViseme(viseme, weight, totalTime, blendTime);
        }

        if (DrawGUIButton("Wide"))
        {
            string viseme = "wide";
            float weight = 1.0f;
            float totalTime = 2.0f;
            float blendTime = 0.1f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.PlayViseme(viseme, weight, totalTime, blendTime);
        }

        if (DrawGUIButton("Happy"))
        {
            string viseme = "112_happy";
            float weight = 1.0f;
            float totalTime = 2.0f;
            float blendTime = 0.1f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.PlayViseme(viseme, weight, totalTime, blendTime);
        }

        if (DrawGUIButton("Sad"))
        {
            string viseme = "130_sad";
            float weight = 1.0f;
            float totalTime = 2.0f;
            float blendTime = 0.1f;
            var characterObj = GameObject.Find(m_characterName);
            ICharacter character = characterObj.GetComponent<ICharacter>();
            character.PlayViseme(viseme, weight, totalTime, blendTime);
        }
    }


    IEnumerator StartProcesses()
    {
        m_nvbg.StartProcess();
        m_externalProcessTTSRelay.StartProcess();
        m_externalProcessElsender.StartProcess();
        //StartCoroutine(StartNPCEditor());
        //StartCoroutine(StartFlores());
        //StartCoroutine(StartLogger());

        while (!(m_nvbg.ProcessLoaded &&
                 m_externalProcessTTSRelay.ProcessLoaded &&
                 m_externalProcessElsender.ProcessLoaded))
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.LogFormat("VHDemo.StartProcesses() - All external processes loaded");
    }


    IEnumerator LoadAudio(string audioPath)
    {
        // download audio

        string urlPath;
        if (audioPath.StartsWith("//"))  // network path
            urlPath = "file://" + audioPath;
        else  // assume absolute path
            urlPath = "file:///" + audioPath;

        Debug.LogFormat("LoadAudio() - {0}", urlPath);

        m_wwwAudio = UnityWebRequestMultimedia.GetAudioClip(urlPath, AudioType.WAV);
        yield return m_wwwAudio.SendWebRequest();

        while (!m_wwwAudio.isDone)
            yield return new WaitForEndOfFrame();

        Debug.LogFormat("LoadAudio() - LoadState - {0}", DownloadHandlerAudioClip.GetContent(m_wwwAudio).loadState);

        // WebGL incorrectly reports the status as 'Unloaded' even if it's loaded.  So, this will endless loop on WebGL
        //while (wwwAudio.audioClip.loadState == AudioDataLoadState.Unloaded ||
        //       wwwAudio.audioClip.loadState == AudioDataLoadState.Loading)
        //    yield return new WaitForEndOfFrame();

        if (!string.IsNullOrEmpty(m_wwwAudio.error))
        {
            Debug.LogErrorFormat("LoadAudio() - Failed to download utterance {0}", audioPath);
        }
    }


    IEnumerator SendTTSAndPlayAudioNoCharacter(AudioSource audioSource, string characterName, string voice, string text)
    {
        // send TTS and wait for reply
        bool getTTSFinished = false;
        m_externalProcessTTSRelay.GetTextToSpeech(characterName, voice, text, (lipsyncSchedule, audioFilePath) =>
        {
            m_textToSpeechLipsyncSchedule = lipsyncSchedule;
            m_textToSpeechAudioFilePath = audioFilePath;
            getTTSFinished = true;
        });
        yield return new WaitUntil(() => getTTSFinished);

        // load the audio file off disk
        yield return StartCoroutine(LoadAudio(m_textToSpeechAudioFilePath));

        audioSource.clip = DownloadHandlerAudioClip.GetContent(m_wwwAudio);
        audioSource.Play();
    }


    void PlayAudio(UnityWebRequest wwwAudio, ICharacter character, string xmlText = "")
    {
        // get the xml part of the remote speech reply message
        string[] remoteSpeechMsgSplit = m_textToSpeechLipsyncSchedule.Split(' ');
        string remoteSpeechXml = string.Join(" ", remoteSpeechMsgSplit, 4, remoteSpeechMsgSplit.Length - 4);

        TtsReader m_ttsReader = FindObjectOfType<TtsReader>();

        // parse the xml result into tts word timing
        string uttName;
        TtsReader.TtsData ttsData = m_ttsReader.ReadTtsXml(remoteSpeechXml, out uttName);

        // convert the tts word timing data to a facefx curve string
        string facefxCurveInfo = VisemeFormatConverter.ConvertTtsToFaceFx(ttsData);

        // build an audio speech file from the curve, xml, and audio clip data
        AudioSpeechFile ttsUtterance = AudioSpeechFile.CreateAudioSpeechFile(facefxCurveInfo, xmlText, DownloadHandlerAudioClip.GetContent(wwwAudio));

        character.PlayAudio(ttsUtterance);

        if (!string.IsNullOrEmpty(xmlText))
            character.PlayXml(ttsUtterance);
    }

    IEnumerator SendTTSAndNvbgAndPlayAudio(AudioSource audioSource, string characterName, string voice, string text)
    {
        // send TTS and wait for reply
        bool getTTSFinished = false;
        m_externalProcessTTSRelay.GetTextToSpeech(characterName, voice, text, (lipsyncSchedule, audioFilePath) =>
        {
            m_textToSpeechLipsyncSchedule = lipsyncSchedule;
            m_textToSpeechAudioFilePath = audioFilePath;
            getTTSFinished = true;
        });
        yield return new WaitUntil(() => getTTSFinished);

        // send NVBG and wait for reply
        bool getNonverbalBehaviorFinished = false;
        m_nvbg.GetNonverbalBehavior(m_characterName, m_nvbgInput, (result) =>
        {
            m_nonverbalBehaviorResult = result;
            getNonverbalBehaviorFinished = true;
        });
        yield return new WaitUntil(() => getNonverbalBehaviorFinished);

        // load the audio file off disk
        yield return StartCoroutine(LoadAudio(m_textToSpeechAudioFilePath));

        var characterObj = GameObject.Find(characterName);
        ICharacter character = characterObj.GetComponent<ICharacter>();
        PlayAudio(m_wwwAudio, character, m_nonverbalBehaviorResult);
    }

    public void PlayGreeting(GameObject target)
    {
        if (m_greetingPlayed)
            return;

        string greeting = "Hello, my name is Kevin. I can answer questions about RIDE. How can I help you today?";

        var voices = m_externalProcessTTSRelay.GetAvailableVoices();
        string voice = voices[m_ttsVoiceIdx];
        AudioSource audioSource = GameObject.FindObjectOfType<Camera>().GetComponent<AudioSource>();
        StartCoroutine(SendTTSAndNvbgAndPlayAudio(audioSource, m_characterName, voice, greeting));

        m_greetingPlayed = true;
    }

    public void ResetGreeting()
    {
        m_greetingPlayed = false;
    }
#endif
}
