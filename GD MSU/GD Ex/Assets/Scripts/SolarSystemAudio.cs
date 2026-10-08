using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SolarSystemAudio : MonoBehaviour
{
    private void Start()
    {
        const int sampleRate = 44100;
        const int sampleCount = sampleRate * 3;
        AudioClip ambience = AudioClip.Create("Solar Ambience", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int index = 0; index < samples.Length; index++)
        {
            float time = (float)index / sampleRate;
            float fade = Mathf.Min(time / 0.4f, (3f - time) / 0.4f, 1f);
            samples[index] = (Mathf.Sin(time * Mathf.PI * 2f * 110f) * 0.025f
                + Mathf.Sin(time * Mathf.PI * 2f * 164.81f) * 0.012f) * fade;
        }
        ambience.SetData(samples, 0);

        AudioSource source = GetComponent<AudioSource>();
        source.clip = ambience;
        source.loop = true;
        source.volume = 0.35f;
        source.spatialBlend = 0f;
        source.Play();
    }
}