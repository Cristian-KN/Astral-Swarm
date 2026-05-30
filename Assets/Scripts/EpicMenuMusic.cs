using UnityEngine;

/// <summary>
/// Genera por código un tema de menú épico-medieval (Re menor, lento y solemne):
/// pads de cuerda por síntesis aditiva, melodía noble, bajo grave, timbales y reverb.
/// Devuelve un AudioClip estéreo en bucle perfecto. Es música ORIGINAL y sin derechos,
/// estilo "mockup orquestal sintetizado" (no orquesta real).
/// </summary>
public static class EpicMenuMusic
{
    const int SR = 44100;

    static float NoteHz(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

    // Progresión en Re menor (8 compases): Dm  Bb  F  C  Gm  Dm  A  A
    // (i  VI  III  VII  iv  i  V  V) — el final V(A) → i(Dm) hace que el bucle resuelva.
    static readonly int[][] Chords =
    {
        new[]{ 50, 53, 57, 62 }, // Dm   (D3 F3 A3 D4)
        new[]{ 46, 50, 53, 58 }, // Bb   (Bb2 D3 F3 Bb3)
        new[]{ 53, 57, 60, 65 }, // F    (F3 A3 C4 F4)
        new[]{ 48, 52, 55, 60 }, // C    (C3 E3 G3 C4)
        new[]{ 43, 46, 50, 55 }, // Gm   (G2 Bb2 D3 G3)
        new[]{ 50, 53, 57, 62 }, // Dm
        new[]{ 45, 49, 52, 57 }, // A    (A2 C#3 E3 A3)
        new[]{ 45, 49, 52, 57 }, // A
    };
    static readonly int[] BassMidi = { 38, 34, 41, 36, 31, 38, 33, 33 }; // raíces graves

    // Melodía solemne (beat global 0..31 en el bucle de 8 compases): midi, beatInicio, duraciónBeats
    static readonly float[][] Melody =
    {
        // compás 0: silencio (deja respirar al pad)
        new[]{ 69f, 4f, 2f }, new[]{ 65f, 6f, 2f },          // A4, F4
        new[]{ 62f, 8f, 4f },                                 // D4 (larga)
        new[]{ 64f,12f, 1f }, new[]{ 65f,13f, 1f }, new[]{ 67f,14f, 2f }, // E4 F4 G4
        new[]{ 69f,16f, 3f },                                 // A4
        new[]{ 70f,20f, 2f }, new[]{ 69f,22f, 2f },           // Bb4 A4
        new[]{ 67f,24f, 2f }, new[]{ 65f,26f, 2f },           // G4 F4
        new[]{ 64f,28f, 3f },                                 // E4 (tensión sobre A), beat31 en silencio -> bucle limpio
    };

    public static AudioClip Generate()
    {
        double bpm = 68.0;
        double beat = 60.0 / bpm;
        int beatsPerBar = 4;
        double barDur = beat * beatsPerBar;
        int loopBars = 8;

        int loopN = Mathf.RoundToInt((float)(loopBars * barDur * SR));
        int bufferN = loopN * 2;                  // renderizamos 2 vueltas para que la reverb/pads encadenen el bucle
        float[] dry = new float[bufferN];

        // --- síntesis de las 2 vueltas (16 compases) ---
        for (int copy = 0; copy < 2; copy++)
        {
            double copyOff = copy * loopBars * barDur;
            for (int bar = 0; bar < loopBars; bar++)
            {
                double bs = copyOff + bar * barDur;

                // Pad de cuerda (acorde)
                int[] chord = Chords[bar];
                foreach (int m in chord)
                    AddTone(dry, bs, barDur, NoteHz(m), 0.085f,
                            atk: 0.5f, dec: 0.4f, sus: 0.85f, rel: 0.8f,
                            harmonics: PadHarm, detune: 0.004f, vibRate: 4.5f, vibDepth: 0.004f);

                // Bajo grave
                AddTone(dry, bs, barDur * 0.98, NoteHz(BassMidi[bar]), 0.16f,
                        atk: 0.06f, dec: 0.3f, sus: 0.7f, rel: 0.4f,
                        harmonics: BassHarm, detune: 0.002f, vibRate: 0f, vibDepth: 0f);

                // Timbales en los tiempos 1 y 3
                AddTimpani(dry, bs, 0.5f);
                AddTimpani(dry, bs + beat * 2, 0.32f);
            }

            // Melodía (mismo patrón en ambas vueltas)
            foreach (var note in Melody)
            {
                double start = copyOff + note[1] * beat;
                double dur = note[2] * beat;
                AddTone(dry, start, dur, NoteHz(Mathf.RoundToInt(note[0])), 0.20f,
                        atk: 0.06f, dec: 0.15f, sus: 0.8f, rel: 0.35f,
                        harmonics: LeadHarm, detune: 0.003f, vibRate: 5.2f, vibDepth: 0.006f);
            }
        }

        // --- reverb estéreo (Freeverb-lite) ---
        float[] left = new float[bufferN];
        float[] right = new float[bufferN];
        Reverb(dry, left, right);

        // --- extraer la 2ª vuelta como bucle perfecto + normalizar ---
        float peak = 0.0001f;
        for (int i = loopN; i < bufferN; i++)
        {
            float al = Mathf.Abs(left[i]); if (al > peak) peak = al;
            float ar = Mathf.Abs(right[i]); if (ar > peak) peak = ar;
        }
        float gain = 0.92f / peak;

        var clip = AudioClip.Create("EpicMenuTheme", loopN, 2, SR, false);
        float[] inter = new float[loopN * 2];
        for (int i = 0; i < loopN; i++)
        {
            inter[2 * i]     = SoftClip(left[loopN + i] * gain);
            inter[2 * i + 1] = SoftClip(right[loopN + i] * gain);
        }
        clip.SetData(inter, 0);
        return clip;
    }

    // amplitudes de armónicos por timbre
    static readonly float[] PadHarm  = { 1f, 0.45f, 0.22f };
    static readonly float[] BassHarm = { 1f, 0.35f };
    static readonly float[] LeadHarm = { 1f, 0.5f, 0.28f };

    static float SoftClip(float x) => (float)System.Math.Tanh(x);

    static float Adsr(float t, float dur, float a, float d, float sus, float r)
    {
        if (t < 0f) return 0f;
        if (t < a) return t / a;
        if (t < a + d) return Mathf.Lerp(1f, sus, (t - a) / d);
        if (t < dur) return sus;
        float rt = t - dur;
        if (rt < r) return sus * (1f - rt / r);
        return 0f;
    }

    static void AddTone(float[] buf, double startSec, double durSec, float freq, float amp,
                        float atk, float dec, float sus, float rel,
                        float[] harmonics, float detune, float vibRate, float vibDepth)
    {
        int s0 = (int)(startSec * SR);
        int n = (int)((durSec + rel) * SR);
        float TWO_PI = 2f * Mathf.PI;
        for (int i = 0; i < n; i++)
        {
            int idx = s0 + i;
            if (idx < 0) continue;
            if (idx >= buf.Length) break;
            float t = i / (float)SR;
            float env = Adsr(t, (float)durSec, atk, dec, sus, rel);
            if (env <= 0f) continue;
            float vib = vibDepth > 0f ? 1f + vibDepth * Mathf.Sin(TWO_PI * vibRate * t) : 1f;
            float s = 0f;
            for (int h = 0; h < harmonics.Length; h++)
            {
                float hf = freq * (h + 1) * vib;
                if (hf > SR * 0.45f) break;
                float a = harmonics[h];
                if (h == 0 && detune > 0f)
                {
                    // dos voces ligeramente desafinadas en el fundamental -> calidez "ensemble"
                    s += 0.5f * a * Mathf.Sin(TWO_PI * hf * (1f + detune) * t);
                    s += 0.5f * a * Mathf.Sin(TWO_PI * hf * (1f - detune) * t);
                }
                else
                {
                    s += a * Mathf.Sin(TWO_PI * hf * t);
                }
            }
            buf[idx] += s * env * amp;
        }
    }

    static void AddTimpani(float[] buf, double startSec, float amp)
    {
        int s0 = (int)(startSec * SR);
        float dur = 0.55f;
        int n = (int)(dur * SR);
        int noiseLen = (int)(0.02f * SR);
        // ruido determinista (no usamos Random para que el clip sea reproducible)
        uint seed = 0x9E3779B9u ^ (uint)s0;
        for (int i = 0; i < n; i++)
        {
            int idx = s0 + i;
            if (idx < 0) continue;
            if (idx >= buf.Length) break;
            float t = i / (float)SR;
            float env = Mathf.Exp(-t * 7f);
            float pitch = 90f * (1f + 0.6f * Mathf.Exp(-t * 28f)); // caída de tono del golpe
            float s = Mathf.Sin(2f * Mathf.PI * pitch * t) * 0.9f;
            if (i < noiseLen)
            {
                seed = seed * 1664525u + 1013904223u;
                float nz = ((seed >> 9) / (float)(1 << 23)) * 2f - 1f;
                s += nz * Mathf.Exp(-t * 120f) * 0.5f;
            }
            buf[idx] += s * env * amp;
        }
    }

    // ---------------- reverb estéreo (combs en paralelo + allpass en serie) ----------------
    static readonly int[] CombTuning = { 1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617 };
    static readonly int[] AllpassTuning = { 556, 441, 341, 225 };
    const int StereoSpread = 23;

    static void Reverb(float[] dry, float[] left, float[] right)
    {
        BuildChannel(dry, left, 0);
        BuildChannel(dry, right, StereoSpread);
    }

    static void BuildChannel(float[] dry, float[] outBuf, int spread)
    {
        const float feedback = 0.84f;
        const float damp = 0.2f;
        const float wetGain = 0.34f;
        const float dryGain = 0.72f;

        var combs = new Comb[CombTuning.Length];
        for (int c = 0; c < combs.Length; c++) combs[c] = new Comb(CombTuning[c] + spread, feedback, damp);
        var aps = new Allpass[AllpassTuning.Length];
        for (int a = 0; a < aps.Length; a++) aps[a] = new Allpass(AllpassTuning[a] + spread, 0.5f);

        for (int i = 0; i < dry.Length; i++)
        {
            float x = dry[i];
            float wet = 0f;
            for (int c = 0; c < combs.Length; c++) wet += combs[c].Process(x);
            wet /= combs.Length;
            for (int a = 0; a < aps.Length; a++) wet = aps[a].Process(wet);
            outBuf[i] = x * dryGain + wet * wetGain;
        }
    }

    class Comb
    {
        readonly float[] buf; int idx; readonly float fb, damp; float store;
        public Comb(int size, float fb, float damp) { buf = new float[Mathf.Max(1, size)]; this.fb = fb; this.damp = damp; }
        public float Process(float x)
        {
            float y = buf[idx];
            store = y * (1f - damp) + store * damp;
            buf[idx] = x + store * fb;
            if (++idx >= buf.Length) idx = 0;
            return y;
        }
    }

    class Allpass
    {
        readonly float[] buf; int idx; readonly float fb;
        public Allpass(int size, float fb) { buf = new float[Mathf.Max(1, size)]; this.fb = fb; }
        public float Process(float x)
        {
            float y = buf[idx];
            float o = -x + y;
            buf[idx] = x + y * fb;
            if (++idx >= buf.Length) idx = 0;
            return o;
        }
    }
}
