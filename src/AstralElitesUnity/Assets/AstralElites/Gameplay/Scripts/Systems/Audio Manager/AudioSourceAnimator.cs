using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioSourceAnimator
{
    public AudioSource Source;

    private readonly List<VolumeControl> controls;

    private readonly float initialVolume;
    private float currentVolume;

    public AudioSourceAnimator(
        AudioSource source,
        params VolumeControl[] audioGroup)
    {
        Source = source;
        initialVolume = source.volume;
        controls = new List<VolumeControl>();

        foreach (var group in audioGroup)
        {
            AddControl(group);
        }

        RefreshValues();
        Source.volume = currentVolume;
    }

    public bool Update(float deltaTime)
    {
        Source.volume = currentVolume;
        return false;
    }

    public void RefreshValues()
    {
        currentVolume = initialVolume;

        for (int i = controls.Count - 1; i >= 0; i--)
        {
            currentVolume *= controls[i].Volume;
        }
    }

    public void AddControl(
        VolumeControl control)
    {
        controls.Add(control);
        control.animators.Add(this);
    }
}
