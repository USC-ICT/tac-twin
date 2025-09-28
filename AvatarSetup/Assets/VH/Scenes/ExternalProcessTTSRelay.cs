using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride;

#if false
// TODO - when this is enable, it is causing Unity compiler trouble where it's not recognized as a MonoBehaviour and can't be used on a gameobject.
//        re-enable when fixed.
//namespace Ride
//{
public class ExternalProcessTTSRelay : RideSystemMonoBehaviour, IExternalProcess, ITextToSpeech
{
    public PublishSubscribeVHMsg m_publishSubscribe;
    public bool m_launchOnStartup = true;

    bool m_processLoaded = false;
    bool m_vrComponentReceived = false;


    static readonly string[] m_ttsVoices = new string[] { "Microsoft|David|Desktop", "Microsoft|Zira|Desktop" };

    bool m_remoteSpeechReplyReceived = false;
    string m_remoteSpeechReply = "";
    string m_remoteSpeechReplyAudioFile = "";


    public override void SystemInit()
    {
        base.SystemInit();

        m_publishSubscribe.Subscribe("vrComponent");
        m_publishSubscribe.Subscribe("RemoteSpeechReply");
        m_publishSubscribe.AddMessageEventHandler(PublishSubscribeMessageEvent);

        if (m_launchOnStartup)
            StartProcess();
    }

    public override void SystemShutdown()
    {
        base.SystemShutdown();

        StopProcess();
    }


    public bool ProcessLoaded { get { return m_processLoaded; } }

    public void StartProcess()
    {
        StartCoroutine(StartTTSRelay());
    }

    public void StopProcess()
    {
        StopTTSRelay();
    }


    public string[] GetAvailableVoices()
    {
        return m_ttsVoices;
    }


    public void GetTextToSpeech(string characterName, string voice, string text, ITextToSpeech.TextToSpeechResult resultCallback)
    {
        StartCoroutine(GetTextToSpeechInternal(characterName, voice, text, resultCallback));
    }

    public void GetTextToSpeechStream(string characterName, string voice, string text, ITextToSpeech.TextToSpeechResult resultCallback)
    {
        throw new NotImplementedException();
    }


    IEnumerator StartTTSRelay()
    {
        // vrComponent ttsmsspeechrelay  *or*
        // vrComponent tts msspeechrelay
        // vrKillComponent tts
        // vrProcEnd tts msspeechrelay

        if (!VHUtils.IsWindows())
            yield break;

        m_publishSubscribe.Publish("vrKillComponent tts");

        yield return new WaitForSeconds(0.5f);

        m_vrComponentReceived = false;
        m_processLoaded = false;
    
        // start external process
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = Application.streamingAssetsPath + "/" + "external" + "/" + "run-ttsrelay.bat",
            Arguments = Application.temporaryCachePath.Replace("/", @"\") + @"\" + "external" + @"\" + "ttsrelay",
            WorkingDirectory = Application.streamingAssetsPath + "/" + "external" + "/"
            //WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized
        });

        float startTime = Time.time;
        float timeOut = 18; // in seconds

        // wait for vrComponent message, or timeout after a period
        while (m_vrComponentReceived == false &&
               Time.time - startTime < timeOut)
        {
            yield return new WaitForEndOfFrame();
        }

