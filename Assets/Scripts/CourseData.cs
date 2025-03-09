using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CourseData : ScriptableObject
{
    public string courseName;
    public Color backgroundColour;

    public void UnpackColour(out float r, out float g, out float b, out float a)
    {
        r = backgroundColour.r;
        g = backgroundColour.g;
        b = backgroundColour.b;
        a = backgroundColour.a;
    }
}
