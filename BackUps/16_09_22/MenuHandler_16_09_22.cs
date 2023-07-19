/*
Version before constant input update revert -> see ActionQueue_16_09_22
*/


using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum Menu {MAIN, SPELLS, ATTACKS, OTHER}

public class MenuHandler : MonoBehaviour
{
	public BattleSystem BattleSystem;
	public BattleHUD BattleHUD;
	public ActionQueue ActionQueue;
	public PauseMenu PauseMenu;
	public DialogueHandler DialogueHandler;

	public bool stopInputs = false;

	public List<Text> ButtonTexts = new List<Text>();
	private List<string> MainMenuTexts = new List<string>();

	//public bool inMainMenu = true;
	Menu CurrentMenu;

#region Unity functions

	void Start (){
		CurrentMenu = Menu.MAIN;
		SetUpButtonTexts();
		// For debug purpose
		// PrintStatus();
		if(BattleSystem.Player.Abilities[0].name != "Element") Debug.Log("ERROR: Menuhandler depends on ELement being at Player.Abilities[0]");
	}

	void Update(){
		if(!stopInputs) CheckInputs();
	}

#endregion

#region Public Functions

	private void LoadAbilityMenu(){
		CurrentMenu = Menu.SPELLS;
		InstantiateSpellButtons();
	}

	private void LoadSpellsMenu(){
		CurrentMenu = Menu.SPELLS;
		InstantiateSpellButtons();
	}

	private void LoadAttacksMenu(){
		CurrentMenu = Menu.ATTACKS;
		InstantiateAttackButtons();
	}

	public void LoadMainMenu(){

		//activate the buttons from the main menu
		CurrentMenu = Menu.MAIN;

		for(int i = 0; i < ButtonTexts.Count; i++){
			ButtonTexts[i].text = MainMenuTexts[i];
		}
	}

	private void LoadBailOutScreen() {
		BattleSystem.state = BattleState.BAILOUT;
		BattleSystem.Enemy.StopAttack();
		PauseMenu.ShowPauseMenu(true);
		CurrentMenu = Menu.OTHER;
	}


#endregion 

#region Private Functions

	private void SetUpButtonTexts(){

		foreach(Text t in ButtonTexts){
			MainMenuTexts.Add(t.text);
		}
	}

	private async void PrintStatus(){
		Debug.Log("Current Menu: "+CurrentMenu);
		await Task.Delay(2000);
	}

	private void CheckInputs(){
		switch(BattleSystem.state){
			case BattleState.PLAYERTURN:
				BattleInputs();
			break;

			case BattleState.QUEUE:
				BattleInputs();
			break;

			case BattleState.DIALOGUE:
				DialogueInputs();
			break;


			case BattleState.RESULT:
				if(Input.GetKeyDown(KeyCode.A)) BattleSystem.End();
				else if(Input.GetKeyDown(KeyCode.D) && PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
			break;


			case BattleState.START:
				if(Input.GetKeyDown(KeyCode.A)) PauseMenu.ContinueGame();
			break;


			case BattleState.WAVEOVER:
				WaveOverInputs();
			break;


			case BattleState.BAILOUT:
				if(Input.GetKeyDown(KeyCode.D)) {
					PauseMenu.LeaveGame();}
			break;


			case BattleState.PLAYERDIED:
				if(Input.GetKeyDown(KeyCode.D)) PauseMenu.LeaveGame();
			break;
		}
	}

	private void BattleInputs(){
		if(Input.GetKeyDown(KeyCode.E) && ActionQueue.ComboButton.gameObject.activeSelf) {
			if(ActionQueue.CurrentComboAction.ultForm && ActionQueue.CurrentComboAction.comboLevel > 2) {
				ActionQueue.ClearComboList();
				ActionQueue.comboLevel = 1;
			}
			BattleSystem.CastComboAbility(ActionQueue.CurrentComboAction);
		}else if(Input.GetKeyDown(KeyCode.P)) LoadBailOutScreen();

		if(CurrentMenu == Menu.MAIN){
			if(Input.GetKeyDown(KeyCode.A)) LoadAttacksMenu();
			else if(Input.GetKeyDown(KeyCode.S) && BattleSystem.state == BattleState.PLAYERTURN) BattleSystem.PassRound();
			else if(Input.GetKeyDown(KeyCode.W)) LoadSpellsMenu();
			else if(Input.GetKeyDown(KeyCode.D)) BattleSystem.CancelLastAction();
		}else if(CurrentMenu == Menu.SPELLS){
			if(Input.GetKeyDown(KeyCode.D)) LoadMainMenu();
			else if(Input.GetKeyDown(KeyCode.S)) BattleSystem.CastSpell(2);
			else if(Input.GetKeyDown(KeyCode.A)) BattleSystem.CastSpell(1);
			else if(Input.GetKeyDown(KeyCode.W)) BattleSystem.CastSpell(0);
		}else if(CurrentMenu == Menu.ATTACKS){
			if(Input.GetKeyDown(KeyCode.D)) LoadMainMenu();
			else if(Input.GetKeyDown(KeyCode.S)) BattleSystem.CastAttack(3);
			else if(Input.GetKeyDown(KeyCode.A)) BattleSystem.CastAttack(2);
			else if(Input.GetKeyDown(KeyCode.W)) BattleSystem.CastAttack(1);
		}
	}

	private void DialogueInputs(){
		if(Input.GetKeyDown(KeyCode.A)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.W)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.S)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.D)) DialogueHandler.ShowNextDialogueText();
	}

	private void WaveOverInputs(){
		if(Input.GetKeyDown(KeyCode.A)) {
			PauseMenu.ContinueGame();
			CurrentMenu = Menu.MAIN;
		}else if(Input.GetKeyDown(KeyCode.D)) PauseMenu.LeaveGame();
		else if(Input.GetKeyDown(KeyCode.W) && PauseMenu.showRepairButton) PauseMenu.RepairWeapon();
	}

	private void InstantiateSpellButtons(){
		PlayerCharacter P = BattleSystem.Player;

		for (int i = 0; i < ButtonTexts.Count; i++){

			if(i == 3) ButtonTexts[i].text = "Back";

			else if(i < P.Elements.Count) ButtonTexts[i].text = P.Elements[i].ToString();

			else ButtonTexts[i].text = "";
		}
	}

	private void InstantiateAttackButtons(){
		PlayerCharacter P = BattleSystem.Player;

		for(int i = 0; i < ButtonTexts.Count; i++){

			if(i == 3) ButtonTexts[i].text = "Back";

			else if(i+1 < P.Abilities.Count && P.Abilities[i+1].comboLevel == 1) {
				ButtonTexts[i].text = P.Abilities[i+1].cover;
			}

			else ButtonTexts[i].text = "";
		}
	}

#endregion
}