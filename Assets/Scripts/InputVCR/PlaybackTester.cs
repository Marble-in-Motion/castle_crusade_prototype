using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;


/**
 * Custom regression testing suite
 **/
public class PlaybackTester : MonoBehaviour
{
    public Recording.InputInfo[] inputs;

    private int recordingFrameRate = 60;

    // initial mode that vcr is operating in
    public TestStatus status = TestStatus.RECORD;

    private enum KeyState { UP, HELD, DOWN };
    public enum TestStatus { RECORD, PLAYBACK, STOP }

    private Dictionary<string, KeyState> keyDownStatuses = new Dictionary<string, KeyState>()
    {
        {Params.LEFT_KEY,KeyState.UP},
        {Params.SHOOT_KEY,KeyState.UP},
        {Params.RIGHT_KEY,KeyState.UP},
        {Params.VOLLEY_KEY,KeyState.UP},
        {Params.VOLLEY_KEY_ALT,KeyState.UP},
        {Params.SK_KEY,KeyState.UP},
        {Params.SK_KEY_ALT,KeyState.UP}
    };

    private int currentTest = 0;
    private int playerId;
    private bool testing = false;

    Recording currentRecording;

    private int currentFrame;

    public void StartRecording()
    {
        currentRecording = new Recording(recordingFrameRate);
        currentFrame = 0;
        status = TestStatus.RECORD;
    }

    public string StopRecording()
    {
        status = TestStatus.STOP;
        return currentRecording.ToString();
    }

    public void StartPlayback(Recording recording)
    {
        currentRecording = recording;
        currentFrame = 0;

        status = TestStatus.PLAYBACK;
    }

    void LateUpdate()
    {
        if (status == TestStatus.PLAYBACK)
        {
            if (currentFrame > currentRecording.totalFrames)
            {
                status = TestStatus.STOP;
                if (testing)
                {
                    if (currentTest < 5)
                    {
                        currentTest += 1;
                        LoadNextTest();
                    }
                    else
                    {
                        currentTest = 0;
                        testing = false;
                    }
                }
            }
            else
            {
                foreach (var input in currentRecording.GetInputs(currentFrame))
                {
                    if (keyDownStatuses[input.inputName] == KeyState.UP && input.buttonState == false)
                    {
                        keyDownStatuses[input.inputName] = KeyState.UP;
                    }
                    else if (keyDownStatuses[input.inputName] == KeyState.UP && input.buttonState == true)
                    {
                        keyDownStatuses[input.inputName] = KeyState.DOWN;
                    }
                    else if (keyDownStatuses[input.inputName] == KeyState.DOWN && input.buttonState == true)
                    {
                        keyDownStatuses[input.inputName] = KeyState.HELD;
                    }
                    else if ((keyDownStatuses[input.inputName] == KeyState.DOWN || keyDownStatuses[input.inputName] == KeyState.HELD) && input.buttonState == false)
                    {
                        keyDownStatuses[input.inputName] = KeyState.UP;
                    }
                }
                currentFrame += 1;
            }
        }
        else if (status == TestStatus.RECORD)
        {
            foreach (var input in inputs)
            {
                input.buttonState = Input.GetKey(input.inputName);
                currentRecording.AddInput(currentFrame, input);
            }
            currentFrame++;
        }
    }

    public bool GetKey(string keyName)
    {
        if (status == TestStatus.PLAYBACK)
            return (keyDownStatuses[keyName] == KeyState.DOWN || keyDownStatuses[keyName] == KeyState.HELD);
        else
            return Input.GetKey(keyName);
    }

    public bool GetKeyDown(string keyName)
    {
        if (status == TestStatus.PLAYBACK)
        {
            return keyDownStatuses[keyName] == KeyState.DOWN;
        }
        else
            return Input.GetKeyDown(keyName);
    }

    public bool GetKeyUp(string keyName)
    {
        if (status == TestStatus.PLAYBACK)
            return keyDownStatuses[keyName] == KeyState.UP;
        else
            return Input.GetKeyUp(keyName);
    }

    private void LoadNextTest()
    {
        string path;
        if (Params.TESTS[currentTest] == "Volley_Test" || Params.TESTS[currentTest] == "Send_Shoot_Test")
        {
            path = String.Format("exports/Recordings/Tests/{0}_{1}.json", Params.TESTS[currentTest], playerId);
        }
        else
        {
            path = String.Format("exports/Recordings/Tests/{0}.json", Params.TESTS[currentTest]);
        }

        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            Recording recording = Recording.ParseRecording(json);
            StartPlayback(recording);
        }
    }

    public void RunTests(int id, int team)
    {
        playerId = team - 1;
        currentTest = 0;
        testing = true;
        LoadNextTest();
    }
}
