using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VHAssets;

namespace Ride.Conversation
{
    /// <summary>
    /// Plays a conversation response with audio + tts
    /// Based on legacy VH toolkit code
    /// </summary>
    [RequireComponent(typeof(TtsReader))]
    public class ConversationResponsePlayer : MonoBehaviour
    {

        TtsReader m_ttsReader;
        AudioClip m_audioClip;

        void Start()
        {
            m_ttsReader ??= GetComponent<TtsReader>();
        }

        public IEnumerator PlayResponse(ICharacter character, ConversationHandler.ConversationResponse response)
        {
            m_audioClip = null;

            yield return LoadAudio(response.ttsAudioFilePath);
            // get the xml part of the remote speech reply message
            string facefxCurveInfo = " ";

            if (response.ttsLipsyncSchedule != string.Empty)
            {
                string remoteSpeechXml = response.ttsLipsyncSchedule.Substring(response.ttsLipsyncSchedule.IndexOf('<'));
                // parse the xml result into tts word timing
                string uttName;
                TtsReader.TtsData ttsData = m_ttsReader.ReadTtsXml(remoteSpeechXml, out uttName);

                // convert the tts word timing data to a facefx curve string
                facefxCurveInfo = VisemeFormatConverter.ConvertTtsToFaceFx(ttsData);
            }
            // build an audio speech file from the curve, xml, and audio clip data
            AudioSpeechFile ttsUtterance = AudioSpeechFile.CreateAudioSpeechFile(facefxCurveInfo, response.nvbgResult, m_audioClip);

            MecanimManager.Get().FindAudioFiles();
            character.PlayAudio(ttsUtterance);

            if (!string.IsNullOrEmpty(response.nvbgResult) && !string.IsNullOrEmpty(response.ttsLipsyncSchedule))
                character.PlayXml(ttsUtterance);

            yield return new WaitForSeconds(ttsUtterance.ClipLength);
        }

        public IEnumerator PlayResponse(ICharacter character, AudioSpeechFile audioSpeechFile)
        {
            MecanimManager.Get().FindAudioFiles();
            character.PlayAudio(audioSpeechFile);
            character.PlayXml(audioSpeechFile);

            yield return new WaitForSeconds(audioSpeechFile.ClipLength);
        }

        public IEnumerator PlayResponse(ICharacter character, AudioSpeechFile audioSpeechFile, string lipsyncSchedule)
        {
            string remoteSpeechXml = lipsyncSchedule.Substring(lipsyncSchedule.IndexOf('<'));
            // parse the xml result into tts word timing
            string uttName;
            TtsReader.TtsData ttsData = m_ttsReader.ReadTtsXml(remoteSpeechXml, out uttName);

            // convert the tts word timing data to a facefx curve string
            string facefxCurveInfo = VisemeFormatConverter.ConvertTtsToFaceFx(ttsData);
            audioSpeechFile.BmlText = facefxCurveInfo;
            audioSpeechFile.ReadBmlData();

            MecanimManager.Get().FindAudioFiles();
            character.PlayAudio(audioSpeechFile);
            character.PlayXml(audioSpeechFile);

            yield return new WaitForSeconds(audioSpeechFile.ClipLength);
        }



        IEnumerator LoadAudio(string audioPath)
        {
            AudioType audioType = AudioType.WAV;

            switch (System.IO.Path.GetExtension(audioPath))
            {
                case ".mp3":
                    audioType = AudioType.MPEG;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
            }

            // download audio
            audioPath = audioPath.Replace("\\", "/");
            string urlPath;
            if (audioPath.StartsWith("//"))  // network path
                urlPath = "file://" + audioPath;
            else  // assume absolute path
                urlPath = "file:///" + audioPath;

            Debug.LogFormat("LoadAudio() - {0}", urlPath);
            System.UriBuilder uriBuilder = new System.UriBuilder("file://" + audioPath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uriBuilder.Uri, audioType))
            {
                var request = www.SendWebRequest();

                while (!request.isDone) yield return null;

                m_audioClip = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}
