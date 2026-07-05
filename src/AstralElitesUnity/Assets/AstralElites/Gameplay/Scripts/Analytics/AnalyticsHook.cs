using System.Collections.Generic;

public class AnalyticsHook<T>
{
    public List<T> data;

    public T Last => data.Count == 0 ? default : data[^1];

    public AnalyticsHook()
    {
        data = new List<T>();
    }

    public void Clear()
    {
        data.Clear();
    }

    public void Callback(T param)
    {
        data.Add(param);
    }
}
