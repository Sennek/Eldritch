using System;

using UnityEngine;

[Serializable]
[ExecuteInEditMode]
public class EasyFauxVO : MonoBehaviour
{
    [SerializeField] protected EasyCharacterVOInfo characterInfo;
    [SerializeField] protected AudioSource audioSource;
    [Range(0, 3)] protected float pitch;
    [Range(0, 3)] protected float amplitude;
    protected float evaluation;

    public float Frequency => 22.38f;
    public bool IsPlaying => audioSource.isPlaying;
    public float ElapsedTime { get; set; }

    public void Play() => audioSource.Play();

    public void Stop() => audioSource.Stop();

    protected void Update()
    {
        if (characterInfo)
        {
            pitch = characterInfo.FauxPitch;
            amplitude = characterInfo.FauxAmplitude;
        }

        if (audioSource)
        {
            if (Application.isPlaying && audioSource.isPlaying) ElapsedTime += Time.deltaTime;
            audioSource.pitch = evaluation = Mathf.Lerp(pitch - amplitude, pitch + amplitude, Mathf.Abs(Mathf.Sin(Frequency * 2 * Mathf.PI * ElapsedTime)));
        }
    }
}
