using UnityEngine;

[CreateAssetMenu(menuName = "Sfx Group")]
public class SfxGroup : AudioAsset
{
    public AudioClip[] Clips;

    public Vector2 VolumeRange;
    public Vector2 PitchRange;
    public int Priority;

    public AudioClip GetClip()
    {
        return Clips[Random.Range(0, Clips.Length)];
    }

    public void Play()
    {

    }
}
