using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
public class Sound : ScriptableObject
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
    // Você pode adicionar mais propriedades aqui, como pitch
}