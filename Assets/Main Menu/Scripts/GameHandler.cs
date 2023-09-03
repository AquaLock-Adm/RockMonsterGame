using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [HideInInspector] public PlayerCharacter Player;
    [HideInInspector] public Weapon PlayerWeapon;
    [HideInInspector] public Armor PlayerArmor;

    private List<List<EnemySettings>> EnemyLibrary;

    public int earnedCredits;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded (Scene scene, LoadSceneMode mode){
        SceneSetups();
    }

    private void SceneSetups(){
        switch(SceneManager.GetActiveScene().name){
            case "Start Menu":
                GameObject.Find("StartMenuHandler").GetComponent<StartMenuHandler>().StartSetup(this);
            break;

            case "Main Menu":
                GameObject.Find("MenuHandler").GetComponent<NewMenuHandler>().StartSetup(this);
            break;

            case "Battle Scene":
                GameObject.Find("BattleSystem").GetComponent<BattleSystem>().GameStart(this);
            break;

            default:
                Debug.LogError("Setup called in unexpected Scene: "+ SceneManager.GetActiveScene().name);
            break;
        }
    }



#region Menu Loaders
    public void LoadTutorial(){
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadBattleScene(){
        SceneManager.LoadScene("Battle Scene");
    }

    public void LoadMainMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    public void LoadStartMenu(){
        SceneManager.LoadScene("Start Menu");
    }

    public void SetPlayer(PlayerCharacter P){
        if(this.Player != null) return;

        this.Player = P;
        DontDestroyOnLoad(P.gameObject);

        this.PlayerWeapon = P.GetWeapon();
        DontDestroyOnLoad(this.PlayerWeapon.gameObject);
        this.PlayerArmor = P.GetArmor();
        DontDestroyOnLoad(this.PlayerArmor.gameObject); 
    }
#endregion



#region EnemyLibrary Functions
    public bool EnemyLibraryInitialized(){
        return this.EnemyLibrary != null;
    }

    public void InitEnemyLibrary(){
        this.EnemyLibrary = new List<List<EnemySettings>>();
    }

    public EnemySettings GetEnemySettingsByName(string name, int stageIndex){
        if(stageIndex >= this.EnemyLibrary.Count || stageIndex < 0){
            Debug.LogError("Invalid Stage index!");
            return null;
        }

        foreach(EnemySettings E in this.EnemyLibrary[stageIndex]){
            if(E.name == name) return E;
        }
        Debug.LogError("Could not find Enemy in EnemyLibrary!");
        return null;
    }

    public void AddNewEnemyToLibrary(EnemySettings E){
        if(!EnemyLibraryInitialized()) {
            Debug.LogError("Enemy Library Not Initialized!");
            return;
        }

        while(this.EnemyLibrary.Count <= E.level){
            this.EnemyLibrary.Add(new List<EnemySettings>());
        }

        this.EnemyLibrary[E.level-1].Add(E);
    }

    public List<EnemySettings> GetAllEnemiesFromStage(int stageIndex){
        return this.EnemyLibrary[stageIndex];
    }

    public int GetSettingsIndexByName(string name, int stageIndexOfSettings){
        if(stageIndexOfSettings >= this.EnemyLibrary.Count || stageIndexOfSettings < 0) return -1;
        int res = -1;

        for(int i=0;i<this.EnemyLibrary[stageIndexOfSettings].Count;i++){
            if(this.EnemyLibrary[stageIndexOfSettings][i].name == name) return i;
        }
        return res;
    }

    public int GetEnemyLibraryStageCount(){
        return this.EnemyLibrary.Count;
    }

    public void InitNewEnemyLibraryStage(int stageIndex, List<EnemySettings> NewEnemies){
        while(this.EnemyLibrary.Count <= stageIndex){
            this.EnemyLibrary.Add(new List<EnemySettings>());
        }

        this.EnemyLibrary[stageIndex] = NewEnemies;
    }

    public void UpdateEnemyLibraryEntryOf(EnemySettings E){
        if(E.level == 0) return;    // Case: Enemy is first enemy of Wave

        if(E.level > this.EnemyLibrary.Count){
            Debug.LogError("Stage for Enemy not initialized!");
            return;
        }

        for(int i=0;i<this.EnemyLibrary[E.level-1].Count; i++){
            if(this.EnemyLibrary[E.level-1][i].name == name){
                this.EnemyLibrary[E.level-1][i] = E;
            }
        }
    }
#endregion
}
