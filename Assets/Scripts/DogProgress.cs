using UnityEngine;

// Per-dog level, xp and command skills.
public class DogProgress : MonoBehaviour
{
    public int level = 1;
    public int xp;
    public float[] skill = new float[4];

    public int XpToNext { get { return 30 + 25 * level; } }

    public bool Learned(Command c) { return skill[(int)c] >= 100f; }

    public int LearnedCount
    {
        get { int n = 0; for (int i = 0; i < 4; i++) if (skill[i] >= 100f) n++; return n; }
    }

    public void ResetProgress()
    {
        level = 1; xp = 0;
        for (int i = 0; i < skill.Length; i++) skill[i] = 0f;
    }

    public void AddXp(int n)
    {
        xp += n;
        while (xp >= XpToNext)
        {
            xp -= XpToNext;
            level++;
            var gm = GameManager.I;
            gm.ui.Toast(gm.dog.dogName + " sasniedza " + level + ". līmeni!");
            gm.audioMgr.Play(Sfx.Level);
        }
    }
}
