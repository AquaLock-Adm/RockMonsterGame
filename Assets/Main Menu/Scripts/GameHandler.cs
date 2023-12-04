using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.5f;
    [Range(0.1f, 3f)] public float pitch = 1f;

}

public class GameHandler : MonoBehaviour
{
    [HideInInspector] public PlayerCharacter Player;
    [HideInInspector] public Weapon PlayerWeapon;
    [HideInInspector] public Armor PlayerArmor;

    private List<List<EnemySettings>> EnemyLibrary;
    private int currentHighestStage = 1;

    public int earnedCredits;

    private Sound currentSound;

    [Header("Audio")]
    [SerializeField] AudioSource AudioSource;
    [SerializeField] Sound SwitchMenuOptionSound;
    [SerializeField] Sound SelectMenuOptionSound;
    [SerializeField] Sound BlockedMenuOptionSound;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        this.currentHighestStage = 1;
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

            case "Stage Select":
                GameObject.Find("Stage Select Handler").GetComponent<StageSelectHandler>().StartSetup(this);
            break;

            case "Battle Scene":
            case "Tutorial":
            case "Boss Enemy Scene":
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

    public void LoadStageSelect(){
        SceneManager.LoadScene("Stage Select");
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



#region Audio
    private void SetSound(Sound S){
        if(currentSound == null || currentSound.name != S.name){
            AudioSource.clip = S.clip;
            AudioSource.volume = S.volume;
            AudioSource.pitch = S.pitch;

            currentSound = S;
        }
    }
    
    public void PlaySwitchMenuOptionSound(){
        SetSound(SwitchMenuOptionSound);
        AudioSource.Play();
    }
    
    public void PlaySelectMenuOptionSound(){
        SetSound(SelectMenuOptionSound);
        AudioSource.Play();
    }
    
    public void PlayBlockedMenuOptionSound(){
        SetSound(BlockedMenuOptionSound);
        AudioSource.Play();
    }
#endregion



#region EnemyLibrary Functions
    public bool EnemyLibraryInitialized(){
        return this.EnemyLibrary != null;
    }

    public bool EnemyLibraryEmpty(){
        if(!EnemyLibraryInitialized()) return true;
        int entryCounts = 0;
        foreach(List<EnemySettings> stageList in this.EnemyLibrary){
            foreach(EnemySettings E in stageList){
                entryCounts++;
            }
        }
        return entryCounts <= 0;
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

#region Getter/Setter
    public int GetCurrentHighestStage(){
        return this.currentHighestStage;
    }
    public void SetCurrentHighestStage(int stage){
        this.currentHighestStage = stage;
    }
#endregion
}