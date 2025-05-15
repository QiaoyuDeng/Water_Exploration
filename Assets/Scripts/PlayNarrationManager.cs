
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// NarrationManager handles the playback of audio clips for each simulation step,
/// based on scenario, day, and step.
/// NarrationManager 用于根据场景编号、天数和步骤编号播放对应的语音文件。
/// </summary>
public class PlayNarrationManager : MonoBehaviour
{
    [Header("Testing Options")]
    public bool skipAudio = false;

    [System.Serializable]
    public struct NarrationKey
    {
        public int scenario; // 0=light rainfall，1=moderate rainfall，2=heavy rainfall
        public int day;      // day 0-6
        public int step;     // step 1-6

        public override int GetHashCode()
        {
            return scenario * 100 + day * 10 + step;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NarrationKey)) return false;
            var other = (NarrationKey)obj;
            return scenario == other.scenario && day == other.day && step == other.step;
        }
    }

    public AudioSource audioSource;

    [System.Serializable]
    public class NarrationEntry
    {
        public int scenario;
        public int day;
        public int step;
        public AudioClip clip;
    }

    public List<NarrationEntry> narrationEntries = new List<NarrationEntry>();
    private Dictionary<NarrationKey, AudioClip> narrationDict = new Dictionary<NarrationKey, AudioClip>(); 

    void Awake()
    {
        narrationDict.Clear();
        foreach (var entry in narrationEntries)
        {
            var key = new NarrationKey { scenario = entry.scenario, day = entry.day, step = entry.step };
            if (!narrationDict.ContainsKey(key))
                narrationDict[key] = entry.clip;
        }
    }

    public void PlayNarration(int scenario, int day, int step)
    {
        var key = new NarrationKey { scenario = scenario, day = day, step = step };
        if (narrationDict.TryGetValue(key, out var clip) && clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Narration not found: Scenario {scenario}, Day {day}, Step {step}");
        }
    }


    public IEnumerator PlayNarrationAndWait(int scenario, int day, int step)
    {
        var key = new NarrationKey { scenario = scenario, day = day, step = step };

        if (narrationDict.TryGetValue(key, out var clip) && clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            float waitTime = skipAudio ? 1f : clip.length;
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            Debug.LogWarning($"Narration not found: Scenario {scenario}, Day {day}, Step {step}");
            yield return null;
        }
    }

    public IEnumerator WaitForNarrationToFinish()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }
    }

    public void StopNarration()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Narration stopped.");
        }
    }



#if UNITY_EDITOR

    [ContextMenu("Auto Load Clips from Resources/Audio/Narration")]
    public void AutoLoadClipsFromResources()
    {
        narrationEntries.Clear();
        narrationDict.Clear();

        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Narration");

        foreach (AudioClip clip in clips)
        {
            if (TryParseName(clip.name, out int scenario, out int day, out int step))
            {
                var entry = new NarrationEntry { scenario = scenario, day = day, step = step, clip = clip };
                var key = new NarrationKey { scenario = scenario, day = day, step = step };
                narrationEntries.Add(entry);
                narrationDict[key] = clip;
            }
        }

        Debug.Log($"✅ Loaded {narrationEntries.Count} clips from Resources/Audio/Narration.");
    }

    [ContextMenu("Auto Load Clips from Assets/Audio/Narration")]
    public void AutoLoadFromAssetsAudioFolder()
    {
        narrationEntries.Clear();
        narrationDict.Clear();

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio/Narration" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) continue;

            if (TryParseName(clip.name, out int scenario, out int day, out int step))
            {
                var entry = new NarrationEntry { scenario = scenario, day = day, step = step, clip = clip };
                var key = new NarrationKey { scenario = scenario, day = day, step = step };
                narrationEntries.Add(entry);
                narrationDict[key] = clip;
            }
        }

        Debug.Log($"✅ Loaded {narrationEntries.Count} clips from Assets/Audio/Narration.");
    }

    private bool TryParseName(string name, out int scenario, out int day, out int step)
    {
        scenario = 0; day = 0; step = 0;

        if (name.StartsWith("S") && name.Contains("_D") && name.Contains("_T"))
        {
            string[] parts = name.Split('_');
            scenario = int.Parse(parts[0].Substring(1));
            day = int.Parse(parts[1].Substring(1));
            step = int.Parse(parts[2].Substring(1));
            return true;
        }
        else if (name.Length == 3 && int.TryParse(name, out int _))
        {
            scenario = int.Parse(name[0].ToString());
            day = int.Parse(name[1].ToString());
            step = int.Parse(name[2].ToString());
            return true;
        }

        Debug.LogWarning($"❌ Unrecognized clip name format: {name}");
        return false;
    }

#endif
}
