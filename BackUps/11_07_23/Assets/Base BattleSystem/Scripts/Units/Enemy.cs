using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemySettings{
    public int level;
    public string name;
    public int hp;

    public int baseKillPrice;
    public int maxKillPrice;

    public List<List<ShieldMode>>[] DefensiveModes = new List<List<ShieldMode>>[2];
    public int defensiveModeIndex = 0;

    public void CopyFromEnemyPrefab(GameObject Prefab){
        Enemy PrefabEnemy = Prefab.GetComponent<Enemy>();

        this.level = PrefabEnemy.GetLevel();
        this.name = PrefabEnemy.GetUnitName();
        this.hp = PrefabEnemy.GetHealthPoints();
        this.DefensiveModes = PrefabEnemy.GetDefensiveModes();
        this.defensiveModeIndex = PrefabEnemy.GetDefensiveModeIndex();
    }
}

public enum ShieldMode{
    LIGHT,
    HEAVY,
    SPECIAL,
    ANY,
    RANDOM,
    UNKNOWN,
    NONE
}

public class Enemy : Unit
{
    [Header("Visual References")]
    [SerializeField] private Text NameText;
    [SerializeField] private Text LevelText;
    [SerializeField] private Slider HpSlider;

    [SerializeField] private GameObject NextShieldVis;
    [SerializeField] private GameObject MiniShieldPrefab;
    [SerializeField] private GameObject MiniShieldCol;

    [SerializeField] private Color LightShieldColor;
    [SerializeField] private Color HeavyShieldColor;
    [SerializeField] private Color SpecialShieldColor;
    [SerializeField] private Color AnyShieldColor;
    [SerializeField] private Color RandomShieldColor;
    [SerializeField] private Color UnknownShieldColor;

    [Header("Enemy Settings")]
    [SerializeField] private bool damageTesting = false;
    [SerializeField] private Text[] DamageTexts = new Text[3];
    [SerializeField] private TextMeshProUGUI AttackRushDamageText;
    [SerializeField] private int damageTextIndex = 0;
    protected int displayingDamageTextCount = 0;

    [SerializeField] private bool critTaken = false;
    [SerializeField] protected bool heldByCombo = false;

    private int attackRushMin = 0;
    private int attackRushMax = 0;
    private float attackRushScaling = 1.0f;
    private int attackRushDamage = 0;

    protected BattleSystem BattleSystem;

    [Header("Enemy Stats:")]

    public bool deathTriggered;
    [SerializeField] protected int timeAfterDeath = 200;

    public List<List<ShieldMode>>[] DefensiveModes = new List<List<ShieldMode>>[2]; // DefensiveModes[0] => CurrentShieldModesList / DefensiveModes[1] => CurrentDiscoveredShieldsList
    [SerializeField] private int defensiveModeIndex = 0;

    [SerializeField] private List<ShieldMode> CurrentShieldModes = new List<ShieldMode>();
    [SerializeField] private List<ShieldMode> CurrentDiscoveredShields = new List<ShieldMode>();
    private int randomShieldIndex = -1;
    public ShieldMode nextShield;
    private int shieldModeIndex = 0;

    [SerializeField] private int level;

    [SerializeField] private int killPrice = 0;
    [SerializeField] private int maxKillPrice = 0; // at 400% HP damage dealt as overkill 

    private int overKill = 0;

    [SerializeField] private int itemDropChance;

#region Unity Functions

    void OnApplicationQuit(){
        Destroy(this.gameObject);
    }

    void OnDestroy(){
        this.deathTriggered = true;
    }
#endregion
    // changed in Enemy_Tutorial.cs
    public override async Task Death(){
        BattleSystem.state = BattleState.ENEMYDIED;
        UpdateEnemyHUD();
        while(this.displayingDamageTextCount > 0 && Application.isPlaying){
            await Task.Yield();
        }
        await Task.Delay(this.timeAfterDeath);

        EnemySettings mySettings = new EnemySettings();
        this.DefensiveModes[1][this.defensiveModeIndex] = this.CurrentDiscoveredShields;
        mySettings.CopyFromEnemyPrefab(this.gameObject);
        BattleSystem.UpdateEnemyLibrary(mySettings);

        CalcKillPrice();
        BattleSystem.EnemyDied(this);
        Destroy(this.gameObject);
    }

