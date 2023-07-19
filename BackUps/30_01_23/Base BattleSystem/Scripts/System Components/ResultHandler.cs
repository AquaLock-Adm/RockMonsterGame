using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultHandler : MonoBehaviour
{
    [Header("System:")]
    public BattleSystem BattleSystem;

    public GameObject MainScreen;

    public GameObject ResultScreen;

    [Header("Ingame Score:")]
    public Text ScoreText;
    public int lastScore;
    [SerializeField] float timeForScoreIncrease = 1f;
    public Text MultiplierText;

    [Header("Waves cleared:")]
    public Text WavesCountText;

    [Header("Monsters defeated:")]
    public Text DefeatCountText;
    public Text DefeatCreditText;
    public Text DefeatLostText;

    [Header("Exp earned:")]
    public Text ExpCountText;
    public Text ExpLostText;

    [Header("Moves used:")]
    public Text MovesCountText;
    public Text MovesCreditText;

    [Header("Items used:")]
    public Text ItemsCountText;
    public Text ItemsCreditText;

    [Header("Deaths:")]
    public Text DeathsCountText;
    public Text DeathsCreditText;

    [Header("Entry cost:")]
    public Text EntryCreditText;

    [Header("Result:")]
    public Text ResultCreditText;

    private int defeatCredits;
    private int movesCredits; 
    private int itemsCredits;
    private int deathCredits;

    void Start(){
        this.ResultScreen.SetActive(false);
    }

    //public void UpdateMultiplier(){ this.MultiplierText.text = "x" + this.BattleSystem.timerMultiplier; }

    public void UpdateScore(int score){

        StartCoroutine(CrawlScoreCredits(score - this.lastScore));
    }

    private IEnumerator CrawlScoreCredits(int increase){

        float timePCredit = this.timeForScoreIncrease / increase;
        this.ScoreText.text = FormatIntegerCount(this.lastScore) + " Cd";

        for(int i = 1; i < increase; i++){
            this.ScoreText.text = FormatIntegerCount(this.lastScore + i) + " Cd";
                yield return new WaitForSeconds(timePCredit);
        }

        this.lastScore += increase;

        yield return null;
    }

    public void ShowResult(){
        this.MainScreen.SetActive(false);
        this.ResultScreen.SetActive(true);

        ShowWavesCleared();
        ShowEnemiesDefeated();
        ShowExpEarned();
        ShowMovesUsed();
        ShowItemsUsed();
        ShowDeathCount();
        ShowEntryCost();
        ShowFinalResult();

        this.BattleSystem.state = BattleState.RESULT;
    }

    /*public void CloseResult(){
        TopScreen.SetActive(true);
        BottomScreen.SetActive(true);
        this.BattleSystem.pauseTimer = false;

        ResultScreen.SetActive(false);
    }*/

    private void ShowWavesCleared(){
        this.WavesCountText.text = this.BattleSystem.wavesClearedCount.ToString();
        if(this.BattleSystem.wavesClearedCount <= 0) this.WavesCountText.color = Color.red;
    }

    private void ShowEnemiesDefeated(){
        this.DefeatCountText.text = this.BattleSystem.enemyDefeatCount.ToString();
        this.defeatCredits = this.BattleSystem.earnedCredits;
        StartCoroutine(CrawlCredits(this.defeatCredits, this.DefeatCreditText));
        if(this.BattleSystem.state == BattleState.PLAYERDIED || this.BattleSystem.state == BattleState.BAILOUT){
            this.DefeatLostText.gameObject.SetActive(true);
            this.defeatCredits = 0;
            this.DefeatCreditText.color = Color.red;
            this.DefeatCountText.color = Color.red;
        }
        else {
            this.DefeatLostText.gameObject.SetActive(false);
            this.DefeatCreditText.color = Color.green;
            this.DefeatCountText.color = Color.green;
        }
        if(this.BattleSystem.enemyDefeatCount <= 0) this.DefeatCountText.color = Color.red;
    }

    private void ShowExpEarned(){
        this.ExpCountText.text = FormatIntegerCount(this.BattleSystem.earnedExp);
        if(this.BattleSystem.state == BattleState.PLAYERDIED){
                // Player died -> losing all exp
            this.ExpLostText.gameObject.SetActive(true);
                // for BS.End()
            this.BattleSystem.earnedExp = 0;
        }else this.ExpLostText.gameObject.SetActive(false);

        if(this.BattleSystem.earnedExp <= 0) this.ExpCountText.color = Color.red;
    }

    private void ShowMovesUsed(){
        this.movesCredits = this.BattleSystem.movesUsedCount * this.BattleSystem.moveCost;
        this.MovesCountText.text = FormatIntegerCount(this.BattleSystem.movesUsedCount);
        if(this.BattleSystem.movesUsedCount <= 0) {
            this.MovesCreditText.color = Color.green;
            this.MovesCountText.color = Color.green;
        }
        else {
            this.MovesCreditText.color = Color.red;
            this.MovesCountText.color = Color.red;
        }

        StartCoroutine(CrawlCredits(-this.movesCredits, this.MovesCreditText));
    }

    private void ShowItemsUsed(){
        this.itemsCredits = this.BattleSystem.totalItemCost; 
        this.ItemsCountText.text = this.BattleSystem.itemsUsedCount.ToString();
        if(this.itemsCredits <= 0){
           this.ItemsCreditText.color = Color.green;
           this.ItemsCountText.color = Color.green;
        }
        else{
           this.ItemsCreditText.color = Color.red;
           this.ItemsCountText.color = Color.red;
        }

        StartCoroutine(CrawlCredits(-this.itemsCredits, this.ItemsCreditText));
    }

    private void ShowDeathCount(){
        int deathCount = 0;
        if(this.BattleSystem.state == BattleState.PLAYERDIED) deathCount = 1;
        this.deathCredits = deathCount * this.BattleSystem.deathCost;
        if(this.deathCredits <= 0){
            this.DeathsCountText.color = Color.green;
            this.DeathsCreditText.color = Color.green;
        }
        else{
            this.DeathsCountText.color = Color.red;
            this.DeathsCreditText.color = Color.red;
        }
        
        this.DeathsCountText.text = deathCount.ToString();

        StartCoroutine(CrawlCredits(-this.deathCredits, this.DeathsCreditText));
    }

    private void ShowEntryCost(){

        if(this.BattleSystem.entryCost <= 0){
            this.EntryCreditText.color = Color.green;
            this.EntryCreditText.color = Color.green;
        }
        else{
            this.EntryCreditText.color = Color.red;
            this.EntryCreditText.color = Color.red;
        }
        StartCoroutine(CrawlCredits(-this.BattleSystem.entryCost, this.EntryCreditText));
    }

    private void ShowFinalResult(){
        int result = this.defeatCredits - this.movesCredits - this.itemsCredits - this.BattleSystem.entryCost - this.deathCredits;

        if(result >= 0) ResultCreditText.color = Color.green;
        else if(result == 0)ResultCreditText.color = Color.black;
        else ResultCreditText.color = Color.red;
        
            // for BS.End()
        this.BattleSystem.earnedCredits = result;

        StartCoroutine(CrawlCredits(result, ResultCreditText));
    }

    private IEnumerator CrawlCredits(int finalCredit,  Text textField){

        textField.text = "";

        if(finalCredit > 0){
            for(int i = 0; i < finalCredit; i += 7){
                textField.text = "+ " + FormatIntegerCount(i) + " Cd";
                yield return new WaitForSeconds(0.01f);
            }

            textField.text = "+ " + FormatIntegerCount(finalCredit) + " Cd";
        }

        else if(finalCredit < 0){
            for(int i = 0; i > finalCredit; i -= 7){
                textField.text = "- " + FormatIntegerCount(-i) + " Cd";
                yield return new WaitForSeconds(0.01f);
            }
            textField.text = "- " + FormatIntegerCount(-finalCredit) + " Cd";
        } else textField.text = finalCredit + " Cd";

        yield return null;
    }

    private string FormatIntegerCount(int num){
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
