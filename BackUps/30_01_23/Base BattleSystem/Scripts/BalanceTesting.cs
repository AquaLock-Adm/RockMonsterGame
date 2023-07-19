using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceTesting : MonoBehaviour
{/*
	public BattleSystem BattleSystem;
	public Unit Caster;
	public Unit Target;

	public int[] givenTotalAtk = {500, 1000, 1500};
	public float[] MAtkPerc = {0.3f, 0.5f, 0.7f};
	public float[] RAtkPerc = {0.3f, 0.5f, 0.7f};

	public ActiveSpell Spell;
	public SpellSuffix Suffix;
	
	public bool checkAllLevels = false;

	public bool checkMaxOnly = false;

	public List<int> TestResults = new List<int>();

	private int origTAtk;
	private int origMAtk;
	private int origRAtk;

    // Start is called before the first frame update
    void Start()
    {
    	/*Caster = BattleSystem.PlayerUnit;
    	origTAtk = Caster.currentTotalAtk;
    	origMAtk = Caster.currentMAtk;
    	origRAtk = Caster.currentRAtk;

    	Target = BattleSystem.EnemyUnit;

    	if(Spell!=ActiveSpell.NONE) {
    		if(checkAllLevels) StartTestAll();
    		else StartTestSingle();
    	}
    }

    private void StartTestAll(){
    	
    	/*Debug.Log("Read Results: TotalAtk|MAtk|RAtk");
    	
    	Debug.Log("Result Order: [Low|Low|Low|], [Low|Medium|Low, Low|High|Low], [Low|Low|Medium, Low|Low|High]");
    	Debug.Log("Result Order: [Medium|Low|Low|], [Medium|Medium|Low, Medium|High|Low], [Medium|Low|Medium, Medium|Low|High]");
    	Debug.Log("Result Order: [High|Low|Low|], [High|Medium|Low, High|High|Low], [High|Low|Medium, High|Low|High]");
    	*/

    	/*Debug.Log("Testing: "+Spell+"o");
    	TestingO();

    	Debug.Log("Testing: "+Spell+"a");
    	TestingA();

    	Debug.Log("Testing: "+Spell+"ro");
    	TestingRO();

    	Debug.Log("Testing: "+Spell+"ha");
    	TestingHA();

    	Debug.Log("Testing: "+Spell+"mana");
    	TestingMANA();

    	Debug.Log("Done...");

    }

    private void StartTestSingle(){
    	/*Debug.Log("Read Results: TotalAtk|MAtk|RAtk");
    	
    	Debug.Log("Result Order: [Low|Low|Low|], [Low|Medium|Low, Low|High|Low], [Low|Low|Medium, Low|Low|High]");
    	Debug.Log("Result Order: [Medium|Low|Low|], [Medium|Medium|Low, Medium|High|Low], [Medium|Low|Medium, Medium|Low|High]");
    	Debug.Log("Result Order: [High|Low|Low|], [High|Medium|Low, High|High|Low], [High|Low|Medium, High|Low|High]");*/
    	/*

    	switch(Suffix){
    		case SpellSuffix.O:
    			Debug.Log("Testing: "+Spell+"o");
    			TestingO();
    		break;

    		case SpellSuffix.A:
    			Debug.Log("Testing: "+Spell+"a");
    			TestingA();
    		break;

    		case SpellSuffix.RO:
    			Debug.Log("Testing: "+Spell+"ro");
    			TestingRO();
    		break;

    		case SpellSuffix.HA:
    			Debug.Log("Testing: "+Spell+"ha");
    			TestingHA();
    		break;

    		case SpellSuffix.MANA:
    			Debug.Log("Testing: "+Spell+"mana");
    			TestingMANA();
    		break;

    		default:
    			Debug.Log("No Spell Suffix given!");
    		break;
    	}
    }

    private void TestingO(){
    	int max = 0;
    	int min = 10000000;

    	for(var i = 0; i < 3; i++){
    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
    	
	    	Caster.currentTotalAtk = givenTotalAtk[i];
	    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
	    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

	    	Caster.GetComponent<Spells>().Target = Target;

	    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.O);

	    	if(checkMaxOnly) {
	    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
	    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
	    	}

	    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[j])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[j]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.O);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[j]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[j]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.O);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}	
    	}
    	
    	if(checkMaxOnly){
	    	TestResults.Add(min);
	    	TestResults.Add(max);
	    }
    }

    private void TestingA(){
    	int max = 0;
    	int min = 10000000;

    	for(var i = 0; i < 3; i++){
    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
    	
	    	Caster.currentTotalAtk = givenTotalAtk[i];
	    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
	    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

	    	Caster.GetComponent<Spells>().Target = Target;

	    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.A);

	    	if(checkMaxOnly) {
	    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
	    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
	    	}

	    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[j])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[j]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.A);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[j]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[j]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.A);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}
    	}

    	if(checkMaxOnly){
	    	TestResults.Add(min);
	    	TestResults.Add(max);
	    }
    }

    private void TestingRO(){
    	int max = 0;
    	int min = 10000000;

    	for(var i = 0; i < 3; i++){
    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
    	
	    	Caster.currentTotalAtk = givenTotalAtk[i];
	    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
	    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

	    	Caster.GetComponent<Spells>().Target = Target;

	    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.RO);

	    	if(checkMaxOnly) {
	    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
	    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
	    	}

	    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[j])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[j]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.RO);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
			    	
		    	}

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[j]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[j]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.RO);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}
    	}

    	if(checkMaxOnly){
	    	TestResults.Add(min);
	    	TestResults.Add(max);
	    }
    }

    private void TestingHA(){

    	int max = 0;
    	int min = 10000000;

    	for(var i = 0; i < 3; i++){
    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
    	
	    	Caster.currentTotalAtk = givenTotalAtk[i];
	    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
	    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

	    	Caster.GetComponent<Spells>().Target = Target;

	    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.HA);

	    	if(checkMaxOnly) {
	    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
	    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
	    	}

	    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[j])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[j]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.HA);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[j]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[j]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.HA);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
		    	
	    	}
    	}

    	if(checkMaxOnly){
	    	TestResults.Add(min);
	    	TestResults.Add(max);
	    }
    }

    private void TestingMANA(){

    	int max = 0;
    	int min = 10000000;

    	for(var i = 0; i < 3; i++){
    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
    	
	    	Caster.currentTotalAtk = givenTotalAtk[i];
	    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
	    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

	    	Caster.GetComponent<Spells>().Target = Target;

	    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.MANA);

	    	if(checkMaxOnly) {
	    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
	    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
	    	}

	    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[j])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[0]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[j]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[0]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.MANA);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}

	    	for(var j = 1; j < 3; j++){
	    		Debug.Log(givenTotalAtk[i]+"|"+(int)(givenTotalAtk[i]*MAtkPerc[0])+"|"+(int)(givenTotalAtk[i]*RAtkPerc[j]));
		    	Caster.currentTotalAtk = givenTotalAtk[i];
		    	Caster.currentMAtk = (int)(givenTotalAtk[i]*MAtkPerc[0]);
		    	Caster.currentRAtk = (int)(givenTotalAtk[i]*RAtkPerc[j]);

		    	Caster.GetComponent<Spells>().Target = Target;

		    	Caster.GetComponent<Spells>().ExecuteSpell(Spell, SpellSuffix.MANA);

		    	if(checkMaxOnly) {
		    		if(Caster.GetComponent<Spells>().testDamage < min) min = Caster.GetComponent<Spells>().testDamage;
		    		if(Caster.GetComponent<Spells>().testDamage > max) max = Caster.GetComponent<Spells>().testDamage;
		    	}

		    	else TestResults.Add(Caster.GetComponent<Spells>().testDamage);
	    	}
    	}

    	if(checkMaxOnly){
	    	TestResults.Add(min);
	    	TestResults.Add(max);
	    }
    }*/
}
