using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public enum Debuff {NONE, BURN, BLAZE, WSICK, FREEZE, PARA, ATK, DEF, SPD, SILENCE, DISABLE, SEAL}

public enum Buff {NONE, ATK, DEF, SPD, CRT}

public class Debuffs : MonoBehaviour
{
	public List<Debuff> ActiveDebuffs = new List<Debuff>();
	[SerializeField] private List<int> DebuffCounts = new List<int>();

	public List<Buff> ActiveBuffs = new List<Buff>();
	[SerializeField] private List<int> BuffCounts = new List<int>();

	public int lightCounter = 0;

	private Unit ThisUnit;

	private ActionQueue ActionQueue;

	private BattleSystem BattleSystem;

	[Header("Debuff Stats")]

	[SerializeField] private int burnDotDamage = 45;
	[SerializeField] private int blazeDotDamage = 90;
	[SerializeField] private int wsickApSteal = 5;

	[SerializeField] private int atkDebuffTier = 0;
	[SerializeField] private int defDebuffTier = 0;
	[SerializeField] private int spdDebuffTier = 0;

	void Awake(){
		ThisUnit = this.gameObject.GetComponent<Unit>();
		BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
		ActionQueue = GameObject.Find("ActionQueue").GetComponent<ActionQueue>();
	}

#region Public functions

	public void ActivateAllDebuffs() {

		if(ActiveDebuffs.Count != DebuffCounts.Count) {
			Debug.Log("ActiveDebuffs.Count != DebuffCounts.Count");
			return;
		}

		ResetStats();

		if(ActiveDebuffs.Count == 0) return;

		for(var i = 0; i < DebuffCounts.Count; i++) {
			DebuffCounts[i]--;
			AddDebuffToQueue(ActiveDebuffs[i]);
		}
	}

	public void ActivateDebuff(Debuff D) {

		switch (D){
			case Debuff.BURN:
				ThisUnit.currentHp -= burnDotDamage;
				ActionQueue.currentText += "\nDealt "+burnDotDamage+" damage!";
			break;

			case Debuff.BLAZE:
				ThisUnit.currentHp -= blazeDotDamage;
				ActionQueue.currentText += "\nDealt "+blazeDotDamage+" damage!";
			break;

			case Debuff.WSICK:
				ThisUnit.currentAp -= wsickApSteal;
				if(ThisUnit.currentAp < 0) ThisUnit.currentAp = 0;
				ActionQueue.currentText = ThisUnit.unitName+" lost "+wsickApSteal+"AP due to Watersickness!";
			break;

			case Debuff.FREEZE:
				ActionQueue.currentText = ThisUnit.unitName+" is Frozen and can't act!";
				//BattleSystem.SkipRound();
			break;

			case Debuff.ATK:
				if(atkDebuffTier > 3) atkDebuffTier = 3;

				if(ThisUnit.isEnemy){
					int deltaAtk = Percentual(ThisUnit.currentTotalAtk, 25*atkDebuffTier);
					ThisUnit.currentTotalAtk -= deltaAtk;
				}

				else{
					int deltaMAtk = Percentual(ThisUnit.currentMAtk, 25*atkDebuffTier);
					int deltaRAtk = Percentual(ThisUnit.currentRAtk, 25*atkDebuffTier);
					int deltaTotalAtk = Percentual(ThisUnit.currentTotalAtk, 25*atkDebuffTier);

					ThisUnit.currentTotalAtk -= deltaTotalAtk;
					ThisUnit.currentMAtk -= deltaMAtk;
					ThisUnit.currentRAtk -= deltaRAtk;
				}

			break;

			case Debuff.DEF:
				if(defDebuffTier > 3) defDebuffTier = 3;

				int deltaDef = Percentual(ThisUnit.currentDef, 25*defDebuffTier);

				ThisUnit.currentDef -= deltaDef;

			break;

			case Debuff.SPD:
				if(spdDebuffTier > 3) spdDebuffTier = 3;

				int deltaSpd = Percentual(ThisUnit.currentSpd, 10*spdDebuffTier);
				ThisUnit.currentSpd -= deltaSpd;
			break;

			case Debuff.SEAL:						// goes indefintely until removed by spell
				int i = ActiveDebuffs.IndexOf(D);
				DebuffCounts[i]++;
			break;

			default:
			break;
		}
	} 

