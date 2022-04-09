using UnityEngine;

[ExecuteInEditMode]
public class PlayAudioOnTrigger : MonoBehaviour
{
    [Tooltip("Automatically gets the AudioSource from the same object if necessary and one is present")]
    [SerializeField] protected AudioSource audioSource;

    protected virtual void Start() { if (!audioSource) audioSource = GetComponent<AudioSource>(); }

    public virtual void PlayAudio() => audioSource.Play();
}
