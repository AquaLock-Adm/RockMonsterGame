using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyLibraryHandler : StandartMenuHandler
{
    [SerializeField] private TextMeshProUGUI EnemyNameText;
    [SerializeField] private TextMeshProUGUI EnemyHPText;
    [SerializeField] private TextMeshProUGUI EnemyDefenceText;
    [SerializeField] private TextMeshProUGUI EnemyPriceText;

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);

        // Debug.Log("For testing: earned cp + 10000");
        // GameHandler.earnedCredits += 10000;

        // this.CreditScoreText.text = GameHandler.earnedCredits.ToString() + "cp";
        
        // SetupSelectBar();
        // SetupComboListOptions();

        // this.optionIndex = 0;
        // SelectBar.HoverSelectText(this.optionIndex);
        // UnhoverBackoption();
    }



    protected override void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.Space) ) GameHandler.LoadMainMenu();
        
        // if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
        //     OptionDown();
        // }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
        //     OptionUp();
        // }else if( Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)  || Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.RightArrow) ){
        //     HoverBackButton();
        // }else if( Input.GetKeyDown(KeyCode.Space) ){
        //     SelectOption();
        // }
    }
}
