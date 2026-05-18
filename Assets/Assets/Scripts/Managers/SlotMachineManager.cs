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
    public TMP_Text betText;

    [Header("Game Settings")]
    public int balance = 1000;

    // Current selected bet
    public int spinCost = 50;

    private bool isSpinning;

    void Start()
    {
        UpdateBalanceUI();
        UpdateBetUI();

        resultText.gameObject.SetActive(false);
    }

    public void Spin()
    {
        if (isSpinning)
            return;

        // Check balance
        if (balance < spinCost)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "NOT ENOUGH BALANCE";
            return;
        }

        // Deduct selected bet amount
        balance -= spinCost;

        UpdateBalanceUI();

        StartCoroutine(SpinSequence());
    }

    IEnumerator SpinSequence()
    {
        isSpinning = true;

        resultText.gameObject.SetActive(false);

        // Start reels one by one
        reels[0].StartSpin();

        yield return new WaitForSeconds(0.6f);

        reels[1].StartSpin();

        yield return new WaitForSeconds(0.6f);

        reels[2].StartSpin();

        // Wait until all reels stop
        while (
            reels[0].IsSpinning ||
            reels[1].IsSpinning ||
            reels[2].IsSpinning
        )
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        CheckWin(
            reels[0].ResultIndex,
            reels[1].ResultIndex,
            reels[2].ResultIndex
        );

        isSpinning = false;
    }

    void CheckWin(int r1, int r2, int r3)
    {
        resultText.gameObject.SetActive(true);

        // All symbols same = WIN
        if (r1 == r2 && r2 == r3)
        {
            int reward = GetReward(r1);

            balance += reward;

            UpdateBalanceUI();

            resultText.text =
                "YOU WIN $" + reward;
        }
        else
        {
            resultText.text = "TRY AGAIN";
        }
    }

    int GetReward(int symbolIndex)
    {
        switch (symbolIndex)
        {
            case 0:
                return spinCost * 2;

            case 1:
                return spinCost * 3;

            case 2:
                return spinCost * 5;

            case 3:
                return spinCost * 10;

            default:
                return 0;
        }
    }

    // BET BUTTONS

    public void SetBet10()
    {
        if (isSpinning)
            return;

        spinCost = 10;

        UpdateBetUI();
    }

    public void SetBet50()
    {
        if (isSpinning)
            return;

        spinCost = 50;

        UpdateBetUI();
    }

    public void SetBet100()
    {
        if (isSpinning)
            return;

        spinCost = 100;

        UpdateBetUI();
    }

    void UpdateBalanceUI()
    {
        balanceText.text =
            "BALANCE : $" + balance;
    }

    void UpdateBetUI()
    {
        betText.text =
            "BET : $" + spinCost;
    }
}