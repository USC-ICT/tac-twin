using System;
using System.Collections;
using System.Collections.Generic;
using Ride.NLP;
using Ride.UI;
using System.Diagnostics;
using Ride.TextToSpeech;
using Ride.SpeechRecognition;

namespace Ride.Conversation
{
    [System.Serializable]
    public class ConversationHandler
    {
        /// <summary>
        /// Simple state machine style controller for a conversation with a VH
        /// </summary>
        [System.Serializable]
        public struct ConversationResponse
        {
            public string inputQuestion;
            public string outputResponse;
            public string ttsAudioFilePath;
            public string ttsLipsyncSchedule;
            public string nvbgResult;

            public string sentimentResult;
            public string entitiesResult;
            public float responseTime;
        }

        public enum ConversationState
        {
            INACTIVE,
            IDLE,
            LISTENING,
            PROCESSING_QUESTION,
            PROCESSING_TTS_NVBG,
            RESPONDING,
        }

#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        public ConversationContext context;
#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        ConversationState m_state = ConversationState.INACTIVE;
        ConversationState m_prevState = ConversationState.INACTIVE;

#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        ConversationResponse m_latestResponseData;

        public ConversationResponse latestResponseData => m_latestResponseData;

        Stopwatch m_stopwatch;

        bool m_ttsProcessCompleted;
        bool m_nvbgProcessCompleted;
        string m_currentSpeechResult;



        public ConversationState state
        {
            get { return m_state; }
            protected set
            {
                m_prevState = m_state;
                m_state = value;

                if (m_state != m_prevState)
                    ConversationStateChange?.Invoke(m_state);

            }
        }

        public event System.Action<ConversationState> ConversationStateChange;
        public event System.Action ConversationBegan;
        public event System.Action ConversationEnded;
        public event System.Action<string> SpeechRecognized;
        public event System.Action<ConversationHandler> ResponseReady;

        public ConversationHandler(ConversationContext context)
        {
            this.context = context;
            m_latestResponseData = new ConversationResponse();
        }

        public void BeginConversation()
        {
            if (state != ConversationState.INACTIVE) return;

            state = ConversationState.IDLE;
            ConversationBegan?.Invoke();
        }

        public void EndConversation()
        {
            state = ConversationState.INACTIVE;
            ConversationEnded?.Invoke();
        }

        public void StartListening()
        {
            if (state != ConversationState.IDLE) return;

            m_latestResponseData.inputQuestion =
            m_currentSpeechResult = string.Empty;

            if (context.GetBackend<ISpeechRecognitionSystem>()?.IsRecognizing != false)
                return;

            context.GetBackend<ISpeechRecognitionSystem>().PartialSpeechRecognized += ProcessSpeechResultPartial;
            context.GetBackend<ISpeechRecognitionSystem>().SpeechRecognized += ProcessSpeechResult;

            context.GetBackend<ISpeechRecognitionSystem>().StartRecognizing();

            if (context.GetBackend<ISpeechRecognitionSystem>().IsRecognizing)
                state = ConversationState.LISTENING;
        }

        public void StopListening()
        {
            if (state != ConversationState.LISTENING) return;

            if (context.GetBackend<ISpeechRecognitionSystem>().IsRecognizing)
                context.GetBackend<ISpeechRecognitionSystem>().StopRecognizing();


            context.GetBackend<ISpeechRecognitionSystem>().PartialSpeechRecognized -= ProcessSpeechResultPartial;
            context.GetBackend<ISpeechRecognitionSystem>().SpeechRecognized -= ProcessSpeechResult;

            if (string.IsNullOrEmpty(m_latestResponseData.inputQuestion))
                m_latestResponseData.inputQuestion = m_currentSpeechResult;

            if (!string.IsNullOrEmpty(m_latestResponseData.inputQuestion))
                AskQuestion(m_latestResponseData.inputQuestion);
            else state = ConversationState.IDLE;
        }

        void ProcessSpeechResultPartial(object sender, SpeechRecognizedEventArgs e)
        {
            m_currentSpeechResult = e.Text;
            SpeechRecognized?.Invoke(m_currentSpeechResult);
        }

        void ProcessSpeechResult(object sender, SpeechRecognizedEventArgs e)
        {
            ProcessSpeechResultPartial(sender, e);
            m_latestResponseData.inputQuestion = m_currentSpeechResult;
        }

