using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CombatantDisplay : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Coroutine      _flashCoroutine;
    private Vector3        _lungeOrigin;

    static readonly int PropFlash      = Shader.PropertyToID("_FlashAmount");
    static readonly int PropFlashColor = Shader.PropertyToID("_FlashColor");
    static readonly int PropOutline    = Shader.PropertyToID("_OutlineEnabled");
    static readonly int PropOutColor   = Shader.PropertyToID("_OutlineColor");

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void SetSprite(Texture2D sheet, int row, int col, int spriteSize = 32)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        sheet.filterMode = FilterMode.Point;
        sheet.wrapMode   = TextureWrapMode.Clamp;
        _sr.sprite = SpriteManager.GetSprite(sheet, row, col, spriteSize);
    }

    // ── Outline ──────────────────────────────────────────────────────────────

    public void SetOutline(bool enabled, Color color)
    {
        _sr.material.SetFloat(PropOutline, enabled ? 1f : 0f);
        _sr.material.SetColor(PropOutColor, color);
    }

    // ── Flash variants ───────────────────────────────────────────────────────

    public void FlashOnHit()     => Flash(Color.white,0.18f);
    public void FlashHeal()      => Flash(new Color(0.2f, 1f,   0.3f),0.5f);
    public void FlashLifesteal() => Flash(new Color(1f,   0.2f, 0.3f),0.5f);
    public void FlashBuff()      => Flash(new Color(1f,   0.65f, 0f),0.5f);

    private void Flash(Color color,float duration)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine(color,duration));
    }

    private IEnumerator FlashRoutine(Color flashColor,float duration)
    {
        _sr.material.SetColor(PropFlashColor, flashColor);
        _sr.material.SetFloat(PropFlash, 1f);
        yield return new WaitForSeconds(0.07f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _sr.material.SetFloat(PropFlash, 1f - elapsed / duration);
            yield return null;
        }
        _sr.material.SetFloat(PropFlash, 0f);
        _flashCoroutine = null;
    }

    // ── Lunge animation ──────────────────────────────────────────────────────

    public IEnumerator LungeForward(Vector3 targetWorldPos)
    {
        _lungeOrigin    = transform.position;
        Vector3 peakPos = Vector3.Lerp(_lungeOrigin, targetWorldPos, 0.8f);

        float t = 0f, dur = 0.1f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            transform.position = Vector3.Lerp(_lungeOrigin, peakPos, t / dur);
            yield return null;
        }
        transform.position = peakPos;

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.07f);
        Time.timeScale = 1f;
    }

    public IEnumerator LungeReturn()
    {
        Vector3 from = transform.position;
        float t = 0f, dur = 0.2f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            transform.position = Vector3.Lerp(from, _lungeOrigin, t / dur);
            yield return null;
        }
        transform.position = _lungeOrigin;
    }

    // ── Sprite animation ─────────────────────────────────────────────────────

    private Sprite[]  _idleSprites;
    private Sprite[]  _attackSprites;
    private float     _idleFps   = 6f;
    private float     _attackFps = 8f;
    private Coroutine _animCoroutine;

    // Pass the Resources path (without extension) to the sliced sprite sheet.
    // The sheet must be imported as Sprite (Multiple) and sliced in the Sprite Editor.
    public bool SetupAnimation(string idlePath, float idleFps = 6f,
                               string attackPath = null, float attackFps = 8f)
    {
        _idleFps       = idleFps;
        _attackFps     = attackFps;
        _idleSprites   = LoadSorted(idlePath);
        _attackSprites = attackPath != null ? LoadSorted(attackPath) : null;

        if (_idleSprites == null || _idleSprites.Length == 0) return false;
        ResumeIdle();
        return true;
    }

    public void SwitchToAttack()
    {
        if (_attackSprites != null && _attackSprites.Length > 0)
            PlayLoop(_attackSprites, _attackFps);
    }

    public void ResumeIdle()
    {
        if (_idleSprites != null && _idleSprites.Length > 0)
            PlayLoop(_idleSprites, _idleFps);
    }

    private void PlayLoop(Sprite[] sprites, float fps)
    {
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimLoop(sprites, fps));
    }

    private IEnumerator AnimLoop(Sprite[] sprites, float fps)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        float delay = 1f / fps;
        int   frame = 0;
        while (true)
        {
            _sr.sprite = sprites[frame];
            frame = (frame + 1) % sprites.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    private static Sprite[] LoadSorted(string resourcePath)
    {
        var sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites == null || sprites.Length == 0) return null;
        System.Array.Sort(sprites, (a, b) =>
            string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return sprites;
    }
}
