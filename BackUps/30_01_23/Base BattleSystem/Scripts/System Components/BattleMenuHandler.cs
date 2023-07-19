using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// mhvg1
public enum Menu {MAIN, FINALACTION, FULLQUEUE, OTHER}

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

	public List<Action> CurrentMenuAttacks = new List<Action>();
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
	public virtual void CheckInputs(){ // Changed in: NBS
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
	// mhf7
	public void LoadMainMenu(){

		//activate the buttons from the main menu
		CurrentMenu = Menu.MAIN;

		for(int i = 0; i < ButtonTexts.Count; i++){
			ButtonTexts[i].text = MainMenuTexts[i];
		}
	}

	public void LoadComboAbilityMenu(){
		List<Action> PlayerAbilities = this.BattleSystem.Player.Abilities;

		for(int A_i = 0; A_i < this.basicAbilitiesCount; A_i++){
			this.ActionQueue.Actions.Add(PlayerAbilities[A_i]);
			// Debug.Log("Checking for: "+PlayerAbilities[A_i].name);
			this.ActionQueue.CheckComboList();

			Action ResA;

			if(this.ActionQueue.CurrentComboAction != null) ResA = this.ActionQueue.CurrentComboAction;
			else ResA = PlayerAbilities[A_i];

			// Debug.Log("Result: "+ResA.name);
			this.CurrentMenuAttacks[A_i] = ResA;

			// this.CurrentMenuAttacks_NBS[i] = this.ActionQueue.CurrentComboAction;
			this.ActionQueue.Actions.RemoveAt(this.ActionQueue.Actions.Count-1);
			this.ActionQueue.CurrentComboAction = null;
		}

		ChangeToNewActionButtons();
		this.CurrentMenu = Menu.FINALACTION;
	}

	public void LoadRoundOverMenu(){
		foreach(Text t in this.ButtonTexts){
			t.text = "";
		}
		this.ButtonTexts[0].text = "End Round";
		this.ButtonTexts[3].text = "Cancel";
		this.CurrentMenu = Menu.FULLQUEUE;
	}

	private void ChangeToNewActionButtons(){
		PlayerCharacter P = this.BattleSystem.Player;

		for(int i = 0; i < this.ButtonTexts.Count; i++){

			if(i == 3) this.ButtonTexts[i].text = "Cancel";

			else if(i < P.Abilities.Count && P.Abilities[i].comboLevel == 1) {
				this.ButtonTexts[i].text = this.CurrentMenuAttacks[i].cover;
			}

			else this.ButtonTexts[i].text = "";
		}
	}
	// mhf8
	private void LoadBailOutScreen(){
		BattleSystem.state = BattleState.BAILOUT;
		PauseMenu.ShowPauseMenu(true);
		CurrentMenu = Menu.OTHER;
	}

#endregion 

#region Helper Functions

	// mhf9
	public void BattleInputs(){
		if(this.CurrentMenu == Menu.MAIN){
			if(Input.GetKeyDown(KeyCode.D)) this.BattleSystem.CancelLastAction();
			else if(Input.GetKeyDown(KeyCode.W) && this.basicAbilitiesCount > 2) this.BattleSystem.CastAttack(2);
			else if(Input.GetKeyDown(KeyCode.A) && this.basicAbilitiesCount > 1) this.BattleSystem.CastAttack(1);
			else if(Input.GetKeyDown(KeyCode.S) && this.basicAbilitiesCount > 0) this.BattleSystem.CastAttack(0);
		}else if(this.CurrentMenu == Menu.FINALACTION){
			if(Input.GetKeyDown(KeyCode.D)) {
				this.BattleSystem.CancelLastAction();
				LoadMainMenu();
			}
			else if(Input.GetKeyDown(KeyCode.W) && this.basicAbilitiesCount > 2) this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[2].name));
			else if(Input.GetKeyDown(KeyCode.A) && this.basicAbilitiesCount > 1) this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[1].name));
			else if(Input.GetKeyDown(KeyCode.S) && this.basicAbilitiesCount > 0) this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[0].name));
		}else if(CurrentMenu == Menu.FULLQUEUE){
			if(Input.GetKeyDown(KeyCode.D)) {
				this.BattleSystem.CancelLastAction();
				LoadComboAbilityMenu();
			}
			else if(Input.GetKeyDown(KeyCode.S)){
				this.BattleSystem.PassRound();
				LoadMainMenu();
			}
		}
	}
	// mhf10
	private void DialogueInputs(){
		if(Input.GetKeyDown(KeyCode.A)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.W)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.S)) DialogueHandler.ShowNextDialogueText();
		else if(Input.GetKeyDown(KeyCode.D)) DialogueHandler.ShowNextDialogueText();
	}

	public virtual void BattleStartMenuInputs(){ // Changed in: NBS
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
	private  void SetupButtonTexts(){
		// if(BattleSystem.Player.Abilities[0].name != "Element") Debug.Log("ERROR: Menuhandler depends on Element being at Player.Abilities[0]");

		InstantiateAttackButtons();

		foreach(Text t in this.ButtonTexts){
			this.MainMenuTexts.Add(t.text);
		}
	}
	// mhf14
	private void InstantiateAttackButtons(){
		PlayerCharacter P = this.BattleSystem.Player;
		this.basicAbilitiesCount = 0;

		for(int i = 0; i < this.ButtonTexts.Count; i++){

			if(i == 3) this.ButtonTexts[i].text = "Cancel";

			else if(i < P.Abilities.Count && P.Abilities[i].comboLevel == 1) {
				this.basicAbilitiesCount++;
				this.ButtonTexts[i].text = P.Abilities[i].cover;
				this.CurrentMenuAttacks.Add(P.Abilities[i]);
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