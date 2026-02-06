using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if !UNITY_EDITOR
using UnityEngine.Networking;
#endif

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

    public MusicGroup Music;

    public VolumeControl TabFade = new(1.0f);
    public VolumeControl MasterVolume = new(1.0f);
    public VolumeControl SfxVolume = new(1.0f);
    public VolumeControl MusicVolume = new(1.0f);

    public AudioSourcePool Pool = new(25);
    public List<AudioSourceAnimator> Animators = new();

    private IInterpolator interpolator;

    private void Awake()
    {
        Pool.Initialise(gameObject);

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

    private void OnApplicationFocus(
        bool hasFocus)
    {
        interpolator.TargetValue = hasFocus ? 1.0f : 0.0f;
    }

    public void PlayClip(
        SfxGroup group)
    {
        if (DisableAudio)
        {
            return;
        }

        var source = Pool.Grab();

        source.clip = group.GetClip();
        source.volume = Random.Range(group.VolumeRange.x, group.VolumeRange.y);
        source.pitch = Random.Range(group.PitchRange.x, group.PitchRange.y);
        source.priority = group.Priority;
        source.loop = false;

        var animator = new AudioSourceAnimator(source, TabFade, MasterVolume, SfxVolume);
        Animators.Add(animator);

        source.Play();
        _ = StartCoroutine(ReturnToPool(animator));
    }

    public void PlayClip(
        LoopGroup group,
        EffectFader fader)
    {
        if (DisableAudio)
        {
            return;
        }

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

    public static void PlayMusic(
        string name)
    {
        if (DisableAudio)
        {
            return;
        }

        _ = instance.StartCoroutine(instance.LoadAndPlayMusic(name));
    }

    public void PlayMusic(
        MusicGroup group)
    {
        if (DisableAudio)
        {
            return;
        }

        var source = Pool.Grab();

        source.clip = group.Music[0];
        source.volume = group.Volume;
        source.priority = 0;
        source.loop = true;

        var animator = new AudioSourceAnimator(source, TabFade, MasterVolume, MusicVolume);
        Animators.Add(animator);

        source.Play();
    }

    private IEnumerator LoadAndPlayMusic(
        string resourcePath)
    {
#if UNITY_EDITOR
        var clip = AssetDatabase.LoadAssetAtPath<MusicGroup>("Assets/AstralElites/Gameplay/Audio/Ambient/Main.asset");

        yield return null;
        if (clip != null)
        {
            PlayMusic(clip);
        }
#else
        string pathToBundle = Application.streamingAssetsPath + "/AssetBundles/audio.data";
        // string pathToBundle = "http://localhost:56743/StreamingAssets/AssetBundles/audio.data";

        using (var request = UnityWebRequestAssetBundle.GetAssetBundle(pathToBundle))
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            AssetBundle bundle = ((DownloadHandlerAssetBundle)request.downloadHandler).assetBundle;

            var clipRequest = bundle.LoadAssetAsync<MusicGroup>("Main");

            while (!clipRequest.isDone)
            {
                yield return null;
            }

            if (clipRequest.asset != null)
            {
                PlayMusic((MusicGroup)clipRequest.asset);
            }

            // Remove from memory to allow custom ( based on Caching) logic do its job
            // bundle.Unload(true);
        }
#endif
    }


    private IEnumerator ReturnToPool(
        AudioSourceAnimator animator)
    {
        yield return new WaitForSeconds(animator.Source.clip.length / animator.Source.pitch);
        animator.Source.Stop();
        Pool.Return(animator.Source);
        _ = Animators.Remove(animator);
    }

    private IEnumerator ManageLoop(
        AudioSourceAnimator animator,
        LoopGroup group,
        EffectFader fader)
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

    public static void Play(
        SfxGroup group)
    {
        instance.PlayClip(group);
    }

    public static void Play(
        LoopGroup group,
        EffectFader fader)
    {
        instance.PlayClip(group, fader);
    }
}
