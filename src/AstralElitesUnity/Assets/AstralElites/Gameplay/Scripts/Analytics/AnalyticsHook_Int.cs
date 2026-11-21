using System.Collections.Generic;

public class AnalyticsHook_Int
{
    private readonly EventField<int> target;
    public List<float> data;

    public AnalyticsHook_Int(GlobalInt _target)
    {
        var ev = new EventField<int>();
        _target.OnChanged += () =>
        {
            ev.Value = _target.Value;
        };
        target = ev;
        data = new List<float>();
    }

    public AnalyticsHook_Int(EventField<int> _target)
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
