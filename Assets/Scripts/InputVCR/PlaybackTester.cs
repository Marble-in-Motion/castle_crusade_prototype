using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class PlaybackTester : MonoBehaviour
{
    public Recording.InputInfo[] inputs;

	private int recordingFrameRate = 60;
	public TestStatus status = TestStatus.RECORD; // initial mode that vcr is operating in

	private enum KeyState {UP, HELD, DOWN};
	public enum TestStatus { RECORD, PLAYBACK, STOP }

	private Dictionary<string,KeyState> keyDownStatuses = new Dictionary<string, KeyState> ()
	{
		{"left",KeyState.UP},
		{"space",KeyState.UP},
		{"right",KeyState.UP},
		{"v",KeyState.UP},
		{"s",KeyState.UP},
		{"d",KeyState.UP},
		{"f",KeyState.UP},
		{"backspace",KeyState.UP},
		{"return",KeyState.UP},
	};
	
	Recording currentRecording;

	private int currentFrame;

	public void StartRecording()
	{
		currentRecording = new Recording( recordingFrameRate );
		currentFrame = 0;
		status = TestStatus.RECORD;
	}

	public string SaveRecording()
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
			}
			else
			{
				foreach (var input in currentRecording.GetInputs ( currentFrame )) {
					if (keyDownStatuses [input.inputName] == KeyState.UP && input.buttonState == false) {
						keyDownStatuses [input.inputName] = KeyState.UP;
					} else if (keyDownStatuses [input.inputName] == KeyState.UP && input.buttonState == true) {
						keyDownStatuses [input.inputName] = KeyState.DOWN;
					} else if (keyDownStatuses [input.inputName] == KeyState.DOWN && input.buttonState == true) {
						keyDownStatuses [input.inputName] = KeyState.HELD;
					} else if ((keyDownStatuses [input.inputName] == KeyState.DOWN || keyDownStatuses [input.inputName] == KeyState.HELD) && input.buttonState == false) {
						keyDownStatuses [input.inputName] = KeyState.UP;
					}
				}
				currentFrame += 1;
			}
		} else if (status == TestStatus.RECORD)
		{
			foreach(var input in inputs)
			{
            	input.buttonState = Input.GetKey(input.inputName);
                currentRecording.AddInput (currentFrame, input);
			}
			currentFrame++;
		}
	}

    public bool GetKey(string keyName)
    {
		if (status == TestStatus.PLAYBACK)
			return (keyDownStatuses [keyName] == KeyState.DOWN ||keyDownStatuses [keyName] == KeyState.HELD);
        else
            return Input.GetKey (keyName);
    }

    public bool GetKeyDown(string keyName)
    {
		if (status == TestStatus.PLAYBACK)
		{
			return keyDownStatuses [keyName] == KeyState.DOWN;
		}
        else
            return Input.GetKeyDown (keyName);
    }

    public bool GetKeyUp(string keyName)
    {	
		if (status == TestStatus.PLAYBACK)
			return keyDownStatuses [keyName] == KeyState.UP;
        else
            return Input.GetKeyUp (keyName);
    }
}