	public void AddDebuff(Debuff D, int index = -1, int count = -1) {

		if(count < 0) count = GetStandartCount(Buff.NONE, D);

		switch(D){
			case Debuff.BURN:

				if(ActiveDebuffs.Contains(Debuff.BLAZE)){
					DebuffCounts[ActiveDebuffs.IndexOf(Debuff.BLAZE)] += 2;
				}

				else if(ActiveDebuffs.Contains(D)) break;

				else{
					if(index < 0){
						ActiveDebuffs.Add(D);
						DebuffCounts.Add(count);
					}
					else{
						ActiveDebuffs.Insert(index, D);
						DebuffCounts.Insert(index, count);
					}
				}

			break;

			case Debuff.BLAZE:
				
				if(ActiveDebuffs.Contains(Debuff.BURN)) RemoveDebuff(Debuff.BURN);
				
				else if(ActiveDebuffs.Contains(D)) DebuffCounts[ActiveDebuffs.IndexOf(D)] += 4;
				
				else{
					if(index < 0){
						ActiveDebuffs.Add(D);
						DebuffCounts.Add(count);
					}
					else{
						ActiveDebuffs.Insert(index, D);
						DebuffCounts.Insert(index, count);
					}
				}

			break;

			case Debuff.ATK:
				if(ActiveDebuffs.Contains(D)) {
					atkDebuffTier++;
					DebuffCounts[ActiveDebuffs.IndexOf(D)]++;
				}
				else {
					if(!ActiveDebuffs.Contains(D)) {
						if(index < 0){
							ActiveDebuffs.Add(D);
							DebuffCounts.Add(count);
						}
						else{
							ActiveDebuffs.Insert(index, D);
							DebuffCounts.Insert(index, count);
						}
					}

					else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
				}
			break;

			case Debuff.DEF:
				if(ActiveDebuffs.Contains(D)) {
					defDebuffTier++;
					DebuffCounts[ActiveDebuffs.IndexOf(D)]++;
				}
				else {
					if(!ActiveDebuffs.Contains(D)) {
						if(index < 0){
							ActiveDebuffs.Add(D);
							DebuffCounts.Add(count);
						}
						else{
							ActiveDebuffs.Insert(index, D);
							DebuffCounts.Insert(index, count);
						}
					}

					else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
				}
			break;

			case Debuff.SPD:
				if(ActiveDebuffs.Contains(D)) {
					spdDebuffTier++;
					DebuffCounts[ActiveDebuffs.IndexOf(D)]++;
				}
				else {
					if(!ActiveDebuffs.Contains(D)) {
						if(index < 0){
							ActiveDebuffs.Add(D);
							DebuffCounts.Add(count);
						}
						else{
							ActiveDebuffs.Insert(index, D);
							DebuffCounts.Insert(index, count);
						}
					}

					else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
				}
			break;

			case Debuff.SILENCE:
				if(ActiveDebuffs.Contains(Debuff.DISABLE)){
					ActionQueue.currentText += "Caused "+ThisUnit.unitName+" to be Sealed!";
					AddDebuff(Debuff.SEAL);
				}

				else if(ActiveDebuffs.Contains(Debuff.SEAL)) return;

				else if(!ActiveDebuffs.Contains(D)) {
					if(index < 0){
						ActiveDebuffs.Add(D);
						DebuffCounts.Add(count);
					}
					else{
						ActiveDebuffs.Insert(index, D);
						DebuffCounts.Insert(index, count);
					}
				}

				else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
			break;

			case Debuff.DISABLE:
				if(ActiveDebuffs.Contains(Debuff.SILENCE)){
					ActionQueue.currentText += "Caused "+ThisUnit.unitName+" to be Sealed!";
					AddDebuff(Debuff.SEAL);
				}

				else if(ActiveDebuffs.Contains(Debuff.SEAL)) return;

				else if(!ActiveDebuffs.Contains(D)) {
					if(index < 0){
						ActiveDebuffs.Add(D);
						DebuffCounts.Add(count);
					}
					else{
						ActiveDebuffs.Insert(index, D);
						DebuffCounts.Insert(index, count);
					}
				}

				else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
			break;

			case Debuff.SEAL:
				if(!ActiveDebuffs.Contains(D)){
					if(ActiveDebuffs.Contains(Debuff.SILENCE)) RemoveDebuff(Debuff.SILENCE);

					if(ActiveDebuffs.Contains(Debuff.DISABLE)) RemoveDebuff(Debuff.DISABLE);
				}
			break;

			default:
				if(!ActiveDebuffs.Contains(D)) {
					if(index < 0){
						ActiveDebuffs.Add(D);
						DebuffCounts.Add(count);
					}
					else{
						ActiveDebuffs.Insert(index, D);
						DebuffCounts.Insert(index, count);
					}
				}

				else DebuffCounts[ActiveDebuffs.IndexOf(D)] = count;
			break;
		}
	}

