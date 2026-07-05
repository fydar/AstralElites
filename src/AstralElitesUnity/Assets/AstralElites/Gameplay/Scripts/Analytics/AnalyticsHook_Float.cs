using System;
using System.Collections.Generic;

[Serializable]
public class AnalyticsHook_Float
{
    private readonly GlobalFloat target;
    public List<float> data;

    public AnalyticsHook_Float(GlobalFloat _target)
    {
        target = _target;
        data = new List<float>();
    }

    public void Capture()
    {
        data.Add(target.Value);
    }

    public void Clear()
    {
        data.Clear();
    }

    public void Recapture()
    {
        data.RemoveAt(data.Count - 1);
        Capture();
    }
}
