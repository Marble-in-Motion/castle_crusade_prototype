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

	float realRecordingTime;

	private enum KeyState {UP, HELD, DOWN};
	public enum TestStatus { RECORD, PLAYBACK, STOP }

	private Dictionary<string,KeyState> keyDownStatuses = new Dictionary<string, KeyState> ()
	{
		{"left",KeyState.UP},
		{"space",KeyState.UP},
		{"right",KeyState.UP}
	};
	
	Recording currentRecording;		// the recording currently in the VCR. Copy or ToString() this to save.
	public float currentTime{
		get {
			return currentFrame / (float) currentFrameRate; }
	}

	public int currentFrameRate{
		get {
			if ( currentRecording == null )
				return recordingFrameRate;
			else
				return currentRecording.frameRate;
		}
	}

	public int currentFrame{ get; private set; }	// current frame of recording/playback		

	public event System.Action finishedPlayback;	// sent when playback finishes

	public void Record()
	{
		if ( currentRecording == null || currentRecording.recordingLength == 0 )
			NewRecording();
		else
			status = TestStatus.RECORD;
	}

	public void NewRecording()
	{
		// start recording live input
		currentRecording = new Recording( recordingFrameRate );
		currentFrame = 0;
		realRecordingTime = 0;
		print ("new recording");
		status = TestStatus.RECORD;
	}
	
	/// <summary>
	/// Play the specified recording, from optional specified time
	/// </summary>
	/// <param name='recording'>
	/// Recording.
	/// </param>
	/// <param name='startRecordingFromTime'>
	/// OPTIONAL: Time to start recording at
	/// </param>
	public void Play( Recording recording, float startRecordingFromTime = 0 )
	{	
		currentRecording = recording;
		currentFrame = recording.GetClosestFrame ( startRecordingFromTime );

		status = TestStatus.PLAYBACK;
	}

	public void Stop()
	{			
		status = TestStatus.STOP;
		currentFrame = 0;
	}

	public Recording GetRecording()
	{
        return currentRecording;
	}
	
	void LateUpdate()
	{	
		if ( status == TestStatus.PLAYBACK )
		{
			if ( currentFrame > currentRecording.totalFrames )
			{
				// end of recording
				if ( finishedPlayback != null )
					finishedPlayback( );
				Stop ();
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
		} else if ( status == TestStatus.RECORD )
		{
			realRecordingTime += Time.deltaTime;
			// record current input to frames, until recording catches up with realtime
			while ( currentTime < realRecordingTime )
			{
                // and keycodes & buttons defined in inputsToRecord
				foreach( var input in inputs )
				{
					print ("recording move");
                	input.buttonState = Input.GetKey( input.inputName );
                    currentRecording.AddInput ( currentFrame, input );
				}
				currentFrame++;
			}
		}
	}

    public bool GetKey( string keyName )
    {
		if ( status == TestStatus.PLAYBACK)
			return (keyDownStatuses [keyName] == KeyState.DOWN ||keyDownStatuses [keyName] == KeyState.HELD);
        else
            return Input.GetKey ( keyName );
    }

    public bool GetKeyDown( string keyName )
    {
		if (status == TestStatus.PLAYBACK)
		{
			return keyDownStatuses [keyName] == KeyState.DOWN;
		}
        else
            return Input.GetKeyDown ( keyName );
    }

    public bool GetKeyUp( string keyName )
    {	
		if (status == TestStatus.PLAYBACK)
			return keyDownStatuses [keyName] == KeyState.UP;
        else
            return Input.GetKeyUp ( keyName );
    }
}
