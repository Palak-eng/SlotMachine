using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LeverAnimation : MonoBehaviour
{
    public Image leverImage;

    public Sprite leverUpSprite;

    public Sprite leverDownSprite;

    public RectTransform leverTransform;

    public float pressDistance = 40f;

    public float animationSpeed = 8f;

    private Vector2 startPos;

    private bool isAnimating;

    void Start()
    {
        startPos = leverTransform.anchoredPosition;
    }

    public void PullLever()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateLever());
        }
    }

    IEnumerator AnimateLever()
    {
        isAnimating = true;

        Vector2 downPos =
            startPos + Vector2.down * pressDistance;

        // Change sprite
        leverImage.sprite = leverDownSprite;

        // Move down
        while (
            Vector2.Distance(
                leverTransform.anchoredPosition,
                downPos
            ) > 1f
        )
        {
            leverTransform.anchoredPosition =
                Vector2.Lerp(
                    leverTransform.anchoredPosition,
                    downPos,
                    animationSpeed * Time.deltaTime
                );

            yield return null;
        }

        yield return new WaitForSeconds(0.08f);

        // Change back sprite
        leverImage.sprite = leverUpSprite;

        // Move up
        while (
            Vector2.Distance(
                leverTransform.anchoredPosition,
                startPos
            ) > 1f
        )
        {
            leverTransform.anchoredPosition =
                Vector2.Lerp(
                    leverTransform.anchoredPosition,
                    startPos,
                    animationSpeed * Time.deltaTime
                );

            yield return null;
        }

        leverTransform.anchoredPosition = startPos;

        isAnimating = false;
    }
}