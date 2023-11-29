using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerActionHandler : MonoBehaviour
{
	protected PlayerCharacter Player;

	const int TICK_TIME_MS = 50;

	[Header("HUD References")]

    [SerializeField] private GameObject ActionBoxPrefab;
	[SerializeField] protected TextMeshProUGUI ComboLevelText;

	[SerializeField] private Slider HeatProgressSlider;

	[SerializeField] private GameObject AttackRushBar;
	[SerializeField] private GameObject AttackRushRemainBox;
	[SerializeField] private GameObject AttackRushDecreaseBox;

	[SerializeField] private GameObject ActionBoxes;
	[SerializeField] protected TextMeshProUGUI ActionText;
	[SerializeField] private List<GameObject> ActionBoxList = new List<GameObject>();

	private float standartActionBoxHeight = 58f;
	private float standartActionBoxWidth = 100f;

	[Header("Action Parameters")]
	[SerializeField] public int comboLevel = 1;
	[SerializeField] protected int maxComboLv = 1;
	[SerializeField] protected int currentMaxAttackLength = 1;

	public List<Action> Actions = new List<Action>();
	private List<string> AvailableComboActionsAsStrings = new List<string>();

	private Action CurrentComboAction;

	private bool stopQueue = false;

	[SerializeField] private int afterComboDelay_ms = 500;

	[Header("Heat Parameters")]
	private int currentHeat = 0;

	[SerializeField] private int[] heatLimits = {0,1,2,12,50,90,120,220,500,0};

	[Header("Heat Charge Parameters")]
	public bool heatChargeDone = false;
	public bool inHeatCharge = false;
	public bool inHeatDischarge = false;

	[SerializeField] private int preChargeTime_ms = 1000;
	[SerializeField] private int chargeIncreaseInterval_ms = 800;

	[Header("Attack Rush Parameters")]
	private int attackRushLevel = -1;
	private int attackRushQueueIndex = -1;

	protected bool inAttackRushPreLoop = false;
	private bool attackRushActive = false;
	public bool attackRushUsed = false;



#region Setup Functions
	public void Setup(PlayerCharacter Player, List<GameObject> HUDReferences){
		this.Player = Player;
		SetupHUDReferences(HUDReferences);
		SetComboLevel();

		CheckHeatChargeAvailability();
		UpdateActionBoxList();
		UpdateHeatBar();

		SetupComboAbilityList();
	}

	private void SetupHUDReferences(List<GameObject> HUDReferences){
		if(HUDReferences.Count < 8){
			Debug.LogError("Not enough HUD References! "+HUDReferences.Count.ToString()+"/8");
			return;
		}

	    this.ActionBoxPrefab = HUDReferences[0];
	    SetStandartActionBoxSizes();
		this.ComboLevelText = HUDReferences[1].GetComponent<TextMeshProUGUI>();
		this.ActionText = HUDReferences[2].GetComponent<TextMeshProUGUI>();
		this.ActionText.text = "";

		this.HeatProgressSlider = HUDReferences[3].GetComponent<Slider>();

		this.ActionBoxes = HUDReferences[4];

		this.AttackRushBar = HUDReferences[5];
		this.AttackRushBar.SetActive(false);
		this.AttackRushRemainBox = HUDReferences[6];
		this.AttackRushDecreaseBox = HUDReferences[7];

	}

	private void SetStandartActionBoxSizes(){
		this.standartActionBoxHeight = this.ActionBoxPrefab.GetComponent<RectTransform>().rect.height;
		this.standartActionBoxWidth = this.ActionBoxPrefab.GetComponent<RectTransform>().rect.width;
	}

	private void SetComboLevel(){
		this.comboLevel = 1;
		this.currentMaxAttackLength = this.comboLevel;
		this.maxComboLv = Player.GetWeapon().actionsPerRound;

		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
	}

	private void SetupComboAbilityList(){
		foreach(Action A in Player.GetAbilityList()){
			if(A.comboLevel > 1 && !this.AvailableComboActionsAsStrings.Contains(A.comboString)){
				this.AvailableComboActionsAsStrings.Add(A.comboString);
			}
		}
	}
#endregion



    public void BattleEnd(){
        Destroy(this);
    }



#region Input Functions During Player turn
    public void CastAttack(int attackIndex){
        List<Action> PlayerAbilities = Player.GetAbilityList();
        if(!IndexInBoundsOfList(attackIndex, PlayerAbilities.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Abilities");
            return;
        }

        Action A = PlayerAbilities[attackIndex].Copy();
        AddAction(A);
    }

    public void CastBlock(int blockIndex){
        List<Action> blocks = Player.GetBlockAbilities();
        if(!IndexInBoundsOfList(blockIndex, blocks.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Blocks");
            return;
        }

        Action A = blocks[blockIndex].Copy();
        AddAction(A);
    }

    public void CancelLastAction(bool gainApBack = true){
        if(this.Actions.Count > 0) {
			RemoveLastAction();
        }
    }

    public virtual async void PassRound(){
        this.inAttackRushPreLoop = false;
        if(Player.state == PlayerState.START) {
            this.heatChargeDone = true;
        }
        Player.state = PlayerState.QUEUE;
        await ExecuteAllActions();
    } // Changed in: PlayerAction_Tutorial.cs
#endregion



	public void SwitchModes(bool playerIsDefending){
		if(playerIsDefending){
			int comboLengthMax = Player.GetCurrentEnemy().GetCurrentAttackSequenceLength();
			this.comboLevel = (int)Mathf.Min((float)this.comboLevel, (float)comboLengthMax);
		}else{
			this.comboLevel = this.currentMaxAttackLength;
		}

		UpdateActionBoxList();
		UpdateVisualizer();
	}

	public void UpdateVisualizer(){
		int visualizerIndex = 0;
		while(visualizerIndex < this.Actions.Count){
			this.ActionBoxList[visualizerIndex].SetActive(true);
			this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = this.Actions[visualizerIndex].cover;

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
				this.ActionBoxList[visualizerIndex].transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "";
				this.ActionBoxList[visualizerIndex].GetComponent<RectTransform>().sizeDelta = new Vector2(this.standartActionBoxWidth, this.standartActionBoxHeight);
			}
			visualizerIndex++;
		}
	}

	protected void UpdateActionBoxList(){
		ClearActionBoxes();
		this.ActionBoxList = new List<GameObject>();
		for(int i = 0; i < this.comboLevel; i++){
			GameObject NewActionBox_GO = Instantiate(this.ActionBoxPrefab);
			NewActionBox_GO.transform.SetParent(this.ActionBoxes.transform);
			this.ActionBoxList.Add(NewActionBox_GO);
			NewActionBox_GO.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().text = "";
		}
	}

	private void ClearActionBoxes(){
		foreach(Transform t in this.ActionBoxes.transform){
			Destroy(t.gameObject);
		}
	}

	public void CheckHeatChargeAvailability(){
		if(this.maxComboLv < 5) this.heatChargeDone = true;
		else this.heatChargeDone = false;
	}

	private void LoadActionBoxes(bool on){
		this.ActionBoxes.SetActive(on);
		if(on){
			CalculateNewComboLevel();
			UpdateActionBoxList();
			UpdateVisualizer();
			UpdateHeatBar();
		}
	}

	private bool IndexInBoundsOfList(int index, int listSize){

        return index < listSize && index >= 0;
    }

	private void printActionQueue(){
		Debug.Log("Printing Actions");
		foreach(Action A in Actions){
			Debug.Log(A.name);
		}
	}


#region Executing Actions
	protected async Task ExecuteAllActions(){
		this.stopQueue = false;
		Enemy TargetEnemy = Player.GetCurrentEnemy();
		TargetEnemy.SetHeldByCombo(true);

		if(Player.defendModeActive){
			await ExecuteBlocks(TargetEnemy);
		}else{
			await ExecuteAttacks(TargetEnemy);
		}

		if(TargetEnemy.deathTriggered){
			await TargetEnemy.Death();
		}else if(Player.deathTriggered) await Player.Death();
		else{
			await Task.Delay(this.afterComboDelay_ms);
			TargetEnemy.SetHeldByCombo(false);
			TargetEnemy.ResetShield();
		}
		ActionQueueEnd();
	}

	private async Task ExecuteAttacks(Enemy TargetEnemy){
		while(this.Actions.Count > 0 && !this.stopQueue) {
			Action CurrentAction = GetNextActionFromActionsList();

			if(CurrentAction.name == "Attack Rush"){
				// Debug.Log("Executing Attack Rush");

				CurrentAction.SetEnemy(TargetEnemy);
				int damageDealt = await CurrentAction.NormalExecute();
				continue;
			}

			// Debug.Log("Executing "+CurrentAction.name+" on "+ this.BattleSystem.Enemy.unitName);
				
			bool addHeatAfterAttack = await TargetEnemy.HandleAction(CurrentAction);

			UpdateVisualizer();

			if(addHeatAfterAttack){
				AddHeat();
			}
		}
	}	

	private async Task ExecuteBlocks(Enemy TargetEnemy){
		int executedActions = 0;

		while(executedActions < TargetEnemy.GetCurrentAttackSequenceLength() && !this.stopQueue) {
			Action CurrentAction = new NoBlock(Player);
			if(this.Actions.Count > 0){
				CurrentAction = GetNextActionFromActionsList();
			}
				
			await TargetEnemy.HandleAction(CurrentAction);

			UpdateVisualizer();

			executedActions++;
		}
	}

	protected virtual void ActionQueueEnd(){
		ClearActionQueue();
		UpdateActionBoxList();

		if(Player.state == PlayerState.QUEUE){
			Player.state = PlayerState.PLAYERTURN;
			Player.SwitchBattleModes();
		}
	} // Changed in: PlayerAction_Tutorial.cs

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
		this.Actions.RemoveAt(0);
		if(this.attackRushQueueIndex >= 0) this.attackRushQueueIndex--;
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

	public void ResetHeatLevel(){
		if(!Application.isPlaying) return;
		this.currentHeat = 0;
		this.comboLevel = 1;
		this.ComboLevelText.text = "Lv."+comboLevel.ToString();
		UpdateHeatBar();
		UpdateActionBoxList();
		UpdateVisualizer();
	}

	protected void UpdateHeatBar(){
		if(this.comboLevel < this.maxComboLv){
			this.HeatProgressSlider.value = this.currentHeat;
			this.HeatProgressSlider.maxValue = this.heatLimits[this.comboLevel];
		}else{
			// maxHeat level reached;
			this.HeatProgressSlider.maxValue = 1;
			this.HeatProgressSlider.value = 1;
		}
        
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
		this.currentMaxAttackLength = this.comboLevel;
	}
#endregion



#region Adding/Removing New Actions
	public virtual void AddAction(Action A){
		this.ActionText.text = A.name;

		this.Actions.Add(A);
		A.QueueAction(this);

		UpdateVisualizer();
		if(PositionsLeftInActionQueue() == 1){
			Player.LoadComboAbilityMenu();
		}else if(PositionsLeftInActionQueue() == 0){
			Player.LoadRoundOverMenu();
		}else{
			Player.LoadMainMenu();
		}
	} // Changed in: PlayerAction_Tutorial.cs

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

	protected void ClearActionQueue(){
		while(this.Actions.Count > 0){
			RemoveLastAction();
		}
	}

	protected int PositionsLeftInActionQueue(){
		if(this.attackRushQueueIndex >= 0){
			return 0;
		}

		return this.comboLevel - this.Actions.Count;
	}
#endregion


#region Checking For Combo Abilities
	public Action PredictComboAbilityWith(Action ActionToAdd){

        this.Actions.Add(ActionToAdd);
        CheckComboList();

        Action ResA;

        if(this.CurrentComboAction != null) ResA = this.CurrentComboAction.Copy();
        else ResA = ActionToAdd;

        this.Actions.RemoveAt(this.Actions.Count-1);
        this.CurrentComboAction = null;
        return ResA;
    }

	private void CheckComboList(){
		string comboListAsString = ActionListToString();

		do {
			for(int comboActionIndex = this.AvailableComboActionsAsStrings.Count-1; comboActionIndex >= 0; comboActionIndex--){
				if(comboStringMatchFound(comboListAsString, comboActionIndex)){
					this.CurrentComboAction = GetComboAction(comboListAsString);
					return;
				}
			}
			comboListAsString = comboListAsString.Substring(1);
		}while(comboListAsString.Length > 1);
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

	private bool comboStringMatchFound(string comboListAsString, int comboActionIndex){

		return comboListAsString == this.AvailableComboActionsAsStrings[comboActionIndex];
	}

	private Action GetComboAction(string comboString){
		Weapon PlayerWeapon = Player.GetWeapon();
		Action FoundComboAction = PlayerWeapon.CombineActions(comboString, this.Actions);
		return FoundComboAction;
	}
#endregion



#region Heat Charge/Discharge Functions
	public async void StartHeatChargeLoop(){
		if(this.heatChargeDone) return;

		this.inHeatCharge = true;
		int currentCharge_ms = 0;

		while (currentCharge_ms < this.preChargeTime_ms && this.inHeatCharge){
			currentCharge_ms += TICK_TIME_MS;
			await Task.Delay(TICK_TIME_MS);
		}
		if(this.inHeatCharge) HeatChargeLoop();
	}

	public void StopHeatChargeLoop(){
		this.inHeatCharge = false;
	}

	private async void HeatChargeLoop(){
		int chargeTime_ms = 0;

		await Task.Delay(TICK_TIME_MS);
		while(this.inHeatCharge){
			chargeTime_ms += TICK_TIME_MS;
			if(chargeTime_ms >= this.chargeIncreaseInterval_ms){
				int heat;
				if(this.comboLevel == 1) heat = 1;
				else heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel]/2);

				AddHeat(heat);
				UpdateActionBoxList();
				UpdateVisualizer();
				chargeTime_ms -= this.chargeIncreaseInterval_ms;
			}
			await Task.Delay(TICK_TIME_MS);
		}

		this.heatChargeDone = true;
		Player.LoadMainMenu();
	}

	public async void StartHeatDischargeLoop(){
		this.inHeatDischarge = true;
		int currentCharge_ms = 0;

		while (currentCharge_ms < this.preChargeTime_ms && this.inHeatDischarge){
			currentCharge_ms += TICK_TIME_MS;
			await Task.Delay(TICK_TIME_MS);
		}
		if(this.inHeatDischarge) HeatDischargeLoop();
	}

	public void StopHeatDischargeLoop(){
		this.inHeatDischarge = false;
	}

	private async void HeatDischargeLoop(){
		int chargeTime_ms = 0;

		await Task.Delay(TICK_TIME_MS);
		while(this.inHeatDischarge){
			chargeTime_ms += TICK_TIME_MS;
			if(chargeTime_ms >= this.chargeIncreaseInterval_ms){
				int heat;
				if(this.comboLevel <= 1) heat = 0;
				else if(this.currentHeat <= 0) heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel-1]/2);
				else heat = (int)Mathf.Ceil((float)this.heatLimits[this.comboLevel]/2);

				LoseHeat(heat);
				UpdateActionBoxList();
				UpdateVisualizer();
				chargeTime_ms -= this.chargeIncreaseInterval_ms;
			}
			await Task.Delay(TICK_TIME_MS);
		}
	}
