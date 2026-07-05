using System;
using UnityEngine;

public class GlobalValue<T> : ScriptableObject
{
    public Action OnChanged;

    [SerializeField]
    protected T currentValue;

    public virtual T Value
    {
        get => currentValue;
        set
        {
            currentValue = value;

            OnChanged?.Invoke();
        }
    }
}
