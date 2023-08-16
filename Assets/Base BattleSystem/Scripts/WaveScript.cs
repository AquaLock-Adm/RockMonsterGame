using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaveScript : MonoBehaviour
{
    [SerializeField] protected List<EnemySettings> EnemySettingsList = new List<EnemySettings>();

    private char[] ALPHABET = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'}; 

    private const int MAX_TRIES_FOR_LOOP = 40;

    public int waveCount = 0;

    public abstract void Setup(int waveIndex);

    public List<EnemySettings> GetEnemySettingsList(){
        return this.EnemySettingsList;
    }

    protected void ResetEnemyInfos(){
        this.EnemySettingsList = new List<EnemySettings>();
    }

    protected void FillEnemyArrays(GameObject[] PrefabArray, int[] LevelArray){
        for(int i = 0; i < PrefabArray.Length; i++){
            EnemySettings cES = new EnemySettings();
            cES.CopyFromEnemyPrefab(PrefabArray[i]);
            cES.level = LevelArray[i];
            this.EnemySettingsList.Add(cES);
        } 
    }

    protected EnemySettings InitDefensiveMode(EnemySettings Settings){
        Settings.DefensiveModes[0] = new List<List<ShieldMode>>();
        Settings.DefensiveModes[1] = new List<List<ShieldMode>>();

        return Settings;
    }

    protected EnemySettings InitDiscoveredShieldsList(EnemySettings Settings,int listCount){
        for(int i=0;i<listCount;i++){
            Settings.DefensiveModes[1].Add(new List<ShieldMode>());
        }
        return Settings;
    }

    protected EnemySettings InitAttackLibrary(EnemySettings Res, int minLength, int maxLength, int codingsPerAbility){
        Res.minAttackSequenceLength = minLength;
        Res.maxAttackSequenceLength = maxLength;

        HashSet<char> usedLetters = new HashSet<char>();
        Dictionary<AbilityType, EnemyAttack> AttackLibrary = new Dictionary<AbilityType, EnemyAttack>();

        EnemyAttack lightAttack = GetNewEnemyAttack(AbilityType.LIGHT, codingsPerAbility, usedLetters);
        AttackLibrary[AbilityType.LIGHT] = lightAttack;

        EnemyAttack heavyAttack = GetNewEnemyAttack(AbilityType.HEAVY, codingsPerAbility, usedLetters);
        AttackLibrary[AbilityType.HEAVY] = heavyAttack;

        EnemyAttack specialAttack = GetNewEnemyAttack(AbilityType.SPECIAL, codingsPerAbility, usedLetters);
        AttackLibrary[AbilityType.SPECIAL] = specialAttack;

        Res.AttackLibrary = AttackLibrary;

        return Res;
    }

    protected EnemyAttack GetNewEnemyAttack(AbilityType aType, int codingsPerAbility, HashSet<char> usedLetters){
        EnemyAttack EA = new EnemyAttack();
        EA.abilityType = aType;
        EA.discovered = new Dictionary<char, bool>();
        List<char> codings = new List<char>();

        for(int i=0;i<codingsPerAbility;i++){
            char code = ' ';
            int tries = 0;

            do{
                int letterIndex = Random.Range(0,26);
                code = ALPHABET[letterIndex];
                tries++;
            }while(!usedLetters.Add(code) && tries < MAX_TRIES_FOR_LOOP);
            if(tries >= MAX_TRIES_FOR_LOOP) code = GetCodeSimple(usedLetters);
            codings.Add(code);
            EA.discovered.Add(code,false);
        }

        EA.codings = codings;
        return EA;
    }

    protected char GetCodeSimple(HashSet<char> usedLetters){
        for(int i = 0; i< ALPHABET.Length; i++){
            if(usedLetters.Add(ALPHABET[i])) return ALPHABET[i];
        }
        Debug.LogError("All Letters of the alphabet used!");
        return ' ';
    }
}