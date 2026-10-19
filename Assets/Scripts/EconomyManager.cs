using UnityEngine;

// Coins, inventory, upgrades and passive income.
public class EconomyManager : MonoBehaviour
{
    public int coins = 50;
    public int food = 3, treats = 2;
    public bool toy, leash, cones;
    public int houseLevel, fieldLevel;
    public int lastPassive;

    public const int MaxUpgrade = 2;
    const float PassiveInterval = 12f;
    float timer;

    public void Earn(int n, bool silent = false)
    {
        coins += n;
        if (!silent) GameManager.I.audioMgr.Play(Sfx.Coin);
    }

    public bool Spend(int n)
    {
        if (coins < n)
        {
            GameManager.I.ui.Toast("Nepietiek monētu (vajag " + n + ").");
            GameManager.I.audioMgr.Play(Sfx.Bad);
            return false;
        }
        coins -= n;
        return true;
    }

    // Overall wellbeing 0..1
    public static float Wellness(DogStatus d)
    {
        return (d.health + (100f - d.Hunger) + d.energy + d.mood) / 400f;
    }

    void Update()
    {
        var gm = GameManager.I;
        if (gm == null || !gm.hasDog || gm.ui.Blocking) return;
        timer += Time.deltaTime;
        if (timer < PassiveInterval) return;
        timer = 0f;
        var d = gm.dog;
        float w = Wellness(d);
        if (d.sick || w <= 0.5f) { lastPassive = 0; return; }
        lastPassive = Mathf.Max(1, Mathf.RoundToInt((1f + (w - 0.5f) * 6f) * (1f + 0.25f * houseLevel)));
        Earn(lastPassive, true);
    }
}
