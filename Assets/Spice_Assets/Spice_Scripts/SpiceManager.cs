using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SpiceManager : MonoBehaviour
{
    private static Process ngspiceProcess;
    private static StreamWriter inputStream;
    private static string spicePath = Directory.GetCurrentDirectory() + "\\Spice64";
    
    public static string SpicePath => spicePath;

    public static Process GetProcess()
    {
        ngspiceProcess ??= new Process
        {
            StartInfo =
            {
                FileName = spicePath + "\\bin\\ngspice_con.exe",
                Arguments = spicePath + "\\circuits\\bipolar_amp.cir",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            }
        };

        return ngspiceProcess;
    }

    public static StreamWriter GetInputStream()
    {
        inputStream ??= ngspiceProcess?.StandardInput;

        return inputStream;
    }
}
