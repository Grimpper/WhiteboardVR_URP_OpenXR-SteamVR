using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SpiceVariable
{
    public string Name { get; }
    public string DisplayName { get; }
    
    public List<float> values;

    public SpiceVariable(string name, List<float> values)
    {
        Name = name;
        DisplayName = GetDisplayName();
        this.values = values;
    }

    public List<float> GetValues(NumberUtils.Unit unit = NumberUtils.Unit.Unitary)
    {
        List<float> scaledList = new List<float>(values);
        for (int i = 0; i < values.Count; i++)
        {
            scaledList[i] /= NumberUtils.GetMagnitude(unit); 
        }

        return scaledList;
    }

    protected virtual string GetDisplayName()
    {
        Regex regex = new Regex(@".\((.*)\)");
        Match match = regex.Match(Name);

        return match.Groups[1].Value;
    }
}
