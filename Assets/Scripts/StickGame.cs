using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Park minigame: a needle spins round a ring; press Space (or click) while it is in the green zone to throw the stick.
// Hit = dog gets happier, centre of the zone = perfect (much happier), miss = that try is lost.
// There are always 5 tries and all of them are used.
public class StickGame : MonoBehaviour
{
    GameManager gm { get { return GameManager.I; } }

    const int Tries = 5;
    const float R = 170f;
    static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    GameObject root;
    RectTransform needle, arcRT, perfRT;
    Image arcImg, perfImg;
    Text info, result;
    bool running, frozen, finished;
    int attempt, good, perfect;
    float angle, speed, arcStart, arcWidth, perfWidth, timer, grace;
    float moodGained;

    public void Begin()
    {
        gm.ui.CloseMenu();
        RectTransform panel;
        root = gm.ui.CreateModal(new Vector2(700, 760), out panel);
        gm.ui.onModalCancel = null;       // no escape: all tries must be used

        var title = gm.ui.NewText(panel, "Nūjas mešana", 40, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.4f),
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(660, 50));
        title.fontStyle = FontStyle.Bold;
        info = gm.ui.NewText(panel, "", 26, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -72), new Vector2(660, 40));

        Vector2 c = new Vector2(0, -10);
        var ring = gm.ui.NewImage("Ring", panel, Center, Center, c, new Vector2(R * 2, R * 2), new Color(1, 1, 1, 0.85f));
        ring.sprite = Art.Ring;

        arcImg = MakeArc(panel, c, R * 2 + 18, new Color(0.3f, 0.85f, 0.4f, 0.95f));
        perfImg = MakeArc(panel, c, R * 2 + 34, new Color(1f, 0.85f, 0.2f, 1f));
        arcRT = arcImg.rectTransform;
        perfRT = perfImg.rectTransform;

        // "Space" key look from the reference
        var keyOuter = gm.ui.NewImage("KeyOuter", panel, Center, Center, c, new Vector2(250, 76), Color.white);
        var keyInner = gm.ui.NewImage("KeyInner", keyOuter.transform, Center, Center, Vector2.zero, new Vector2(240, 66), Color.black);
        gm.ui.NewText(keyInner.transform, "Space", 34, TextAnchor.MiddleCenter, new Color(0.65f, 0.65f, 0.65f), Center, Center, Vector2.zero, new Vector2(240, 66));

        needle = gm.ui.NewImage("Needle", panel, Center, new Vector2(0.5f, 0f), c, new Vector2(7, R + 14), new Color(0.9f, 0.15f, 0.15f)).rectTransform;

        result = gm.ui.NewText(panel, "", 34, TextAnchor.MiddleCenter, Color.white,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 62), new Vector2(660, 50));
        gm.ui.NewText(panel, "Space vai klikšķis – mest nūju, kad adata ir zaļajā zonā. Dzeltenais centrs = perfekts metiens!", 20,
            TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.75f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(660, 40));

        attempt = 0; good = 0; perfect = 0; moodGained = 0f;
        finished = false; frozen = false; running = true;
        grace = 0.5f;
        NextAttempt();
    }

    Image MakeArc(RectTransform panel, Vector2 pos, float size, Color col)
    {
        var im = gm.ui.NewImage("Arc", panel, Center, Center, pos, new Vector2(size, size), col);
        im.sprite = Art.Ring;
        im.type = Image.Type.Filled;
        im.fillMethod = Image.FillMethod.Radial360;
        im.fillOrigin = (int)Image.Origin360.Top;
        im.fillClockwise = true;
        return im;
    }

    void NextAttempt()
    {
        attempt++;
        arcStart = Random.Range(0f, 360f);
        arcWidth = Mathf.Lerp(72f, 42f, (attempt - 1) / (float)(Tries - 1));
        perfWidth = arcWidth * 0.26f;
        speed = 220f + 28f * (attempt - 1);
        angle = Mathf.Repeat(arcStart + 180f, 360f);

        arcImg.fillAmount = arcWidth / 360f;
        arcRT.localRotation = Quaternion.Euler(0, 0, -arcStart);
        perfImg.fillAmount = perfWidth / 360f;
        perfRT.localRotation = Quaternion.Euler(0, 0, -(arcStart + (arcWidth - perfWidth) * 0.5f));
        needle.localRotation = Quaternion.Euler(0, 0, -angle);
        result.text = "";
        frozen = false;
        RefreshInfo();
    }

    void RefreshInfo()
    {
        info.text = "Mēģinājums " + attempt + "/" + Tries + "   |   Labi: " + good + "   Perfekti: " + perfect;
    }

    void Press()
    {
        float d = Mathf.Repeat(angle - arcStart, 360f);
        var dog = gm.dog;
        dog.Mod(0, -1, -3, 0);
        if (d <= arcWidth)
        {
            bool perf = Mathf.Abs(d - arcWidth * 0.5f) <= perfWidth * 0.5f;
            if (perf)
            {
                perfect++;
                dog.Mod(0, 0, 0, 16);
                moodGained += 16;
                gm.economy.Earn(3, true);
                gm.dogProg.AddXp(10);
                result.text = "Perfekti! " + dog.dogName + " ir ļoti laimīgs!";
                result.color = new Color(1f, 0.9f, 0.3f);
                gm.audioMgr.Play(Sfx.Level);
            }
            else
            {
                good++;
                dog.Mod(0, 0, 0, 8);
                moodGained += 8;
                gm.economy.Earn(1, true);
                gm.dogProg.AddXp(4);
                result.text = "Labi! " + dog.dogName + " priecājas.";
                result.color = new Color(0.5f, 1f, 0.55f);
                gm.audioMgr.Play(Sfx.Good);
            }
            gm.audioMgr.Play(Sfx.Bark);
        }
        else
        {
            result.text = "Garām! Mēģinājums zaudēts.";
            result.color = new Color(1f, 0.5f, 0.5f);
            gm.audioMgr.Play(Sfx.Bad);
        }
        RefreshInfo();
        frozen = true;
        timer = attempt >= Tries ? 1.8f : 0.9f;
        if (attempt >= Tries)
        {
            finished = true;
            result.text += "\nKopā: Labi " + good + ", Perfekti " + perfect + ", garām " + (Tries - good - perfect);
        }
    }

    void End()
    {
        running = false;
        gm.ui.CloseModal(root);
        root = null;
        gm.playerProg.AddXp(2 * (good + perfect));
        gm.ui.Toast("Nūjas mešana pabeigta: " + perfect + " perfekti, " + good + " labi. Garastāvoklis +" + Mathf.RoundToInt(moodGained) + ".");
    }

    void Update()
    {
        if (!running) return;
        float dt = Time.unscaledDeltaTime;
        if (grace > 0f) grace -= dt;

        if (frozen)
        {
            timer -= dt;
            if (timer <= 0f)
            {
                if (finished) End();
                else NextAttempt();
            }
            return;
        }

        angle = Mathf.Repeat(angle + speed * dt, 360f);
        needle.localRotation = Quaternion.Euler(0, 0, -angle);

        if (grace > 0f) return;
        var kb = Keyboard.current;
        var ms = Mouse.current;
        if ((kb != null && kb.spaceKey.wasPressedThisFrame) || (ms != null && ms.leftButton.wasPressedThisFrame)) Press();
    }
}
