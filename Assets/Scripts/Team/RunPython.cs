using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class RunPython
{
    Process p;

    public RunPython(string scriptPath)
    {
        p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = @"python";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
        p.StartInfo.Arguments = scriptPath;
    }

    public string Interact(int teamId)
    {

        StreamWriter streamWriter = p.StandardInput;

        streamWriter.WriteLine(teamId);
        
        string output = p.StandardOutput.ReadToEnd();
        //UnityEngine.Debug.Log(output);

        //string err = p.StandardError.ReadToEnd();
        //UnityEngine.Debug.Log(err);



        return output;
    }

    public void Run()
    {
        p.Start();
        
    }

    public bool IsRunning()
    {
        if(p.HasExited) { return false; }
        else { return true; }
    }


}