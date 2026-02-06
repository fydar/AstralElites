using UnityEngine;

[CreateAssetMenu(menuName = "Music Group")]
public class MusicGroup : AudioAsset
{
    public AudioClip[] Music;

    public float Volume = 1.0f;
    public int Priority;

    public void Play()
    {

    }
}
