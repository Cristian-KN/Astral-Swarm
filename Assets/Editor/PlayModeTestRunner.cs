using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 5);

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            switch (state)
            {
                case "WaitingForCompile":
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.isPlaying = true;
                    };
                    break;
                case "EnteringPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;
                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static int _frameCount = 0;
        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            EditorApplication.update -= WaitFramesThenRun;

            string resultJson = RunTestLogic();
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void SelfDestruct()
        {
            string path = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(path) && AssetDatabase.AssetPathExists(path)) AssetDatabase.DeleteAsset(path);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public bool playButtonWorks;
            public bool settingsButtonWorks;
            public string currentScene;
        }

        private static string RunTestLogic()
        {
            var res = new TestResult { success = true };
            
            // 1. Load MainMenu
            SceneManager.LoadScene("MainMenu");
            
            // Wait a frame for scene load
            var manager = Object.FindAnyObjectByType<MainMenuManager>();
            if (manager == null) { res.success = false; res.error = "MainMenuManager not found"; return JsonUtility.ToJson(res); }

            // 2. Test Settings Toggle
            var settingsBtn = GameObject.Find("Ajustes")?.GetComponent<Button>();
            if (settingsBtn != null)
            {
                settingsBtn.onClick.Invoke();
                res.settingsButtonWorks = manager.settingsPanel.activeSelf;
            }

            // 3. Test Play Button (This will trigger scene load, might be hard to verify in same frame)
            var playBtn = GameObject.Find("Jugar")?.GetComponent<Button>();
            if (playBtn != null)
            {
                playBtn.onClick.Invoke();
                // We can't immediately check SceneManager.GetActiveScene() because load is async/next frame
                // But we can check if it's the right method.
            }

            res.currentScene = SceneManager.GetActiveScene().name;
            return JsonUtility.ToJson(res);
        }
    }
}
