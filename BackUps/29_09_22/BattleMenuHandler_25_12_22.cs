// Version before merge with NBS


using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// mhvg1
public enum Menu {MAIN, SPELLS, ATTACKS, OTHER}

public class BattleMenuHandler : MonoBehaviour
{
	// mhvg2
	public BattleSystem BattleSystem;
	public ActionQueue ActionQueue;
	public PauseMenu PauseMenu;
	public DialogueHandler DialogueHandler;
	// mhvg2

	public GameObject InputDarkFilter;

	// mhvg3
	public bool stopInputs = false;

	public int basicAbilitiesCount = 0;

	// mhvg4
	public List<Text> ButtonTexts = new List<Text>();
	public List<string> MainMenuTexts = new List<string>();
	// mhvg4

	//public bool inMainMenu = true;
	// mhvg5
	public Menu CurrentMenu;

#region Unity functions

	// mhf1
	private void Start(){
		CurrentMenu = Menu.MAIN;
		SetupButtonTexts();
		// For debug purpose
		// PrintStatus();
	}
	// mhf2
	private void Update(){
		if(!stopInputs) CheckInputs();
		else this.InputDarkFilter.SetActive(true);
	}

#endregion

#region Main Functions

	// mhf3
	public virtual void CheckInputs(){
		switch(BattleSystem.state){
			case BattleState.PLAYERTURN:
				this.InputDarkFilter.SetActive(false);
				BattleInputs();
			break;

			case BattleState.DIALOGUE:
				this.InputDarkFilter.SetActive(false);
				DialogueInputs();
			break;

			case BattleState.RESULT:
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.A)) BattleSystem.End();
				else if(Input.GetKeyDown(KeyCode.D) && PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
			break;

			case BattleState.SETUP:
				this.InputDarkFilter.SetActive(false);
				BattleStartMenuInputs();
			break;

			case BattleState.WAVEOVER:
				this.InputDarkFilter.SetActive(false);
				WaveOverInputs();
			break;

			case BattleState.BAILOUT:
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.D)) {
					PauseMenu.LeaveGame();}
			break;

			case BattleState.PLAYERDIED:
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.D)) PauseMenu.LeaveGame();
			break;
			default:
				this.InputDarkFilter.SetActive(true);
			break;
		}
	}
	// mhf4
	public virtual void LoadAbilityMenu(){
		CurrentMenu = Menu.SPELLS;
		InstantiateSpellButtons();
	}
	// mhf5
	private void LoadSpellsMenu(){
		CurrentMenu = Menu.SPELLS;
		InstantiateSpellButtons();
	}
	// mhf6
	public virtual void LoadAttacksMenu(){
		CurrentMenu = Menu.ATTACKS;
		InstantiateAttackButtons();
	}
	// mhf7
	public void LoadMainMenu(){

		//activate the buttons from the main menu
		CurrentMenu = Menu.MAIN;

		for(int i = 0; i < ButtonTexts.Count; i++){
			ButtonTexts[i].text = MainMenuTexts[i];
		}
	}
	// mhf8
	private void LoadBailOutScreen(){
		BattleSystem.state = BattleState.BAILOUT;
		BattleSystem.Enemy.StopAttack();
		PauseMenu.ShowPauseMenu(true);
		CurrentMenu = Menu.OTHER;
	}

#endregion 

