// working version after readability changes


using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueue : MonoBehaviour
{
	[Header("System")]
	// aqvg1
	public BattleSystem BattleSystem;
	// aqvg1

	// aqvg2
	[SerializeField] private int maxComboLv = 5;
	public int comboLevel = 1;
	// aqvg2

	// aqvg3
	public GameObject ComboButton;
	public GameObject ComboBoxes;
	[SerializeField] private List<GameObject> ComboBoxList = new List<GameObject>();
	public Text ComboLevelText;
	// aqvg3

	// aqvg4
	public GameObject ActionBoxes;
	[SerializeField] private List<GameObject> ActionBoxList = new List<GameObject>();
	public Text ActionText;
	// aqvg4

	// aqvg5
	public List<Action> Actions = new List<Action>();
	public List<Action> ComboList = new List<Action>();
	public List<string> AvailableComboActionsAsStrings = new List<string>(); // idk, need another var name, this one sucks
	// aqvg5

	// aqvg6
	public int firstComboAbilityIndex = 0;
	// aqvg7
	public int comboListOverFlow = 0;
	// aqvg8
	public Action CurrentComboAction;

	// aqvg9
	public int currentHeat = 0;
	public int heatGainThisRound = 0;
	public int[] heatLimits = {0,350,1200,6000,25000, int.MaxValue};
	public int[] heatValues = {50,175,560,1400,0};
	public float roundEndHeatf = 0.7f;
	public float fullComboHeatf = 1.5f;
	// aqvg9

	// aqvg10
    public int maxMovesQueued = 10;

    // aqvg11
	public bool stopQueue = false;
	public bool pauseQueue = false;
	[SerializeField] public int timePerAction = 2000;
	// aqvg11

#region SetUp Functions
	void Start(){
		ComboLevelText.text = "Lv."+comboLevel.ToString();
		ActionText.text = "";
		ActionBoxes = GameObject.Find("Action Boxes");
		ComboBoxes = GameObject.Find("Combo Boxes");

		GetActionBoxList();
		GetComboBoxList();

		SetMaxComboLevel();

		this.ComboButton.SetActive(false);
	}
	private void GetActionBoxList(){
		for(int i = 0; i < ActionBoxes.transform.childCount; i++){
			ActionBoxes.transform.GetChild(i).gameObject.SetActive(false);
			ActionBoxList.Add(ActionBoxes.transform.GetChild(i).gameObject);
		}
	}
	private void GetComboBoxList(){
		for(int i = 0; i < ComboBoxes.transform.childCount; i++){
			ComboBoxes.transform.GetChild(i).gameObject.SetActive(false);
			ComboBoxList.Add(ComboBoxes.transform.GetChild(i).gameObject);
		}
	}
	private void SetMaxComboLevel(){
		this.maxComboLv = 1;
		foreach(Action A in BattleSystem.Player.Abilities) if(A.comboLevel > this.maxComboLv) this.maxComboLv = A.comboLevel;
	}
#endregion

#region Execute All Actions Functions
	public async Task ExecuteAllActions(){
		BattleSystem.state = BattleState.QUEUE;
		this.ComboButton.SetActive(false);

		SpentPlayerActionPoints();
		
		//printActionQueue();
		ClearComboList();

		int actionsExecutedCount = 0;
		while(Actions.Count > 0 && !stopQueue) {
			if(!pauseQueue){
    			List<Task> tasksTmp = new List<Task>();

				Action CurrentAction = GetNextActionFromActionsList();

				if(!BattleSystem.disableHeat)AddHeat(CurrentAction);

				if(CurrentAction.ultForm) tasksTmp.Add(CurrentAction.UltExecute());
				else tasksTmp.Add(CurrentAction.NormalExecute());
				
				CurrentAction.PermanentEffect();

				tasksTmp.Add(Task.Delay(timePerAction));

				await Task.WhenAll(tasksTmp);
				actionsExecutedCount++;
			}
			else await Task.Yield();
		}
		if(!BattleSystem.disableHeat)RoundEndHeat(actionsExecutedCount);
	}
	public void StopQueue(){
        this.stopQueue = true;
        ClearActionQueue();	
	}
	private void SpentPlayerActionPoints(){
		BattleSystem.Player.currentActionPoints -= BattleSystem.Player.spentActionPoints;
		BattleSystem.Player.spentActionPoints = 0;
	}
	private Action GetNextActionFromActionsList(){
		if(this.Actions.Count <= 0){
			Debug.LogError("ExecuteAllActions() tried to get an Action when ActionsList was empty");
			return null;
		}

		this.ActionText.text = "";

		Action First = Actions[0];
		Actions.RemoveAt(0);

		UpdateVisualizer();
		return First;
	}
#endregion

#region Action Heat Functions
	private void AddHeat(Action CurrentAction){
		int heatGain = CalculateHeatGainFromAction(CurrentAction);

		this.heatGainThisRound += heatGain;
		this.currentHeat += heatGain;
	}
	private void RoundEndHeat(int actionsExecutedCount){
		if(IsFullComboExecuted(actionsExecutedCount)) this.currentHeat += GetFullComboHeatGain();
		CalculateNewComboLevel();
		RoundEndHeatDecrease();
		CalculateNewComboLevel();
		this.heatGainThisRound = 0;
	}
	private int CalculateHeatGainFromAction(Action A){
		float heatMultiplicator = ((float)BattleSystem.abilityDecayLevelCount/100)*BattleSystem.AbilityDecayArray[A.abilityIndex];
		//Debug.Log("Heat: +"+((int)Mathf.Round(heatValues[A.comboLevel-1] * heatMultiplicator)).ToString());

		int heatGain = (int)Mathf.Round(heatValues[A.comboLevel-1] * heatMultiplicator);
		return heatGain;
	}
	private bool IsFullComboExecuted(int actionsExecutedCount){

		return actionsExecutedCount == this.maxMovesQueued;
	}
	private int GetFullComboHeatGain(){

		return (int)Mathf.Round(((float)this.heatGainThisRound * this.fullComboHeatf) - this.heatGainThisRound);
	}
	private void CalculateNewComboLevel(){
		this.comboLevel = 0;
		while(this.currentHeat >= this.heatLimits[this.comboLevel] && this.comboLevel+1 <= this.maxComboLv) this.comboLevel++;

		ComboLevelText.text = "Lv."+comboLevel.ToString();
		//Debug.Log("ComboLv: "+this.comboLevel.ToString());
	}
	private void RoundEndHeatDecrease(){
		int oldHeat = this.currentHeat;

		this.currentHeat -= (int)Mathf.Round((this.currentHeat - this.heatLimits[this.comboLevel-1])*this.roundEndHeatf);
		if(this.currentHeat>0)this.currentHeat--;
		//Debug.Log("CurrentHeat = "+oldHeat.ToString()+" - "+(oldHeat - this.currentHeat).ToString());
	}
#endregion

#region Adding new Actions to the ActionQueue
	public void AddAction(Action A){
		ActionText.text = A.name;
		if(!ActionQueueFull()) Actions.Add(A);
		else this.comboListOverFlow++;

		if(!ActionIsComboAbility(A)) AddToComboList(A);

		UpdateVisualizer();
	}
	private void AddToComboList(Action A){
		this.ComboButton.SetActive(false);
		if(ComboListIsFull()) ComboList.RemoveAt(0);
		ComboList.Add(A);
		if(ComboList.Count > 1) CheckComboList();
		UpdateComboVisualizer();
	}
	private void RefillComboList(){
		foreach(Action A in Actions){
			if(!ActionIsComboAbility(A)) {
				if(ComboListIsFull()) ComboList.RemoveAt(0);
				ComboList.Add(A);
			}else ComboList = new List<Action>();
		}
	}
	public bool ActionQueueFull(){

		return Actions.Count >= this.maxMovesQueued;
	}
	private bool ActionIsComboAbility(Action A){

		return A.comboLevel >= 2;
	}
	private void UpdateVisualizer(){
		int visualizerIndexAfterFirstCheck = CompareAbilityListWithBoxes(this.Actions, this.ActionBoxList);
		CheckRemainingBoxes(visualizerIndexAfterFirstCheck, this.ActionBoxList);
	}
	private bool ComboListIsFull(){

		return ComboList.Count >= comboLevel;
	}
	private void CheckComboList(){
		string comboListAsString = ComboQueueIntoString();
		//Debug.Log(comboListAsString);
		int aIndex = this.firstComboAbilityIndex;

		do {
			for(int comboActionIndex = this.AvailableComboActionsAsStrings.Count-1; comboActionIndex >= 0; comboActionIndex--){
				if(comboStringMatchFound(comboListAsString, comboActionIndex)){
					//Debug.Log("match");
					CurrentComboAction = GetComboAction(comboListAsString);
					CheckUltForm(CurrentComboAction);
					EnableComboButton(CurrentComboAction.cover);
					return;
				}
			}
			comboListAsString = comboListAsString.Substring(1);
		}while(comboListAsString.Length > 1);
	}
	private void UpdateComboVisualizer(){
		int visualizerIndexAfterFirstCheck = CompareAbilityListWithBoxes(this.ComboList, this.ComboBoxList);
		CheckRemainingBoxes(visualizerIndexAfterFirstCheck, this.ComboBoxList);
	}
	private int CompareAbilityListWithBoxes(List<Action> AbilityList, List<GameObject> BoxList){
		int visualizerIndex = 0;
		while(visualizerIndex < AbilityList.Count){
			BoxList[visualizerIndex].SetActive(true);
			BoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<Text>().text = AbilityList[visualizerIndex].cover;
			visualizerIndex++;
		}
		return visualizerIndex;	
	}
	private void CheckRemainingBoxes(int visualizerIndex, List<GameObject> BoxList){
		while(visualizerIndex < BoxList.Count){
			BoxList[visualizerIndex].SetActive(false);
			visualizerIndex++;
		}
	}
	private string ComboQueueIntoString(){
		string res = "";

		foreach(Action A in this.ComboList){
			res += A.comboList;
		}

		return res;
	}
	private bool comboStringMatchFound(string comboListAsString, int comboActionIndex){

		return comboListAsString == this.AvailableComboActionsAsStrings[comboActionIndex];
	}
	private Action GetComboAction(string comboString){
		Action FoundComboAction = BattleSystem.Player.Weapon.CombineActions(comboString, this.ComboList);
		return FoundComboAction;
	}
	private void CheckUltForm(Action ComboAction){
		if(ComboAction.comboLevel == this.maxComboLv) {
			ComboAction.ultForm = true;
		}
	}
	private void EnableComboButton(string buttonText){
		ComboButton.SetActive(true);
		GameObject.Find("Button Text").GetComponent<Text>().text = buttonText;
	}
	public bool MaxActionsQueued(){

		return ActionQueueFull() && this.comboListOverFlow >= this.comboLevel;
	}
#endregion

#region Removing Actions from the ActionQueue
	public Action RemoveLastAction(bool refillComboList = true) {
		this.ComboButton.SetActive(false);

		ActionText.text = "";
		Action Rem = Actions[Actions.Count-1];
		Actions.RemoveAt(Actions.Count-1);

		ComboList = new List<Action>();
		this.comboListOverFlow = 0;
		RefillComboList();

		UpdateVisualizer();
		UpdateComboVisualizer();
		if(ComboList.Count > 1) CheckComboList();
		return Rem;
	}
	public void ClearActionQueue(){
		while(Actions.Count > 0){
			RemoveLastAction(false);
		}
	}
	public void ClearComboList(){
		this.ComboButton.SetActive(false);

		while(ComboList.Count > 0){
			ComboList.RemoveAt(0);
		}
		UpdateComboVisualizer();
	}
#endregion

	public void SetComboLevel(int lv){
		ClearActionQueue();
		this.comboLevel = lv;
		this.currentHeat = this.heatLimits[lv-1];
		this.heatGainThisRound = 0;
		ComboLevelText.text = "Lv."+comboLevel.ToString();
	}
	
	public void printActionQueue(){
		Debug.Log("Printing Actions");
		foreach(Action A in Actions){
			Debug.Log(A.name);
		}
	}
}