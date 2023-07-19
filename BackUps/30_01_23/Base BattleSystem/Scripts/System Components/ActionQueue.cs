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
	[SerializeField] public int maxComboLv = 5;
	public bool stopQueue = false;
	public bool pauseQueue = false;

	[Header("Combo Heat Settings")]
	public int[] heatLimits = {0,100,250,500,1000};
	public int[] heatValues = {50,100,200,400,5};
	
	[Header("System Counters")]
	public int comboLevel = 1;
	public Action CurrentComboAction;

	public int currentHeat = 0;

	[Header("Runtime References")]
	public List<Action> Actions = new List<Action>();
	public List<Action> ComboList = new List<Action>();
	public List<string> AvailableComboActionsAsStrings = new List<string>(); // idk, need another var name, this one sucks

	[Header("UI References")]
    public GameObject ActionBoxPrefab;
	public Text ComboLevelText;

	public Slider HeatProgressSlider;

	public GameObject ActionBoxes;
	[SerializeField] public List<GameObject> ActionBoxList = new List<GameObject>();
	public Text ActionText;

#region Setup Functions
	void Start(){
		Setup();
	}
	private void Setup(){
		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		this.ActionText.text = "";
		this.ActionBoxes = GameObject.Find("Action Boxes");
		UpdateActionBoxList();

		UpdateHeatBar();
	}
	private void GetActionBoxList(){
		for(int i = 0; i < this.ActionBoxes.transform.childCount; i++){
			this.ActionBoxes.transform.GetChild(i).gameObject.SetActive(false);
			this.ActionBoxList.Add(this.ActionBoxes.transform.GetChild(i).gameObject);
		}
	}
	public void SetMaxComboLevel(){
		this.maxComboLv = 1;
		foreach(Action A in BattleSystem.Player.Abilities) if(A.comboLevel > this.maxComboLv) this.maxComboLv = A.comboLevel;
	}
#endregion

#region Execute All Actions Functions
	public async Task ExecuteAllActions(){
		BattleSystem.state = BattleState.QUEUE;

		while(Actions.Count > 0 && !stopQueue) {
			if(!pauseQueue){

				Action CurrentAction = GetNextActionFromActionsList();

				if(CurrentAction.BattleSystem == null) {
					Debug.LogError("WARNING: BattleSystem must be set at beginning of BattleScene!");
					continue;
				}

				await CurrentAction.SetBattleUnits();
					
				await CurrentAction.NormalExecute();

				if(CurrentAction.damage > 0){
					if(!BattleSystem.disableHeat)AddHeat(CurrentAction);
				}
			}
			else await Task.Yield();
		}
		UpdateActionBoxList();
	}
	public void StopQueue(){
        this.stopQueue = true;
        ClearActionQueue();	
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
		this.currentHeat += heatGain;
		CalculateNewComboLevel();
		UpdateHeatBar();

		// if(this.comboLevel < this.maxComboLv){
		// 	Debug.Log("Heat: "+this.currentHeat.ToString()+"/"+this.heatLimits[this.comboLevel]);
		// }else{
		// 	Debug.Log("Heat: "+this.currentHeat.ToString()+"/-");
		// }
	}
	private int CalculateHeatGainFromAction(Action A){
		if(this.comboLevel >= this.maxComboLv) return 0;
		return this.heatValues[A.comboLevel-1];
	}
	private void CalculateNewComboLevel(){
		if(this.comboLevel >= this.maxComboLv) {
			this.comboLevel = this.maxComboLv;
		}
		else{
			while(this.comboLevel+1 <= this.maxComboLv && this.currentHeat >= this.heatLimits[this.comboLevel]) {
				this.currentHeat -= this.heatLimits[this.comboLevel];
				this.comboLevel++;
			}
			if(this.comboLevel >= this.maxComboLv){
				this.ComboLevelText.text = "Lv.MAX";
			}else this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		}
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
	public void ResetHeatLevel(){
		this.currentHeat = 0;
		this.comboLevel = 1;
		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
	}
#endregion

#region Adding new Actions to the ActionQueue
	public void AddAction(Action A){
		this.ActionText.text = A.name;

		this.Actions.Add(A);

		UpdateVisualizer();
		BattleMenuHandler BMH = this.BattleSystem.BattleMenuHandler;
		if(PositionsLeftInActionQueue() == 1){
			BMH.LoadComboAbilityMenu();

		}else if(PositionsLeftInActionQueue() == 0){
			BMH.LoadRoundOverMenu();
		}
	}
	private void UpdateVisualizer(){
		int visualizerIndex = 0;
		while(visualizerIndex < this.Actions.Count){
			this.ActionBoxList[visualizerIndex].SetActive(true);
			this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<Text>().text = this.Actions[visualizerIndex].cover;
			visualizerIndex++;
		}
		while(visualizerIndex < this.ActionBoxList.Count){
			this.ActionBoxList[visualizerIndex].SetActive(true);
			this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<Text>().text = "";
			visualizerIndex++;
		}
	}
	public void CheckComboList(){
		string comboListAsString = "";

		foreach(Action A in this.Actions){
			comboListAsString += A.comboList;
		}

		do {
			for(int comboActionIndex = this.AvailableComboActionsAsStrings.Count-1; comboActionIndex >= 0; comboActionIndex--){
				if(comboStringMatchFound(comboListAsString, comboActionIndex)){
					// Debug.Log("match");
					this.CurrentComboAction = GetComboAction(comboListAsString);
					return;
				}
			}
			comboListAsString = comboListAsString.Substring(1);
		}while(comboListAsString.Length > 1);
	}
	private void UpdateActionBoxList(){
		ClearActionBoxes();
		this.ActionBoxList = new List<GameObject>();
		for(int i = 0; i < this.comboLevel; i++){
			GameObject NewActionBox_GO = Instantiate(this.ActionBoxPrefab);
			NewActionBox_GO.transform.SetParent(this.ActionBoxes.transform);
			this.ActionBoxList.Add(NewActionBox_GO);
			NewActionBox_GO.transform.Find("Text").gameObject.GetComponent<Text>().text = "";
		}
	}
	private void ClearActionBoxes(){
		foreach(Transform t in this.ActionBoxes.transform){
			Destroy(t.gameObject);
		}
	}
	public bool comboStringMatchFound(string comboListAsString, int comboActionIndex){

		return comboListAsString == this.AvailableComboActionsAsStrings[comboActionIndex];
	}
	private Action GetComboAction(string comboString){
		Action FoundComboAction = BattleSystem.Player.Weapon.CombineActions(comboString, this.ComboList);
		return FoundComboAction;
	} // <-- necessary? correct?
#endregion

#region Removing Actions from the ActionQueue
	public Action RemoveLastAction(bool refillComboList = true) {
		// this.ComboButton.SetActive(false);

		this.ActionText.text = "";
		Action Rem = this.Actions[this.Actions.Count-1];
		this.Actions.RemoveAt(this.Actions.Count-1);

		// this.ComboList = new List<Action>();
		// this.comboListOverFlow = 0;
		// RefillComboList();

		UpdateVisualizer();
		// UpdateComboVisualizer();
		// if(this.ComboList.Count > 1) CheckComboList();
		return Rem;
	}
	public void ClearActionQueue(){
		while(Actions.Count > 0){
			RemoveLastAction(false);
		}
	}
#endregion

	private int PositionsLeftInActionQueue(){

		return this.comboLevel - this.Actions.Count;
	}
	
	public void printActionQueue(){
		Debug.Log("Printing Actions");
		foreach(Action A in Actions){
			Debug.Log(A.name);
		}
	}
}