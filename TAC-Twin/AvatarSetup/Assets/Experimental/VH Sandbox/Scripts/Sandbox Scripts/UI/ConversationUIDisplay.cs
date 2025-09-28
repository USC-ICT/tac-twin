using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ride.UI;
using UnityEngine.EventSystems;
using Ride.Conversation;

namespace Ride.Examples
{
    /// <summary>
    /// Simple UI text display script for a conversation
    /// </summary>
    public class ConversationUIDisplay : MenuMono
    {
        [SerializeField] RideTextTMPro m_characterTextLabel;
        [SerializeField] RideTextTMPro m_conversationStateTextLabel;
        [SerializeField] RideTextTMPro m_lastInputTextLabel;
        [SerializeField] RideTextTMPro m_lastOutputTextLabel;
        [SerializeField] RideTextTMPro m_responseTimeTextLabel;
        [SerializeField] RideTextTMPro m_lastTTSResultTextLabel;
        [SerializeField] RideTextTMPro m_lastNVBGResultTextLabel;
        [SerializeField] RideTextTMPro m_lastSentimentTextLabel;
        [SerializeField] RideTextTMPro m_lastEntitiesTextLabel;

        private void Awake()
        {
            m_characterTextLabel.text =
            m_conversationStateTextLabel.text =
            m_lastInputTextLabel.text =
            m_lastNVBGResultTextLabel.text =
            m_lastTTSResultTextLabel.text =
            m_lastOutputTextLabel.text =
            m_lastSentimentTextLabel.text =
            m_lastEntitiesTextLabel.text =
            string.Empty;
        }

        public void UpdateUIDisplay(ConversationHandler conversation)
        {
            m_characterTextLabel.text = conversation.context.character.name;
            m_conversationStateTextLabel.text = conversation.state.ToString();

            m_lastInputTextLabel.text = conversation.latestResponseData.inputQuestion;
            m_lastOutputTextLabel.text = conversation.latestResponseData.outputResponse;

            m_lastTTSResultTextLabel.text = conversation.latestResponseData.ttsLipsyncSchedule;
            m_lastNVBGResultTextLabel.text = conversation.latestResponseData.nvbgResult;

            m_lastSentimentTextLabel.text = conversation.latestResponseData.sentimentResult;
            m_lastEntitiesTextLabel.text = conversation.latestResponseData.entitiesResult;

            m_responseTimeTextLabel.text = conversation.latestResponseData.responseTime.ToString();
        }
    }
}