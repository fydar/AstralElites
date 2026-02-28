using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeMethodLoad()
    {
        var audioManager = new GameObject("Audio Manager");
        DontDestroyOnLoad(audioManager);

        instance = audioManager.AddComponent<AudioManager>();
    }

    public static bool DisableAudio { get; set; } = false;

    private string BundleBasePath => Application.streamingAssetsPath + "/AssetBundles";

    public MusicGroup Music;

    public VolumeControl TabFade = new(1.0f);
    public VolumeControl MasterVolume = new(1.0f);
    public VolumeControl SfxVolume = new(1.0f);
    public VolumeControl MusicVolume = new(1.0f);

    public GameObjectPool<AudioSource> Pool = new();
    public List<AudioSourceAnimator> Animators = new();

    private IInterpolator interpolator;

    private void Awake()
    {
        var template = new GameObject("Audio Source");
        var templateAudioSource = template.AddComponent<AudioSource>  ();
        Pool.Template = templateAudioSource;
        Pool.Initialise(transform);
        interpolator = new LinearInterpolator(8.0f) { Value = 1.0f };
    }

    private void Start()
    {
        interpolator.TargetValue = 1.0f;
        interpolator.Value = 1.0f;
    }

    private void Update()
    {
        if (DisableAudio)
        {
            return;
        }

        SfxVolume.Volume = SfxVolume.Volume;
        MusicVolume.Volume = MusicVolume.Volume;
        interpolator.Update(Time.unscaledDeltaTime);
        TabFade.Volume = interpolator.Value;

        for (int i = Animators.Count - 1; i >= 0; i--)
        {
            var animator = Animators[i];
            _ = animator.Update(Time.deltaTime);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        interpolator.TargetValue = hasFocus ? 1.0f : 0.0f;
    }

    public static void Play(BunnyReference<SfxGroup> reference)
    {
        if (DisableAudio) return;
        _ = instance.StartCoroutine(instance.LoadAndPlaySfx(reference));
    }

    public static void Play(BunnyReference<LoopGroup> reference, EffectFader fader)
    {
        if (DisableAudio) return;
        _ = instance.StartCoroutine(instance.LoadAndPlayLoop(reference, fader));
    }

    public static void PlayMusic(BunnyReference<MusicGroup> reference)
    {
        if (DisableAudio) return;
        _ = instance.StartCoroutine(instance.LoadAndPlayMusic(reference));
    }

    private IEnumerator LoadAndPlaySfx(BunnyReference<SfxGroup> reference)
    {
        var request = BunnyLoader.LoadAssetAsync(reference, BundleBasePath);
        yield return request;

        if (request.asset != null)
        {
            PlayClip(request.asset);
        }
        else
        {
            Debug.LogError($"Failed to load SFX: {request.error}");
        }
    }

    private IEnumerator LoadAndPlayLoop(BunnyReference<LoopGroup> reference, EffectFader fader)
    {
        var request = BunnyLoader.LoadAssetAsync(reference, BundleBasePath);
        yield return request;

        if (request.asset != null)
        {
            PlayClip(request.asset, fader);
        }
        else
        {
            Debug.LogError($"Failed to load Loop: {request.error}");
        }
    }

    private IEnumerator LoadAndPlayMusic(BunnyReference<MusicGroup> reference)
    {
        var request = BunnyLoader.LoadAssetAsync(reference, BundleBasePath);
        yield return request;

        if (request.asset != null)
        {
            PlayMusicClip(request.asset);
        }
        else
        {
            Debug.LogError($"Failed to load Music: {request.error}");
        }
    }

    private void PlayClip(SfxGroup group)
    {
        if (DisableAudio) return;

        var clip = group.GetClip();
        if (clip == null) return;

        var source = Pool.Grab();

        source.clip = clip;
        source.volume = Random.Range(group.VolumeRange.x, group.VolumeRange.y);
        source.pitch = Random.Range(group.PitchRange.x, group.PitchRange.y);
        source.priority = group.Priority;
        source.loop = false;

        var animator = new AudioSourceAnimator(source, TabFade, MasterVolume, SfxVolume);
        Animators.Add(animator);

        source.Play();
        _ = StartCoroutine(ReturnToPool(animator));
    }

    private void PlayClip(LoopGroup group, EffectFader fader)
    {
        if (DisableAudio) return;
        if (group.LoopedAudio == null) return;

        var source = Pool.Grab();

        source.clip = group.LoopedAudio;
        source.pitch = group.PitchRange.x;
        source.volume = group.VolumeRange.x;
        source.loop = true;
        source.priority = group.Priority;

        var animator = new AudioSourceAnimator(source, TabFade, MasterVolume, SfxVolume);
        Animators.Add(animator);

        source.Play();
        _ = StartCoroutine(ManageLoop(animator, group, fader));
    }

    private void PlayMusicClip(MusicGroup group)
    {
        if (DisableAudio) return;

        var source = Pool.Grab();

        source.clip = group.Music[0];
        source.volume = group.Volume;
        source.priority = 0;
        source.loop = true;

        var animator = new AudioSourceAnimator(source, TabFade, MasterVolume, MusicVolume);
        Animators.Add(animator);

        source.Play();
    }

    private IEnumerator ReturnToPool(AudioSourceAnimator animator)
    {
        yield return new WaitForSeconds(animator.Source.clip.length / animator.Source.pitch);
        animator.Source.Stop();
        Pool.Return(animator.Source);
        _ = Animators.Remove(animator);
    }

    private IEnumerator ManageLoop(AudioSourceAnimator animator, LoopGroup group, EffectFader fader)
    {
        var FadeControl = new VolumeControl(0.0f);
        animator.AddControl(FadeControl);
        while (true)
        {
            fader.Update(Time.unscaledDeltaTime);
            FadeControl.Volume = fader.Value;
            yield return null;
        }
    }
}