#region Helper Functions

	// mhf9
	public virtual void BattleInputs(){
		if(Input.GetKeyDown(KeyCode.E) && ActionQueue.ComboButton.gameObject.activeSelf) {
			if(ActionQueue.CurrentComboAction.ultForm) ActionQueue.comboLevel = 1;

			BattleSystem.CastComboAbility(ActionQueue.CurrentComboAction);

		}//else if(Input.GetKeyDown(KeyCode.P)) LoadBailOutScreen();

		if(CurrentMenu == Menu.MAIN){
			if(Input.GetKeyDown(KeyCode.A)) LoadAttacksMenu();
			else if(Input.GetKeyDown(KeyCode.S) && this.BattleSystem.state == BattleState.PLAYERTURN) BattleSystem.PassRound();
			else if(Input.GetKeyDown(KeyCode.W)) LoadSpellsMenu();
			else if(Input.GetKeyDown(KeyCode.D)) this.BattleSystem.CancelLastAction();
		}else if(CurrentMenu == Menu.SPELLS){
			if(Input.GetKeyDown(KeyCode.D)) LoadMainMenu();
			else if(Input.GetKeyDown(KeyCode.S) && this.BattleSystem.Player.Elements.Count > 2) this.BattleSystem.CastSpell(2);
			else if(Input.GetKeyDown(KeyCode.A) && this.BattleSystem.Player.Elements.Count > 1) this.BattleSystem.CastSpell(1);
			else if(Input.GetKeyDown(KeyCode.W) && this.BattleSystem.Player.Elements.Count > 0) this.BattleSystem.CastSpell(0);
		}else if(CurrentMenu == Menu.ATTACKS){
			if(Input.GetKeyDown(KeyCode.D)) LoadMainMenu();
			else if(Input.GetKeyDown(KeyCode.S) && this.basicAbilitiesCount > 2) this.BattleSystem.CastAttack(3);
			else if(Input.GetKeyDown(KeyCode.A) && this.basicAbilitiesCount > 1) this.BattleSystem.CastAttack(2);
			else if(Input.GetKeyDown(KeyCode.W) && this.basicAbilitiesCount > 0) this.BattleSystem.CastAttack(1);
		}
	}
	// mhf10
	private void DialogueInputs(){
		if(Input.GetKeyDown(KeyCode.A)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.W)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.S)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.D)) DialogueHandler.ShowNextDialogueText();
	}

	public virtual void BattleStartMenuInputs(){
		if(Input.GetKeyDown(KeyCode.A)) this.PauseMenu.StartButtonTest('A');
		else if(Input.GetKeyDown(KeyCode.W)) this.PauseMenu.StartButtonTest('W');
		else if(Input.GetKeyDown(KeyCode.S)) this.PauseMenu.StartButtonTest('S');
		else if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.StartButtonTest('D');
	}
	// mhf11
	private void WaveOverInputs(){
		if(Input.GetKeyDown(KeyCode.A)) {
			PauseMenu.ContinueGame();
			CurrentMenu = Menu.MAIN;
		}else if(Input.GetKeyDown(KeyCode.D)) PauseMenu.LeaveGame();
		else if(Input.GetKeyDown(KeyCode.W) && PauseMenu.showRepairButton) PauseMenu.RepairWeapon();
	}
	// mhf12
	public virtual void SetupButtonTexts(){
		if(BattleSystem.Player.Abilities[0].name != "Element") Debug.Log("ERROR: Menuhandler depends on Element being at Player.Abilities[0]");

		foreach(Text t in this.ButtonTexts){
			this.MainMenuTexts.Add(t.text);
		}
	}
	// mhf13
	private void InstantiateSpellButtons(){
		PlayerCharacter P = this.BattleSystem.Player;

		for (int i = 0; i < ButtonTexts.Count; i++){

			if(i == 3) ButtonTexts[i].text = "Back";

			else if(i < P.Elements.Count) ButtonTexts[i].text = P.Elements[i].ToString();

			else ButtonTexts[i].text = "";
		}
	}
	// mhf14
	public virtual void InstantiateAttackButtons(){
		PlayerCharacter P = this.BattleSystem.Player;
		this.basicAbilitiesCount = 0;

		for(int i = 0; i < this.ButtonTexts.Count; i++){

			if(i == 3) this.ButtonTexts[i].text = "Back";

			else if(i+1 < P.Abilities.Count && P.Abilities[i+1].comboLevel == 1) {
				this.basicAbilitiesCount++;
				this.ButtonTexts[i].text = P.Abilities[i+1].cover;
			}

			else this.ButtonTexts[i].text = "";
		}
	}
	// mhf15
	private async void PrintStatus(){
		Debug.Log("Current Menu: "+CurrentMenu);
		await Task.Delay(2000);
	}

#endregion
}