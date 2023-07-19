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
	private BattleSystem BattleSystem;
	public ActionQueue ActionQueue;
	public PauseMenu PauseMenu;
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

	private bool queueAttackRushW = false;
	private bool queueAttackRushA = false;
	private bool queueAttackRushS = false;

	private bool attackRushInputsEnabled = false;

#region Unity functions

	// mhf2
	private void Update(){
		this.stopInputs = (this.BattleSystem == null);
		if(!this.stopInputs) CheckInputs();
		else this.InputDarkFilter.SetActive(true);
	}

#endregion

#region Main Functions

	public void Setup(BattleSystem BS){
		this.BattleSystem = BS;
		this.CurrentMenu = Menu.MAIN;
		this.InputDarkFilter.SetActive(false);
		SetupButtonTexts();
		LoadMainMenu();
	}

	// mhf3
	public virtual void CheckInputs(){ // Changed in: NBS
		switch(this.BattleSystem.state){
			case BattleState.START:
			case BattleState.PLAYERTURN:
				this.InputDarkFilter.SetActive(false);
				BattleInputs();
			break;

			case BattleState.RESULT:
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.A) && this.BattleSystem.ResultHandler.finishedDisplayingResult) this.BattleSystem.End();
				// else if(Input.GetKeyDown(KeyCode.D) && this.PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
			break;

			case BattleState.SETUP:
				this.InputDarkFilter.SetActive(false);
				BattleStartMenuInputs();
			break;

			case BattleState.WAVEOVER:
				this.InputDarkFilter.SetActive(false);
				WaveOverInputs();
			break;

			case BattleState.PLAYERDIED:
				this.InputDarkFilter.SetActive(false);
				if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.LeaveGame();
			break;

			case BattleState.ATTACKRUSH:
				if(this.attackRushInputsEnabled) AttackRushInputs();
				else this.InputDarkFilter.SetActive(true);
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
			if(i == 3 && !this.ActionQueue.heatChargeDone) ButtonTexts[i].text = "Charge";
			else if(i == 3 && this.ActionQueue.heatChargeDone && this.ActionQueue.Actions.Count <= 0) ButtonTexts[i].text = "Discharge";
			else if(i == 3) ButtonTexts[i].text = "Cancel";
		}
	}

	public void LoadComboAbilityMenu(){
		List<Action> PlayerAbilities = this.BattleSystem.Player.GetAbilityList();

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
		for(int i = 0; i < 3; i++){
			this.ButtonTexts[i].text = "End Round";
		}
		this.ButtonTexts[3].text = "Cancel";
		this.CurrentMenu = Menu.FULLQUEUE;
	}

	public void EnableAttackRushInputs(bool on){
		this.InputDarkFilter.SetActive(!on);
		this.attackRushInputsEnabled = on;
	}

	private void ChangeToNewActionButtons(){
		PlayerCharacter P = this.BattleSystem.Player;

		for(int i = 0; i < this.ButtonTexts.Count; i++){

			if(i == 3) {
				if(!this.ActionQueue.heatChargeDone) this.ButtonTexts[i].text = "Charge";
				else this.ButtonTexts[i].text = "Cancel";
			}

			else if(i < P.GetAbilityList().Count) {
				this.ButtonTexts[i].text = this.CurrentMenuAttacks[i].cover;
			}

			else this.ButtonTexts[i].text = "";
		}
	}

#endregion 

