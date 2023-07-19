using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Importer : MonoBehaviour
{
    [Header("Dungeon Run Variables")]

    public int entryCost = 500;         // set 500 credits for testing
    public int monsterWaveSize = 10;        // how many monsters spawn in one wave
    public int moveCost = 5;                   // one move costs 5 credits
    public int deathCost = 5000;                   // if the player dies he has to pay an extra penalty

    public bool useDialogueScript = false;
    public string dialogueScriptName;

    public bool useWaveScript = false;
    public string waveScriptName;
    public GameObject WaveScriptPrefab;

    [Header("Unit Variables")]
    public GameObject PlayerPrefab;
    public GameObject[] EnemyPrefabs;
}
