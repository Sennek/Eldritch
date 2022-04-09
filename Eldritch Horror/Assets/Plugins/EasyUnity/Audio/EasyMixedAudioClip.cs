using UnityEngine;
using System;
using Object = UnityEngine.Object;

[Serializable]
public class EasyMixedAudioClip
{
    public AudioClip Clip;
    public EasyAudioMix Mix;

    protected AudioSource playingSource;

    public EasyMixedAudioClip(AudioClip clip, EasyAudioMix mix)
    {
        Clip = clip;
        Mix = mix;
    }

    public void PlayOneShot(Transform parent)
    {
        if (!Clip) return;
        AudioSource audioSource = GetNewAudioSource(parent);
        audioSource.PlayOneShot(Clip);
        Object.Destroy(audioSource.gameObject, Clip.length);
    }

    public void PlayOneShot(Transform parent, Vector3 position)
    {
        if (!Clip) return;
        AudioSource audioSource = GetNewAudioSource(parent, position);
        audioSource.PlayOneShot(Clip);
        Object.Destroy(audioSource.gameObject, Clip.length);
    }

    public void PlayLoop(Transform parent)
    {
        if (!Clip || playingSource) return;
        playingSource = GetNewAudioSource(parent);
        playingSource.clip = Clip;
        playingSource.Play();
    }

    protected virtual AudioSource GetNewAudioSource(Transform parent) => GetNewAudioSource(parent, parent ? parent.position : Vector3.zero);

    protected virtual AudioSource GetNewAudioSource(Transform parent, Vector3 position)
    {
        GameObject go = new GameObject($"Audio_{Clip.name}");
        go.transform.SetParent(parent);
        go.transform.position = position;
        AudioSource audioSource = go.AddComponent<AudioSource>();
        if (Mix)
        {
            audioSource.volume = Mix.Volume;
            audioSource.pitch = Mix.Pitch;
            audioSource.panStereo = Mix.StereoPan;
            audioSource.outputAudioMixerGroup = Mix.MixerGroup;
        }
        return audioSource;
    }

    public void StopLoop() { if (playingSource) Object.Destroy(playingSource.gameObject); }
}
