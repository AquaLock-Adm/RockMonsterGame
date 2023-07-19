/*
LOG: Version before 23_08_22 constant input change REVERT
*/




using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueue : MonoBehaviour
{
	[Header("System")]
	public BattleSystem BattleSystem;

	public BattleHUD BattleHUD;

	public MenuHandler MenuHandler;

	[SerializeField] private int maxComboLv = 5;
	public GameObject ComboButton;
	public int comboLevel = 1;
	public int removedFromComboQueue = 0; // used for the case that a combomechanic removes actions from combo queue, full expl in function RemoveFromComboQueue()
	public GameObject ComboBoxes;
	public Text ComboLevelText;
	public int firstComboAbilityIndex = 0;
	public int comboListOverFlow = 0;
	public List<Action> ComboList = new List<Action>();
	public List<string> ComboActionStrings = new List<string>(); // idk, need another var name, this one sucks
	public Action CurrentComboAction;
	[SerializeField] private List<GameObject> ComboBoxList = new List<GameObject>();

	public int currentHeat = 0;
	public int heatGainThisRound = 0;
	public int[] heatLimits = {0,350,1200,6000,25000, int.MaxValue};
	public int[] heatValues = {50,175,560,1400,0};
	public float roundEndHeatf = 0.7f;
	public float fullComboHeatf = 1.5f;

	public GameObject ActionBoxes;
	public Text ActionText;
	public List<Action> Actions = new List<Action>();

	public Queue<string> TextQueue = new Queue<string>();
	[SerializeField] private List<GameObject> ActionBoxList = new List<GameObject>();

	public int actionBoxCount = 0;

    public int maxMovesQueued = 10;

	public bool stopQueue = false;

	public bool pauseQueue = false;

    [SerializeField] public int timePerAction = 2000;


	void Start(){
		ComboLevelText.text = "Lv."+comboLevel.ToString();
		ActionText.text = "";
		ActionBoxes = GameObject.Find("Action Boxes");
		ComboBoxes = GameObject.Find("Combo Boxes");

		GetActionBoxList();
		GetComboBoxList();

		this.maxComboLv = 1;
		foreach(Action A in BattleSystem.Player.Abilities) if(A.comboLevel > this.maxComboLv) this.maxComboLv = A.comboLevel;

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

	public async Task ExecuteAllActions(){
		BattleSystem.state = BattleState.QUEUE;

		BattleSystem.Player.currentActionPoints -= BattleSystem.Player.spentActionPoints;
		BattleSystem.Player.spentActionPoints = 0;
		
		//printActionQueue();
		ClearComboList();
		
		List<Action> ActiveActions = new List<Action>();
		foreach(Action AqA in this.Actions) ActiveActions.Add(AqA);

		ClearActionQueue();

		int nActionsExecuted = 0;
		while(ActiveActions.Count > 0 && !stopQueue) {
			if(!pauseQueue){
    			List<Task> tasksTmp = new List<Task>();

				Action CurrentAction = ActiveActions[0];
				ActiveActions.RemoveAt(0);

				if(!BattleSystem.disableHeat)AddHeat(CurrentAction.comboLevel, CurrentAction.abilityIndex);

				if(CurrentAction.ultForm) tasksTmp.Add(CurrentAction.UltExecute());
				else tasksTmp.Add(CurrentAction.NormalExecute());
				
				CurrentAction.PermanentEffect();

				tasksTmp.Add(Task.Delay(timePerAction));

				await Task.WhenAll(tasksTmp);
				nActionsExecuted++;
			}
			else await Task.Yield();
		}
		if(!BattleSystem.disableHeat)RoundEndHeat(nActionsExecuted);
	}

	private void AddHeat(int comboLvl, int abilityIndex){
		float heatf = ((float)BattleSystem.abilityDecayLevelCount/100)*BattleSystem.AbilityDecayArray[abilityIndex];
		//Debug.Log("Heat: +"+((int)Mathf.Round(heatValues[comboLvl-1] * heatf)).ToString());
		int gain = (int)Mathf.Round(heatValues[comboLvl-1] * heatf);
		this.heatGainThisRound += gain;
		this.currentHeat += gain;
	}

	private void RoundEndHeat(int nActionsExecuted){
		if(nActionsExecuted == this.maxMovesQueued) this.currentHeat += (int)Mathf.Round(((float)this.heatGainThisRound * fullComboHeatf) - this.heatGainThisRound);
		
		int oldHeat = this.currentHeat;

		this.comboLevel = 0;
		while(this.currentHeat >= this.heatLimits[this.comboLevel] && this.comboLevel+1 <= this.maxComboLv) this.comboLevel++;

		this.currentHeat -= (int)Mathf.Round((this.currentHeat - this.heatLimits[this.comboLevel-1])*this.roundEndHeatf);
		if(this.currentHeat>0)this.currentHeat--;

		//Debug.Log("CurrentHeat = "+oldHeat.ToString()+" - "+(oldHeat - this.currentHeat).ToString());

		this.comboLevel = 0;
		while(this.currentHeat >= this.heatLimits[this.comboLevel] && this.comboLevel+1 <= this.maxComboLv) this.comboLevel++;

		ComboLevelText.text = "Lv."+comboLevel.ToString();
		//Debug.Log("ComboLv: "+this.comboLevel.ToString());

		this.heatGainThisRound = 0;
	}

	public void AddAction(Action A){
		ActionText.text = A.name;
		if(Actions.Count < this.maxMovesQueued) Actions.Add(A);
		else this.comboListOverFlow++;

		if(A.comboLevel < 2) AddToComboList(A);

		UpdateVisualizer();
	}

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

	private void UpdateVisualizer(){
		int i = 0;
		while(i < Actions.Count){
			ActionBoxList[i].SetActive(true);
			ActionBoxList[i].transform.Find("Text").gameObject.GetComponent<Text>().text = Actions[i].cover;
			i++;
		}
		while(i < ActionBoxList.Count){
			ActionBoxList[i].SetActive(false);
			i++;
		}
	}

	private void AddToComboList(Action A){
		this.ComboButton.SetActive(false);
		if(ComboList.Count >= comboLevel) ComboList.RemoveAt(0);
		ComboList.Add(A);
		if(ComboList.Count > 1) CheckComboList();
		UpdateComboVisualizer();
	}

	private void RefillComboList(){
		foreach(Action A in Actions){
			if(A.comboLevel < 2) {
				if(ComboList.Count >= comboLevel) ComboList.RemoveAt(0);
				ComboList.Add(A);
			}else ComboList = new List<Action>();
		}
	}

	private void UpdateComboVisualizer(){
		int i = 0;
		while(i<ComboList.Count){
			ComboBoxList[i].SetActive(true);
			ComboBoxList[i].transform.Find("Text").gameObject.GetComponent<Text>().text = ComboList[i].cover;
			i++;
		}
		while(i<comboLevel){
			ComboBoxList[i].SetActive(false);
			i++;
		}
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

	private void CheckComboList(){
		string comboS = ComboQueueIntoString();

		//Debug.Log(comboS);
		int aIndex = this.firstComboAbilityIndex;

		do {
			for(int i=this.ComboActionStrings.Count-1;i>=0;i--){
				if(comboS == this.ComboActionStrings[i]) {
					//Debug.Log("match");
					//Debug.Log(BattleSystem.Player.Abilities[aIndex+i].name); 
					CurrentComboAction = BattleSystem.Player.Weapon.CombineActions(comboS, this.ComboList);
					if(CurrentComboAction.comboLevel == this.maxComboLv) {
						CurrentComboAction.ultForm = true;
					}
					ComboButton.SetActive(true);
					GameObject.Find("Button Text").GetComponent<Text>().text = CurrentComboAction.cover;
					return;
				}
			}
			comboS = comboS.Substring(1);
		}
		while(comboS.Length > 1);
	}

	private string ComboQueueIntoString(){
		string res = "";

		foreach(Action A in this.ComboList){
			res += A.comboList;
		}

		return res;
	} 

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