#region Helper Functions

	// mhf9
	private void BattleInputs(){
		QueueAttackRushInputs();

		if(this.CurrentMenu == Menu.MAIN){
			if(BattleSystem.state == BattleState.START && !this.ActionQueue.heatChargeDone && this.ActionQueue.Actions.Count <= 0){
				HeatChargeInputs();
			}else if(this.ActionQueue.Actions.Count <= 0){
				HeatDischargeInputs();
			}
			NormalAttackCastInputs();
		}else if(this.CurrentMenu == Menu.FINALACTION){
			FinalActionInputs();
		}else if(CurrentMenu == Menu.FULLQUEUE){
			FullQueueInputs();
		}
	}
	private void QueueAttackRushInputs(){
		if(Input.GetKeyDown(KeyCode.W)){
			if(IsChargeCheck('W')) return;
			this.queueAttackRushW = true;
			BattleSystem.StartAttackRushQueue();
		}else if(Input.GetKeyUp(KeyCode.W) && this.queueAttackRushW){
			StopAttackRushQueue();
		}else if(Input.GetKeyDown(KeyCode.A) && !this.queueAttackRushW && !this.queueAttackRushS){
			if(IsChargeCheck('A')) return;
			this.queueAttackRushA = true;
			BattleSystem.StartAttackRushQueue();
		}else if(Input.GetKeyUp(KeyCode.A) && this.queueAttackRushA){
			StopAttackRushQueue();
		}else if(Input.GetKeyDown(KeyCode.S) && !this.queueAttackRushW && !this.queueAttackRushA){
			if(IsChargeCheck('S')) return;
			this.queueAttackRushS = true;
			BattleSystem.StartAttackRushQueue();
		}else if(Input.GetKeyUp(KeyCode.S) && this.queueAttackRushS){
			StopAttackRushQueue();
		}
	}
	private void HeatChargeInputs(){
		if(Input.GetKeyDown(KeyCode.D)) {
			StopAttackRushQueue();
			this.ActionQueue.StartHeatChargeLoop();
		}
		else if(Input.GetKeyUp(KeyCode.D)) {
			this.ActionQueue.StopHeatChargeLoop();
		}
	}
	private void HeatDischargeInputs(){
		if(Input.GetKeyDown(KeyCode.D)) {
			StopAttackRushQueue();
			this.ActionQueue.StartHeatDischargeLoop();
		}
		else if(Input.GetKeyUp(KeyCode.D)) {
			BattleSystem.StopAttackRushQueue();
			this.ActionQueue.StopHeatDischargeLoop();
		}
	}
	private void NormalAttackCastInputs(){
		if(Input.GetKeyDown(KeyCode.D) && this.ActionQueue.Actions.Count > 0 && this.ActionQueue.heatChargeDone) {
			StopAttackRushQueue();
			this.BattleSystem.CancelLastAction();
			LoadMainMenu();
		}
		else if(Input.GetKeyDown(KeyCode.W) && this.basicAbilitiesCount > 2) {
			if(!queueAttackRushW) StopAttackRushQueue();
			if(IsChargeCheck('W')) return;
			this.BattleSystem.CastAttack(2);
		}
		else if(Input.GetKeyDown(KeyCode.A) && this.basicAbilitiesCount > 1) {
			if(!queueAttackRushA) StopAttackRushQueue();
			if(IsChargeCheck('A')) return;
			this.BattleSystem.CastAttack(1);
		}
		else if(Input.GetKeyDown(KeyCode.S) && this.basicAbilitiesCount > 0) {
			if(!queueAttackRushS) StopAttackRushQueue();
			if(IsChargeCheck('S')) return;
			this.BattleSystem.CastAttack(0);
		}
	}
	private void FinalActionInputs(){
		if(Input.GetKeyDown(KeyCode.D)) {
			StopAttackRushQueue();
			this.BattleSystem.CancelLastAction();
			LoadMainMenu();
		}
		else if(Input.GetKeyDown(KeyCode.W) && this.basicAbilitiesCount > 2) {
			if(!queueAttackRushW) StopAttackRushQueue();
			if(IsChargeCheck('W')) return;
			this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[2].name));
		}
		else if(Input.GetKeyDown(KeyCode.A) && this.basicAbilitiesCount > 1) {
			if(!queueAttackRushA) StopAttackRushQueue();
			if(IsChargeCheck('A')) return;
			this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[1].name));
		}
		else if(Input.GetKeyDown(KeyCode.S) && this.basicAbilitiesCount > 0) {
			if(!queueAttackRushS) StopAttackRushQueue();
			if(IsChargeCheck('S')) return;
			this.BattleSystem.CastAttack(this.BattleSystem.GetAbilityIndexByString(this.CurrentMenuAttacks[0].name));
		}
	}
	private void FullQueueInputs(){
		if(Input.GetKeyDown(KeyCode.D)) {
			StopAttackRushQueue();
			this.BattleSystem.CancelLastAction();
			if(this.ActionQueue.Actions.Count == this.ActionQueue.comboLevel-1 && this.ActionQueue.Actions.Count > 0) LoadComboAbilityMenu();
			else LoadMainMenu();
		}else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W)){
			StopAttackRushQueue();
			this.BattleSystem.PassRound();
			LoadMainMenu();
		}
	}

	private bool IsChargeCheck(char key){
		return (ActionQueue.heatCharge || ActionQueue.heatDischarge);
	}

	private void StopAttackRushQueue(){
		this.queueAttackRushW = false;
		this.queueAttackRushA = false;
		this.queueAttackRushS = false;
		BattleSystem.StopAttackRushQueue();
	}

	protected virtual void BattleStartMenuInputs(){ // Changed in: NBS
		if(Input.GetKeyDown(KeyCode.A)) this.PauseMenu.StartButtonTest('A');
		else if(Input.GetKeyDown(KeyCode.W)) this.PauseMenu.StartButtonTest('W');
		else if(Input.GetKeyDown(KeyCode.S)) this.PauseMenu.StartButtonTest('S');
		else if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.StartButtonTest('D');
	}
	// mhf11
	private void WaveOverInputs(){
		if(Input.GetKeyDown(KeyCode.A) && this.PauseMenu.ContinueButton.activeSelf) {
			this.PauseMenu.ContinueGame();
			CurrentMenu = Menu.MAIN;
		}else if(Input.GetKeyDown(KeyCode.D) && this.PauseMenu.LeaveButton.activeSelf) this.PauseMenu.LeaveGame();
		else if(Input.GetKeyDown(KeyCode.W) && this.PauseMenu.NextStageButton.activeSelf) BattleSystem.NextStage();
	}

	private void AttackRushInputs(){
		if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S)){
			BattleSystem.Enemy.AttackRushHit();
		}
	}
	// mhf12
	private  void SetupButtonTexts(){

		InstantiateAttackButtons();

		foreach(Text t in this.ButtonTexts){
			this.MainMenuTexts.Add(t.text);
		}
	}
	// mhf14
	private void InstantiateAttackButtons(){
		PlayerCharacter P = BattleSystem.Player;
		this.basicAbilitiesCount = 0;

		List<Action> PlayerAbilities = BattleSystem.Player.GetAbilityList();

		for(int i = 0; i < this.ButtonTexts.Count; i++){

			if(i == 3) this.ButtonTexts[i].text = "Cancel";

			else if(i < PlayerAbilities.Count && PlayerAbilities[i].comboLevel == 1) {
				this.basicAbilitiesCount++;
				this.ButtonTexts[i].text = PlayerAbilities[i].cover;
				this.CurrentMenuAttacks.Add(PlayerAbilities[i]);
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