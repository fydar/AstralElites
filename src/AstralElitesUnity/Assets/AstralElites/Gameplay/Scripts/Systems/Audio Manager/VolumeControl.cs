using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VolumeControl
{
    [Range(0, 1)]
    [SerializeField]
    private float _volume;
    public readonly List<AudioSourceAnimator> animators;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;

            for (int i = animators.Count - 1; i >= 0; i--)
            {
                animators[i].RefreshValues();
            }
        }
    }

    public VolumeControl() : this(1.0f) { }

    public VolumeControl(float volume)
    {
        _volume = volume;
        animators = new List<AudioSourceAnimator>();
    }
}
