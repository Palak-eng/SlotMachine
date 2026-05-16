using System.Collections;
using TMPro;
using UnityEngine;

public class SlotMachineManager : MonoBehaviour
{
    [Header("Reels")]
    public ReelController[] reels;

    [Header("UI")]
    public TMP_Text balanceText;
    public TMP_Text resultText;

    [Header("Game Settings")]
    public int balance = 1000;
    public int spinCost = 50;

    private bool isSpinning;

    void Start()
    {
        UpdateBalanceUI();

        resultText.gameObject.SetActive(false);
    }

    public void Spin()
    {
        if (isSpinning)
            return;

        if (balance < spinCost)
            return;

        balance -= spinCost;

        UpdateBalanceUI();

        StartCoroutine(SpinSequence());
    }

    IEnumerator SpinSequence()
{
    isSpinning = true;

    resultText.gameObject.SetActive(false);

    reels[0].StartSpin();

    yield return new WaitForSeconds(0.3f);

    reels[1].StartSpin();

    yield return new WaitForSeconds(0.3f);

    reels[2].StartSpin();

    yield return new WaitForSeconds(3.5f);

    CheckWin(
        reels[0].ResultIndex,
        reels[1].ResultIndex,
        reels[2].ResultIndex
    );

    isSpinning = false;
}
    void CheckWin(int r1, int r2, int r3)
    {
        if (r1 == r2 && r2 == r3)
        {
            int reward = GetReward(r1);

            balance += reward;

            UpdateBalanceUI();

            resultText.gameObject.SetActive(true);

            resultText.text = "YOU WIN $" + reward;
        }
        else
        {
            resultText.gameObject.SetActive(true);

            resultText.text = "TRY AGAIN";
        }
    }

    int GetReward(int symbolIndex)
    {
        switch (symbolIndex)
        {
            case 0:
                return 100;

            case 1:
                return 200;

            case 2:
                return 300;

            case 3:
                return 500;

            default:
                return 0;
        }
    }

    void UpdateBalanceUI()
    {
        balanceText.text = "Balance : $" + balance;
    }
}