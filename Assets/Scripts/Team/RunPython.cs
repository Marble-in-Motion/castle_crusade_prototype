using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class RunPython
{
    public static string Run(string imagePath)
    {
        //UnityEngine.Debug.Log(imagePath);
        Process p = new Process();
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.FileName = @"python";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
        p.StartInfo.Arguments = @"C:\Users\SP\Documents\WORK\GP\tensorflow\tensorflow-for-poets-2\scripts\label_image_spesh.py"; //@" - m scripts.label_image tf_files/retrained_graph.pb " + imagePath; //";  //scriptPath + " " + imagePath; // start the python program with two parameters
        p.Start(); // start the process (the python program)

        StreamWriter myStreamWriter = p.StandardInput;
        imagePath = @"C:\Users\SP\Documents\WORK\GP\tensorflow\tensorflow-for-poets-2\tf_files\test_data_resize\5\img57.jpg";
 
        myStreamWriter.WriteLine(imagePath);

        string output = p.StandardOutput.ReadToEnd();
        UnityEngine.Debug.Log(output);
        string err = p.StandardError.ReadToEnd();
        UnityEngine.Debug.Log(err);
        p.WaitForExit();

        return output;
    }
}