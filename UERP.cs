#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Threading.Tasks;
using Discord;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class UERP
{
    private const string APPLICATION_ID = "1140434957240643595";
    private static Discord.Discord _discord;

    private static long _startTimestamp;
    private static bool _playMode = false;

    #region Initialization
    static UERP()
    {
        DelayStart();
    }

    private static async void DelayStart(int delay = 1000)
    {
        await Task.Delay(delay);
        if (DiscordRunning())
        {
            Init();
        }
    }

    private static void Init()
    {
        try
        {
            Debug.Log("Started Rich Presence");
            _discord = new Discord.Discord(long.Parse(APPLICATION_ID), (long)CreateFlags.Default);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }
        
        var timeSpan = TimeSpan.FromMilliseconds(EditorAnalyticsSessionInfo.elapsedTime);
        _startTimestamp = DateTimeOffset.Now.Add(timeSpan).ToUnixTimeSeconds();
        
        EditorApplication.playModeStateChanged += PlayModeChanged;
    
        UpdateActivity();
        
        if (DiscordRunning())
        {
            _ = UpdateLoop();
        }
    }
    #endregion

    #region Update

    private static void Update()
    {
        //moved out of the update loop and into a slower async loop
    }
    
    private static async Task UpdateLoop()
    {
        while (true)
        {
            if (_discord != null && DiscordRunning())
            {
                _discord.RunCallbacks();
            }
        
            await Task.Delay(5000); // Delay for 5 seconds before running callbacks again
        }
    }

    private static void PlayModeChanged (PlayModeStateChange state)
    {
        if (EditorApplication.isPlaying == _playMode) return;
        _playMode = EditorApplication.isPlaying;

        UpdateActivity();
    }

    private static void UpdateActivity()
    {
        if (_discord == null)
        {
            Init();
            return;
        }
        
        var sceneName = SceneManager.GetActiveScene().name;
        string stateMessage;

        if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
        {
            stateMessage = "Editing an Unsaved Scene";
        }
        else
        {
            stateMessage = "Editing" + sceneName;
        }

        var activity = new Activity
        {
            
            State = stateMessage,
            Details = Application.productName,
            Timestamps = { Start = _startTimestamp },
            Assets =
                {
                    LargeImage = "unity-icon",
                    LargeText = "Unity " + Application.unityVersion,
                    SmallImage = EditorApplication.isPlaying ? "play-mode" : "edit-mode",
                    SmallText = EditorApplication.isPlaying ? "Play mode" : "Edit mode",
                },
        };

        _discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Result.Ok) Debug.LogError(result.ToString());
        });

    }
    #endregion

    private static bool DiscordRunning()
    {
        var processes = Process.GetProcessesByName("Discord");

        if (processes.Length == 0)
        {
            processes = Process.GetProcessesByName("DiscordPTB");

            if (processes.Length == 0)
            {
                processes = Process.GetProcessesByName("DiscordCanary");
            }
        }
        return processes.Length != 0;
    }

}
#endif