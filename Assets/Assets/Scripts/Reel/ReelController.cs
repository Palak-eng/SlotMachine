using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    [Header("Symbols")]
    public Image[] symbols;

    [Header("Sprites")]
    public Sprite[] symbolSprites;

    [Header("Spin Settings")]
    public float spinSpeed = 1400f;
    public float spinDuration = 2f;
    public float deceleration = 2800f; // units/sec² — replaces broken Lerp

    private bool isSpinning;

    private float symbolSpacing = 160f;
    private float bottomLimit = -240f;
    private float centerY = 80f;

    public int ResultIndex { get; private set; }

    public void StartSpin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinRoutine());
        }
    }

    IEnumerator SpinRoutine()
    {
        isSpinning = true;

        float timer = 0f;
        float currentSpeed = spinSpeed;
        bool slowingDown = false;
        bool stopRecycling = false;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= spinDuration)
            {
                slowingDown = true;
            }

            if (slowingDown)
            {
                // FIX 1: Use MoveTowards for frame-rate-independent linear deceleration.
                // Lerp(speed, 0, deltaTime * k) is exponential decay and varies with frame rate.
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            if (currentSpeed < 250f)
            {
                stopRecycling = true;
            }

            MoveSymbols(currentSpeed, stopRecycling);

            // Exit when a symbol is close enough to snap, OR when the reel has
            // fully stopped (speed == 0). The second condition is essential:
            // MoveTowards actually reaches 0, and if no symbol happens to be
            // within 5 units of centerY at that moment, IsCentered() returns
            // false and the coroutine loops forever — leaving isSpinning = true
            // permanently and blocking every subsequent StartSpin() call.
            bool fullyStopped = slowingDown && currentSpeed <= 0f;
            bool readyToSnap  = slowingDown && currentSpeed < 80f && IsCentered();

            if (readyToSnap || fullyStopped)
            {
                Image centerSymbol = SnapToCenter();
                DetermineResult(centerSymbol);
                break;
            }

            yield return null;
        }

        isSpinning = false;
    }

    void MoveSymbols(float speed, bool stopRecycling)
    {
        foreach (Image symbol in symbols)
        {
            RectTransform rect = symbol.rectTransform;
            rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

            if (rect.anchoredPosition.y < bottomLimit)
            {
                float highestY = GetHighestY();
                rect.anchoredPosition = new Vector2(
                    rect.anchoredPosition.x,
                    highestY + symbolSpacing
                );

                // FIX 3: Always recycle position so symbols never disappear during slowdown.
                // Only randomize the sprite while the result isn't locked in yet.
                if (!stopRecycling)
                {
                    symbol.sprite = symbolSprites[Random.Range(0, symbolSprites.Length)];
                }
                // When stopRecycling, the symbol wraps back to the top but keeps its
                // current sprite, so the visible result stays consistent.
            }
        }
    }

    float GetHighestY()
    {
        float highest = symbols[0].rectTransform.anchoredPosition.y;

        foreach (Image symbol in symbols)
        {
            float y = symbol.rectTransform.anchoredPosition.y;
            if (y > highest) highest = y;
        }

        return highest;
    }

    bool IsCentered()
    {
        foreach (Image symbol in symbols)
        {
            float y = symbol.rectTransform.anchoredPosition.y;
            if (Mathf.Abs(y - centerY) < 5f) return true;
        }

        return false;
    }

    // FIX 2: Returns the symbol that was snapped to center, and repositions ALL
    // other symbols relative to it so the reel is perfectly aligned on stop.
    Image SnapToCenter()
    {
        // Find the symbol closest to the center line.
        Image closestSymbol = null;
        float closestDistance = Mathf.Infinity;

        foreach (Image symbol in symbols)
        {
            float distance = Mathf.Abs(symbol.rectTransform.anchoredPosition.y - centerY);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSymbol = symbol;
            }
        }

        // Sort a copy of the array by descending Y (top → bottom visual order).
        Image[] sorted = (Image[])symbols.Clone();
        System.Array.Sort(sorted, (a, b) =>
            b.rectTransform.anchoredPosition.y.CompareTo(a.rectTransform.anchoredPosition.y));

        // Find where the center symbol sits in the sorted order.
        int centerIdx = System.Array.IndexOf(sorted, closestSymbol);

        // Reposition every symbol so they are evenly spaced around centerY.
        for (int i = 0; i < sorted.Length; i++)
        {
            int offset = centerIdx - i; // positive = above center, negative = below
            sorted[i].rectTransform.anchoredPosition = new Vector2(
                sorted[i].rectTransform.anchoredPosition.x,
                centerY + offset * symbolSpacing
            );
        }

        return closestSymbol;
    }

    // FIX 2 cont.: Accepts the already-found center symbol instead of re-searching.
    void DetermineResult(Image centerSymbol)
    {
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (centerSymbol.sprite == symbolSprites[i])
            {
                ResultIndex = i;
                break;
            }
        }
    }
}
