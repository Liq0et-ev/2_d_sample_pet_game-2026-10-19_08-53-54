using System;
using UnityEngine;

public enum Sfx { Click, Good, Bad, Coin, Bark, Level }

// All sound is synthesised at startup: short effects plus a looping ambience per map zone.
public class AudioManager : MonoBehaviour
{
    const int SR = 22050;
    const float LoopLen = 8f;

    AudioSource sfxSource;
    AudioSource[] amb = new AudioSource[7];
    AudioClip[] sfx = new AudioClip[6];
    public bool muted;

    public void Build()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        sfx[(int)Sfx.Click] = Clip("click", 0.06f, t => Sine(880, t) * Env(t, 0.06f) * 0.4f);
        sfx[(int)Sfx.Good] = Clip("good", 0.36f, t => Arp(t, new float[] { 523.25f, 659.25f, 783.99f }, 0.12f) * 0.4f);
        sfx[(int)Sfx.Bad] = Clip("bad", 0.3f, t => Saw(Mathf.Lerp(220, 150, t / 0.3f), t) * Env(t, 0.3f) * 0.3f);
        sfx[(int)Sfx.Coin] = Clip("coin", 0.2f, t => (t < 0.07f ? Sine(1319, t) : Sine(1760, t)) * Env(t, 0.2f) * 0.35f);
        sfx[(int)Sfx.Level] = Clip("level", 0.6f, t => Arp(t, new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.15f) * 0.4f);
        sfx[(int)Sfx.Bark] = Clip("bark", 0.4f, t =>
        {
            float tt = t % 0.2f;
            if (t > 0.35f) return 0f;
            float n = Mathf.Sin(tt * 2f * Mathf.PI * 260f) + Mathf.Sin(tt * 2f * Mathf.PI * 390f) * 0.5f;
            return n * Mathf.Exp(-tt * 18f) * 0.3f;
        });

        // Ambience, one per zone (+ the open plaza).
        amb[(int)ZoneType.Home] = Loop("home", t => Chord(t, 130.81f, 164.81f, 196f) * (0.7f + 0.3f * Mathf.Sin(t * 2f * Mathf.PI * 0.25f)) * 0.5f);
        amb[(int)ZoneType.Park] = Loop("park", ParkSample());
        amb[(int)ZoneType.Training] = Loop("training", t =>
        {
            float gate = Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 2f));
            float[] notes = { 220f, 261.63f, 329.63f, 261.63f };
            float n = notes[(int)(t * 4f) % 4];
            return (Tone(110, t) * gate * 0.35f + Tone(n, t) * 0.25f) * 0.7f;
        });
        amb[(int)ZoneType.Clinic] = Loop("clinic", t => (Tone(261.63f, t) + Tone(392f, t) * 0.7f) * (0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * 0.125f)) * 0.3f);
        amb[(int)ZoneType.Shop] = Loop("shop", t =>
        {
            float[] pent = { 523.25f, 587.33f, 659.25f, 783.99f, 880f, 659.25f, 587.33f, 783.99f };
            int k = (int)(t / 1f); float tt = t - k;
            return Tone(pent[k % 8], t) * Mathf.Exp(-tt * 4f) * 0.35f + Tone(130.81f, t) * 0.12f;
        });
        amb[(int)ZoneType.Welfare] = Loop("welfare", t =>
        {
            float chime = (t % 4f) < 1.2f ? Tone(1046.5f, t) * Mathf.Exp(-(t % 4f) * 3f) * 0.25f : 0f;
            return Chord(t, 196f, 246.94f, 293.66f) * 0.45f + chime;
        });
        amb[6] = Loop("plaza", PlazaSample());

        for (int i = 0; i < amb.Length; i++)
        {
            if (amb[i] == null) continue;
            amb[i].volume = 0f;
            amb[i].Play();
        }
    }

    public void Play(Sfx s)
    {
        if (muted || sfxSource == null) return;
        sfxSource.PlayOneShot(sfx[(int)s], 0.8f);
    }

    void Update()
    {
        var gm = GameManager.I;
        int zi = gm.currentZone == null ? 6 : (int)gm.currentZone.type;
        for (int i = 0; i < amb.Length; i++)
        {
            if (amb[i] == null) continue;
            float target = (!muted && i == zi) ? 0.28f : 0f;
            amb[i].volume = Mathf.MoveTowards(amb[i].volume, target, Time.unscaledDeltaTime * 0.4f);
        }
    }

    // ---- synthesis helpers ----
    static float Sine(float f, float t) { return Mathf.Sin(2f * Mathf.PI * f * t); }
    static float Saw(float f, float t) { return ((t * f) % 1f) * 2f - 1f; }
    static float Env(float t, float len) { return Mathf.Clamp01(1f - t / len) * Mathf.Clamp01(t * 200f); }
    // Loop-safe tone: frequency snapped so an integer number of cycles fits the 8s loop.
    static float Tone(float f, float t) { return Mathf.Sin(2f * Mathf.PI * Mathf.Round(f * LoopLen) / LoopLen * t); }
    static float Chord(float t, float a, float b, float c) { return (Tone(a, t) + Tone(b, t) * 0.8f + Tone(c, t) * 0.6f) * 0.3f; }

    static float Arp(float t, float[] notes, float step)
    {
        int i = Mathf.Min(notes.Length - 1, (int)(t / step));
        float tt = t - i * step;
        return Sine(notes[i], t) * Mathf.Exp(-tt * 6f) * Mathf.Clamp01(tt * 300f);
    }

    static Func<float, float> ParkSample()
    {
        var rng = new System.Random(3);
        int n = (int)(SR * LoopLen);
        float[] wind = new float[n];
        float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float noise = (float)(rng.NextDouble() * 2 - 1);
            lp += (noise - lp) * 0.02f;
            wind[i] = lp * 1.4f * (0.6f + 0.4f * Mathf.Sin(i / (float)SR * 2f * Mathf.PI * 0.125f));
        }
        return t =>
        {
            int idx = Mathf.Clamp((int)(t * SR), 0, n - 1);
            float chirp = 0f;
            float[] starts = { 1f, 3.2f, 5.5f, 6.1f };
            foreach (float s in starts)
            {
                float tt = t - s;
                if (tt >= 0f && tt < 0.18f) chirp += Sine(Mathf.Lerp(2500, 3600, tt / 0.18f), t) * Mathf.Sin(tt / 0.18f * Mathf.PI) * 0.18f;
            }
            return wind[idx] + chirp;
        };
    }

    static Func<float, float> PlazaSample()
    {
        var rng = new System.Random(11);
        int n = (int)(SR * LoopLen);
        float[] wind = new float[n];
        float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            lp += ((float)(rng.NextDouble() * 2 - 1) - lp) * 0.012f;
            wind[i] = lp * 1.2f;
        }
        return t => wind[Mathf.Clamp((int)(t * SR), 0, n - 1)] + Tone(174.6f, t) * 0.05f;
    }

    static AudioClip Clip(string name, float len, Func<float, float> f)
    {
        int n = Mathf.CeilToInt(SR * len);
        var data = new float[n];
        for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)SR), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, SR, false);
        c.SetData(data, 0);
        return c;
    }

    AudioSource Loop(string name, Func<float, float> f)
    {
        var clip = Clip(name, LoopLen, f);
        var go = new GameObject("Amb_" + name);
        go.transform.SetParent(transform, false);
        var s = go.AddComponent<AudioSource>();
        s.clip = clip; s.loop = true; s.playOnAwake = false;
        return s;
    }
}
