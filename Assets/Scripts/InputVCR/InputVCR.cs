/* InputVCR.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------
 * Place on any object you wish to use to record or playback any inputs for
 * Switch modes to change current behaviour
 *   - Passthru : object will use live input commands from player
 *   - Record : object will use, as well as record, live input commands from player
 *   - Playback : object will use either provided input string or last recorded string rather than live input
 *   - Pause : object will take no input (buttons/axis will be frozen in last positions)
 * 
 * -----------
 * Recordings are all saved to the 'currentRecording' member, which you can get with GetRecording(). This can then be copied 
 * to a new Recording object to be saved and played back later.
 * Call ToString() on these recordings to get a text version of this if you want to save a recording after the program exits.
 * -----------
 * To use, place in a gameobject, and have all scripts in the object refer to it instead of Input.
 * 
 * eg: instead of Input.GetButton( "Jump" ), you would use vcr.GetButton( "Jump" ), where vcr is a 
 * reference to the component in that object
 * If VCR is in playback mode, and the "Jump" input was recorded, it will give the recorded input state, 
 * otherwise it will just pass through the live input state
 * 
 * Note, InputVCR can't be statically referenced like Input, since you may have multiple objects playing
 * different recordings, or an object playing back while another is taking live input...
 * ----------
 * Use this snippet in scripts you wish to replace Input with InputVCR, so they can be used in objects without a VCR as well:
 
  private bool useVCR;
  private InputVCR vcr;
  
  void Awake()
  {
    Transform root = transform;
	while ( root.parent != null )
		root = root.parent;
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
 * Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
 * Easy! 
 * -------------
 * More information and tools at grapefruitgames.com, @eddiecameron, or support@grapefruitgames.com
 * 
 * This script is open source under the GNU LGPL licence. Do what you will with it! 
 * http://www.gnu.org/licenses/lgpl.txt
 * 
 */
using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class InputVCR : MonoBehaviour
{
	#region Inspector properties
    public Recording.InputInfo[] inputsToRecord;

	public int recordingFrameRate = 30;
    [SerializeField]
	private InputVCRMode _mode = InputVCRMode.Passthru; // initial mode that vcr is operating in
	public InputVCRMode mode
	{
		get { return _mode; }
	}
	#endregion
	
	float realRecordingTime;

	private enum KeyState {UP, HELD, DOWN};
	public enum InputVCRMode { Passthru, Record, Playback, Pause }

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
	
	/// <summary>
	/// Start recording. Will append to already started recording
	/// </summary>
	public void Record()
	{
		if ( currentRecording == null || currentRecording.recordingLength == 0 )
			NewRecording();
		else
			_mode = InputVCRMode.Record;
	}
	
	/// <summary>
	/// Starts a new recording. If old recording wasn't saved it will be lost forever!
	/// </summary>
	public void NewRecording()
	{
		// start recording live input
		currentRecording = new Recording( recordingFrameRate );
		currentFrame = 0;
		realRecordingTime = 0;

		_mode = InputVCRMode.Record;
	}
	
	/// <summary>
	/// Start playing back the current recording, if present.
	/// If currently paused, will just resume
	/// </summary>
	public void Play()
	{
		// if currently paused during playback, will continue
		if ( mode == InputVCRMode.Pause )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recording
			Play ( currentRecording );
		}
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
		
		_mode = InputVCRMode.Playback;
	}
	
	/// <summary>
	/// Pause recording or playback. All input will be blocked while paused
	/// </summary>
	public void Pause()
	{
		_mode = InputVCRMode.Pause;
	}
	
	/// <summary>
	/// Stop recording or playback and rewind Live input will be passed through
	/// </summary>
	public void Stop()
	{			
		_mode = InputVCRMode.Passthru;
		currentFrame = 0;
	}
		
	/// <summary>
	/// Gets a copy of the current recording
	/// </summary>
	/// <returns>
	/// The recording.
	/// </returns>
	public Recording GetRecording()
	{
        return currentRecording;
	}
	
	void LateUpdate()
	{	
		if ( _mode == InputVCRMode.Playback )
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
		} else if ( _mode == InputVCRMode.Record )
		{	
			realRecordingTime += Time.deltaTime;
			// record current input to frames, until recording catches up with realtime
			while ( currentTime < realRecordingTime )
			{
                // and keycodes & buttons defined in inputsToRecord
				foreach( var input in inputsToRecord )
				{
                    switch ( input.inputType )
                    {
                    case Recording.InputInfo.InputType.Axis:
                        input.axisValue = Input.GetAxis( input.inputName );
                        break;
                    case Recording.InputInfo.InputType.Button:
                        input.buttonState = Input.GetButton( input.inputName );
                        break;
                    case Recording.InputInfo.InputType.Key:
                        input.buttonState = Input.GetKey( input.inputName );
                        break;

                    default:
                        Debug.Log( "Unsupported input type : " + input.inputType );
                        break;
                    }
                    currentRecording.AddInput ( currentFrame, input );
				}
					
				currentFrame++;
			}
		}
	}
	
	// These methods replace those in Input, so that this object can ignore whether it is record
    public bool GetKey( string keyName )
    {
        if ( _mode == InputVCRMode.Pause )
            return false;

        if ( _mode == InputVCRMode.Playback)
			return (keyDownStatuses [keyName] == KeyState.DOWN ||keyDownStatuses [keyName] == KeyState.HELD);
        else
            return Input.GetKey ( keyName );
    }

    public bool GetKeyDown( string keyName )
    {
        if ( _mode == InputVCRMode.Pause )
            return false;

		if (_mode == InputVCRMode.Playback)
		{
			return keyDownStatuses [keyName] == KeyState.DOWN;
		}
        else
            return Input.GetKeyDown ( keyName );
    }

    public bool GetKeyUp( string keyName )
    {
        if ( _mode == InputVCRMode.Pause )
            return false;

		if (_mode == InputVCRMode.Playback)
			return keyDownStatuses [keyName] == KeyState.UP;
        else
            return Input.GetKeyUp ( keyName );
    }
}
