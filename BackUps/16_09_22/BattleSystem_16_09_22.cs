/*
Version before constant input update revert -> see ActionQueue_16_09_22
*/


/*
    Hauptstück des gesamten Battle ablaufs. ALLE prozesse die gestartet werden kommen entweder hier vorbei oder werden größtenteils auch von hier gestartet.
    Viele wichtige daten und referenzen werden hier gehalten und können per referenz auch von hier geholt werden.
*/

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* ENUM über die Stadien des Spielablaufs. Das GESAMTE system nutzt diese um sich zu orientieren was für prozesse gerade ablaufen
    BattleState.START => begin des spiels, in diesem stadium werden alle objekte erstellt und alle variablen iniziiert.
                        Dieses Stadium kommt nur EINMAL am ANFANG der Szene(nachdem AWAKE()) vor
                        Dieses Stadium wird am ENDE der BattleSystem.Awake() bendet und geht in BattleState.PLAYERTURN über

    BattleState.PLAYERTURN => In diesem Stadium hat der Spieler die kontrolle. Dh. dass die normalen ingame inputs hier angenommen werden
                            Genaueres in MenuHandler.cs
                            Dieses Stadium wird durch die eingabe der PassRound() im MenuHandler beendet und geht dann in die BattleState.QUEUE über 

    BattleState.QUEUE => In diesem Stadium werden die aktionen ausgeführt die im playerturn an die Actionqueue gegebn wurden.
                            Hier sind die Spieler inputs gesperrt.
                            Der Großteil diese Stadiums wird von ActionQueue.cs ausgeführt und läuft in Actions.cs ab
                            Dieses Stadium wird beendet sobald alle Actionen der ActionQueue ausgeführt wurden ODER die jetzige Wave abgeschlossen wird ODER der Spieler stirbt ODER der BAILOUT benutzt wurde.
                            Danach geht es je nachdem in BattleState.WAVEOVER, BattleState.BAILOUT, BattleState.PLAYERDIED oder in die normale BattleState.PLAYERTURN über. 
    
    BattleState.WAVEOVER => Dieser Stadium gibt an dass die jetzige Wave beendet wurde. Hier wird das pause menü geladen und auf entscheidung(eingabe) des Spielers gewartet.
                            Will der Spieler das level verlassen wird in die BattleState.RESULT übergegangen und der Resultscreen geladen
                            Will der spieler die nächste wave annehmen, wird zurück in BattleState.PLAYERTURN geleitet.

    BattleState.PLAYERDIED => Dieses stadium wird kurz zeitig eingesetzt um dem resultscreen zu sagen,dass der spieler gestorben ist. Weiteres in ResultScreenHandler.cs
                            Sobald der resultscreen die information gespeichert hat wird dieses stadium auch verlassen und geht in BattleState.RESULT über.

    BattleState.BAILOUT => Ähnlicher wie PLAYERDIED wird dieses Stadium nur benutzt um dem Resultscreen zu sagen das der spieler den bailout benutzt hat.

    BattleState.RESULT => In diesem stadium wird der resultscreen angezeigt. Der einzige Playerinput der hier gilt ist der, der das spiel(szene) beendet.
                            NOTE: stand jetzt wird die szene einfach neugeladen wenn das spiel beendet wird.

    BattleState.DIALOGUE => In diesem stadium werden die dialogue texte angezeigt und per spieler input die texte weiter geskippt.
                            nachdem alle texte durchgelaufen sind wird in das vorherige stadium zurückgeleitet.
                            NOTE: gerade in bearbeitung!

*/
public enum BattleState {START, PLAYERTURN, QUEUE,  DIALOGUE, WAVEOVER, BAILOUT, PLAYERDIED, RESULT}

public class BattleSystem : MonoBehaviour
{
    [Header("Dungeon Run Variables")]
    /*
    Daten für die einstellungen des jeweiligen Dungeons/Levels
        entryCost - Eintrittskosten für das betreten des Dungeons.
        monsterWaveSize - Anzahl der monster in einer wave
        moveCost - credit kosten für das ausführen einer action.
        deathCost - credit kosten für den fall dass der Spieler stirbt
        useDialogueScript - boolean die angibt ob ein Dialogue script genutzt wird
        useWaveScript - boolean die angibt ob ein Wave script genutzt wird
    */

    [SerializeField] public int entryCost;
    [SerializeField] private int monsterWaveSize;
    [SerializeField] public int moveCost;
    [SerializeField] public int deathCost;
    [SerializeField] public bool useDialogueScript;
    [SerializeField] public bool useWaveScript;

    [Header("State")]
    // variable über das jetzige stadium des spielablaufs
    public BattleState state;

    // veraltet, wurde in verbindung mit altem dialogue script benutzt
    public bool stopTextCrawl = false;

    [Header("Unit Variables")]
    /*
    Variablen über die aktuellen Einheiten des spiels
        PlayerPrefab - prefab des Spieler characters. Gefunden unter assets/prefabs/Characters
        EnemyPrefabs - prefabs der Enemy charactere. Gefunden unter assets/prefabs/enemies
        NOTE: werden zurzeit per HAND gesetzt durch einstellen im unity editor.

        Player - referenz auf spieler charakter
        Enemy - referenz auf actuellen HAUPT gegner
        NextEnemy - veraltet! wurde genutzt für anzeige welche den nächsten gegner voraus gezeigt hat.
        EnemyList - Liste über die referenzen auf ALLE gegner der jetzigen Wave.
        maxAp - gibt die maximale Anzahl der ap an, die ein spieler auf einmal haben kann
    */
    public GameObject PlayerPrefab;
    public GameObject[] EnemyPrefabs;
    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Enemy Enemy;
    [SerializeField] public Enemy NextEnemy;
    public List<Enemy> EnemyList = new List<Enemy>();