    public void BattleSetup(){
        if(!gameObject.activeSelf) gameObject.SetActive(true);
        this.AttackRushDamageText.gameObject.SetActive(false);

        SetCurrentShieldModes();
        
        this.nextShield = this.CurrentShieldModes[0];
        InitCurrentDiscoveredShields();
        SetShieldVis();

        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();

        while(damageTextIndex < 3){
            DamageTexts[damageTextIndex].text = "";
            damageTextIndex++;
        }
        damageTextIndex = 0;
        BattleUpdateLoop();
    }

    public async Task<bool> HandleAction(Action A){
        A.SetEnemy(this);
        bool addHeat = false;
        bool hitDetect = false;

        int damageDealt = await A.NormalExecute(BattleSystem);
        if(BattleSystem.state == BattleState.QUEUE){
            if(this.shieldModeIndex < this.CurrentShieldModes.Count){
                addHeat = CheckShieldMode(A.AbilityType);
                if(this.nextShield == ShieldMode.NONE && this.healthPoints <= 0){
                    this.deathTriggered = true;
                }
            }else {
                hitDetect = true;
                DealDamage(A.damage);
                int lifeGain = (int)Mathf.Ceil(A.damage * ((float)A.Player.GetLifeSteal()/100.0f));
                A.Player.Heal(lifeGain);
                addHeat = true;
            }
        }else {
            MissedAttack();
        }

        if(hitDetect) A.TriggerOnHit();
        if(this.deathTriggered) A.TriggerOnDeath();
        this.critTaken = false;
        return addHeat;
    }

    public async Task<int> HandleAttackRushFinisher(Action F){
        F.SetEnemy(this);
        await F.NormalExecute(BattleSystem);
        DealAttackRushDamage(F.damage);
        DisplayAttackRushDamage();
        int lifeGain = (int)Mathf.Ceil(F.damage * ((float)F.Player.GetLifeSteal()/100.0f));
        F.Player.Heal(lifeGain);
        await FadeOutAttackRushDamageText();
        return F.damage;
    }

    public virtual void UpdatePlaceHolder(){
        /*
        Used in Child classes to use functions on void Update()
        */
    }

    public int DealDamage(int dmg){
        int damageDealt = 0;

        // Debug.Log("Damage in: "+dmg.ToString());

        ShowDamageText(dmg);

        if(dmg > 0) {
            damageDealt += (int)Mathf.Min((float)dmg, (float)this.healthPoints);
            if(!this.damageTesting) {
                this.overKill += (int)Mathf.Min(0.0f, (float)(this.healthPoints - dmg)) *(-1);
                DepleteHp(dmg);
            }
        }
        return damageDealt;
    }

    public void DealWeaponDOT(int dmg){
        if(!this.damageTesting){
            if(this.healthPoints <= 0) return;
            this.healthPoints = (int)Mathf.Max((float)(this.healthPoints-dmg), 1f);
        }
    }

    public async void MissedAttack(){
        Text cText = DamageTexts[damageTextIndex];
        damageTextIndex = (damageTextIndex+1)%3;
        if(!this.deathTriggered) cText.text = "Miss!";
        await Task.Delay(this.textShowTime);
        if(!this.deathTriggered) cText.text = "";
    }

    public void Crit(){
        if(!this.gameObject.activeSelf){
            Debug.Log("Crit on "+this.unitName);
            return;
        }
        this.critTaken = true;
    }

    public void EnableAttackRush(int attackMin, int attackMax, float damageScaling){
        this.attackRushMin = attackMin;
        this.attackRushMax = attackMax;
        this.attackRushScaling = damageScaling;
        this.attackRushDamage = 0;
    }

    public void AttackRushHit(){
        if(this.nextShield != ShieldMode.NONE){
            BattleSystem.ActionQueue.CancelAttackRush();
            return;
        }
        int damage = (int)Mathf.Ceil(Random.Range(this.attackRushMin, this.attackRushMax+1) * this.attackRushScaling);

        DealAttackRushDamage(damage);
        DisplayAttackRushDamage();
    }

    public int GetAttackRushDamage(){
        return this.attackRushDamage;
    }

    private void DealAttackRushDamage(int dmg){
        if(dmg > 0) {
            if(!this.damageTesting) {
                this.overKill += (int)Mathf.Min(0.0f, (float)(this.healthPoints - dmg)) *(-1);
                DepleteHp(dmg);
            }
            this.attackRushDamage += dmg;
        }
    }

    private void DisplayAttackRushDamage(){
        this.AttackRushDamageText.gameObject.SetActive(true);
        this.AttackRushDamageText.text = this.attackRushDamage.ToString();
    }

    private async Task FadeOutAttackRushDamageText(){
        await Task.Delay(1000);
        if(Application.isPlaying) this.AttackRushDamageText.gameObject.SetActive(false);
    }

