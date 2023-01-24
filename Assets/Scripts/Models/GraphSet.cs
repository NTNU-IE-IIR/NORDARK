using System.Collections.Generic;
using UnityEngine;

public class GraphSet
{
    public string Title;
    public Color Color;
    public List<float> Abscissas;
    public List<float> Ordinates;

    public GraphSet(string title, Color color)
    {
        Title = title;
        Color = color;
        Abscissas = new List<float>();
        Ordinates = new List<float>();
    }

    public void Add(float abscissa, float ordinate)
    {
        Abscissas.Add(abscissa);
        Ordinates.Add(ordinate);
    }

    public void Clear()
    {
        Abscissas.Clear();
        Ordinates.Clear();
    }
}
