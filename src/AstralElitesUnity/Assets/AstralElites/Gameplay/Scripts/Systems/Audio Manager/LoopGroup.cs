using UnityEngine;

[CreateAssetMenu(menuName = "Loop Group")]
public class LoopGroup : AudioAsset
{
    public AudioClip LoopedAudio;

    public Vector2 VolumeRange;
    public Vector2 PitchRange;
    public float PerlinSpeed = 5.0f;
    public int Priority;

    public void Play()
    {

    }
}
