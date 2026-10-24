using System;
using UnityEngine;

namespace NepalTrial.Settings
{
    [Serializable]
    public sealed class LocalGameSettings
    {
        public int schemaVersion = 1;
        public float steeringSensitivity = 1f;
        public float soundVolume = 0.8f;
        public int graphicsQuality = 1;
        public bool vibrationEnabled = true;
    }

    public static class GameSettingsRules
    {
        public static void Normalize(LocalGameSettings data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.steeringSensitivity = Mathf.Clamp(float.IsNaN(data.steeringSensitivity) ? 1f : data.steeringSensitivity, 0.25f, 2f);
            data.soundVolume = Mathf.Clamp01(float.IsNaN(data.soundVolume) ? 0.8f : data.soundVolume);
            data.graphicsQuality = Mathf.Clamp(data.graphicsQuality, 0, 2);
        }
    }

    public sealed class LocalSettingsStore
    {
        private const string Key = "NepalTrial.Settings.v1";

        public LocalGameSettings Load()
        {
            var json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return new LocalGameSettings();
            try
            {
                var data = JsonUtility.FromJson<LocalGameSettings>(json);
                if (data == null || data.schemaVersion != 1) return new LocalGameSettings();
                GameSettingsRules.Normalize(data);
                return data;
            }
            catch (ArgumentException)
            {
                return new LocalGameSettings();
            }
        }

        public void Save(LocalGameSettings data)
        {
            GameSettingsRules.Normalize(data);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
