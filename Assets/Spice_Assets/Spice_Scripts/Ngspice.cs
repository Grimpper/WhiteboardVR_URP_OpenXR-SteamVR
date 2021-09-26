using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Ngspice : MonoBehaviour
{
    [SerializeField] private bool debug = false;
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Launch ngspice"))
        {
            Process ngspiceProcess = SpiceManager.GetProcess();
            ngspiceProcess.Start();
            LogRunningProcessName("ngspice_con");

            ngspiceProcess.StandardOutput.ReadToEnd();
            ngspiceProcess.WaitForExit();
        }
       
        if (GUI.Button(new Rect(170, 10, 150, 50), "Read output"))
        {
           SpiceParser.ReadFile(debug); 
        }
    }

    private void LogRunningProcessName(string strName)
    {
        Process[] processes = Process.GetProcessesByName(strName);

        if (processes.Length <= 0) return;
        
        foreach (var process in processes)
            Debug.Log(process.ProcessName+ " is running");
    }
}
