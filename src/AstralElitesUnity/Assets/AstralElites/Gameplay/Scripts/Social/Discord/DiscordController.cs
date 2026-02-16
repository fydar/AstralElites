using UnityEngine;

#if UNITY_STANDALONE || UNITY_EDITOR
using Discord.Sdk;
using System;
#endif

public class DiscordController : MonoBehaviour
{
    public static DiscordController Instance;

#if UNITY_STANDALONE || UNITY_EDITOR
    public InspectorLog Log;

    [Header("Game")]
    public GlobalInt Highscore;
    public int LastHighscore;

    [Header("Services")]
    public ulong applicationId;

    private Client client;
    private Activity currentActivity;
#endif

    public void EndGame(int score)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (score > LastHighscore)
        {
            SetHighscore(score);
        }
#endif
    }

    public void StartNewGame()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        Log.Log("Starting New Game");

        var activityTimestamps = new ActivityTimestamps();
        activityTimestamps.SetStart((ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        client.UpdateRichPresence(currentActivity, callback =>
        {
            if (callback.Successful())
            {
                Log.Log("Rich Presence Updated");
            }
            else
            {
                Log.Log(callback.ToString());
            }
        });
#endif
    }

    private void Awake()
    {
        Instance = this;

#if UNITY_STANDALONE || UNITY_EDITOR
        client = new Client();
        client.SetApplicationId(applicationId);

        client.Connect();

        client.SetStatusChangedCallback((status, error, errorDetail) =>
        {
            switch (status)
            {
                case Client.Status.Connected:
                    Log.Log("Connected to Discord");
                    break;
                case Client.Status.Disconnected:
                    Log.Log("Disconnected from Discord");
                    break;
                case Client.Status.Connecting:
                    Log.Log("Connecting to Discord");
                    break;
                case Client.Status.Ready:
                    Log.Log("Ready");
                    break;
                case Client.Status.Reconnecting:
                    Log.Log("Reconnecting");
                    break;
                case Client.Status.Disconnecting:
                    Log.Log("Disconnecting");
                    break;
                case Client.Status.HttpWait:
                    Log.Log("HttpWait");
                    break;
            }
        });

        currentActivity = new Activity();
        currentActivity.SetType(ActivityTypes.Playing);
        currentActivity.SetDetails("Shooting Asteroids");
        currentActivity.SetStatusDisplayType(StatusDisplayTypes.Details);

        SetHighscore(Highscore.Value);

        client.UpdateRichPresence(currentActivity, callback =>
        {
            if (callback.Successful())
            {
                Log.Log("Rich Presence Updated");
            }
            else
            {
                Log.Log(callback.ToString());
            }
        });
#endif
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        client.Disconnect();
    }
#endif

    private void SetHighscore(int highscore)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        LastHighscore = highscore;

        var rank = Rank.GetRank(highscore);

        var assets = new ActivityAssets();
        assets.SetLargeImage(rank.DiscordAsset);
        assets.SetLargeText($"{rank.DisplayName} ({rank.RequiredScore:###,##0}+)");
        assets.SetLargeUrl("https://fydar.dev/play/astralelites");

        currentActivity.SetState($"Highscore: {highscore:###,##0}");
        currentActivity.SetAssets(assets);

        Log.Log($"Setting Highscore: {highscore:###,##0}, rank of {rank.DisplayName}");

        client.UpdateRichPresence(currentActivity, callback =>
        {
            if (callback.Successful())
            {
                Log.Log("Rich Presence Updated");
            }
            else
            {
                Log.Log(callback.ToString());
            }
        });
#endif
    }
}
