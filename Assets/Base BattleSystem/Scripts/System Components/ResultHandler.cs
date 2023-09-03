using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ResultHandler : MonoBehaviour
{
    [Header("System:")]
    private BattleSystem BattleSystem;

    public GameObject MainScreen;
    public GameObject ResultScreen;

    public GameObject ContinueButton;

    [Header("Ingame Score:")]
    public Text ScoreText;

    [Header("Waves cleared:")]
    public Text WavesCountText;

    [Header("Monsters defeated:")]
    public Text DefeatCountText;

    [Header("Result:")]
    public Text ResultCreditText;
    public Text ResultLostText;

    public bool finishedDisplayingResult = false;
    private int defeatCredits;

    public void Setup(BattleSystem BS){
        this.BattleSystem = BS;
        this.finishedDisplayingResult = false;
        this.ContinueButton.SetActive(false);
        this.ResultScreen.SetActive(false);
        this.ScoreText.text = "0 Cd";
    }

    public async void ShowResult(){
        BattleSystem.Player.BattleEnd();

        if(this.BattleSystem.state == BattleState.PLAYERDIED){
            this.defeatCredits = 0;
        }else this.defeatCredits = this.BattleSystem.earnedCredits;

        this.BattleSystem.state = BattleState.RESULT;
        this.MainScreen.SetActive(false);
        this.ResultScreen.SetActive(true);

        await ShowFinalResult();

        this.BattleSystem.earnedCredits = this.defeatCredits;

        this.ContinueButton.SetActive(true);
        this.finishedDisplayingResult = true;
    }

    private async Task ShowFinalResult(){
        this.WavesCountText.text = this.BattleSystem.wavesClearedCount.ToString();
        if(this.BattleSystem.wavesClearedCount <= 0) this.WavesCountText.color = Color.red;

        this.DefeatCountText.text = this.BattleSystem.enemyDefeatCount.ToString();

        if(this.defeatCredits <= 0){
            this.ResultLostText.gameObject.SetActive(true);
            this.DefeatCountText.color = Color.red;
            this.ResultCreditText.color = Color.red;
        }
        else {
            this.ResultLostText.gameObject.SetActive(false);
            this.DefeatCountText.color = Color.green;
            this.ResultCreditText.color = Color.green;
        }

        await CrawlCredits(this.BattleSystem.earnedCredits, ResultCreditText);
    }

    public async Task CrawlCredits(int finalCredit, Text textField){
        if(finalCredit < 0) {
            Debug.LogError("Expected a positive Credit. Got "+finalCredit.ToString());
            return;
        }

        textField.text = "";

        int cAmount = 0;

        while(cAmount < finalCredit){
            textField.text = FormatIntegerCount(cAmount) + " Cd";
            cAmount += 7;
            await Task.Delay(10);
        }

        textField.text = FormatIntegerCount(finalCredit);
    }

    public string FormatIntegerCount(int num){
        string numString = num.ToString();

        int i = 0;
        int signCount = 0;
        while(i + signCount < numString.Length){
            if(i != 0){
                numString = numString.Insert(numString.Length - i - signCount, ",");
                signCount++;
            }
            i += 3;
        }
        return numString;
    }
}
