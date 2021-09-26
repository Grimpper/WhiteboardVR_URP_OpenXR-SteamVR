using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSpiceVariable : SpiceVariable
{
    public TimeSpiceVariable(string name, List<float> values) : base(name, values)
    {
    }

    protected override string GetDisplayName()
    {
        return Name;
    }
}
