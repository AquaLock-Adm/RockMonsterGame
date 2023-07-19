using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueue : MonoBehaviour
{
	[Header("System Reference")]
	public BattleSystem BattleSystem;

	[Header("System Settings")]
    public int maxMovesQueued = 10;
	[SerializeField] public int maxComboLv = 5;
	[SerializeField] public int timePerAction = 2000;
	public bool stopQueue = false;
	public bool pauseQueue = false;

	[Header("Combo Heat Settings")]
	public int[] heatLimits = {0,350,1200,6000,25000, int.MaxValue};
	public int[] heatValues = {50,175,560,1400,0};
	public float roundEndHeatf = 0.7f;
	public float fullComboHeatf = 1.5f;
	
	[Header("System Counters")]
	public int comboLevel = 1;
	public Action CurrentComboAction;

	public int comboListOverFlow = 0;

	public int currentHeat = 0;
	public int heatGainThisRound = 0;

	[Header("Runtime References")]
	public List<Action> Actions = new List<Action>();
	public List<Action> ComboList = new List<Action>();
	public List<string> AvailableComboActionsAsStrings = new List<string>(); // idk, need another var name, this one sucks

	[Header("UI References")]
	public GameObject ComboButton;
	public GameObject ComboBoxes;
	[SerializeField] public List<GameObject> ComboBoxList = new List<GameObject>();
	public Text ComboLevelText;

	public Slider HeatProgressSlider;

	public GameObject ActionBoxes;
	[SerializeField] public List<GameObject> ActionBoxList = new List<GameObject>();
	public Text ActionText;


#region Setup Functions
	void Start(){
		Setup();
	}
	public virtual void Setup(){
		this.ComboLevelText.text = "Lv."+this.comboLevel.ToString();
		this.ActionText.text = "";
		this.ActionBoxes = GameObject.Find("Action Boxes");
		this.ComboBoxes = GameObject.Find("Combo Boxes");

		GetActionBoxList();
		GetComboBoxList();

		SetMaxComboLevel();
		UpdateHeatBar();

		this.ComboButton.SetActive(false);
	}
	private void GetActionBoxList(){
		for(int i = 0; i < this.ActionBoxes.transform.childCount; i++){
			this.ActionBoxes.transform.GetChild(i).gameObject.SetActive(false);
			this.ActionBoxList.Add(this.ActionBoxes.transform.GetChild(i).gameObject);
		}
	}
	private void GetComboBoxList(){
		for(int i = 0; i < ComboBoxes.transform.childCount; i++){
			ComboBoxes.transform.GetChild(i).gameObject.SetActive(false);
			ComboBoxList.Add(ComboBoxes.transform.GetChild(i).gameObject);
		}
	}
	public void SetMaxComboLevel(){
		this.maxComboLv = 1;
		foreach(Action A in BattleSystem.Player.Abilities) if(A.comboLevel > this.maxComboLv) this.maxComboLv = A.comboLevel;
	}
#endregion

#region Execute All Actions Functions
	public virtual async Task ExecuteAllActions(){
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

				if(CurrentAction.ultForm) tasksTmp.Add(CurrentAction.UltExecute());
				else tasksTmp.Add(CurrentAction.NormalExecute());
				
				CurrentAction.PermanentEffect();

				tasksTmp.Add(Task.Delay(timePerAction));

				await Task.WhenAll(tasksTmp);
				if(CurrentAction.damage > 0){
					if(!BattleSystem.disableHeat)AddHeat(CurrentAction);
				}
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
	public virtual void AddHeat(Action CurrentAction){
		int heatGain = CalculateHeatGainFromAction(CurrentAction);

		this.heatGainThisRound += heatGain;
		this.currentHeat += heatGain;
		CalculateNewComboLevel();
		UpdateHeatBar();
	}
	private void RoundEndHeat(int actionsExecutedCount){
		if(IsFullComboExecuted(actionsExecutedCount)) this.currentHeat += GetFullComboHeatGain();
		CalculateNewComboLevel();
		RoundEndHeatDecrease();
		this.heatGainThisRound = 0;
		UpdateHeatBar();
	}
	public virtual int CalculateHeatGainFromAction(Action A){
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
	public virtual void CalculateNewComboLevel(){
		// Debug.Log("Heat: "+this.currentHeat.ToString()+" / "+this.heatLimits[this.comboLevel].ToString());

		while(this.currentHeat > this.heatLimits[this.comboLevel] && this.comboLevel+1 <= this.maxComboLv) {
			this.currentHeat -= this.heatLimits[this.comboLevel];
			this.comboLevel++;
		}

		// Debug.Log("Combo Level: "+this.comboLevel.ToString());

		ComboLevelText.text = "Lv."+comboLevel.ToString();
	}
	private void RoundEndHeatDecrease(){
		if(this.currentHeat == 0){
			if(this.comboLevel > 1) this.comboLevel--;
			else return;
		}else{
			int heatLoss = (int)Mathf.Round(this.heatGainThisRound * (1.0f - this.roundEndHeatf)); // i.e if 1000 heat is gained lose 300

			this.currentHeat = (int)Mathf.Clamp(this.currentHeat - heatLoss, 0.0f, (float)this.heatLimits[this.comboLevel]);
		}


		// Debug.Log("After Round End");
		// Debug.Log("Heat: "+this.currentHeat.ToString()+" / "+this.heatLimits[this.comboLevel].ToString());
		// Debug.Log("Combo Level: "+this.comboLevel.ToString());
	}
	public void UpdateHeatBar(){
		if(this.comboLevel < this.maxComboLv){
			this.HeatProgressSlider.value = this.currentHeat;
			this.HeatProgressSlider.maxValue = this.heatLimits[this.comboLevel];
		}else{
			// maxHeat level reached;
			this.HeatProgressSlider.maxValue = 1;
			this.HeatProgressSlider.value = 1;
		}
        
	}
#endregion

#region Adding new Actions to the ActionQueue
	public virtual void AddAction(Action A){
		this.ActionText.text = A.name;
		if(!ActionQueueFull()) this.Actions.Add(A);
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
	public virtual void CheckComboList(){
		string comboListAsString = ComboQueueIntoString();
		//Debug.Log(comboListAsString);

		do {
			for(int comboActionIndex = this.AvailableComboActionsAsStrings.Count-1; comboActionIndex >= 0; comboActionIndex--){
				if(comboStringMatchFound(comboListAsString, comboActionIndex)){
					//Debug.Log("match");
					this.CurrentComboAction = GetComboAction(comboListAsString);
					CheckUltForm(this.CurrentComboAction);
					EnableComboButton(this.CurrentComboAction.cover);
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
	public bool comboStringMatchFound(string comboListAsString, int comboActionIndex){

		return comboListAsString == this.AvailableComboActionsAsStrings[comboActionIndex];
	}
	public Action GetComboAction(string comboString){
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
	public virtual Action RemoveLastAction(bool refillComboList = true) {
		this.ComboButton.SetActive(false);

		this.ActionText.text = "";
		Action Rem = this.Actions[this.Actions.Count-1];
		this.Actions.RemoveAt(this.Actions.Count-1);

		this.ComboList = new List<Action>();
		this.comboListOverFlow = 0;
		RefillComboList();

		UpdateVisualizer();
		UpdateComboVisualizer();
		if(this.ComboList.Count > 1) CheckComboList();
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