using UnityEngine;

public enum Temperament { Active, Calm, Timid }

// Four needs of the active dog: health, hunger, energy, mood.
public class DogStatus : MonoBehaviour
{
    public string dogName = "Reksis";
    public Temperament temperament;
    public Color coat = new Color(0.8f, 0.55f, 0.3f);

    public float health = 100f, food = 80f, energy = 100f, mood = 70f;
    public bool sick;

    float sickTimer;

    public float Hunger { get { return 100f - food; } }

    public static string TemperamentName(Temperament t)
    {
        switch (t)
        {
            case Temperament.Active: return "Aktīvs";
            case Temperament.Calm: return "Mierīgs";
            default: return "Bailīgs";
        }
    }

    public string TemperamentInfo()
    {
        switch (temperament)
        {
            case Temperament.Active: return "Aktīvs: ātri nogurst, bet labāk veic kustību komandas.";
            case Temperament.Calm: return "Mierīgs: labi koncentrējas, labāk apgūst sēdēšanu, gulēšanu un gaidīšanu.";
            default: return "Bailīgs: vajag daudz pozitīvas pieredzes; sliktā garastāvoklī mācās slikti.";
        }
    }

    public void Init(string n, Temperament t, Color c)
    {
        dogName = n; temperament = t; coat = c;
        health = 100f; food = 75f; energy = 100f;
        mood = t == Temperament.Timid ? 50f : 70f;
        sick = false; sickTimer = 0f;
        var v = GetComponent<DogVisual>();
        if (v != null) v.Recolor(c);
    }

    public void Mod(float h, float f, float e, float m)
    {
        health = Mathf.Clamp(health + h, 1f, 100f);
        food = Mathf.Clamp(food + f, 0f, 100f);
        energy = Mathf.Clamp(energy + e, 0f, 100f);
        mood = Mathf.Clamp(mood + m, 0f, 100f);
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || !gm.hasDog) return;
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        food = Mathf.Max(0f, food - 0.30f * dt);

        float eLoss = 0.06f * dt * (temperament == Temperament.Active ? 1.5f : 1f);
        if (gm.dogFollow != null && gm.dogFollow.Moving) eLoss *= 3f;
        energy = Mathf.Max(0f, energy - eLoss);

        float md = -0.08f;
        if (food < 25f) md -= 0.25f;
        if (energy < 15f) md -= 0.10f;
        if (sick) md -= 0.20f;
        mood = Mathf.Clamp(mood + md * dt, 0f, 100f);

        if (food <= 0f) health = Mathf.Max(1f, health - 0.4f * dt);
        else if (sick) health = Mathf.Max(5f, health - 0.15f * dt);
        else if (food > 50f && health < 100f) health = Mathf.Min(100f, health + 0.05f * dt);

        sickTimer += dt;
        if (sickTimer > 60f)
        {
            sickTimer = 0f;
            float chance = 0.06f + (food < 20f ? 0.15f : 0f) + (mood < 20f ? 0.10f : 0f);
            if (!sick && Random.value < chance)
            {
                sick = true;
                gm.ui.Toast(dogName + " saslima! Nogādā to veterinārajā klīnikā.");
                gm.audioMgr.Play(Sfx.Bad);
            }
        }

        var vis = GetComponent<DogVisual>();
        if (vis != null && vis.status != null)
        {
            string s = "";
            if (sick) s = "+";
            else if (food < 25f) s = "!";
            else if (energy < 15f) s = "zZ";
            else if (mood > 80f) s = "<3";
            vis.status.text = s;
        }
    }
}
