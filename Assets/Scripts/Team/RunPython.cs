using System;
using System.Diagnostics;
using System.IO;

public class RunPython
{
    public static string Run(string scriptPath, string imagePath)
    {
        Process p = new Process();
        p.StartInfo.FileName = @"C:\Python27\python.exe";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
        p.StartInfo.Arguments = scriptPath + " " + imagePath; // start the python program with two parameters
        p.Start(); // start the process (the python program)
        StreamReader s = p.StandardOutput;
        String output = s.ReadToEnd();
        string[] r = output.Split(new char[] { ' ' }); // get the parameter
        Console.WriteLine(r[0]);
        p.WaitForExit();

        return output;
    }
}