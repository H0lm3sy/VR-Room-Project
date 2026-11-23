using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class EerieCarnival : MonoBehaviour
{
    public float bpm = 110f;
    [Range(0f, 0.05f)] public float wobbleDepth = 0.012f;
    [Range(0f, 0.2f)] public float volume = 0.12f;

    // MIDI notes: A minor with a few "wrong" notes (D#, G#, Bb) for unease
    static readonly int[] melody = {
        69,72,76,75, 76,72,69,68,
        69,72,77,76, 75,72,70,69,
        64,67,71,70, 71,67,64,63
    };

    class Voice { public bool active; public double freq, phase, age, decay, amp; }
    readonly Voice[] voices = new Voice[10];

    double sampleRate, beatTimer, time;
    int beatCount;
    readonly System.Random rng = new System.Random();

    void Awake()
    {
        for (int i = 0; i < voices.Length; i++) voices[i] = new Voice();
        sampleRate = AudioSettings.outputSampleRate;

        // Spooky space + muffled tone
        var rev = gameObject.AddComponent<AudioReverbFilter>();
        rev.reverbPreset = AudioReverbPreset.Cave;
        var lp = gameObject.AddComponent<AudioLowPassFilter>();
        lp.cutoffFrequency = 4500f;
    }

    void Start() { GetComponent<AudioSource>().Play(); }

    double MidiToFreq(int n, double centsDetune = 0)
        => 440.0 * Math.Pow(2.0, (n - 69 + centsDetune / 100.0) / 12.0);

    void Trigger()
    {
        int beat = beatCount % 3;
        double detune = (rng.NextDouble() - 0.5) * 40; // up to +/-20 cents out of tune

        StartVoice(MidiToFreq(melody[beatCount % melody.Length], detune), 3.0, 1.0);

        if (beat == 0) StartVoice(MidiToFreq(45), 1.5, 0.9);          // "oom"
        else
        {
            StartVoice(MidiToFreq(60, detune), 4.0, 0.35);          // "pah"
            StartVoice(MidiToFreq(64, -detune), 4.0, 0.35);
        }
        beatCount++;
    }

    void StartVoice(double freq, double decay, double amp)
    {
        foreach (var v in voices)
            if (!v.active) { v.active = true; v.freq = freq; v.phase = 0; v.age = 0; v.decay = decay; v.amp = amp; return; }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double dt = 1.0 / sampleRate;
        for (int i = 0; i < data.Length; i += channels)
        {
            if (beatTimer <= 0)
            {
                Trigger();
                // Tempo drifts slightly so it feels unstable
                beatTimer += (60.0 / bpm) * (1.0 + 0.05 * Math.Sin(time * 0.3));
            }
            beatTimer -= dt;
            time += dt;

            double wob = 1.0 + wobbleDepth * Math.Sin(2 * Math.PI * 0.4 * time);
            double s = 0;

            foreach (var v in voices)
            {
                if (!v.active) continue;
                v.phase += 2 * Math.PI * v.freq * wob * dt;
                double env = Math.Exp(-v.age * v.decay);
                // Bell/music-box timbre with an inharmonic partial
                double tone = Math.Sin(v.phase) + 0.4 * Math.Sin(2 * v.phase)
                            + 0.2 * Math.Sin(3 * v.phase) + 0.25 * Math.Sin(2.76 * v.phase);
                s += tone * env * v.amp;
                v.age += dt;
                if (env < 0.001) v.active = false;
            }

            float sample = (float)(s * volume);
            for (int c = 0; c < channels; c++) data[i + c] = sample;
        }
    }
}