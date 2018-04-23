using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class RunPython
{
    Process p;

    public RunPython()
    {
        p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = @"python";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
        p.StartInfo.Arguments = @"C:\Users\SP\Documents\WORK\GP\tensorflow\tensorflow-for-poets-2\scripts\label_image_spesh.py";
        p.Start(); // start the process (the python program)

    }

    public string Interact(string imagePath)
    {
        StreamWriter streamWriter = p.StandardInput;

        streamWriter.WriteLine(imagePath);

        string output = p.StandardOutput.ReadToEnd();
        UnityEngine.Debug.Log(output);

        string err = p.StandardError.ReadToEnd();
        UnityEngine.Debug.Log(err);

        return output;
    }

    public string Interact2(int teamId)
    {
        StreamWriter streamWriter = p.StandardInput;

        streamWriter.WriteLine(teamId);

        string output = p.StandardOutput.ReadToEnd();
        //UnityEngine.Debug.Log(output);

        string err = p.StandardError.ReadToEnd();
        UnityEngine.Debug.Log(err);

        return output;
    }


}