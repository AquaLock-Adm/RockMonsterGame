using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ActionQueue : MonoBehaviour
{
	[Header("System Reference")]
	private BattleSystem BattleSystem;
	public BattleMenuHandler BattleMenuHandler;

	[Header("System Settings")]
	[SerializeField] public int maxComboLv = 5;
	[SerializeField] private int afterComboDelay = 500; //ms
	public bool stopQueue = false;
	public bool pauseQueue = false;

	private float standartActionBoxHeight = 58f;
	private float standartActionBoxWidth = 100f;

	[Header("Combo Heat Settings")]
	public int[] heatLimits = {1,2,3,4,5,6,7,8,9};
	
	[Header("System Counters")]
	public int comboLevel = 1;
	public Action CurrentComboAction;

	private bool heatDisabledForOneRound = false;

	public bool heatCharge = false;
	public bool heatChargeDone = true;
	public bool heatDischarge = false;

	public int currentHeat = 0;

	[SerializeField] private int attackRushQueueIndex = -1;
	[SerializeField] private int attackRushLevel = -1;
	private bool attackRushActive = false;

	[Header("Runtime References")]
	public List<Action> Actions = new List<Action>();
	public List<string> AvailableComboActionsAsStrings = new List<string>(); // idk, need another var name, this one sucks

	[Header("UI References")]
    public GameObject ActionBoxPrefab;
	public Text ComboLevelText;

	public Slider HeatProgressSlider;

	[SerializeField] private GameObject AttackRushBar;
	[SerializeField] private GameObject AttackRushRemainBox;
	[SerializeField] private GameObject AttackRushDecreaseBox;

	public GameObject ActionBoxes;
	[SerializeField] public List<GameObject> ActionBoxList = new List<GameObject>();
	public Text ActionText;

#region Setup Functions
	public void Setup(BattleSystem BS){
		this.BattleSystem = BS;
		this.ActionText.text = "";
		this.ActionBoxes = GameObject.Find("Action Boxes");
		this.comboLevel = 1;
		this.maxComboLv = BS.Player.GetWeapon().actionsPerRound;

		this.AttackRushBar.SetActive(false);

		// for testing
		// this.comboLevel = 3;
		// this.maxComboLv = 3; 

		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		SetupComboAbilityList();
		SetStandartActionBoxSizes();
		UpdateActionBoxList();

		UpdateHeatBar();
	}

	private void SetupComboAbilityList(){
		foreach(Action A in BattleSystem.Player.GetAbilityList()){
			if(A.comboLevel > 1 && !this.AvailableComboActionsAsStrings.Contains(A.comboString)){
				this.AvailableComboActionsAsStrings.Add(A.comboString);
			}
		}
	}
	private void SetStandartActionBoxSizes(){
		this.standartActionBoxHeight = ActionBoxPrefab.GetComponent<RectTransform>().rect.height;
		this.standartActionBoxWidth = ActionBoxPrefab.GetComponent<RectTransform>().rect.width;
	}
#endregion

#region Execute All Actions Functions
	public async Task ExecuteAllActions(){
		this.stopQueue = false;
		Enemy TargetEnemy = BattleSystem.Enemy;
		TargetEnemy.SetHeldByCombo(true);

		while(Actions.Count > 0 && !stopQueue) {
			if(!pauseQueue){

				Action CurrentAction = GetNextActionFromActionsList();

				if(CurrentAction.name == "Attack Rush"){
					Debug.Log("Executing Attack Rush");

					CurrentAction.SetEnemy(TargetEnemy);
					int damageDealt = await CurrentAction.NormalExecute(BattleSystem);
					continue;
				}

				// Debug.Log("Executing "+CurrentAction.name+" on "+ this.BattleSystem.Enemy.unitName);
					
				bool addHeatAfterAttack = await TargetEnemy.HandleAction(CurrentAction);


				if(addHeatAfterAttack && !this.heatDisabledForOneRound){
					AddHeat();
				}
				
			}
			else await Task.Yield();
		}

		if(TargetEnemy.deathTriggered){
			await TargetEnemy.Death();
			if(BattleSystem.state == BattleState.QUEUE) await BattleSystem.Player.Death(); // Note: Never going to happen because of lifeSteal and Deathheal
		}else if(BattleSystem.Player.deathTriggered) await BattleSystem.Player.Death();
		else{
			await Task.Delay(this.afterComboDelay);
			TargetEnemy.SetHeldByCombo(false);
			TargetEnemy.ResetShield();
		}
		ActionQueueEnd();
	}
	private void ActionQueueEnd(){
		ClearActionQueue();
		UpdateActionBoxList();

		this.heatDisabledForOneRound = false;

		if(this.BattleSystem.state == BattleState.QUEUE){
			this.BattleSystem.state = BattleState.PLAYERTURN;
		}
	}
	public void StopQueue(){
        this.stopQueue = true;
	}
	private Action GetNextActionFromActionsList(){
		if(this.Actions.Count <= 0){
			Debug.LogError("ExecuteAllActions() tried to get an Action when ActionsList was empty");
			return null;
		}

		this.ActionText.text = "";

		Action First = Actions[0];
		Actions.RemoveAt(0);
		if(this.attackRushQueueIndex >= 0) this.attackRushQueueIndex--;

		UpdateVisualizer();
		return First;
	}
#endregion

#region Action Heat Functions
	private void AddHeat(int heatGain = 1){
		this.currentHeat += heatGain;
		CalculateNewComboLevel();
		UpdateHeatBar();
	}
	private void LoseHeat(int heatLoss = 1){
		if(this.comboLevel <= 1) return;

		if(this.currentHeat <= 0 && this.comboLevel > 1){
			this.currentHeat = this.heatLimits[this.comboLevel-1];
			this.comboLevel--;

			if(this.comboLevel >= this.maxComboLv){
				this.ComboLevelText.text = "Lv.MAX";
			}else this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		}

		this.currentHeat = (int)Mathf.Max(0.0f, this.currentHeat - heatLoss);
		UpdateHeatBar();
	}
	private void CalculateNewComboLevel(){
		if(this.comboLevel >= this.maxComboLv && this.currentHeat >= 0) {
			this.comboLevel = this.maxComboLv;
			this.currentHeat = 0;
		}
		else{
			while(this.comboLevel+1 <= this.maxComboLv && this.currentHeat >= this.heatLimits[this.comboLevel]) {
				this.currentHeat = 0;
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
		if(!Application.isPlaying) return;
		this.currentHeat = 0;
		this.comboLevel = 1;
		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		UpdateHeatBar();
		UpdateActionBoxList();
		UpdateVisualizer();
	}
	public void DisableHeatForOneRound(){
		this.heatDisabledForOneRound = true;
	}
#endregion

#region Heat Charge/Discharge Functions
	public async void StartHeatChargeLoop(){
		if(this.heatChargeDone) return;
		this.heatCharge = true;
		int preChargeTime = 1000;
		int currentCharge = 0;
		while (currentCharge < preChargeTime && this.heatCharge){
			currentCharge += 50;
			await Task.Delay(50);
		}
		if(this.heatCharge) HeatChargeLoop();
	}
	public void StopHeatChargeLoop(){
		this.heatCharge = false;
	}
	private async void HeatChargeLoop(){
		int chargeTime_ms = 0;
		int chargeDelayTime_ms = 50;
		int chargeIncreaseTime = 800;

		await Task.Delay(chargeDelayTime_ms);
		while(this.heatCharge){
			chargeTime_ms += chargeDelayTime_ms;
			if(chargeTime_ms >= chargeIncreaseTime){
				int heat;
				if(this.comboLevel == 1) heat = 1;
				else heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel]/2);

				AddHeat(heat);
				UpdateActionBoxList();
				UpdateVisualizer();
				chargeTime_ms -= chargeIncreaseTime;
			}
			await Task.Delay(chargeDelayTime_ms);
		}

		this.heatChargeDone = true;
		this.BattleMenuHandler.LoadMainMenu();
	}

	public async void StartHeatDischargeLoop(){
		this.heatDischarge = true;
		int preChargeTime = 1000;
		int currentCharge = 0;
		while (currentCharge < preChargeTime && this.heatDischarge){
			currentCharge += 50;
			await Task.Delay(50);
		}
		if(this.heatDischarge) HeatDischargeLoop();
	}
	public void StopHeatDischargeLoop(){
		this.heatDischarge = false;
	}
	private async void HeatDischargeLoop(){
		int chargeTime_ms = 0;
		int chargeDelayTime_ms = 50;
		int chargeIncreaseTime = 800;

		await Task.Delay(chargeDelayTime_ms);
		while(this.heatDischarge){
			chargeTime_ms += chargeDelayTime_ms;
			if(chargeTime_ms >= chargeIncreaseTime){
				int heat;
				if(this.comboLevel <= 1) heat = 0;
				else if(this.currentHeat <= 0) heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel-1]/2);
				else heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel]/2);

				LoseHeat(heat);
				UpdateActionBoxList();
				UpdateVisualizer();
				chargeTime_ms -= chargeIncreaseTime;
			}
			await Task.Delay(chargeDelayTime_ms);
		}

		// this.BattleMenuHandler.LoadMainMenu();
	}