    public void ResetShield(){
        if(!this.deathTriggered){
            if(this.nextShield == ShieldMode.NONE) RotateShieldModes();

            this.shieldModeIndex = 0;
            this.nextShield = this.CurrentShieldModes[0];
            if(this.randomShieldIndex > 0)this.CurrentShieldModes[this.randomShieldIndex] = ShieldMode.RANDOM;
            SetShieldVis();
        }
    }

    public void Hide(bool enable){
        NameText.gameObject.SetActive(!enable);
        LevelText.gameObject.SetActive(!enable);
        HpSlider.gameObject.SetActive(!enable);
    }

    public void CopyFrom(Enemy Other){
        this.depletionTime = Other.depletionTime;
        this.pointsPerTick = Other.pointsPerTick;
        this.tickRate = Other.tickRate;
        this.textShowTime = Other.textShowTime;

        this.unitName = Other.unitName;

        this.level = Other.level;

        this.healthPoints = Other.healthPoints;
        this.maxHealthPoints = Other.maxHealthPoints;

        this.damageTesting = Other.damageTesting;
        
        this.killPrice = Other.killPrice;
        this.itemDropChance = Other.itemDropChance;
    }

    public void PrintStatus(){
        Debug.Log("Status Enemy "+ this.unitName+":");
        string s = "\nHUD References:\n"
                +  "NameText Set: "+ (this.NameText != null).ToString()+"\n"
                +  "LevelText Set: " + (this.LevelText != null).ToString()+"\n"
                +  "HpSlider Set: " + (this.HpSlider != null).ToString()+"\n"
                +  "BattleSystem Set: " + (this.BattleSystem != null).ToString()+"\n\n"

                +  "Enemy Stats:\n"

                +  "Level: " + this.level.ToString()+"\n"
                +  "Health: " + this.healthPoints.ToString()+"/"+this.maxHealthPoints.ToString()+"\n"
                +  "Kill Price: " + this.killPrice.ToString()+"\n";
        Debug.Log(s);
    }

    private async void BattleUpdateLoop(){
        while(!this.deathTriggered){
            if(BattleSystem.state == BattleState.PLAYERTURN || BattleSystem.state == BattleState.QUEUE || BattleSystem.state == BattleState.START){
                UpdateEnemyHUD();
            }
            // UpdatePlaceHolder();
            await Task.Yield();
        }
    }

    private async void ShowDamageText(int amount){
        if(amount < 0) return;

        this.displayingDamageTextCount++;

        int cAmount = 0;
        Text cText = DamageTexts[damageTextIndex];
        damageTextIndex = (damageTextIndex+1)%3;

        this.CalculateTickRate(amount);
        int timeWaited = 0;

        while(cAmount < amount){
            timeWaited += this.tickRate;
            if(cText != null){
                cText.text = cAmount.ToString();
                if(this.critTaken) cText.text += "!";
            }else break;
            cAmount += this.pointsPerTick;
            await Task.Delay(this.tickRate);
        }

        cText.text = amount.ToString();
        if(this.critTaken) cText.text += "!";
        this.critTaken = false;
        await Task.Delay((int)Mathf.Clamp(this.depletionTime - timeWaited, 0.0f, this.depletionTime));
        
        await Task.Delay(this.textShowTime);
        if(Application.isPlaying) cText.text = "";
        
        this.displayingDamageTextCount--;
    }
    
    protected void UpdateEnemyHUD(){
        NameText.text = unitName;
        LevelText.text = "Lv."+ this.level.ToString();
        HpSlider.value = healthPoints;
        HpSlider.maxValue = maxHealthPoints;
    }

    private void InitDefensiveModes(){
        this.DefensiveModes= new List<List<ShieldMode>>[2]; 
        this.DefensiveModes[0] = new List<List<ShieldMode>>();
        this.DefensiveModes[1] = new List<List<ShieldMode>>();
    }

    private void SetCurrentShieldModes(){
        if(this.defensiveModeIndex >= this.DefensiveModes[0].Count){
            Debug.LogError("Error: Setting DefensiveModes!");
            Debug.Log("Size:" + this.DefensiveModes[0].Count.ToString());
            Debug.Log("Index: " + this.defensiveModeIndex.ToString());
            return;
        }

        this.CurrentShieldModes = this.DefensiveModes[0][this.defensiveModeIndex];
        this.CurrentDiscoveredShields = this.DefensiveModes[1][this.defensiveModeIndex];
        if(this.CurrentShieldModes.Contains(ShieldMode.RANDOM)) this.randomShieldIndex = this.CurrentShieldModes.IndexOf(ShieldMode.RANDOM);
    }