        m_processLoaded = true;
    }

    void StopTTSRelay()
    {
        m_publishSubscribe.Publish("vrKillComponent tts");

        //System.Threading.Thread.Sleep(250);

        // pskill tts

        //System.Threading.Thread.Sleep(250);

        m_processLoaded = false;
    }


    IEnumerator GetTextToSpeechInternal(string characterName, string voice, string text, ITextToSpeech.TextToSpeechResult resultCallback)
    {
        // send RemoteSpeechCmd and wait for reply

        string remoteSpeechCmd = CreateRemoteSpeechCmdMessage(characterName, voice, text);

        m_remoteSpeechReplyReceived = false;
        m_remoteSpeechReply = "";
        m_remoteSpeechReplyAudioFile = "";

        m_publishSubscribe.Publish(remoteSpeechCmd);

        float startTime = Time.time;
        float timeOut = 10; // in seconds

        while (m_remoteSpeechReplyReceived == false &&
               Time.time - startTime < timeOut)
            yield return new WaitForEndOfFrame();

        if (resultCallback != null)
            resultCallback(m_remoteSpeechReply, m_remoteSpeechReplyAudioFile);
    }


    void PublishSubscribeMessageEvent(object sender, string message)
    {
        string[] splitargs = message.Split(' ');

        if (splitargs.Length > 0)
        {
            if (splitargs[0] == "vrComponent")
            {
                // vrComponent ttsmsspeechrelay  *or*
                // vrComponent tts msspeechrelay

                if (splitargs[1] == "tts")
                {
                    m_vrComponentReceived = true;
                }
            }
            else if (splitargs[0] == "RemoteSpeechReply")
            {
#if false
                    RemoteSpeechReply ChrBrad 1 OK: <?xml version="1.0" encoding="UTF-8"?>
                                          <speak>
                                           <soundFile name="D:\Projects\vhtoolkit\trunk\data\cache\audio\utt_20121109_153323_ChrBrad_1.wav" />
                                           <viseme start="0" articulation="0.4" type="open" />
                                           <mark name="T0" time="0.0454374998807907" />
                                           <word start="0.0454374998807907" end="0.275687515735626">
                                           <viseme start="0.0454374998807907" articulation="0.3" type="open" />
                                           <viseme start="0.156937494874001" articulation="0.5" type="open" />
                                           <viseme start="0.156937494874001" articulation="0.4" type="tBack" />
                                           <viseme start="0.156937494874001" articulation="0.6" type="wide" />
                                           <mark name="T1" time="0.275687515735626" />
                                           </word>
                                           <mark name="T2" time="0.275687515735626" />
                                           <word start="0.275687515735626" end="0.660875022411346">
                                           <viseme start="0.344500005245209" articulation="0.5" type="open" />
                                           <viseme start="0.344500005245209" articulation="0.4" type="tBack" />
                                           <viseme start="0.344500005245209" articulation="0.6" type="wide" />
                                           <viseme start="0.435937494039536" articulation="0.1" type="open" />
                                           <viseme start="0.435937494039536" articulation="0.7" type="W" />
                                           </word>
                                           <viseme start="0.660875022411346" articulation="0.4" type="open" />
                                           <mark name="T3" time="0.767687499523163" />
                                           <mark name="T4" time="0.767687499523163" />
                                           <word start="0.767687499523163" end="0.989937543869019">
                                           <viseme start="0.767687499523163" articulation="0.3" type="open" />
                                           <viseme start="0.855187475681305" articulation="0.6" type="wide" />
                                           <viseme start="0.855187475681305" articulation="0.5" type="open" />
                                           <viseme start="0.855187475681305" articulation="0.4" type="tBack" />
                                           <mark name="T5" time="0.989937543869019" />
                                           </word>
                                           <mark name="T6" time="0.989937543869019" />
                                           <word start="0.989937543869019" end="1.12156248092651">
                                           <viseme start="0.989937543869019" articulation="0.55" type="open" />
                                           <viseme start="1.02781248092651" articulation="0.7" type="W" />
                                           <viseme start="1.02781248092651" articulation="0.1" type="open" />
                                           <mark name="T7" time="1.12156248092651" />
                                           </word>
                                           <mark name="T8" time="1.12156248092651" />
                                           <word start="1.12156248092651" end="1.39037501811981">
                                           <viseme start="1.12156248092651" articulation="0.5" type="W" />
                                           <viseme start="1.12156248092651" articulation="0.3" type="ShCh" />
                                           <viseme start="1.12156248092651" articulation="0.4" type="tRoof" />
                                           <viseme start="1.25906252861023" articulation="0.55" type="W" />
                                           <viseme start="1.25906252861023" articulation="0.4" type="open" />
                                           <mark name="T9" time="1.39037501811981" />
                                           </word>
                                           <viseme start="1.39037501811981" articulation="0.4" type="open" />
                                          </speak>
#endif

                m_remoteSpeechReplyReceived = true;
                m_remoteSpeechReply = message;

                {
                    // really ugly
                    int start = message.IndexOf(@"<soundFile name=""") + 17;
                    int end = message.IndexOf(@""" />", start);
                    m_remoteSpeechReplyAudioFile = message.Substring(start, end - start);
                }
            }
        }
    }


    static string CreateRemoteSpeechCmdMessage(string characterName, string voice, string text)
    {
#if false
            RemoteSpeechCmd speak ChrBrad 1 Festival_voice_cmu_us_jmk_arctic_clunits ../../data/cache/audio/utt_20121109_153323_ChrBrad_1.aiff <?xml version="1.0" encoding="utf-16"?><speech id="sp1" ref="Anybody-1" type="application/ssml+xml"><mark name="T0" />Hi
                                    <mark name="T1" /><mark name="T2" />there,
                                    <mark name="T3" /><mark name="T4" />how
                                    <mark name="T5" /><mark name="T6" />are
                                    <mark name="T7" /><mark name="T8" />you.
                                    <mark name="T9" /></speech>

            RemoteSpeechCmd speak Ellie 1000001 Microsoft|Zira|Desktop D:/edwork/clovr/clovr/core/clovr/Assets/StreamingAssets/data/cache/audio/utt_170227_143912_821_MicrosoftZiraDesktop.aiff <?xml version="1.0" encoding="UTF-8"?><speech id=sp1 ref="Anybody-1" type="application/ssml+xml">Sorry to hear that.</speech>
#endif

        string audioFileName = string.Format("utt_{0}_{1}", DateTime.Now.ToString("yyMMdd_HHmmss_fff"), voice.Replace("|", ""));
        
        string outputPath = Application.streamingAssetsPath + "/data/cache/audio/";

        // add timing markers
        string[] textSplit = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        int markerNum = 0;
        string textMarkers = "";
        foreach (var s in textSplit)
        {
            textMarkers += string.Format(@"<mark name=""T{0}"" />", markerNum);
            markerNum++;
            textMarkers += s;
            textMarkers += " \n   ";
            textMarkers += string.Format(@"<mark name=""T{0}"" />", markerNum);
            markerNum++;
        }

        string speechXML = string.Format(@"<speech id=""{0}"" ref=""Anybody-1"" type=""application/ssml+xml"">{1}</speech>", "sp1", textMarkers);

        //response = @"RemoteSpeechCmd speak Rachel 1000001 Microsoft|Zira|Desktop ../../../inetpub/wwwroot/VHMsgAsp/Audio/" + AudioFileName + @".aiff <?xml version=""1.0"" encoding=""UTF-8""?>" + SpeechXML;
        string remoteSpeechCmd = string.Format(@"RemoteSpeechCmd speak {0} 1000001 {1} {2}{3}.aiff <?xml version=""1.0"" encoding=""UTF-8""?>{4}",
            characterName, voice, outputPath, audioFileName, speechXML);

        Debug.LogFormat("ExternalProcessTTSRelay.CreateRemoteSpeechCmdMessage() - {0}", remoteSpeechCmd);

        return remoteSpeechCmd;
    }   
}
//}
#endif