	public void ChangeDebuff(string from, string to) {
		Debuff F = GetDebuffFromString(from);
		Debuff T = GetDebuffFromString(to);
		int index = ActiveDebuffs.IndexOf(F);

		RemoveDebuff(F);
		AddDebuff(T, index);
	}

	public void RemoveDebuff(Debuff D, string name = null) {
		if(D == Debuff.NONE) D = GetDebuffFromString(name);

		if(!ActiveDebuffs.Contains(D)) return;

		int index = ActiveDebuffs.IndexOf(D);

		ActiveDebuffs.RemoveAt(index);
		DebuffCounts.RemoveAt(index);
	}

	public void AddBuff(Buff B, int index = -1, int count = -1) {

		if(count < 0) count = GetStandartCount(B);

		if(!ActiveBuffs.Contains(B)) {
			if(index < 0){
				ActiveBuffs.Add(B);
				BuffCounts.Add(count);
			}
			else{
				ActiveBuffs.Insert(index, B);
				BuffCounts.Insert(index, count);
			}
		}

		else BuffCounts[ActiveBuffs.IndexOf(B)] = count;
	}

	public void RemoveBuff(Buff B) {

		if(!ActiveBuffs.Contains(B)) return;

		int index = ActiveBuffs.IndexOf(B);

		ActiveBuffs.RemoveAt(index);
		BuffCounts.RemoveAt(index);
	}

#endregion

#region Private functions

	private Debuff GetDebuffFromString(string s) {
		Debuff D = Debuff.NONE;

		switch(s){
			case "Burn":
				D = Debuff.BURN;
			break;

			case "Blaze":
				D = Debuff.BLAZE;
			break;

			case "Watersickness":
				D = Debuff.WSICK;
			break;

			case "Freeze":
				D = Debuff.FREEZE;
			break;

			case "Atk Debuff":
				D = Debuff.ATK;
			break;

			case "Def Debuff":
				D = Debuff.DEF;
			break;

			case "Spd Debuff":
				D = Debuff.SPD;
			break;

			case "Disabled":
				D = Debuff.DISABLE;
			break;

			case "Silenced":
				D = Debuff.SILENCE;
			break;

			case "Sealed":
				D = Debuff.SEAL;
			break;

			default:
				Debug.Log("Debuff not found: "+ s);
			break;
		}

		return D;
	}

	private string GetStringFromDebuff(Debuff D) {
		string s = "";

		switch(D){
			case Debuff.BURN:
				s = "Burn";
			break;

			case Debuff.BLAZE:
				s = "Blaze";
			break;

			case Debuff.WSICK:
				s = "Watersickness";
			break;

			case Debuff.FREEZE:
				s = "Freeze";
			break;

			case Debuff.ATK:
				s = "Atk Debuff";
			break;

			case Debuff.DEF:
				s = "Def Debuff";
			break;

			case Debuff.SPD:
				s = "Spd Debuff";
			break;

			case Debuff.DISABLE:
				s = "Disabled";
			break;

			case Debuff.SILENCE:
				s = "Silenced";
			break;

			case Debuff.SEAL:
				s = "Sealed";
			break;

			default:
				Debug.Log("Debuff not found: "+D);
			break;
		}

		return s;
	}

	private void AddDebuffToQueue(Debuff D) {
		int index = ActiveDebuffs.IndexOf(D);

		Action A;

		if(DebuffCounts[index]<=0) {
			AddDebuffRemovalToQueue(D);
		}

		else {
			switch(D){
				case Debuff.BURN:

					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);

					if(DebuffCounts[index]-1 <= 0) AddDebuffChangeToQueue("Burn", "Blaze");

				break;

				case Debuff.BLAZE:

					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);

				break;

				case Debuff.WSICK:
					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);
				break;

