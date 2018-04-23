using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;

public class Playback : MonoBehaviour
{
	private int recordingFrameRate = 60;

	private enum KeyState {UP, HELD, DOWN};

	private Dictionary<string,KeyState> keyDownStatuses = new Dictionary<string, KeyState> ()
	{
		{"left",KeyState.UP},
		{"space",KeyState.UP},
		{"right",KeyState.UP}
	};
}

