using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCaster : MonoBehaviour
{
	private Unit ThisUnit;

	[SerializeField] private BattleHUD BattleHUD;

	[SerializeField] private BattleSystem BattleSystem;

	/*void Awake(){
		ThisUnit = this.GetComponent<Unit>();
		BattleHUD = GameObject.Find("BattleHUD").GetComponent<BattleHUD>();
		BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
	}

	public void ExecuteRound(int seed){
		switch (seed) {
			case 0:
				//StartCoroutine(Attack0());
				BattleSystem.PassRound();
			break;
		}
	}

	private IEnumerator Attack0(){
		yield return new WaitForSeconds(0.4f);
		
		
		
		Debug.Log(ThisUnit.unitName + " passed the round!");
		BattleSystem.PassRound();
	}*/
}