    // referenz auf die Aktionen die von TestCombo.ActivateEffect() getestet werden. Siehe Action.cs
    public List<Action> TestAbilities = new List<Action>();

    public int maxAp = 99;

    [Header("System Counters")]
    /*
    variablen über hauptsächlich daten die wichtig sind für die anzeige(HUD) und das resultat
        enemyDefeatCount - wie viele gegner bereits besiegt wurden - genutzt vom killcounter
        enemyWaveIndex - an welchem punkt in der liste der Gegner befindet sich der aktuelle gegner
        earnedCredits - wie viele credits bereits verdient wurden - genutzt vom scorecounter
        earnedExp - -||- exp verdient - nur genutzt vom result screen

        wavesClearedCount - besiegte waves
        movesUsedCount - anzahl benutzte actionen
        itemsUsedCount - benutze items
        totalItemCost - credits aller genutzen items, werden vom endscore abgezogen

        totalDamageThisRound - sehr unwichtig, weil nicht mehr genutzt
        bonusActionPointGain - anzahl der ap die am anfang der runde zu den ap des spielers hinzugerechnet werden.

    */
    public int enemyDefeatCount = 0;
    public int enemyWaveIndex = 0;
    
    [SerializeField] public int earnedCredits = 0;
    public int earnedExp = 0;

    public int wavesClearedCount = 0;

    public int movesUsedCount = 0;

    public int itemsUsedCount = 0;
    public int totalItemCost = 0;
    //[SerializeField] private int actionScore = 0;
    //[SerializeField] private int maxActionScore = 35;

    public int totalDamageThisRound = 0;
    //public int bonusActionPointGain = 0;

    [Header("Former BattleHUD")]

    /*
    referenzen über visuelle darstellungs elemente der staten der Aktuellen einheiten.
        apGainTextShowTime - zeit über die der AP gain text am anfang der playerturn angezeigt wird, in ms.

        PlayerNameText - referenz auf den Namenstext des PlayerStatusScreens
        PlayerApText - referenz auf den aptext des PlayerStatusScreens
        PlayerApGainText - referenz auf den apgaintext des PlayerStatusScreens
        PlayerHpSlider - referenz auf des Hpsliders des PlayerStatusScreens
        PlayerShieldSlider - referenz auf des Shieldsliders des PlayerStatusScreens
        PlayerManaSlider - referenz auf des Manasliders des PlayerStatusScreens
        NOTE: genutzt von PlayerCharacter.cs


        EnemyNameText - referenz auf den Namenstext des actuellen gegners
        EnemyLevelText - referenz auf den leveltext des actuellen gegners
        EnemyHpSlider - referenz auf des Hpsliders des actuellen gegners
        EnemyShieldSlider - referenz auf des Shieldsliders des actuellen gegners

    */

    [SerializeField] private int apGainTextShowTime = 1000; // in ms

    [SerializeField] public Text PlayerNameText;
    [SerializeField] public Text PlayerApText;
    [SerializeField] public Text PlayerApGainText;
    [SerializeField] public Slider PlayerHpSlider;
    [SerializeField] public Slider PlayerShieldSlider;
    [SerializeField] public Slider PlayerManaSlider;


    public Text KillCountText;

    [Header("")]
    [SerializeField] public Text EnemyNameText;
    [SerializeField] public Text EnemyLevelText;
    [SerializeField] public Slider EnemyHpSlider;
    [SerializeField] public Slider EnemyShieldSlider;

    [Header("Other System References")]
    /*
    referenzen auf die Gameobjecte der anderen System bestandteile wichtig für den ablauf der spielprozesse
    diese werden in der Setup funktion auch an die jeweiligen systemteile verteilt (siehe BattleSyste.SetUpSystem())
        Importer - AXED, gibs nicht mehr
        Wavescript - referenz auf das actuelle Wavescript objekt welches erzeugt wird, wenn ein wavescript für das level benutzt wird(Siehe WaveScript.cs)
        MainMenu - referenz auf das mainmenu gameobject im canvas welches die buttons der playerinputs beinhaltet
    */

    public Importer Importer;

    //public BattleHUD BattleHUD;
    public MenuHandler MenuHandler;
    public ActionQueue ActionQueue;
    public PauseMenu PauseMenu;
    public ResultHandler ResultHandler;
    public DialogueHandler DialogueHandler;
    public WaveScript WaveScript;

    public GameObject MainMenu;

    [Header("Battle Multipliers")]
    /*
    variablen über werte die zur ausführung von actionen(angriffen) genutzt werden.
    größtenteils schadensmultiplikatoren
        critMultiplier - multiplikator für einen getroffenen crit
        heavyAttackMultiplier - multiplikator für den schaden eines Heavy(siehe Action.cs)
        specialAttackMultiplier - multiplikator für den schaden eines Special(siehe Action.cs)
        ultAbilityDamageMultiplier - multiplikator für den schaden der UltForm einer aktion(siehe Action.cs)

        attackIntoShieldMultiplier - multiplikator für den schaden einer action auf eine unit mit shield(>0)

        spellStrongMultiplier - multiplikator für den schaden einer action mit vorteilhaftem element(siehe BattleSystem.ElementWeakToo())
        spellWeakMultiplier - multiplikator für den schaden einer action mit nachteilhaftem element
    */

    [SerializeField] public float critMultiplier = 1.5f;
    [SerializeField] public float heavyAttackMultiplier = 1.2f;
    [SerializeField] public float specialAttackMultiplier = 1.1f;
    [SerializeField] public float ultAbilityDamageMultiplier = 1.1f;


    [SerializeField] public float attackIntoShieldMultiplier = 0.9f;

    [SerializeField] public float spellStrongMultiplier = 1.5f;
    [SerializeField] public float spellWeakMultiplier = 0.75f;
    