				case Debuff.FREEZE:
					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);
				break;

				case Debuff.DISABLE:
					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);
				break;

				case Debuff.SILENCE:
					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);
				break;

				case Debuff.SEAL:
					A = new Action();
					A.identifier = "Debuff";
					A.PrimaryUnit = ThisUnit;
					A.debuffName = GetStringFromDebuff(D);
					A.Debuff = D;

					ActionQueue.AddNewAction(A);
				break;

				default:
					ActivateDebuff(D);
				break;
			}
		}
	}

	private void AddDebuffChangeToQueue(string from, string to) {
		Action A  = new Action();
		A.PrimaryUnit = ThisUnit;
		A.identifier = "DebuffChange";
		A.debuffName = from;
		A.debuffNameSec = to;

		ActionQueue.AddNewAction(A);
	}

	private void AddDebuffRemovalToQueue(Debuff D) {
		string name = GetStringFromDebuff(D);
		Action A = new Action();
		A.identifier = "DebuffRemoval";
		A.PrimaryUnit = ThisUnit;
		A.debuffName = name;

		ActionQueue.AddNewAction(A);
	}

	private int GetStandartCount(Buff B = Buff.NONE, Debuff D = Debuff.NONE) {
		int val = 0;
		switch(B){
			case Buff.NONE:
				val = -1;
			break;

			default:
				val = 3;
			break;
		}

		if(val < 0){
			switch(D){
				case Debuff.BURN:
					val = 3;
				break;

				case Debuff.BLAZE:
					val = 1;
				break;

				case Debuff.WSICK:
					val = 3;
				break;

				case Debuff.FREEZE:
					val = 2;
				break;

				case Debuff.DISABLE:
					val = 5;
				break;

				case Debuff.SILENCE:
					val = 5;
				break;


				default:
					val = 3;
				break;
			}
		}

		return val;
	}

	private void ResetStats(){
		ThisUnit.currentMAtk = ThisUnit.baseMAtk;
		ThisUnit.currentRAtk = ThisUnit.baseRAtk;
		ThisUnit.currentDef = ThisUnit.baseDef;
		ThisUnit.currentSpd = ThisUnit.baseSpd;
		ThisUnit.currentCrt = ThisUnit.baseCrt;
	}

	private int Percentual(int _whole, int _perc){
		float part = (float)_whole;
		part = part/100;

		float res = part*(float)_perc;

		return (int)res;
	}

#endregion

#region public StatusCheck functions 

	public bool CheckStatusBurn() {

		if(CheckStatusBlaze()) return true;

		if(ActiveDebuffs.Contains(Debuff.BURN)) return true;

		return false;
	}

	public bool CheckStatusBlaze() {
		
		if(ActiveDebuffs.Contains(Debuff.BLAZE)) return true;

		return false;
	}

	public bool CheckStatusWSick() {

		if(ActiveDebuffs.Contains(Debuff.WSICK)) return true;

		return false;
	}

	public bool CheckStatusFreeze() {
		
		if(ActiveDebuffs.Contains(Debuff.FREEZE)) return true;

		return false;
	}

	public bool CheckStatusPara() {

		if(ActiveDebuffs.Contains(Debuff.PARA)) return true;

		return false;
	}

	public bool CheckStatusNoEle() {

		if(CheckStatusBurn()) return false;

		if(CheckStatusFreeze()) return false;

		if(CheckStatusWSick()) return false;

		if(CheckStatusPara()) return false;

		return true;
	}

	public bool CheckStatusSilence(){
		if(ActiveDebuffs.Contains(Debuff.SILENCE) || ActiveDebuffs.Contains(Debuff.SEAL)) return true;

		else return false;
	}

	public bool CheckStatusDisable(){
		if(ActiveDebuffs.Contains(Debuff.DISABLE) || ActiveDebuffs.Contains(Debuff.SEAL)) return true;

		else return false;
	}

	public bool CheckStatusSeal(){
		if(ActiveDebuffs.Contains(Debuff.SEAL)) return true;

		else return false;
	}

#endregion
}*/
