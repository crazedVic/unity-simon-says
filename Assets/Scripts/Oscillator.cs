using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=GqHFGMy_51c&ab_channel=DanoKablamo
public class Oscillator : MonoBehaviour
{

    [SerializeField]
    AudioSource audiosource;

    private double frequency = 261.0;
    private double increment; //determined by frequency
    private double phase; //actual location on the wave
    private double sampling_frequency = 48000.0;

    private float gain = 0.3F; //actual volume of the sound

    private void Awake()
    {
        GameManager.PlaySound += PlaySound;
    }

    private void PlaySound(double frequency, float duration)
    {
        StartCoroutine(PlayThenStop(frequency, duration));
    }

    IEnumerator PlayThenStop(double frequency, float duration)
    {
        this.frequency = frequency;
        audiosource.Play();
        yield return new WaitForSeconds(duration);
        audiosource.Stop();

    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        // source of sound, channels is # of speakers

        increment = frequency * 2.0 * Mathf.PI / sampling_frequency;

        for (int i = 0; i< data.Length; i += channels)
        {
            phase += increment;

            // this actually produces the sound
            //data[i] = (float)(gain * Mathf.Sin((float)phase));
            data[i] = (float)(gain * Mathf.PingPong((float)phase, 1.0f));

            if(channels == 2)
            {
                data[i + 1] = data[i];

            }

            if (phase > (Mathf.PI * 2))
            {
                phase = 0.0;
            }
        }

    }
}
