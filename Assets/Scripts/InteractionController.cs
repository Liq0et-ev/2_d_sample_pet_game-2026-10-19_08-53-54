using UnityEngine;
using UnityEngine.InputSystem;

// E interaction with buildings, plus zone tracking for ambience.
public class InteractionController : MonoBehaviour
{
    void Update()
    {
        var gm = GameManager.I;
        Vector2 p = gm.player.position;
        gm.currentZone = World.ZoneAt(p);

        if (gm.ui.Blocking) { gm.ui.SetPrompt(""); return; }

        var near = World.NearestInteract(p, 3.2f);
        gm.ui.SetPrompt(near == null ? "" : "[E]  " + near.name);

        var kb = Keyboard.current;
        if (near != null && kb != null && kb.eKey.wasPressedThisFrame)
        {
            gm.audioMgr.Play(Sfx.Click);
            gm.OpenZone(near.type);
        }
    }
}
