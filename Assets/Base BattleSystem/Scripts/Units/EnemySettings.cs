using System.Collections.Generic;
using UnityEngine;

public class EnemySettings{
    private char[] ALPHABET = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'}; 
    
    public int level;
    public string name;
    public int hp;

    public int damage;
    public int battleSpeed;

    public int baseKillPrice;
    public int maxKillPrice;

    public List<List<ShieldMode>>[] DefensiveModes = new List<List<ShieldMode>>[2];
    public int defensiveModeIndex = 0;

    public Dictionary<AbilityType, EnemyAttack> AttackLibrary;

    public int minAttackSequenceLength = 0;
    public int maxAttackSequenceLength = 3;

    public void InitDefensiveMode(){
        this.DefensiveModes[0] = new List<List<ShieldMode>>();
        this.DefensiveModes[1] = new List<List<ShieldMode>>();
    }

    public void InitDiscoveredShieldsList(){
        for(int i=0;i<this.DefensiveModes[0].Count;i++){
            this.DefensiveModes[1].Add(new List<ShieldMode>());
        }
    }

    public void InitAttackLibrary(int minLength, int maxLength, int codingsCount){
        if(codingsCount < 3 || codingsCount > 26){
            Debug.LogError("Argument Error: For 3 Attacks, the given codingsCount must be between 3 and 26 was "+codingsCount.ToString());
            return;
        }
        this.minAttackSequenceLength = minLength;
        this.maxAttackSequenceLength = maxLength;

        List<char> AvailableCharsForCodesList = GetAvailableCharsList(codingsCount);
        this.AttackLibrary = new Dictionary<AbilityType, EnemyAttack>();

        EnemyAttack LightAttack = new EnemyAttack(AbilityType.LIGHT);
        EnemyAttack HeavyAttack = new EnemyAttack(AbilityType.HEAVY);
        EnemyAttack SpecialAttack = new EnemyAttack(AbilityType.SPECIAL);

        LightAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);
        HeavyAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);
        SpecialAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);

        while(AvailableCharsForCodesList.Count > 0){
            int ranChoice = Random.Range(1,4);
            switch(ranChoice){
                case 1:
                    LightAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);
                    break;
                case 2:
                    HeavyAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);
                    break;
                case 3:
                    SpecialAttack.AddRandomCodeFromAvailableList(AvailableCharsForCodesList);
                    break;
                default:
                    Debug.LogError("Switch Error!");
                    return;
            }
        }

        this.AttackLibrary[AbilityType.LIGHT] = LightAttack;
        this.AttackLibrary[AbilityType.HEAVY] = HeavyAttack;
        this.AttackLibrary[AbilityType.SPECIAL] = SpecialAttack;
    }

    private List<char> GetAvailableCharsList(int charCount){
        List<char> Res = new List<char>();
        List<char> PossibleCharsList = new List<char>(ALPHABET);
        
        for(int i=0;i<charCount;i++){
            int ranInd = Random.Range(0, PossibleCharsList.Count);
            Res.Add(PossibleCharsList[ranInd]);
            PossibleCharsList.RemoveAt(ranInd);
        }

        return Res;
    }

    public void CopyFromEnemyPrefab(GameObject Prefab){
        Enemy PrefabEnemy = Prefab.GetComponent<Enemy>();

        this.damage = PrefabEnemy.GetDamage();
        this.battleSpeed = PrefabEnemy.GetBattleSpeed();
        this.level = PrefabEnemy.GetLevel();
        this.name = PrefabEnemy.unitName;
        this.hp = PrefabEnemy.healthPoints;

        this.DefensiveModes = PrefabEnemy.GetDefensiveModes();
        this.defensiveModeIndex = PrefabEnemy.GetDefensiveModeIndex();

        this.AttackLibrary = PrefabEnemy.GetAttackLibrary();
        this.minAttackSequenceLength = PrefabEnemy.GetMinAttackSequenceLength();
        this.maxAttackSequenceLength = PrefabEnemy.GetMaxAttackSequenceLength();
    }
}

public class EnemyAttack{
    public AbilityType abilityType;
    public List<char> codings = new List<char>();
    public Dictionary<char, bool> discovered = new Dictionary<char, bool>();

    public EnemyAttack(){
        
    }

    public EnemyAttack(AbilityType aType){
        this.abilityType = aType;
        this.discovered = new Dictionary<char, bool>();
        this.codings = new List<char>();
    }

    public void AddRandomCodeFromAvailableList(List<char> AvailableCharsList){
        if(AvailableCharsList.Count <= 0){
            Debug.LogError("AvailableCharsList already Empty!");
            return;
        }

        int ranIndex = Random.Range(0, AvailableCharsList.Count);
        this.codings.Add(AvailableCharsList[ranIndex]);
        this.discovered.Add(AvailableCharsList[ranIndex], false);
        AvailableCharsList.RemoveAt(ranIndex);
    }

    public override string ToString(){
        string s = "";
        s += this.abilityType.ToString() +": ";
        foreach(char c in this.codings){
            s += c.ToString() + " - ";
            s += this.discovered[c].ToString()+" | ";
        }
        return s;
    }
}
