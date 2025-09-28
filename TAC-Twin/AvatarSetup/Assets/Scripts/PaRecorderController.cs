using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder;
#endif

public class PaRecorderController 
{
#if UNITY_EDITOR
    private RecorderControllerSettings m_controllerSettings;
    private RecorderController m_recorderController;
#endif
    //private bool isRecording = false;


    public void StartRecording()
    {
#if UNITY_EDITOR
        //if (isRecording) { return; }

        string movieName = SceneManager.GetActiveScene().name + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        StartRecording(movieName);
#endif
    }


    public void StartRecording(string movieName)
    {
#if UNITY_EDITOR
        //if (isRecording) { return; }
        SetUpRecording(movieName);
        m_recorderController.PrepareRecording();
        m_recorderController.StartRecording();
        //isRecording = true;
#endif
    }


    public void StopRecording()
    {
#if UNITY_EDITOR
        //if (isRecording == false) { return; }
        //isRecording = false;
        if(m_recorderController == null) return;
        m_recorderController.StopRecording();
#endif
    }


    private void SetUpRecording(string movieName)
    {
#if UNITY_EDITOR
        m_controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_recorderController = new RecorderController(m_controllerSettings);

        var movieRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder.name = "Foo";
        movieRecorder.Enabled = true;
        movieRecorder.AudioInputSettings.PreserveAudio = true;

        movieRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 1920,
            OutputHeight = 1080
        };

        string savePath = Path.Combine(Application.dataPath, "../Movies");
        Directory.CreateDirectory(savePath); // Make sure the folder exists

        movieRecorder.OutputFile = Path.Combine(savePath, movieName);

        m_controllerSettings.AddRecorderSettings(movieRecorder);
        m_controllerSettings.SetRecordModeToManual();
        m_controllerSettings.FrameRate = 60.0f;
#endif
    }
}
