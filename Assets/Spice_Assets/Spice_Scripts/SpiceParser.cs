using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpiceParser : MonoBehaviour
{
    private static string PathToRead;
    private static string PathToWrite = SpiceManager.SpicePath + "\\circuits\\write_test.cir";
    private static string title = String.Empty;
    private static string date = String.Empty;
    private static string plotName = String.Empty;
    private static string flags = String.Empty;
    private static int varNum = 0;
    private static int pointNum = 0;
    private static Dictionary<int, SpiceVariable> variables = new Dictionary<int, SpiceVariable>();

    public static string Title => title;
    public static string Date => date;
    public static string PlotName => plotName;
    public static string Flags => plotName;
    public static string VarNum => plotName;
    public static string PointNum => plotName;
    public static Dictionary<int, SpiceVariable> Variables => variables;

    private static string GetOutputPath(string inputPath)
    {
        StreamReader inputFile = new StreamReader(inputPath);
        string line;
        
        while ((line = inputFile.ReadLine()) != null)
        {
            if (!line.Contains("write ")) 
                continue;
            
            Regex regex = new Regex(@"write (.*)");
            Match match = regex.Match(line);

            return SpiceManager.SpicePath + match.Groups[1].Value;
        }

        return null;
    }
    
    public static void WriteFile(string str)
    {
        StreamWriter writer = new StreamWriter(PathToWrite, false);
        writer.WriteLine(str);
        writer.Close();
    }

    public static void ReadFile(bool debug = false)
    {
        PathToRead = GetOutputPath(SpiceManager.GetProcess().StartInfo.Arguments);

        if (PathToRead == null) 
            return;
        
        Debug.Log("Reading from: " + PathToRead);
        
        StreamReader outputFile = new StreamReader(PathToRead);

        ParseInformation(outputFile);
        ParseVariables(outputFile, ref variables, varNum);
        ParseValues(outputFile, ref variables);

        if (debug)
        {
            LogSpiceInfo();
            LogSpiceVariables(in variables);
        }

        outputFile.Close();
    }

    private static void ParseInformation(in StreamReader file)
    {
        string line;
        
        while ((line = file.ReadLine()) != null)
        {
            Regex regex;
            Match match;
            
            if (line.Contains("Title:"))
            {
                regex = new Regex(@"Title: (.*)");
                match = regex.Match(line);

                title = match.Groups[1].Value;
            }
            
            if (line.Contains("Date:"))
            {
                regex = new Regex(@"Date: (.*)");
                match = regex.Match(line);

                date = match.Groups[1].Value;
            }
            
            if (line.Contains("Plotname:"))
            {
                regex = new Regex(@"Plotname: (.*)");
                match = regex.Match(line);

                plotName = match.Groups[1].Value;
            }
            
            if (line.Contains("Flags:"))
            {
                regex = new Regex(@"Flags: (.*)");
                match = regex.Match(line);

                flags = match.Groups[1].Value;
            }
            
            if (line.Contains("No. Variables:"))
            {
                regex = new Regex(@"No. Variables: (\d+)");
                match = regex.Match(line);

                varNum = int.Parse(match.Groups[1].Value);
            }
            
            if (line.Contains("No. Points:"))
            {
                regex = new Regex(@"No. Points: (\d+)");
                match = regex.Match(line);

                pointNum = int.Parse(match.Groups[1].Value);
                
                break;
            }
        }
    }

    private static void ParseVariables(in StreamReader file, ref Dictionary<int, SpiceVariable> variables,
        int numberOfVariables)
    {
        string line; 
        variables.Clear();

        while ((line = file.ReadLine()) != null)
        {
            if (!line.Equals("Variables:")) continue;

            Regex regexVariables = new Regex(@"\t(\d)\t(.+)\t(.+)");

            for (int i = 0; i < numberOfVariables; i++)
            {
                if ((line = file.ReadLine()) == null) break;
                
                Match varMatch = regexVariables.Match(line);
                
                SpiceVariable variable = varMatch.Groups[2].Value == "time" ? 
                    new TimeSpiceVariable(varMatch.Groups[2].Value, new List<float>()) :
                    new SpiceVariable(varMatch.Groups[2].Value, new List<float>());

                variables.Add(int.Parse(varMatch.Groups[1].Value), variable);
            }
            
            break;
        }
    }
    private static void ParseValues(in StreamReader file, ref Dictionary<int, SpiceVariable> variables)
    {
        string line;
        bool valuesFound = false;

        while ((line = file.ReadLine()) != null)
        {
            if (!valuesFound && !line.Equals("Values:")) continue;

            valuesFound = true;

            Regex regexValues = new Regex(@"( \d)?\t(.+)");
            
            for (int i = 0; i < variables.Count; i++)
            {
                if ((line = file.ReadLine()) == null) break;
                
                variables.TryGetValue(i, out SpiceVariable variable);
                
                Match varMatch = regexValues.Match(line);
                
                variable?.values.Add(float.Parse(varMatch.Groups[2].Value));
            }
        }
    }

    public static void LogSpiceVariables(in Dictionary<int, SpiceVariable> variables)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            variables.TryGetValue(i, out SpiceVariable variable);
            
            if (variable == null) return;
            
            Debug.Log(variable.Name + ": \n");

            foreach (float value in variable.GetValues())
            {
                Debug.Log(value + ", ");
            }
        }
    }
    
    public static void LogSpiceInfo()
    {
        Debug.Log(title);
        Debug.Log(date);
        Debug.Log(plotName);
        Debug.Log(flags);
        Debug.Log(varNum);
        Debug.Log(pointNum);
    }
}
