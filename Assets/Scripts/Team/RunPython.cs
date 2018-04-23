using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class RunPython
{
    Process p;
    StreamWriter streamWriter;

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
        streamWriter = p.StandardInput;

    }

    public string Interact(int teamId)
    {
        

        streamWriter.WriteLine(teamId);

        string output = p.StandardOutput.ReadToEnd();
        //UnityEngine.Debug.Log(output);

        //string err = p.StandardError.ReadToEnd();
        //UnityEngine.Debug.Log(err);



        return output;
    }

    public void Start()
    {
        p.Start();
    }

    public bool IsRunning()
    {
        if(p.HasExited) { return false; }
        else { return true; }
    }


}