    [Header("Ability Decay aka Spam Protection")]
    /*
    Für infos über spam decay -> mechanix.txt
        AbilityDecayArray - array über alle abilitydecay stufen aller fähigkeiten. Index stimmt mit Action.abilityIndex überein.
        abilityDecayMultiplierMax, abilityDecayMultiplierMin - bestimmen den rahmen des maximalen und minimalen multiplikators den ein spelldecay annehmen kann
        abilityDecayLevelCount - anzahl der level/stufen in der der abilitydecay runtegestuft wird.
                                    Die veringerung des spelldecays per level wird mittels diesem und den min und max werten oben am anfang des spiels errechnet.
        abilityDecayLevelLoss, abilityDecayLevelGain - wie viele level/stufen des abilitydecays gewonnen/verloren werden wenn eine fähigkeit eingesetzt wird
    */

    [SerializeField] public int[] AbilityDecayArray;
    [SerializeField] private float abilityDecayMultiplierMax = 1.05f;
    [SerializeField] private float abilityDecayMultiplierMin = 0.55f;
    [SerializeField] public int abilityDecayLevelCount = 20;
    //[SerializeField] private int abilityDecayLevelLoss = 1;
    //[SerializeField] private int abilityDecayLevelGain = 1;

    [Header("MSG Colors")]
    /*
    Farben für nachrichten welche vom altem text system genutzt wurden
    das Text-system ist bereits gelöscht ich wollte nur die farben behalten für später vlt
    */
    public Color gainColor;
    public Color playerAttackColor;
    public Color enemyAttackColor;
    public Color playerMissColor;
    public Color enemyMissColor;
    public Color playerCritColor;
    public Color enemyCritColor;
    public Color itemColor;
    public Color standartColor;

    [Header("Element Matrix")]
    /*
    Matrix über die schwächen und stärken der elemente.
    1 - vorteilhaft
    -1 - nachteilhaft
    für genaueres siehe BattleSystem.ElementWeakToo();
    */
    private int[,] ElementWeaknessMatrix = new int[,] 
                                            {
                                            { 0,  1,  0, -1},
                                            {-1,  0,  1,  0},
                                            { 0, -1,  0,  1},
                                            { 1,  0, -1,  0}
                                            };

    [Header("For Testing")]
    /*
    variablen welche ich ab und zu zum ausprobieren benutze
        disableApCost - stopt den gebrauch von ap(funktioniert aber nicht mehr)
        spawnMax - maximale anzahl von zu spawnenden gegnern.
    */

    public bool disableAbilityDecay = false;

    public bool disableHeat = false;

    public bool disableTestCombo = false;

    [SerializeField] public bool disableApCost = false;

    [SerializeField] private int spawnMax = 10000;  //10k

