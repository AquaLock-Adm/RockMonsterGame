using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuHandler_NBS : BattleMenuHandler
{
    [Header("NBS Variables")]
    [Header("")]
	public GameObject StartNotice_NBS;
	public GameObject StartNoticeDark_NBS;

#region Override Functions

	public override void CheckInputs(){
		switch(this.BattleSystem.state){
			case BattleState.PLAYERTURN:
				this.InputDarkFilter.SetActive(false);
				BattleInputs();
			break;

			// case BattleState.DIALOGUE:
			// 	this.InputDarkFilter.SetActive(false);
			// 	DialogueInputs();
			// break;

			// case BattleState.RESULT:
			// 	this.InputDarkFilter.SetActive(false);
			// 	if(Input.GetKeyDown(KeyCode.A)) BattleSystem.End();
			// 	else if(Input.GetKeyDown(KeyCode.D) && PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
			// break;

			case BattleState.SETUP:
				this.InputDarkFilter.SetActive(false);
				BattleStartMenuInputs();
			break;

			// case BattleState.WAVEOVER:
			// 	this.InputDarkFilter.SetActive(false);
			// 	WaveOverInputs();
			// break;

			// case BattleState.BAILOUT:
			// 	this.InputDarkFilter.SetActive(false);
			// 	if(Input.GetKeyDown(KeyCode.D)) {
			// 		PauseMenu.LeaveGame();}
			// break;

			// case BattleState.PLAYERDIED:
			// 	this.InputDarkFilter.SetActive(false);
			// 	if(Input.GetKeyDown(KeyCode.D)) PauseMenu.LeaveGame();
			// break;
			default:
				this.InputDarkFilter.SetActive(true);
			break;
		}
	}

	public override void BattleStartMenuInputs(){
		if(Input.GetKeyDown(KeyCode.S)) UnloadStartNotice_NBS();
	}
#endregion

#region New Functions
	private async void UnloadStartNotice_NBS(){
		this.StartNotice_NBS.SetActive(false);
		this.StartNoticeDark_NBS.SetActive(false);

		await this.BattleSystem.PlayerTurn();
	}
#endregion
}