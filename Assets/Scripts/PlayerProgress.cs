using UnityEngine;

// Player level and xp. Level persists between dogs.
public class PlayerProgress : MonoBehaviour
{
    public int level = 1;
    public int xp;

    public int XpToNext { get { return 40 + 35 * level; } }

    public void AddXp(int n)
    {
        xp += n;
        while (xp >= XpToNext)
        {
            xp -= XpToNext;
            level++;
            var gm = GameManager.I;
            gm.ui.Toast("Spēlētāja līmenis: " + level + "! Pārbaudi uzlabojumus mājā un laukumā.");
            gm.audioMgr.Play(Sfx.Level);
        }
    }
}
