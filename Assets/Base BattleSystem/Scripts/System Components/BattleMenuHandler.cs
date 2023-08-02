using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenuHandler : MonoBehaviour
{
	// mhvg2
	[SerializeField] protected BattleSystem BattleSystem;
	// public ActionQueue ActionQueue;
	public PauseMenu PauseMenu;
	// mhvg2

	[SerializeField] protected GameObject InputDarkFilter;

	// mhvg3
	[SerializeField] private bool stopInputs = false;

#region Unity functions

	// mhf2
	private void Update(){
		this.stopInputs = (this.BattleSystem == null);

		this.InputDarkFilter.SetActive(this.stopInputs);

		if(!this.stopInputs) CheckInputs();

	}

#endregion

#region Main Functions

	public void Setup(BattleSystem BS){
		this.BattleSystem = BS;
		this.InputDarkFilter.SetActive(false);
	}

	// mhf3
	protected virtual void CheckInputs(){
		switch(this.BattleSystem.state){

			case BattleState.RESULT: // <<----- KEEP
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.A) && this.BattleSystem.ResultHandler.finishedDisplayingResult) this.BattleSystem.End();
				// else if(Input.GetKeyDown(KeyCode.D) && this.PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
			break;

			case BattleState.SETUP: // <<----- KEEP
				this.InputDarkFilter.SetActive(false);
				BattleStartMenuInputs();
			break;

			case BattleState.WAVEOVER: // <<----- KEEP
				this.InputDarkFilter.SetActive(false);
				WaveOverInputs();
			break;

			case BattleState.PLAYERDIED: // <<----- KEEP
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.LeaveGame();
			break;

			default:
				this.InputDarkFilter.SetActive(true);
			break;
		}
	} // Changed in: NBS, BattleMenuHandler_Tutorial.cs

#endregion 

#region Helper Functions

	protected virtual void BattleStartMenuInputs(){ // Changed in: NBS
		if(Input.GetKeyDown(KeyCode.A)) this.PauseMenu.StartButtonTest('A');
		else if(Input.GetKeyDown(KeyCode.W)) this.PauseMenu.StartButtonTest('W');
		else if(Input.GetKeyDown(KeyCode.S)) this.PauseMenu.StartButtonTest('S');
		else if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.StartButtonTest('D');
	}
	// mhf11
	protected void WaveOverInputs(){
		if(Input.GetKeyDown(KeyCode.A) && this.PauseMenu.ContinueButton.activeSelf) {
			this.PauseMenu.ContinueGame();
		}else if(Input.GetKeyDown(KeyCode.D) && this.PauseMenu.LeaveButton.activeSelf) this.PauseMenu.LeaveGame();
		else if(Input.GetKeyDown(KeyCode.W) && this.PauseMenu.NextStageButton.activeSelf) BattleSystem.NextStage();
	}

#endregion
}