#endregion



#region Attack Rush Functions
	public async void StartAttackRushQueue(){
        if(this.attackRushUsed || this.comboLevel < 5) return;

        this.inAttackRushPreLoop = true;
        int waitBeforeAttackRushQueued = 1000;
        int currentWait = 0;

        while (currentWait < waitBeforeAttackRushQueued && this.inAttackRushPreLoop){
            currentWait += TICK_TIME_MS;
            await Task.Delay(TICK_TIME_MS);
        }

        if(this.inAttackRushPreLoop) QueueAttackRush();
        this.inAttackRushPreLoop = false;
    }

    public void StopAttackRushQueue(){
        this.inAttackRushPreLoop = false;
    }

	private void QueueAttackRush(){
		if(this.attackRushQueueIndex >= 0) return;
		AttackRushAction A = new AttackRushAction(Player); 
		RemoveLastAction();
		this.attackRushQueueIndex = this.Actions.Count;
		this.attackRushLevel = this.comboLevel - this.Actions.Count;
		AddAction(A);
	}

	public void CancelAttackRush(){
		this.attackRushActive = false;
	}

	public async void StartAttackRushHeatDrain(){
		this.attackRushUsed = true;
		int totalHeat = GetTotalHeatForHeatDrain();
		int stopHeat = GetStopHeat(this.attackRushLevel);
		int totalDrain = totalHeat - stopHeat;

		LoadActionBoxes(false);
		LoadHeatDrainBar(true, totalDrain);
		float origWidth = this.AttackRushDecreaseBox.GetComponent<RectTransform>().rect.width;

		float drainPerTick = 2.5f;
		float currentDrain = 0.0f;
		this.attackRushActive = true;

		await Task.Delay(TICK_TIME_MS);
		while(totalHeat > stopHeat && Application.isPlaying && Player.state == PlayerState.ATTACKRUSH){
			if(!this.attackRushActive) break;
			currentDrain += drainPerTick;
			if(currentDrain >= 1.0f){
				totalHeat = (int)Mathf.Max(0.0f, (float)(totalHeat - (int)currentDrain));
				currentDrain -= (int)currentDrain;

				float percDrained = (float)(totalHeat - stopHeat)/(float)totalDrain;
				float newWidth = origWidth * percDrained;
				this.AttackRushDecreaseBox.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, this.standartActionBoxHeight);
			}
			await Task.Delay(TICK_TIME_MS);
		}

		int finisherLevel = 0;
		if(this.attackRushActive){
			this.attackRushActive = false;
			if(Player.GetCurrentEnemy().GetAttackRushDamage() > 0){
				finisherLevel = (totalDrain - totalHeat + stopHeat)/100;
			}
		}else{
			totalHeat = stopHeat;
		}
		SetComboLevelAfterAttackRush(totalHeat);
		StopAttackRush(finisherLevel);
	}

	private async void StopAttackRush(int finisherLevel){
		Player.EnableAttackRushInputs(false);
		this.AttackRushDecreaseBox.SetActive(false);

		if(finisherLevel <= 0){
			await Task.Delay(500);
		}else{
			Action rushFinisher = Player.GetWeapon().GetAttackRushFinisher(finisherLevel);
			await Player.GetCurrentEnemy().HandleAttackRushFinisher(rushFinisher);
		}
		
		this.attackRushLevel = -1;
		LoadHeatDrainBar(false);
		LoadActionBoxes(true);
		Player.state = PlayerState.QUEUE;
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

	private float GetRushAttackWidth(int boxesCovered, int drainOver = 0){
		float res = this.standartActionBoxWidth;
		if(boxesCovered < 1) return res;

		res = this.standartActionBoxWidth * boxesCovered + 10 * (boxesCovered-1);
		if(drainOver > 0) res += ( (float)drainOver/100f ) * this.standartActionBoxWidth;
		return res;
	}
#endregion

}