        public void AskQuestion(string questionText)
        {
            questionText = questionText.Replace("\n", "").Replace("\r", "").Trim();
            if (state != ConversationState.IDLE
            && state != ConversationState.LISTENING) return;

            var request = new NlpRequest(questionText);

            m_latestResponseData.outputResponse = string.Empty;
            m_latestResponseData.ttsLipsyncSchedule = string.Empty;
            m_latestResponseData.ttsAudioFilePath = string.Empty;
            m_latestResponseData.nvbgResult = string.Empty;

            m_latestResponseData.inputQuestion = request.content;

            state = ConversationState.PROCESSING_QUESTION;

            m_stopwatch = new Stopwatch();
            m_stopwatch.Start();

            context.GetBackend<INlpSystem>().Request(request, ProcessAnswer);
            context.GetBackend<INLPEntitiesSystem>()?.AnalyzeEntities(questionText, ProcessEntities);
            context.GetBackend<INlpSentimentSystem>()?.AnalyzeSentiment(questionText, ProcessSentiment);

        }

        void ProcessAnswer(NlpResponse response)
        {
            var answer = response;
            if (answer == null)
                m_latestResponseData.outputResponse = "Sorry, I can't answer that.";
            else
                m_latestResponseData.outputResponse = answer.content[0].Trim();


            m_stopwatch.Stop();
            m_latestResponseData.responseTime = (float)System.Math.Round(m_stopwatch.Elapsed.TotalMilliseconds / 1000.0f, 2);

            state = ConversationState.PROCESSING_TTS_NVBG;

            string filteredResponse = m_latestResponseData.outputResponse
                .Replace('\n', ' ')
                .Replace('\t', ' ')
                .Replace('\r', ' ')
                .Replace("&", "&amp;")
                .Replace(":", "");

            m_ttsProcessCompleted = m_nvbgProcessCompleted = false;

            LoadTextToSpeech(filteredResponse);
            LoadNVBG(filteredResponse);

        }

        void ProcessSentiment(NlpResponse response)
        {
            NlpSentimentResponse output = (NlpSentimentResponse)response;
            m_latestResponseData.sentimentResult =
                "Text Analytics:\n" +
                "\nSentiment:\t " + output.mainSentiment +
                "\nPositive:\t " + output.positiveScore +
                "\nNeutral:\t " + output.neutralScore +
                "\nNegative:\t " + output.negativeScore;

            ConversationStateChange?.Invoke(state);
        }

        void ProcessEntities(NlpResponse response)
        {
            NLPEntitiesResponse output = (NLPEntitiesResponse)response;
            string s = "Entities:";
            string ss = s;

            foreach (var e in output.entities)
            {
                s += "\nText: " + e.text + ", Categorie: " + e.category;
            }

            if (ss != s)
            {
                m_latestResponseData.entitiesResult = s;
            }
            else
            {
                m_latestResponseData.entitiesResult = s + "\nNo result";
            }

            ConversationStateChange?.Invoke(state);
        }

        public void LoadTextToSpeech(string response)
        {
            if (context.GetBackend<ILipsyncedTextToSpeechSystem>() != null)
            {
                var voices = context.GetBackend<ILipsyncedTextToSpeechSystem>().GetAvailableVoices();
                string voice = voices[context.voiceIndex];
                context.GetBackend<ILipsyncedTextToSpeechSystem>().CreateTextToSpeech(voice, response, ProcessTextToSpeech);
            }

            else
                m_ttsProcessCompleted = true;
        }

        void ProcessTextToSpeech(string lipsyncSchedule, string audioFilePath)
        {
            m_latestResponseData.ttsLipsyncSchedule = lipsyncSchedule;
            m_latestResponseData.ttsAudioFilePath = audioFilePath;

            m_ttsProcessCompleted = true;

            NotifyReadyToRespond();
        }

        public void LoadNVBG(string input)
        {
            if (context.GetBackend<INonverbalGeneratorSystem>() != null)
                context.GetBackend<INonverbalGeneratorSystem>().GetNonverbalBehavior("Kevin", input, ProcessNVBG);
            else
                m_nvbgProcessCompleted = true;
        }

        void ProcessNVBG(string result)
        {
            m_latestResponseData.nvbgResult = result;

            m_nvbgProcessCompleted = true;

            NotifyReadyToRespond();
        }

        //Invokes a callback once the response is ready
        void NotifyReadyToRespond()
        {
            if (!m_ttsProcessCompleted || !m_nvbgProcessCompleted) return;

            if (state == ConversationState.INACTIVE) return;

            state = ConversationState.RESPONDING;
            ResponseReady?.Invoke(this);
        }

        //This needs to be called after a response is ready to return back to Idle
        public void CompleteResponse()
        {
            if (state != ConversationState.RESPONDING) return;
            state = ConversationState.IDLE;

            m_nvbgProcessCompleted = m_ttsProcessCompleted = false;
        }

        public void ForceCompleteNVBG() => m_nvbgProcessCompleted = true;
        public void ForceCompleteTTS() => m_ttsProcessCompleted = true;

    }
}