using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponType {SWORD, SCYTHE, AXE, DAGGER, NONE} // bsvg1

// wc0
public abstract class Weapon : MonoBehaviour
{   
    // wvg1
    public string weaponName;

    public WeaponType Type;

    // wvg2
    public PlayerCharacter User;

    public List<Action> AbilityList;

    // wvg3
    public int baseAttackMax;
    public int baseAttackMin;
    public int attackMax;
    public int attackMin;
    // wvg3

    // wvg4
    public int lightAttackCost;
    public int heavyAttackCost;
    public int specialAttackCost;
    // wvg4

    // wvg5
    public int crit;
    // wvg5

    // wvg6
    public float bonusCritDamagef;
    public float bonusCritDamagefbase;
    // wvg6

    // wvg7
    public int maxDurability;
    public int durability;
    // wvg7

    // wvg8
    public string weaponStatus;

    [Header("Proficiency Acc:")]
    // wvg9
    public int level0AccuracyBase = 50; // these values are for lvl 0 proficiency
    public int currentProficiency = 0;
    // wvg9


    // wvg10
    [SerializeField] private bool stageOneBroken = false;
    [SerializeField] private bool stageTwoBroken = false;
    [SerializeField] private bool stageThreeBroken = false;
    [SerializeField] private Text WeaponText;
    // wvg10

    // wf1
    public abstract void InitAbilityList();

    public abstract List<Action> GetCompleteMoveList();
    // wf2
    public abstract Action CombineActions(string comboList, List<Action> Action_l);
    // wf3
    public abstract List<Action> GetMovesToTest();
    // wf4
    public abstract void SetWeaponType();
    
