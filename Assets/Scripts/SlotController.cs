using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotController : MonoBehaviour
{
    public enum GameState { Idle, Spinning, Resolving, Gambling }

    [Header("Game State")]
    public GameState currentState = GameState.Idle;

    [Header("Main UI & Reels")]
    public Button spinButton;
    public TextMeshProUGUI resultText;
    public Toggle forceWinToggle;
    public Reel[] reels; 

    [Header("Betting System")]
    public int playerBalance = 1000;
    public int currentBet = 10;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public Button betUpBtn;
    public Button betDownBtn;
    public Button betMaxBtn;
    public Button resetBtn; // NEW: Reset Button

    [Header("Popups")]
    public GameObject paytablePanel;
    public Button openPaytableBtn;
    public Button closePaytableBtn;
    
    public GameObject gamblePanel;
    public Button gambleYesBtn;
    public Button gambleNoBtn;

    [Header("RNG Data")]
    public Sprite[] possibleSymbols;

    private int pendingWinAmount = 0;

    private void Start()
    {
        // Bind Main Buttons
        if (spinButton != null) spinButton.onClick.AddListener(AttemptSpin);
        if (resetBtn != null) resetBtn.onClick.AddListener(ResetGame); // NEW
        
        // Bind Betting Buttons
        if (betUpBtn != null) betUpBtn.onClick.AddListener(() => ChangeBet(10));
        if (betDownBtn != null) betDownBtn.onClick.AddListener(() => ChangeBet(-10));
        if (betMaxBtn != null) betMaxBtn.onClick.AddListener(SetMaxBet); // FIXED

        // Bind Paytable Buttons
        if (openPaytableBtn != null) openPaytableBtn.onClick.AddListener(() => paytablePanel.SetActive(true));
        if (closePaytableBtn != null) closePaytableBtn.onClick.AddListener(() => paytablePanel.SetActive(false));

        // Bind Gamble Buttons
        if (gambleYesBtn != null) gambleYesBtn.onClick.AddListener(GambleDoubleOrNothing);
        if (gambleNoBtn != null) gambleNoBtn.onClick.AddListener(CollectWinnings);

        UpdateUI();
        
        // Ensure popups start hidden
        if (gamblePanel != null) gamblePanel.SetActive(false);
        if (paytablePanel != null) paytablePanel.SetActive(true);
        if (resultText != null) resultText.text = "Press Spin!";
    }

    // NEW: Dedicated Max Bet Logic
    private void SetMaxBet()
    {
        if (currentState != GameState.Idle) return;
        
        if (playerBalance > 0)
        {
            currentBet = playerBalance;
        }
        UpdateUI();
    }

    private void ChangeBet(int amount)
    {
        if (currentState != GameState.Idle) return;
        
        currentBet += amount;

        // Clamp bet amounts
        if (currentBet < 10) currentBet = 10;
        if (currentBet > playerBalance) currentBet = playerBalance;
        
        UpdateUI();
    }

    private void AttemptSpin()
    {
        // Prevent spinning if busy or broke
        if (currentState != GameState.Idle || playerBalance < currentBet || currentBet == 0) return;
        
        playerBalance -= currentBet;
        UpdateUI();
        StartCoroutine(SpinSequence());
    }

    private IEnumerator SpinSequence()
    {
        currentState = GameState.Spinning;
        spinButton.interactable = false; 
        if (resultText != null) resultText.text = "Spinning...";

        // Tell all 3 reels to start moving
        foreach (Reel reel in reels) reel.StartSpinning();

        yield return new WaitForSeconds(2f);

        Sprite result1, result2, result3;

        // FIXED: Force Win Toggle Logic (Now picks a random winning symbol)
        if (forceWinToggle != null && forceWinToggle.isOn)
        {
            Sprite forcedWinSymbol = possibleSymbols[Random.Range(0, possibleSymbols.Length)];
            result1 = result2 = result3 = forcedWinSymbol;
        }
        else
        {
            // True RNG Logic
            result1 = possibleSymbols[Random.Range(0, possibleSymbols.Length)];
            result2 = possibleSymbols[Random.Range(0, possibleSymbols.Length)];
            result3 = possibleSymbols[Random.Range(0, possibleSymbols.Length)];
        }

        // Stop reels one by one
        reels[0].StopAt(result1);
        yield return new WaitForSeconds(0.5f);
        
        reels[1].StopAt(result2);
        yield return new WaitForSeconds(0.5f);
        
        reels[2].StopAt(result3);
        yield return new WaitForSeconds(0.5f);

        EvaluateWin(result1, result2, result3);
    }

    private void EvaluateWin(Sprite r1, Sprite r2, Sprite r3)
    {
        if (r1 == r2 && r2 == r3)
        {
            pendingWinAmount = currentBet * 10; // 10x payout for a match
            if (resultText != null) resultText.text = $"JACKPOT! Win: {pendingWinAmount}";
            
            // Open gamble panel instead of returning to idle
            currentState = GameState.Gambling;
            if (gamblePanel != null) gamblePanel.SetActive(true); 
        }
        else
        {
            if (resultText != null) resultText.text = "Try Again!";
            ResetToIdle();
        }
    }

    private void GambleDoubleOrNothing()
    {
        if (gamblePanel != null) gamblePanel.SetActive(false);
        
        int flip = Random.Range(0, 2); // 50/50 chance (returns 0 or 1)

        if (flip == 1)
        {
            pendingWinAmount *= 2;
            if (resultText != null) resultText.text = $"GAMBLE WON! Payout: {pendingWinAmount}";
            playerBalance += pendingWinAmount;
        }
        else
        {
            pendingWinAmount = 0;
            if (resultText != null) resultText.text = "GAMBLE LOST! Payout: 0";
        }
        
        UpdateUI();
        ResetToIdle();
    }

    private void CollectWinnings()
    {
        if (gamblePanel != null) gamblePanel.SetActive(false);
        playerBalance += pendingWinAmount;
        UpdateUI();
        ResetToIdle();
    }

    // NEW: Reset Game Logic
    private void ResetGame()
    {
        if (currentState != GameState.Idle) return; // Only reset if not spinning
        
        playerBalance = 1000;
        currentBet = 10;
        pendingWinAmount = 0;
        
        if (resultText != null) resultText.text = "Game Reset! Press Spin!";
        UpdateUI();
    }

    private void ResetToIdle()
    {
        currentState = GameState.Idle;
        spinButton.interactable = true; 
        pendingWinAmount = 0;
    }

    private void UpdateUI()
    {
        if (balanceText != null) balanceText.text = $"Balance: ${playerBalance}";
        if (betText != null) betText.text = $"Bet: ${currentBet}";
    }
}