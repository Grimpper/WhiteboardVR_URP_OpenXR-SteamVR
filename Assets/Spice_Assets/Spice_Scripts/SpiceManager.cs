using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SpiceManager : MonoBehaviour
{
    private static Process ngspiceProcess;
    private static StreamWriter inputStream;

    
    public static Process GetProcess()
    {
        ngspiceProcess ??= new Process
        {
            StartInfo =
            {
                FileName = "F:\\Unity_Projects\\ngspice_test\\Spice64\\bin\\ngspice_con.exe",
                Arguments = "F:\\Unity_Projects\\ngspice_test\\Spice64\\circuits\\bipolar_amp.cir",
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
