using System;
using UnityEngine;

namespace NepalTrial.Progress
{
    /// <summary>Local save/load adapter. PlayerPrefs stores a JSON document on the current device.</summary>
    public sealed class LocalProgressStore
    {
        private const string Key = "NepalTrial.Progress.v1";
        private readonly string storageKey;

        public LocalProgressStore(string customKey = null)
        {
            storageKey = string.IsNullOrEmpty(customKey) ? Key : customKey;
        }

        public LocalProgressData Load()
        {
            string json = PlayerPrefs.GetString(storageKey, "");
            if (string.IsNullOrEmpty(json)) return new LocalProgressData();
            try
            {
                LocalProgressData data = JsonUtility.FromJson<LocalProgressData>(json);
                return data != null && data.schemaVersion == 1 ? data : new LocalProgressData();
            }
            catch (ArgumentException)
            {
                return new LocalProgressData();
            }
        }

        public void Save(LocalProgressData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            PlayerPrefs.SetString(storageKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
