using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    [SerializeField] public string dungeonName;
    [SerializeField] public int entryCost;
    [SerializeField] public int moveCost;
    [SerializeField] public int deathCost;

    [SerializeField] public int waveSize;
    [SerializeField] public int mostWavesCleared;

    [SerializeField] public List<GameObject> EnemyPrefabs;

    [SerializeField] public bool useWaveScript = false;
    [SerializeField] public string waveScriptName = "";
    [SerializeField] public GameObject WaveScriptPrefab;

    [SerializeField] public bool useDialogueScript = false;
    [SerializeField] public string dialogueScriptName = "";

    public DungeonRun LastRun;

    public DungeonRun BestRun;

    public List<DungeonRun> AllRunsList = new List<DungeonRun>();

    public void AddNewRun(DungeonRun NewRun){
        NewRun.PrintStatus();
        this.AllRunsList.Add(NewRun);

        this.LastRun = NewRun;

        if(this.AllRunsList.Count == 1) this.BestRun = NewRun;
        else this.BestRun = CompareRuns(this.BestRun, NewRun);
        if(NewRun.wavesCleared > this.mostWavesCleared) this.mostWavesCleared = NewRun.wavesCleared;
    }

    private DungeonRun CompareRuns(DungeonRun Run1, DungeonRun Run2){
        if(Run1.wavesCleared > Run2.wavesCleared) return Run1;
        else if(Run1.wavesCleared < Run2.wavesCleared) return Run2;
        else {
            if(Run1.creditsEarned > Run2.creditsEarned) return Run1;
            else if(Run1.creditsEarned < Run2.creditsEarned) return Run2;
            else {
                if(Run1.time < Run2.time) return Run1;
                else if(Run1.time > Run2.time) return Run2;
            }
        }
        return Run1;
    }

    public void PrintStatus(){
        Debug.Log("Status Dungeon "+this.dungeonName);

        string s = "EntryCost: " + this.entryCost.ToString()+"\n"
                +  "MoveCost: " + this.moveCost.ToString()+"\n"
                +  "DeathCost: " + this.deathCost.ToString()+"\n\n"

                +  "WaveSize: " + this.waveSize.ToString()+"\n"
                +  "MostWavesCleared: " + this.mostWavesCleared.ToString();
        Debug.Log(s);
        PrintEnemyPrefabs();
        PrintWaveScript();
        PrintDialogueScript();
    }

    public void PrintEnemyPrefabs(){
        string s = "Enemy Prefabs: \n\n";
        if(this.EnemyPrefabs.Count < 1) s += "EMPTY";
        else {
            foreach(GameObject Enemy_GO in this.EnemyPrefabs){
                s += Enemy_GO.GetComponent<Enemy>().unitName + "\n";
            }
        }
        Debug.Log(s);
    }

    public void PrintWaveScript(){
        string s = "WaveScript:\n\nUseWaveScript: "+this.useWaveScript.ToString()+"\n";
        if(this.WaveScriptPrefab != null){
            s += "Script Set: TRUE\n"
                +"Name: " + this.waveScriptName;
        }else{
            s += "Script Set: FALSE\n";
        }

        Debug.Log(s);
    }

    public void PrintDialogueScript(){
        string s = "DialogueScript:\n\n"
                +  "UseDialogueScript: " + this.useDialogueScript.ToString()+"\n"
                +  "DialogueScriptName: " + this.dialogueScriptName+"\n";
        // if(this.WaveScriptPrefab != null){
        //     s += "Script Set: TRUE"
        //         +"Name: " + this.waveScriptName;
        // }else{
        //     s += "Script Set: FALSE";
        // }

        Debug.Log(s);
    }
}

public class DungeonRun {
    public int creditsEarned;
    public int time; // in ms
    public int wavesCleared;
    public int movesUsed;
    public int enemiesDefeated;
    public int expEarned;

    public void PrintStatus(){
        Debug.Log("Printing Dungeon Run:");

        string s = "Credits Earned: "+this.creditsEarned.ToString()+"Cp\n"
                  +"Exp Earned: "+this.expEarned.ToString()+"\n"
                  +"Time: "+this.time.ToString()+"ms\n"
                  +"Moves Used: "+this.movesUsed.ToString()+"\n"
                  +"Enemies Defeated: "+this.enemiesDefeated.ToString()+"\n"
                  +"Waves Cleared: "+this.wavesCleared.ToString()+"\n";

        Debug.Log(s);
    }
}