    private void InitCurrentDiscoveredShields(){
        if(this.CurrentDiscoveredShields.Count > 0) return;
        this.CurrentDiscoveredShields = new List<ShieldMode>();

        this.CurrentDiscoveredShields.Add(this.CurrentShieldModes[0]);
        for(int i=1;i<this.CurrentShieldModes.Count;i++){
            this.CurrentDiscoveredShields.Add(ShieldMode.UNKNOWN);
        }
    }

    private void RotateShieldModes(){
        this.DefensiveModes[0][this.defensiveModeIndex] = this.CurrentShieldModes;
        this.DefensiveModes[1][this.defensiveModeIndex] = this.CurrentDiscoveredShields;

        if(this.DefensiveModes[0].Count < 1) return;

        this.defensiveModeIndex = (this.defensiveModeIndex+1)%this.DefensiveModes[0].Count;

        this.CurrentShieldModes = this.DefensiveModes[0][this.defensiveModeIndex];
        this.CurrentDiscoveredShields = this.DefensiveModes[1][this.defensiveModeIndex];
        if(this.CurrentDiscoveredShields.Count < this.CurrentShieldModes.Count) InitCurrentDiscoveredShields();
    }

    private bool CheckShieldMode(AbilityType aType){
        bool addHeat = false;
        if(this.CurrentShieldModes[this.shieldModeIndex] == ShieldMode.ANY || (int)aType == (int)this.CurrentShieldModes[this.shieldModeIndex]){
            this.shieldModeIndex++;
            addHeat = true;
        }
        if(this.shieldModeIndex < this.CurrentShieldModes.Count) {
            this.nextShield = this.CurrentShieldModes[this.shieldModeIndex];
            if(this.shieldModeIndex == this.randomShieldIndex) this.CurrentDiscoveredShields[this.shieldModeIndex] = ShieldMode.RANDOM;
            else this.CurrentDiscoveredShields[this.shieldModeIndex] = this.CurrentShieldModes[this.shieldModeIndex];
        }
        else this.nextShield = ShieldMode.NONE;
        SetShieldVis();
        return addHeat;
    }

    private void RollRandomShield(){
        int ran = Random.Range(1,41);

        if(ran <=10) this.CurrentShieldModes[this.randomShieldIndex] = ShieldMode.SPECIAL;
        else if(ran <= 20) this.CurrentShieldModes[this.randomShieldIndex] = ShieldMode.HEAVY;
        else if(ran <= 30) this.CurrentShieldModes[this.randomShieldIndex] = ShieldMode.ANY;
        else this.CurrentShieldModes[this.randomShieldIndex] = ShieldMode.LIGHT;
    }

    private char AbilityTypeToChar(AbilityType aType){
        char res = ' ';
        switch(aType){
            case AbilityType.LIGHT:
                res = 'L';
            break;

            case AbilityType.HEAVY:
                res = 'H';
            break;

            case AbilityType.SPECIAL:
                res = 'S';
            break;

            default:
                res = ' ';
                Debug.LogError("Forgot to add AbilityType to switch!");
            break;
        }

        return res;
    }

    private void DepleteHp(int amount){
        if(amount <= 0) return;

        this.healthPoints = (int)Mathf.Max((float)(this.healthPoints-amount), 0.0f);
        if(this.healthPoints <= 0) this.deathTriggered = true;
    }

    private void SetShieldVis(){
        SetMiniShields();

        if(!this.NextShieldVis.activeSelf) this.NextShieldVis.SetActive(true);
        Image shieldImg = this.NextShieldVis.GetComponent<Image>();
        switch(this.nextShield){
            case ShieldMode.LIGHT:
                shieldImg.color = new Color(this.LightShieldColor.r,this.LightShieldColor.g,this.LightShieldColor.b);
            break;
            case ShieldMode.HEAVY:
                shieldImg.color = new Color(this.HeavyShieldColor.r,this.HeavyShieldColor.g,this.HeavyShieldColor.b);
            break;
            case ShieldMode.SPECIAL:
                shieldImg.color = new Color(this.SpecialShieldColor.r,this.SpecialShieldColor.g,this.SpecialShieldColor.b);
            break;
            case ShieldMode.ANY:
                shieldImg.color = new Color(this.AnyShieldColor.r,this.AnyShieldColor.g,this.AnyShieldColor.b);
            break;
            case ShieldMode.NONE:
                this.NextShieldVis.SetActive(false);
            break;

            default:
                Debug.LogError("Missing ShieldMode in Switch!");
            break;
        }
    }