#endregion

#region Adding new Actions to the ActionQueue
	public void AddAction(Action A){
		this.ActionText.text = A.name;

		this.Actions.Add(A);
		A.QueueAction(this);

		UpdateVisualizer();
		BattleMenuHandler BMH = this.BattleSystem.BattleMenuHandler;
		if(PositionsLeftInActionQueue() == 1){
			BMH.LoadComboAbilityMenu();
		}else if(PositionsLeftInActionQueue() == 0){
			BMH.LoadRoundOverMenu();
		}else{
			BMH.LoadMainMenu();
		}
	}
	public void UpdateVisualizer(){
		int visualizerIndex = 0;
		while(visualizerIndex < this.Actions.Count){
			this.ActionBoxList[visualizerIndex].SetActive(true);
			this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<Text>().text = this.Actions[visualizerIndex].cover;

			if(visualizerIndex == this.attackRushQueueIndex){
				float rushAttackBoxWidth = GetRushAttackWidth(this.attackRushLevel);
				this.ActionBoxList[visualizerIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(rushAttackBoxWidth, this.standartActionBoxHeight);
			}

			visualizerIndex++;
		}
		while(visualizerIndex < this.ActionBoxList.Count){
			if(this.attackRushQueueIndex >= 0){
				this.ActionBoxList[visualizerIndex].SetActive(false);
			}else{
				this.ActionBoxList[visualizerIndex].SetActive(true);
				this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<Text>().text = "";
				this.ActionBoxList[visualizerIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(this.standartActionBoxWidth, this.standartActionBoxHeight);
			}
			visualizerIndex++;
		}
	}
	public void CheckComboList(){
		string comboListAsString = ActionListToString();

		do {
			for(int comboActionIndex = this.AvailableComboActionsAsStrings.Count-1; comboActionIndex >= 0; comboActionIndex--){
				// Debug.Log(comboListAsString);
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
	private string ActionListToString(){
		string res = "";
		foreach(Action A in this.Actions){
			if(A.comboLevel == 1){
				res += A.comboString;
			}
		}
		return res;
	}
	public bool comboStringMatchFound(string comboListAsString, int comboActionIndex){

		return comboListAsString == this.AvailableComboActionsAsStrings[comboActionIndex];
	}
	private Action GetComboAction(string comboString){
		Weapon PlayerWeapon = BattleSystem.Player.GetWeapon();
		Action FoundComboAction = PlayerWeapon.CombineActions(comboString, this.Actions);
		return FoundComboAction;
	} // <-- necessary? correct?
	private void LoadActionBoxes(bool on){
		this.ActionBoxes.SetActive(on);
		if(on){
			ClearActionBoxes();
			UpdateVisualizer();
			CalculateNewComboLevel();
			UpdateHeatBar();
		}
	}
#endregion

#region Removing Actions from the ActionQueue
	public Action RemoveLastAction() {
		this.ActionText.text = "";
		Action Rem = this.Actions[this.Actions.Count-1];
		this.Actions.RemoveAt(this.Actions.Count-1);
		Rem.DequeueAction(this);

		if(this.attackRushQueueIndex >= 0){
			this.attackRushQueueIndex = -1;
		}

		UpdateVisualizer();
		return Rem;
	}
	public void ClearActionQueue(){
		while(Actions.Count > 0){
			RemoveLastAction();
		}
	}
#endregion

#region Attack Rush Functions
	public void QueueAttackRush(){
		if(this.attackRushQueueIndex >= 0) return;
		AttackRushAction A = new AttackRushAction(BattleSystem.Player); 
		RemoveLastAction();
		this.attackRushQueueIndex = this.Actions.Count;
		this.attackRushLevel = this.comboLevel - this.Actions.Count;

		/*
		Def RushAttackExecute() func
		Drain Heat and Rush over func
		??? Finisher attack ???
		*/
		AddAction(A);
	}
	private float GetRushAttackWidth(int boxesCovered, int drainOver = 0){
		float res = this.standartActionBoxWidth;
		if(boxesCovered < 1) return res;

		res = this.standartActionBoxWidth * boxesCovered + 10 * (boxesCovered-1);
		if(drainOver > 0) res += ( (float)drainOver/100f ) * this.standartActionBoxWidth;
		return res;
	}
	public async void StartAttackRushHeatDrain(){
		BattleSystem.attackRushUsed = true;
		int totalHeat = GetTotalHeatForHeatDrain();
		int stopHeat = GetStopHeat(this.attackRushLevel);
		int totalDrain = totalHeat - stopHeat;

		LoadActionBoxes(false);
		LoadHeatDrainBar(true, totalDrain);
		float origWidth = this.AttackRushDecreaseBox.GetComponent<RectTransform>().rect.width;

		int tickTime_ms = 50;
		float drainPerTick = 2.5f;
		float currentDrain = 0.0f;
		this.attackRushActive = true;

		await Task.Delay(tickTime_ms);
		while(totalHeat > stopHeat && Application.isPlaying && BattleSystem.state == BattleState.ATTACKRUSH){
			if(!this.attackRushActive) break;
			currentDrain += drainPerTick;
			if(currentDrain >= 1.0f){
				totalHeat = (int)Mathf.Max(0.0f, (float)(totalHeat - (int)currentDrain));
				currentDrain -= (int)currentDrain;

				float percDrained = (float)(totalHeat - stopHeat)/(float)totalDrain;
				float newWidth = origWidth * percDrained;
				this.AttackRushDecreaseBox.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, this.standartActionBoxHeight);
			}
			await Task.Delay(tickTime_ms);
		}

		int finisherLevel = 0;
		if(this.attackRushActive){
			this.attackRushActive = false;
			if(BattleSystem.Enemy.GetAttackRushDamage() > 0){
				finisherLevel = (totalDrain - totalHeat + stopHeat)/100;
			}
		}else{
			totalHeat = stopHeat;
		}
		SetComboLevelAfterAttackRush(totalHeat);
		StopAttackRush(finisherLevel);
	}
	public void CancelAttackRush(){
		this.attackRushActive = false;
	}
	private void LoadHeatDrainBar(bool on, int drainValue = 0){
		this.AttackRushBar.SetActive(on);
		if(on){
			SetRemainBoxWidth();
			if(drainValue <= 0){
				Debug.Log("No Drain Value for Decrease Box Width given!");
				return;
			}
			SetDecreaseBoxWidth(drainValue);
		}
	}
	private void SetRemainBoxWidth(){
		if(this.attackRushLevel < this.comboLevel) {
			this.AttackRushRemainBox.SetActive(true);

			float remainBoxWidth = GetRushAttackWidth(this.comboLevel - this.attackRushLevel);
			this.AttackRushRemainBox.GetComponent<RectTransform>().sizeDelta = new Vector2(remainBoxWidth, this.standartActionBoxHeight);
		}
		else this.AttackRushRemainBox.SetActive(false);
	}
	private void SetDecreaseBoxWidth(int drainValue){
		int drainOver = drainValue - this.attackRushLevel * 100;
		this.AttackRushDecreaseBox.SetActive(true);
		float decreaseBoxWidth = GetRushAttackWidth(this.attackRushLevel, drainOver);
		this.AttackRushDecreaseBox.GetComponent<RectTransform>().sizeDelta = new Vector2(decreaseBoxWidth, this.standartActionBoxHeight);
	}
	private int GetTotalHeatForHeatDrain(){
		int res = 0;
		res = this.comboLevel * 100;
		if(this.currentHeat > 0){
			res += (int)Mathf.Round( ( (float)this.currentHeat / (float)this.heatLimits[this.comboLevel] ) * 100f );
		}
		return res;
	}
	private int GetStopHeat(int attackRushLevel){
		return (this.comboLevel - attackRushLevel)*100;
	}
	private void SetComboLevelAfterAttackRush(int remainHeat){
		if(remainHeat <= 0){
			this.comboLevel = 1;
			this.currentHeat = 0;
			return;
		}
		this.comboLevel = remainHeat/100;
		int rest = remainHeat - (this.comboLevel*100);
		float rest_f = (float)rest/100f;
		this.currentHeat = (int)Mathf.Floor((float)this.heatLimits[this.comboLevel]*rest_f);
	}
	private async void StopAttackRush(int finisherLevel){
		this.BattleMenuHandler.EnableAttackRushInputs(false);
		this.AttackRushDecreaseBox.SetActive(false);

		if(finisherLevel <= 0){
			await Task.Delay(500);
		}else{
			Action rushFinisher = BattleSystem.Player.GetWeapon().GetAttackRushFinisher(finisherLevel);
			await BattleSystem.Enemy.HandleAttackRushFinisher(rushFinisher);
		}
		
		this.attackRushLevel = -1;
		LoadHeatDrainBar(false);
		LoadActionBoxes(true);
		BattleSystem.state = BattleState.QUEUE;
	}
#endregion

	private int PositionsLeftInActionQueue(){
		if(this.attackRushQueueIndex >= 0){
			return 0;
		}

		return this.comboLevel - this.Actions.Count;
	}
	
	public void printActionQueue(){
		Debug.Log("Printing Actions");
		foreach(Action A in Actions){
			Debug.Log(A.name);
		}
	}
}