    [SerializeField] private int[] log1;  // decay values for lv1-5 combo abilities
    //[SerializeField] public List<Action> log2; // empty
    //[SerializeField] private List<int> log3; // empty



#region System SetUp
    private void Awake(){
        state = BattleState.START;

        ImportSettings();
        SetUpSystem();
        SetUpBattle();
        PauseMenu.ShowPauseMenu(true);
        // the beginning pause menu unlocks the Enemy attack loop and calls PlayerTurn()
    }
    private void OnApplicationQuit(){
        
        Enemy.StopAttack();
    }
    private void ImportSettings(){
        Importer = GameObject.Find("Importer").GetComponent<Importer>();

        entryCost = Importer.entryCost;
        monsterWaveSize = Importer.monsterWaveSize;
        moveCost = Importer.moveCost;
        deathCost = Importer.deathCost;

        PlayerPrefab = Importer.PlayerPrefab;
        EnemyPrefabs = Importer.EnemyPrefabs;
    }
    private void SetUpSystem(){
        MenuHandler = GameObject.Find("MenuHandler").GetComponent<MenuHandler>();
        ActionQueue = GameObject.Find("ActionQueue").GetComponent<ActionQueue>();
        PauseMenu = GameObject.Find("PauseMenu").GetComponent<PauseMenu>();
        ResultHandler = GameObject.Find("ResultHandler").GetComponent<ResultHandler>();
        DialogueHandler = GameObject.Find("DialogueHandler").GetComponent<DialogueHandler>();

        MenuHandler.BattleSystem = this;
        MenuHandler.ActionQueue = ActionQueue;
        MenuHandler.PauseMenu = PauseMenu;
        MenuHandler.DialogueHandler = DialogueHandler;

        ActionQueue.BattleSystem = this;
        ActionQueue.MenuHandler = MenuHandler;

        PauseMenu.BattleSystem = this;
        PauseMenu.ActionQueue = ActionQueue;
        PauseMenu.ResultHandler = ResultHandler;
        PauseMenu.MenuHandler = MenuHandler;

        ResultHandler.BattleSystem = this;
        ResultHandler.MenuHandler = MenuHandler;

        DialogueHandler.BattleSystem = this;
        DialogueHandler.MenuHandler = MenuHandler;
        DialogueHandler.ActionQueue = ActionQueue;
    }
    private void SetUpDialogueScript(){
        DialogueHandler.dialogueScriptName = Importer.dialogueScriptName;
        DialogueHandler.ActivateDialogueScript();
    }
    private void SetUpWaveScript(){
        // spawn wavescript
        WaveScript = Instantiate(Importer.WaveScriptPrefab).GetComponent<WaveScript>();
        WaveScript.waveScriptName = Importer.waveScriptName;
        WaveScript.EnemyPrefabs = EnemyPrefabs;
        WaveScript.Activate();
    }
    private void SetUpBattle(){

        if(Importer.useWaveScript) SetUpWaveScript();
        AddEnemys();

        SetCurrentUnits();

        if(Importer.useDialogueScript) SetUpDialogueScript();

        //ResultHandler.UpdateMultiplier();
    }
    private void SetCurrentUnits(){
        SetUpPlayer();

        Enemy = EnemyList[0];
        Enemy.gameObject.SetActive(true);
        SetNextEnemy();
    }
    private void SetUpPlayer(){
        // Marcel.object 
        // Marcel.Player.weapon = marcel.weapon
        // -||- armor
        /*
        switch(marcel.playername){
            case "Hoover":
                Player = new Hoover();

            case "Whatever":
            .....
        }
        */
        this.PlayerApGainText.text = "";
        GameObject P = Instantiate(PlayerPrefab);
        Player = P.GetComponent<PlayerCharacter>();
        Player.Setup(this);

        SetUpPlayerAbilities();
        SetUpPlayerItems();
    }
    private void SetUpPlayerAbilities(){
        this.TestAbilities = Player.Weapon.GetMovesToTest();

        Action A;
        int aI = 0;
        // NOTE: Element fähigkeit IMMER als erstes, weil der MenuHandler darauf aufbaut
        A = new Element(SpellElement.none);
        A.abilityIndex = aI;
        aI++;

        Player.Abilities.Add(A);
        Player.Elements.Add(SpellElement.FIRE);
        //this.TestAbilities.Add(A);

        A = new Light();
        A.abilityIndex = aI;
        aI++;;

        //A.manaCost += 7;
        //A.element = SpellElement.FIRE;

        Player.Abilities.Add(A);
        //this.TestAbilities.Add(A);

        
        A = new Heavy();
        A.abilityIndex = aI;
        aI++;;

        //A.manaCost += 7;
        //A.element = SpellElement.FIRE;

        Player.Abilities.Add(A);
        //this.TestAbilities.Add(A);

        /*A = new Special();
        A.abilityIndex = 1;
        
        //A.manaCost += 7;
        //A.element = SpellElement.FIRE;

        Player.Abilities.Add(A);
        //this.TestAbilities.Add(A);*/

        if(!this.disableTestCombo){
            A = new TestCombo();
            A.abilityIndex = aI;
            aI++;;
            Player.Abilities.Add(A);

            Debug.Log(this.TestAbilities.Count.ToString()+" abilities in TestCombo.");
        }


        int lastIndex = Player.Abilities.Count-1;

        List<Action> MoveList = Player.Weapon.GetMoveList();

        ActionQueue.firstComboAbilityIndex = lastIndex+1;

        foreach(Action CA in MoveList) {
            ActionQueue.ComboActionStrings.Add(CA.comboList);
            CA.abilityIndex = lastIndex+1;
            lastIndex++;
            Player.Abilities.Add(CA);
        }

        Debug.Log((lastIndex+1).ToString()+" player abilities");
        SetAbilityDecay();

        /*Ability Ability3 = new Ability();
        Ability3.type = AbilityType.SPELL;
        Ability3.spellName = "Water";
        Ability3.element = Element.WATER;
        Ability3.apCost = 6;
        Ability2.manaCost = 12;
        Ability3.damage = 100 + (Player.level * 24);
        Ability3.accuracy = 90;
        Ability3.crit = 5;
        Player.Abilities.Add(Ability3);

        Ability Ability4 = new Ability();
        Ability4.type = AbilityType.SPELL;
        Ability4.spellName = "Air";
        Ability4.element = Element.AIR;
        Ability4.apCost = 3;
        Ability2.manaCost = 12;
        Ability4.damage = 50 + (Player.level * 19);
        Ability4.accuracy = 90;
        Ability4.crit = 15;
        Player.Abilities.Add(Ability4);*/
    }
    private void SetAbilityDecay(){
        this.AbilityDecayArray = new int[Player.Abilities.Count];
        for(int i=0; i < Player.Abilities.Count; i++){
            this.AbilityDecayArray[i] = abilityDecayLevelCount;
        }
    }
    private void SetUpPlayerItems(){
        /*Item I = new Item();
        I.type = ItemType.heal;
        I.value = 300;
        I.useCost = 460;

        Player.Items.Add(I);*/

        Item R = new Item();
        R.type = ItemType.repair;
        R.value = 2000;
        R.useCost = 250;

        Player.Items.Add(R);

        Player.CountAllItems();
    }
#endregion

#region System Run Functions
    public void NextWave(){
        ClearEnemyList();
        AddEnemys();

        Enemy.Hide(false);

        enemyWaveIndex = 0;
        Enemy = EnemyList[0];
        Enemy.gameObject.SetActive(true);
        /*if(actionScore >= 10){
            actionScore = 10;
            ResultHandler.UpdateMultiplier();
        }*/
        SetNextEnemy();
        Enemy.StartAttack();
        Player.currentActionPoints = Player.startActionPoints;
        PlayerTurn();
    }
    private void ClearEnemyList(){
        foreach (Unit U in EnemyList){
            GameObject GO = U.gameObject;
            Destroy(GO);
        }

        for(int i = 0; i < monsterWaveSize; i++){
            EnemyList.RemoveAt(0);
        }
    }
    private void AddEnemys(){

        if(Importer.useWaveScript && wavesClearedCount < WaveScript.waveCount){
            EnemyList = WaveScript.NextWave();
            foreach (Enemy E in EnemyList){
                E.Setup(this);
                E.gameObject.SetActive(false);
            }
        }
        else{
            List<GameObject> EnemiesToBeSpawned = RandomizeWave();
        
            foreach (GameObject EnemyGO in EnemiesToBeSpawned){
                GameObject E = Instantiate(EnemyGO);
                E.GetComponent<Enemy>().Setup(this);
                EnemyList.Add(E.GetComponent<Enemy>());
        
                E.SetActive(false);
            }
        }
    }
    private List<GameObject> RandomizeWave(){
        List<GameObject> UnitGameObjectList = new List<GameObject>();


        int[] spawnValues = new int[EnemyPrefabs.Length];
        for(int i = 0; i < EnemyPrefabs.Length; i++){
            Enemy E = EnemyPrefabs[i].GetComponent<Enemy>();
            if(i == 0) spawnValues[i] = (int)Mathf.Round((spawnMax*E.spawnPercentual)/100);
            else spawnValues[i] = spawnValues[i-1] + (int)Mathf.Round((spawnMax*E.spawnPercentual)/100);
        }

        for(int i = 0; i < monsterWaveSize; i++){
            int ranVal = Random.Range(0, spawnMax);
            
            int border = spawnValues[0];
            int index = 0;
            while(ranVal > border){
                index++;
                border = spawnValues[index];
            }

            UnitGameObjectList.Add(EnemyPrefabs[index]);
        }
        return UnitGameObjectList;
    }
    private void SetNextEnemy() {
        if(enemyWaveIndex < EnemyList.Count -1) NextEnemy = EnemyList[enemyWaveIndex+1];
        else NextEnemy = null;
    }
    public void PlayerTurn() {
        totalDamageThisRound = 0; // used for an old mechanic in the actionscore
        state = BattleState.PLAYERTURN;
    }
    public async void GainAp(int n, int totalTime = -1){
        if(totalTime < 0) totalTime = this.apGainTextShowTime;
        if(Player.currentActionPoints +n > this.maxAp) n = this.maxAp - Player.currentActionPoints;
        showApGain(n, totalTime);

        await Task.Delay((int)Mathf.Round((totalTime/2)));

        int tickRate = 1;
        int pointsPerTick = 1;


        while(n < (int)Mathf.Round((totalTime/2) / tickRate)){
            tickRate++;
        }

        pointsPerTick = (int)Mathf.Round(n/((totalTime/2)/tickRate));

        //Debug.Log(n.ToString()+"n: "+this.tickRate.ToString()+"ms Tick Rate / "+this.pointsPerTick.ToString()+"ppT");

        int gain = 0;

        while(gain < n && Player.currentActionPoints <= maxAp){
            Player.currentActionPoints += pointsPerTick;
            gain += pointsPerTick;
            await Task.Delay(tickRate);
        }

        if(gain < n) Player.currentActionPoints -= n - gain;
    }
    private async void showApGain(int n, int totalTime){
        this.PlayerApGainText.text = "+"+n.ToString();
        await Task.Delay((int)Mathf.Round((totalTime/2)));

        int tickRate = 1;
        int pointsPerTick = 1;


        while(n < (int)Mathf.Round((totalTime/2) / tickRate)){
            tickRate++;
        }

        pointsPerTick = (int)Mathf.Round(n/((totalTime/2)/tickRate));

        //Debug.Log(n.ToString()+"n: "+this.tickRate.ToString()+"ms Tick Rate / "+this.pointsPerTick.ToString()+"ppT");

        int gain = n;

        while(gain < n && Player.currentActionPoints <= maxAp){
            gain -= pointsPerTick;
            this.PlayerApGainText.text = "+"+gain.ToString();
            await Task.Delay(tickRate);
        }

        if(gain > 0) this.PlayerApGainText.text = "+0";
        await Task.Delay(50);

        this.PlayerApGainText.text = "";
    }
    public async void PassRound(){
        state = BattleState.QUEUE;
        GainAp(Player.actionPointGain);
        await ActionQueue.ExecuteAllActions();
        if(!ActionQueue.stopQueue) {
            PlayerTurn();
        }
        else ActionQueue.stopQueue = false;
    }
    public void EnemyDies(){
        //int heal = (int)Mathf.Round((Player.maxHealthPoints-Player.healthPoints)*0.7f);

        earnedCredits += Enemy.killPrice;
        earnedExp += Enemy.killExp;
        ResultHandler.UpdateScore(earnedCredits);

        enemyDefeatCount++;

        /*if(totalDamageThisRound >= Enemy.maxHealthPoints) IncreaseActionScore(3);
        else if(totalDamageThisRound <= (int)Mathf.Round(Enemy.healthPoints/4)) IncreaseActionScore(-2);
        else if(totalDamageThisRound <= 0) IncreaseActionScore(-4);*/
        
        if(CheckForHit(Enemy.itemDropChance)) {
            Debug.Log("Item dropped here");

            Item R = new Item();
            R.type = ItemType.repair;
            R.value = 2000;
            R.useCost = 250;

            Player.Items.Add(R);

            Player.CountAllItems();
        }

        //BattleHUD.UpdateHp(-heal, Player);
        Enemy.StopAttack();
        Enemy.gameObject.SetActive(false);

        do {
            enemyWaveIndex++;

            UpdateKillCount();

            if (enemyWaveIndex == monsterWaveSize){  // if end of enemy array get next wave
                ActionQueue.stopQueue = true;
                ActionQueue.ClearActionQueue();
                Enemy.Hide(true);
                WaveOver();
                return;
            }
            Enemy = EnemyList[enemyWaveIndex];
        }while(Enemy.healthPoints <= 0); // find alive enemy

        Enemy.gameObject.SetActive(true);
        Enemy.StartAttack();

        SetNextEnemy();
    }
    public void PlayerDies(){
        state = BattleState.PLAYERDIED;
        ActionQueue.stopQueue = true;
        ActionQueue.ClearActionQueue(); // not really necessary
        PauseMenu.ShowPauseMenu(true);
    }
    public void UpdateKillCount(){
        KillCountText.text = enemyDefeatCount.ToString();
    }
    public void WaveOver(){

        state = BattleState.WAVEOVER;
        wavesClearedCount++;
        Player.spentActionPoints = 0;
        Player.currentActionPoints = 0;

        if(ActionQueue.comboLevel >= 2) ActionQueue.SetComboLevel(2);
        else ActionQueue.SetComboLevel(1);

        SetAbilityDecay();

        PauseMenu.ShowPauseMenu(true);
    }
    public void End(){ 
        //ResultHandler.CloseResult();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
#endregion

#region Battle Actions Functions
    public async Task EnemyAttack(Enemy E){

        if(!CheckForHit(E.accuracy)){
            await Player.MissedAttack();
            return;
        }

        int damage = CalcUnitDamage(E);

        if(CheckForHit(E.crit)){
            damage = (int)Mathf.Round(damage*critMultiplier);
            Player.Crit();
            //newAction.textColor = playerCritColor;
        }

        await Player.DealDamage(damage);
    }
    public async Task<int> DealAOE(Action A, int unitCount){
        Debug.Log("AOE("+unitCount.ToString()+") called");
        int damageDealt = 0;
        int origDamage = A.baseDamage;
        bool oMH = A.multiHitON;
        //Debug.Log("multiHitON: "+oMH.ToString());
        for(int n = (int)Mathf.Min((float)unitCount, (float)(monsterWaveSize - enemyWaveIndex)); n > 0; n-- ){
            Enemy Ec = EnemyList[enemyWaveIndex + n-1];

            A.Enemy = Ec;
            A.enemySet = true;
            A.damage = origDamage;
            A.multiHitON = oMH;
            damageDealt += await A.NormalExecute(); // WARNING! elemental weakness factor is still on top
        }
        return damageDealt;
    }
    public async Task<int> MultiHit(Action A, int hitCount){ 
        Debug.Log("MultiHit("+hitCount.ToString()+") called");
        int damageDealt = 0;
        int oDamage = A.damage;
        List<Task<int>> t = new List<Task<int>>();

        for(int n = 0; n< hitCount && A.Enemy.healthPoints > 0; n++){
            A.damage = oDamage;
            A.enemySet = true;
            //A.printThisAction = true;
            damageDealt += await A.NormalExecute();
            //t.Add(A.NormalExecute());
            //await Task.Delay(400);
        }
        foreach(Task<int> T in t) damageDealt += await T;
        return damageDealt;
    }
    public async Task<int> DealOverFlow(int dmg){
        int damageDealt = 0;
        while(dmg > 0 && state == BattleState.QUEUE){
            int damageShield = dmg;
            if(Enemy.shield > 0) damageShield = (int)Mathf.Round(damageShield * Enemy.attackIntoShieldMultiplier);
            int dmgd = await this.Enemy.DealDamage(dmg);
            damageDealt += dmgd;
            dmg = damageShield - dmgd;
        }
        return damageDealt;
    }
    public void RepairWeapon(){
        Item R = Player.Items.Find(x => x.type == ItemType.repair);
        Player.Weapon.Repair(R.value);
        Player.Items.Remove(R);
        itemsUsedCount++;
        totalItemCost += R.useCost;
        Player.repairItemCount--;
    }
    public void CastAttack(int attackIndex){
        if(attackIndex >= Player.Abilities.Count || Player.Abilities[attackIndex].comboLevel > 1) return;
        Action A = Player.Abilities[attackIndex].Copy();
        CastAbility(A);
    }
    public void CastSpell(int eleIndex){
        if(eleIndex >= Player.Elements.Count) return;

        Action A = new Element(Player.Elements[eleIndex]);
        A.abilityIndex = 0; 
        CastAbility(A); 
    }
    private void CastAbility(Action A){
        if(ActionQueue.Actions.Count >= ActionQueue.maxMovesQueued && ActionQueue.comboListOverFlow < ActionQueue.comboLevel){
            ActionQueue.AddAction(A);

        }else if(ActionQueue.Actions.Count < ActionQueue.maxMovesQueued){
            if (Player.currentActionPoints < Player.spentActionPoints + A.apCost){

                //StartCoroutine(ActionQueue.ShowText("Not enough AP!", 0.7f, BattleState.PLAYERTURN));                 // defined in ActionQueue.cs
                //MenuHandler.LoadMainMenu();
                return;
            }

            ActionQueue.AddAction(A);

            Player.spentActionPoints += A.apCost;
            if(disableApCost) Player.spentActionPoints -= A.apCost;
        }else return;

        MenuHandler.LoadMainMenu();
    }
    public void CastComboAbility(Action A){

        if (Player.currentActionPoints < Player.spentActionPoints + A.combinationCost){

            //StartCoroutine(ActionQueue.ShowText("Not enough AP!", 0.7f, BattleState.PLAYERTURN));                 // defined in ActionQueue.cs
            MenuHandler.LoadMainMenu();
            return;
        }
        int nRemove = (int)Mathf.Clamp(A.comboLevel - ActionQueue.comboListOverFlow,1.0f, (float)A.comboLevel);

        Debug.Log("Remove: "+nRemove);

        for(int i = 0; i < nRemove; i++) CancelLastAction();

        
        ActionQueue.ClearComboList();


        A.abilityIndex = GetAbilityIndex(A.name);

        ActionQueue.AddAction(A);

        Player.spentActionPoints += A.apCost;
        if(disableApCost) Player.spentActionPoints -= A.apCost;

        MenuHandler.LoadMainMenu();
    }
    public void CancelLastAction(bool gainApBack = true) {

        if(ActionQueue.Actions.Count > 0) {

            // muss so, um einen bug zu umgehen, falls eine ult form gecancelt wird
            if(ActionQueue.Actions[ActionQueue.Actions.Count-1].comboLevel > ActionQueue.comboLevel) ActionQueue.comboLevel = ActionQueue.Actions[ActionQueue.Actions.Count-1].comboLevel;
            

            Action A = ActionQueue.RemoveLastAction();                 // defined in ActionQueue.cs
            if(gainApBack) Player.spentActionPoints -= A.apCost;
        }
    }
#endregion

#region Helper Functions
    public bool CheckForHit(int accuracy){                 /// !!!!!!!!! Also used for checking crits! (same functionality)
        int ran = Random.Range(1, 101);
        return ran <= accuracy;
    }
    public int CalcUnitDamage(Unit U){ return Random.Range(U.attackMin, U.attackMax+1); }
    public int AbilityDecay(int baseDmg, int abilityIndex){
        if(disableAbilityDecay) return baseDmg;

        float l = (abilityDecayMultiplierMax - abilityDecayMultiplierMin)/abilityDecayLevelCount;
        float mult = abilityDecayMultiplierMin+l*AbilityDecayArray[abilityIndex];

        int res = (int)Mathf.Round(baseDmg*mult);

        Action cA = Player.Abilities[abilityIndex];

        int[] decayVals = GetComboDecayValues();
        // for testing
        this.log1 = decayVals;

        int decayLoss = decayVals[cA.comboLevel-1];
        AbilityDecayArray[abilityIndex] = (int)Mathf.Clamp(AbilityDecayArray[abilityIndex] - decayLoss, 0.0f, float.MaxValue);

        foreach(Action A in Player.Abilities){
            if(A.abilityIndex != abilityIndex && A.comboLevel <= cA.comboLevel) {
                AbilityDecayArray[A.abilityIndex] = (int)Mathf.Clamp(AbilityDecayArray[A.abilityIndex]+1, 0.0f, abilityDecayLevelCount);
            }
        }

        return res;
    }
    private int[] GetComboDecayValues(){
        int[] cAttacks = {0,0,0,0,0}; // counting how many abilities are of combolevel 1 - 5

        foreach(Action A in Player.Abilities){
            if(A.comboLevel > 0 && A.comboLevel <= 5) cAttacks[A.comboLevel-1]++;
        }

        int[] res = {0,0,0,0,0};

        for(int i=0;i<res.Length;i++){
            res[i] = cAttacks[i];
            if(i+1 >= ActionQueue.comboLevel) res[i]--;
        } 

        return res;
    }
    public IEnumerator CrawlText(string txt, Text textField, float maxCrawlTime){
        stopTextCrawl = false;

        float timePerSign = maxCrawlTime/txt.Length;
        timePerSign = Mathf.Round(timePerSign*1000);
        timePerSign /= 1000;

        textField.text = "";

        for(int i = 0; i < txt.Length; i++){
            if(!stopTextCrawl){
                textField.text += txt[i];
                yield return new WaitForSeconds(timePerSign);
            }
        }

        stopTextCrawl = false;

        yield return null;
    }
    public SpellElement CombineElements( List<SpellElement> BaseElements){      // TODO

        return BaseElements[0];
    }
    public int ElementWeakTo(SpellElement AttackElement, SpellElement DefendElement){
        if(AttackElement == SpellElement.none) return 0;
        if(DefendElement == SpellElement.none) return -1;

        int attackEleIndex = GetElementIndex(AttackElement);
        int defendEleIndex = GetElementIndex(DefendElement);

        if(attackEleIndex < 0 || defendEleIndex < 0) Debug.Log("Something went wrong!");

        return ElementWeaknessMatrix[attackEleIndex, defendEleIndex];
    }
    private int GetElementIndex(SpellElement Input){
        int index = -1;

        switch(Input){
            case SpellElement.WATER:
                index = 0;
            break;

            case SpellElement.FIRE:
                index = 1;
            break;

            case SpellElement.AIR:
                index = 2;
            break;

            case SpellElement.EARTH:
                index = 3;
            break;

            default:
                Debug.Log("Nani ?  Unknown Element?");
            break;
        }

        return index;
    }

    private int GetAbilityIndex(string aName){
        int index = 0;
        foreach(Action A in Player.Abilities){
            if(A.name == aName) return index;
            index++;
        }
        Debug.Log("Ability Name not found in Player Abilities");
        return -1;
    }
#endregion

#region shelved functionality
    /*private void IncreaseActionScore(int amount){
        actionScore += amount;
        if(actionScore > maxActionScore) actionScore = maxActionScore;
        else if(actionScore < 0 ) {
            actionScore = 0;
            int stage = (int)(actionScore/5);
            ResultHandler.UpdateMultiplier();
        }
        else {
            int stage = (int)(actionScore/5);
            ResultHandler.UpdateMultiplier();
        }
    }*/
    /*private void AddSpellDecay(int AbilityIndex){
        
        currentSpellDecay++;
        if(currentSpellDecay > 2) currentSpellDecay = 2;

        lastAbilityDecayIndex = AbilityIndex;
        //Debug.Log("Ability Decay: "+ Player.Abilities[AbilityIndex].spellName+"|"+currentSpellDecay);
    }*/
    /*private void ResetSpellDecay(){
        AbilityDecayReminder[0] = new Stack<int>();
        AbilityDecayReminder[1] = new Stack<int>();  

        lastAbilityDecayIndex = -1;
        currentSpellDecay = 0;
    }*/
    /*public bool UseHealItem(){
        if(Player.healItemCount > 0){
            Item I = Player.Items.Find(I => I.type == ItemType.heal);
            if(I == null) Debug.Log("An heal item went out of count!");
            Player.healthPoints += I.value;
            // Should be fixed in BattleHUD.DepleteHp() -- if(Player.healthPoints > Player.maxHealthPoints) Player.healthPoints = Player.maxHealthPoints;          // if healed to full you will still lose hp due to the hp depletion being slower than this.
            Player.Items.Remove(I);
            itemsUsedCount++;
            totalItemCost += I.useCost;
            Player.healItemCount--;

            Action newAction = new Action();
            newAction.id = "InfoText";
            //newAction.textColor = itemColor;
            ActionQueue.IntersectNewAction(newAction);
            return true;
        }
        else return false;
    }*/
    /*public void QueueAttack(Action A){
        int baseDamage = CalcUnitDamage(Player);

        //int damageSpamDecay = (int)Mathf.Round((baseDamage*0.2f)*currentSpellDecay);

        int damage = baseDamage; // - damageSpamDecay;

        A.id = "BasicAttack";
        A.cover = "ATTACK";
        A.ap = Player.attackCost;
        //CurrentAction.textColor = playerAttackColor;

        ActionQueue.AddAction(A);                   // defined in ActionQueue.cs
    }
    public async Task ExecuteAttack(Action Attack){
        Player.spentActionPoints -= Player.attackCost; 

        movesUsedCount++;

        if(!CheckForHit(Player.accuracy)){
            await Enemy.MissedAttack();
            Player.currentActionPoints -= Player.attackCost;
            return;
        }

        int damage = CalcUnitDamage(Player);

        if(CheckForHit(Player.crit)){
            damage = (int)Mathf.Round(damage*critMultiplier);
            Enemy.Crit();
            //newAction.textColor = playerCritColor;
        }

        Player.Weapon.DamageToDurability(Enemy.damageToWeapons);
        damage = CheckAbilityDecay(damage, Attack.abilityIndex);
        await Enemy.DealDamage(damage);
    }
    private void QueueSpell(Action A){
        A.id = "CastSpell";
        A.cover = A.Ability.spellName.ToUpper();
        A.ap = A.Ability.apCost;
        A.mana = A.Ability.manaCost;
        A.damage = A.Ability.damage;
        A.accuracy = A.Ability.accuracy;
        A.crit = A.Ability.crit;
        //CurrentAction.textColor = playerAttackColor;
        A.element = A.Ability.element;

        ActionQueue.AddAction(A);
    }
    public async Task ExecuteSpell(Action Spell){
        Player.spentActionPoints -= Spell.ap; 

        movesUsedCount++;
        List<Task> tasksTmp;

        if(Spell.mana > Player.mana){
            tasksTmp = new List<Task>();
            tasksTmp.Add(Enemy.MissedAttack());
            tasksTmp.Add(Player.DepleteMana(Player.mana));
            await Task.WhenAll(tasksTmp);
            Player.currentActionPoints -= Spell.ap;
            return;
        }

        if(!CheckForHit(Spell.accuracy)){
            //newAction.textColor = playerMissColor;
            await Enemy.MissedAttack();
            Player.currentActionPoints -= Spell.ap;
            return;
        }

        int weakness = ElementWeakTo(Enemy.element, Spell.element);
        // returns 1 if less damage
        // returns 0 if neutral
        // returns -1 if more damage

        if(weakness == 1) Spell.damage = (int)Mathf.Round(Spell.damage*spellWeakMultiplier);

        if(weakness == -1) Spell.damage = (int)Mathf.Round(Spell.damage*spellStrongMultiplier);

        if(CheckForHit(Spell.crit)){
            Spell.damage = (int)Mathf.Round(Spell.damage*critMultiplier);
            Enemy.Crit();
            //newAction.textColor = playerCritColor;
        }
        Spell.damage = CheckAbilityDecay(Spell.damage, Spell.abilityIndex);
        tasksTmp = new List<Task>();
        tasksTmp.Add(Player.DepleteMana(Spell.mana));
        tasksTmp.Add(Enemy.DealDamage(Spell.damage));
        await Task.WhenAll(tasksTmp);
    }
    public int AbilityDecay(int baseDmg, int abilityIndex){
        if(disableAbilityDecay) return baseDmg;

        float l = (abilityDecayMultiplierMax - abilityDecayMultiplierMin)/abilityDecayLevelCount;
        float mult = abilityDecayMultiplierMin+l*AbilityDecayArray[abilityIndex];

        int res = (int)Mathf.Round(baseDmg*mult);
        
        if(AbilityDecayArray[abilityIndex]-abilityDecayLevelLoss > 0){ // prev.: >= 0
            AbilityDecayArray[abilityIndex] -= abilityDecayLevelLoss;
        }
        for(int i=0;i<AbilityDecayArray.Length;i++){
            if(i!=abilityIndex) {
                AbilityDecayArray[i] += abilityDecayLevelGain;
                if(AbilityDecayArray[i] > abilityDecayLevelCount) AbilityDecayArray[i] = abilityDecayLevelCount;
            }
        }
        return res;
    }
    private List<int> GetLowestDecayIndexes(Action cA){
        List<int> tmp = new List<int>();
        List<int> res = new List<int>();
        for(int i=0;i<cA.comboLevel;i++) {
            tmp.Add(AbilityDecayArray[i]);
            res.Add(i);
        }
        for(int i=0;i<tmp.Count;i++){
            int compare1I = i;
            int compare2I = i+1;
            bool switched = false;
            while(compare2I<tmp.Count && tmp[compare1I] > tmp[compare2I]){
                int tmpI = tmp[compare2I];
                int tmpVal = res[compare2I];

                tmp[compare2I] = tmp[compare1I];
                res[compare2I] = res[compare1I];

                tmp[compare1I] = tmpI;
                tmp[compare1I] = tmpVal;

                compare1I++;
                compare2I++;
                switched = true;
            }
            if(switched) i = -1;
        }

        
        // for testing
        List<int> log3_l = new List<int>();
        foreach(int i in res) log3_l.Add(i);
        this.log3 = log3_l;


        foreach(Action A in Player.Abilities){
            if(A.comboLevel >= cA.comboLevel){
                int resIndex = 0;
                while(resIndex < res.Count && AbilityDecayArray[A.abilityIndex] >= AbilityDecayArray[res[resIndex]]) resIndex++;

                if(resIndex < cA.comboLevel) res[resIndex] = A.abilityIndex;
            }
        }

        return res;
    }*/
#endregion
} // EOF