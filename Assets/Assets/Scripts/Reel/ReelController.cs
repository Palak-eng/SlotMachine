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

    public float deceleration = 2800f;

    private bool isSpinning;

    private float symbolSpacing = 160f;

    private float bottomLimit = -320f;

    private float centerY = 0f;

    public int ResultIndex { get; private set; } = -1;

    public bool IsSpinning => isSpinning;

    public void StartSpin()
    {
        if (symbols == null || symbols.Length == 0)
        {
            Debug.LogError("Symbols array is empty.");
            return;
        }

        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogError("Symbol sprites array is empty.");
            return;
        }

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

            // Start slowing
            if (timer >= spinDuration)
            {
                slowingDown = true;
            }

            // Smooth deceleration
            if (slowingDown)
            {
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    deceleration * Time.deltaTime
                );
            }

            // Stop changing symbols near the end
            if (currentSpeed < 250f)
            {
                stopRecycling = true;
            }

            MoveSymbols(currentSpeed, stopRecycling);

            // Begin snapping BEFORE fully stopping
            bool readyToSnap =
                slowingDown &&
                currentSpeed < 180f &&
                IsCentered();

            bool fullyStopped =
                slowingDown &&
                currentSpeed <= 0f;

            if (readyToSnap || fullyStopped)
            {
                yield return StartCoroutine(
                    SmoothSnapToCenter()
                );

                Image centerSymbol = GetCenterSymbol();

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

            rect.anchoredPosition +=
                Vector2.down * speed * Time.deltaTime;

            // Recycle symbol
            if (rect.anchoredPosition.y < bottomLimit)
            {
                float highestY = GetHighestY();

                rect.anchoredPosition = new Vector2(
                    rect.anchoredPosition.x,
                    highestY + symbolSpacing
                );

                // Randomize only during fast spin
                if (!stopRecycling)
                {
                    symbol.sprite =
                        symbolSprites[
                            Random.Range(0, symbolSprites.Length)
                        ];
                }
            }
        }
    }

    float GetHighestY()
    {
        float highest = float.NegativeInfinity;

        foreach (Image symbol in symbols)
        {
            float y = symbol.rectTransform.anchoredPosition.y;

            if (y > highest)
            {
                highest = y;
            }
        }

        return highest;
    }

    bool IsCentered()
    {
        foreach (Image symbol in symbols)
        {
            float y = symbol.rectTransform.anchoredPosition.y;

            // Larger threshold = smoother final stop
            if (Mathf.Abs(y - centerY) < 35f)
            {
                return true;
            }
        }

        return false;
    }

    Image GetCenterSymbol()
    {
        Image closestSymbol = null;

        float closestDistance = Mathf.Infinity;

        foreach (Image symbol in symbols)
        {
            float distance =
                Mathf.Abs(
                    symbol.rectTransform.anchoredPosition.y -
                    centerY
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestSymbol = symbol;
            }
        }

        return closestSymbol;
    }

    IEnumerator SmoothSnapToCenter()
    {
        Image closestSymbol = GetCenterSymbol();

        float offset =
            centerY -
            closestSymbol.rectTransform.anchoredPosition.y;

        Vector2[] startPositions =
            new Vector2[symbols.Length];

        for (int i = 0; i < symbols.Length; i++)
        {
            startPositions[i] =
                symbols[i].rectTransform.anchoredPosition;
        }

        // Slightly longer smooth settle
        float duration = 0.22f;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            t = Mathf.Clamp01(t);

            // Smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i].rectTransform.anchoredPosition =
                    Vector2.Lerp(
                        startPositions[i],
                        startPositions[i] + Vector2.up * offset,
                        t
                    );
            }

            yield return null;
        }

        // Exact final alignment
        for (int i = 0; i < symbols.Length; i++)
        {
            symbols[i].rectTransform.anchoredPosition =
                startPositions[i] + Vector2.up * offset;
        }
    }

    void DetermineResult(Image centerSymbol)
    {
        ResultIndex = -1;

        if (centerSymbol == null)
            return;

        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (centerSymbol.sprite == symbolSprites[i])
            {
                ResultIndex = i;

                return;
            }
        }

        Debug.LogWarning(
            "Center symbol sprite not found in symbolSprites array."
        );
    }
}