    private void SetMiniShields(){
        foreach(Transform child in this.MiniShieldCol.transform){
            Destroy(child.gameObject);
        }

        for(int msi = this.shieldModeIndex+1; msi < this.CurrentShieldModes.Count; msi++){
            GameObject MiniShieldGO = Instantiate(this.MiniShieldPrefab, this.MiniShieldCol.transform);
            Color clr = GetMiniShieldColor(msi);
            MiniShieldGO.GetComponent<Image>().color = new Color(clr.r,clr.g,clr.b);
        }
    }

    private Color GetMiniShieldColor(int shieldIndex){
        if(shieldIndex > this.CurrentDiscoveredShields.Count){
            Debug.LogError("Index out of bounds!");
            return new Color();
        }
        Color res = new Color();

        switch(this.CurrentDiscoveredShields[shieldIndex]){
            case ShieldMode.UNKNOWN:
                res = new Color(this.UnknownShieldColor.r,this.UnknownShieldColor.g,this.UnknownShieldColor.b);
            break;

            case ShieldMode.LIGHT:
                res = new Color(this.LightShieldColor.r,this.LightShieldColor.g,this.LightShieldColor.b);
            break;
            case ShieldMode.HEAVY:
                res = new Color(this.HeavyShieldColor.r,this.HeavyShieldColor.g,this.HeavyShieldColor.b);
            break;
            case ShieldMode.SPECIAL:
                res = new Color(this.SpecialShieldColor.r,this.SpecialShieldColor.g,this.SpecialShieldColor.b);
            break;
            case ShieldMode.ANY:
                res = new Color(this.AnyShieldColor.r,this.AnyShieldColor.g,this.AnyShieldColor.b);
            break;
            case ShieldMode.RANDOM:
                res = new Color(this.RandomShieldColor.r,this.RandomShieldColor.g,this.RandomShieldColor.b);
            break;

            default:
                Debug.LogError("Color missing in Switch!");
            break;
        }

        return res;
    }

    private void CalcKillPrice(){
        // Debug.Log("Overkill: "+this.overKill.ToString()+", MaxHP: " +this.maxHealthPoints.ToString());

        int overkillToMaxHealth_p;

        if(this.maxHealthPoints == 0) overkillToMaxHealth_p = this.overKill*100;
        else overkillToMaxHealth_p = (int)Mathf.Round(((float)this.overKill/(float)this.maxHealthPoints)*100);

        // Debug.Log("OverKillperc: "+overkillToMaxHealth_p.ToString()+"%");
        // Debug.Log("baseKillPrice: "+this.killPrice.ToString());

        int maxKillPrice_p = overkillToMaxHealth_p/4;
        int bonusCredit = (int)Mathf.Round(((float)this.maxKillPrice/100) * maxKillPrice_p);
        bonusCredit = (int)Mathf.Min(bonusCredit+this.killPrice, this.maxKillPrice) - this.killPrice;
        // Debug.Log("Bonus: " + bonusCredit.ToString());

        this.killPrice += bonusCredit;
        // Debug.Log("Calc KP: "+this.killPrice.ToString());
    }

#region Getter - Setter Functions

    public int GetKillPrice(){
        return this.killPrice;
    }

    public int GetItemDropChance(){
        return this.itemDropChance;
    }

    public int GetLevel(){
        return this.level;
    }

    public List<List<ShieldMode>>[] GetDefensiveModes(){
        return this.DefensiveModes;
    }

    public int GetDefensiveModeIndex(){
        return this.defensiveModeIndex;
    }

    public void SetLevel(int lv){
     this.level = lv;
    }

    public void SetHeldByCombo(bool on){
        this.heldByCombo = on;
        if(on && this.randomShieldIndex > 0) RollRandomShield();
    }

    public void CopyFromEnemySettings(EnemySettings Settings){
        this.level = Settings.level;
        this.unitName = Settings.name;
        this.maxHealthPoints = Settings.hp;
        this.healthPoints = Settings.hp;

        this.killPrice = Settings.baseKillPrice;
        this.maxKillPrice = Settings.maxKillPrice;

        this.DefensiveModes[0] = Settings.DefensiveModes[0];
        this.DefensiveModes[1] = Settings.DefensiveModes[1];
        this.defensiveModeIndex = Settings.defensiveModeIndex;
    }
#endregion
}