    // wf5
    void Awake(){
        this.WeaponText = GameObject.Find("Weapon Status Text").GetComponent<Text>();
    }
    // wf6
    public void Setup(PlayerCharacter PC){
        this.User = PC;
        PC.Weapon = this;
        this.currentProficiency = this.User.proficiencyLevel;
        //SetAttackCost();

        SetAttackDamage();
        SetWeaponType();
        //TestComboAccuracy();
    }
    // wf7
    private void SetAttackDamage(){

        this.attackMax = this.baseAttackMax;
        this.attackMin = this.baseAttackMin;

        this.User.attackMin = this.attackMin;
        this.User.attackMax = this.attackMax;
        this.User.crit = this.crit;
    }
    // wf8
    public int GetComboAccuracy(int cLvl){
        if(this.User == null) Debug.LogError("User references not set yet, use Weapon.Setup() before");
        if(this.currentProficiency != this.User.proficiencyLevel) {
            this.currentProficiency = this.User.proficiencyLevel;
            Debug.Log("Proficiency was set incorrectly. Setting it now to "+this.currentProficiency);
        }
        if(cLvl > 5 || cLvl < 1) {
            Debug.LogError("ERROR!!! ComboLevel out of Bounds(1-5)");
            return 0;
        }
        if(cLvl == 1) {
            // Debug.Log("PlayerProf: "+this.User.proficiencyLevel.ToString()+"\nSet Prof: "+this.currentProficiency.ToString()+"\nCalc Acc for ComboLvl."+cLvl.ToString()+": 95%");
            return 95;
        }
        if(this.Type == WeaponType.NONE) return 95;

        int baseAccuracy = this.level0AccuracyBase - (10*(cLvl-1))-1;
        int levelOfMaxAccuracy = (int)Mathf.Clamp((cLvl-1)*30, 0.0f, float.MaxValue);

        float accPerLvl = (95.0f-(float)baseAccuracy)/(float)levelOfMaxAccuracy;
        //Debug.Log("C_lvl "+cLvl.ToString()+" / "+accPerLvl.ToString());

        int res = (int)Mathf.Clamp(Mathf.Round(baseAccuracy+(this.currentProficiency*accPerLvl)), 0.0f, 95.0f);

        // Debug.Log("PlayerProf: "+this.User.proficiencyLevel.ToString()+"\nSet Prof: "+this.currentProficiency.ToString()+"\nCalc Acc for ComboLvl."+cLvl.ToString()+": "+res.ToString()+"%");

        return res;
    }
    // wf9
    public void DamageToDurability(int dmg){
        if(this.durability <= this.maxDurability/2 && this.durability > this.maxDurability/4){

            if(!this.stageOneBroken){
                this.attackMin = (int)Mathf.Round(this.baseAttackMin*0.8f);
                this.attackMax = (int)Mathf.Round(this.baseAttackMax*0.8f);

                this.User.attackMax = this.attackMax;
                this.User.attackMin = this.attackMin;  

                this.stageOneBroken = true;
                this.weaponStatus = "Wpn: x";
                this.WeaponText.text = this.weaponStatus;
            }

            this.durability -= dmg/2;
        }

        else if(this.durability <= this.maxDurability/4 && this.durability > 0){
            if(!this.stageTwoBroken){
                this.attackMin = (int)Mathf.Round(this.baseAttackMin*0.6f);
                this.attackMax = (int)Mathf.Round(this.baseAttackMax*0.6f);

                this.User.attackMax = this.attackMax;
                this.User.attackMin = this.attackMin;  

                this.stageTwoBroken = true;
                this.weaponStatus = "Wpn: xx";
                this.WeaponText.text = this.weaponStatus;
            }

            this.durability -= dmg/3;
        }

        else if(this.durability <= 0){
            if(!this.stageThreeBroken){
                this.attackMin = (int)Mathf.Round(this.baseAttackMin*0.1f);
                this.attackMax = (int)Mathf.Round(this.baseAttackMin*0.1f);

                this.User.attackMax = this.attackMax;
                this.User.attackMin = this.attackMin;  

                this.stageThreeBroken = true;

                this.weaponStatus = "Wpn: xxx";
                this.WeaponText.text = this.weaponStatus;
            }
        }

        else {
            this.weaponStatus = "Wpn: o";
            this.WeaponText.text = this.weaponStatus;
            this.durability -= dmg;
        }
    }
    // wf10
    public void Repair(int val){
        this.durability += val;

        if(this.durability > 0) {
            this.attackMin = (int)Mathf.Round(this.baseAttackMin*0.6f);
            this.attackMax = (int)Mathf.Round(this.baseAttackMax*0.6f);
            
            this.stageThreeBroken = false;
            this.weaponStatus = "Wpn: xx";
            this.WeaponText.text = this.weaponStatus;
        }

        if(this.durability > this.maxDurability/4) {
            this.attackMin = (int)Mathf.Round(this.baseAttackMin*0.8f);
            this.attackMax = (int)Mathf.Round(this.baseAttackMax*0.8f); 

            this.stageTwoBroken = false;
            this.weaponStatus = "Wpn: x";
            this.WeaponText.text = this.weaponStatus;
        }

        if(this.durability > this.maxDurability/2) {
            this.attackMin = this.baseAttackMin;
            this.attackMax = this.baseAttackMax;

            this.stageOneBroken = false;
            this.weaponStatus = "Wpn: o";
            this.WeaponText.text = this.weaponStatus;
        }

        this.User.attackMin = this.attackMin;
        this.User.attackMax = this.attackMax;

        if(this.durability > this.maxDurability) this.durability = this.maxDurability;
    }
    // wf11
    private void TestComboAccuracy(){
        int tProf = this.currentProficiency;
        for(int cProf = 0; cProf <= 100; cProf +=20){
            Debug.Log("Test Proficiency = "+cProf.ToString()+"\n");

            this.currentProficiency = cProf;
            int c1 = GetComboAccuracy(1);
            int c2 = GetComboAccuracy(2);
            int c3 = GetComboAccuracy(3);
            int c4 = GetComboAccuracy(4);
            int c5 = GetComboAccuracy(5);

            string s = "\nCombo Lv 1 : "+c1.ToString()+"\n"+
                        "Combo Lv 2 : "+c2.ToString()+"\n"+
                        "Combo Lv 3 : "+c3.ToString()+"\n"+
                        "Combo Lv 4 : "+c4.ToString()+"\n"+
                        "Combo Lv 5 : "+c5.ToString();


            Debug.Log(s);
        }

        this.currentProficiency = tProf;
    }

    public void PrintStatus(){
        Debug.Log("Status Weapon "+this.weaponName);
        string s = "Type: " + this.Type.ToString()+"\n"
                +  "User: " + this.User.unitName+"\n\n"
                
                +  "BaseAttackMax: " + this.baseAttackMax.ToString()+"\n"
                +  "BaseAttackMin: " + this.baseAttackMin.ToString()+"\n"
                +  "AttackMax: " + this.attackMax.ToString()+"\n"
                +  "AttackMin: " + this.attackMin.ToString()+"\n\n"

                +  "LightAttackCost: " + this.lightAttackCost.ToString()+"\n"
                +  "HeavyAttackCost: " + this.heavyAttackCost.ToString()+"\n"
                +  "SpecialAttackCost: " + this.specialAttackCost.ToString()+"\n\n"

                +  "Crit: " + this.crit.ToString()+"\n\n"

                +  "BonusCritDamagef: " + this.bonusCritDamagef.ToString()+"\n"
                +  "BonusCritDamagefbase: " + this.bonusCritDamagefbase.ToString()+"\n\n"
                
                +  "MaxDurability: " + this.maxDurability.ToString()+"\n"
                +  "Durability: " + this.durability.ToString()+"\n\n"

                +  "Level0AccuracyBase: " + this.level0AccuracyBase.ToString()+"\n"
                +  "CurrentProficiency: " + this.currentProficiency.ToString()+"\n";

        Debug.Log(s);
    }
}