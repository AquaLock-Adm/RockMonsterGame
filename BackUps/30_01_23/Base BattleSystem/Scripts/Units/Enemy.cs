using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : Unit
{
    [SerializeField] public Text NameText;
    [SerializeField] public Text LevelText;
    [SerializeField] public Slider HpSlider;
    [SerializeField] public Slider ShieldSlider;

    [SerializeField] public Text[] DamageTexts = new Text[3];
    [SerializeField] public int damageTextIndex = 0;

    [SerializeField] public bool stopAttack = true;
    [SerializeField] public bool critTaken = false;


    public BattleSystem BattleSystem;

    [Header("Enemy Stats:")]

    private bool deathTriggered;

    public int moveSpeed = 10; // 10 <-> 500
    
    public int killPrice;
    public int killExp;

    [Header("Spawn Value between 1-10")]
    public int spawnValue;
    public int itemDropChance;

    public int damageToWeapons;

    [SerializeField] private float way = 0.0f;
    [SerializeField] private float maxWay = 1000.0f;
    public Vector3 origPos;

    [Header("Base Stats at lvl 1:")]
    
    [SerializeField] private int baseHp;
    [SerializeField] private int baseShield;
    [SerializeField] private int baseAtkMax;
    [SerializeField] private int baseAtkMin;
    [SerializeField] private int baseMoveSpeed;
    [SerializeField] private int baseDamageToWeapons;
    [SerializeField] private int baseKillPrice;
    [SerializeField] private int baseKillExp;

    [Header("Stats change depending on what level is rolled.")]

    [Header("HP Increase per level:")]
    [SerializeField] private int hpIncrease;
    [SerializeField] private int hpIncreaseStartLevel;
    [SerializeField] private int hpIncreaseMax;
    [SerializeField] private bool hpMaxSet;

    [Header("Shield Increase per level:")]
    [SerializeField] private int shieldIncrease;
    [SerializeField] private int shieldIncreaseStartLevel;
    [SerializeField] private int shieldIncreaseMax;
    [SerializeField] private bool shieldMaxSet;

    [Header("Attack Increase per level:")]
    [SerializeField] private int atkMaxIncrease;
    [SerializeField] private int atkMaxIncreaseStartLevel;
    [SerializeField] private int atkMaxIncreaseMax;
    [SerializeField] private bool atkMaxMaxSet;
    [SerializeField] private int atkMinIncrease;
    [SerializeField] private int atkMinIncreaseStartLevel;
    [SerializeField] private int atkMinIncreaseMax;
    [SerializeField] private bool atkMinMaxSet;

    [Header("Speed Decrease per level:")]
    [SerializeField] private int spdIncrease;
    [SerializeField] private int spdIncreaseStartLevel;
    [SerializeField] private int spdIncreaseMax;
    [SerializeField] private bool spdMaxSet;

    [Header("Damage to Weapons Increase per level:")]
    [SerializeField] private int wpDmgIncrease;
    [SerializeField] private int wpDmgIncreaseStartLevel;
    [SerializeField] private int wpDmgIncreaseMax;
    [SerializeField] private bool wpDmgMaxSet;

    [Header("Kill Credit Increase per level:")]
    [SerializeField] private int cdIncrease;
    [SerializeField] private int cdIncreaseStartLevel;
    [SerializeField] private int cdIncreaseMax;
    [SerializeField] private bool cdMaxSet;

    [Header("Exp Gain Increase per level:")]
    [SerializeField] private int expIncrease;
    [SerializeField] private int expIncreaseStartLevel;
    [SerializeField] private int expIncreaseMax;
    [SerializeField] private bool expMaxSet;

    public float GetWay(){
        return this.way;
    }

    void Awake(){
        SetupStats();
    }

    void Update(){
        if(this.BattleSystem.state == BattleState.PLAYERTURN || this.BattleSystem.state == BattleState.QUEUE){
            UpdateEnemyHUD();
            Move();
        }
        // UpdatePlaceHolder();
    }

    void OnDestroy(){
        //Debug.Log("Destroying "+this.unitName);
        this.stopAttack = true;
    }

    public void Setup(){
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();

        while(damageTextIndex < 3){
            DamageTexts[damageTextIndex].text = "";
            damageTextIndex++;
        }
        damageTextIndex = 0;

        this.origPos = transform.position;
        this.attackIntoShieldMultiplier = this.BattleSystem.attackIntoShieldMultiplier;
    }

    private void Move(){
        // IMPORTANT! Moving Enemy gameObject on top of enemy box
        float deltaWay = -1 * Time.deltaTime * this.moveSpeed;
        this.way += Mathf.Abs(deltaWay);
        transform.position += new Vector3(deltaWay, 0, 0);

        if(this.way >= this.maxWay){
            ReachedPlayer();
        }
    }

    private void ReachedPlayer(){
        // Debug.Log("Reached Player!");
        transform.position = this.origPos;
        this.way = 0.0f;
    }
    
    public void UpdateEnemyHUD(){
        NameText.text = unitName;
        LevelText.text = "Lv."+ this.level.ToString();
        HpSlider.value = healthPoints;
        HpSlider.maxValue = maxHealthPoints;

        ShieldSlider.value = shield;
        ShieldSlider.maxValue = maxShield; 
    }

    public virtual void UpdatePlaceHolder(){
        /*
        Used in Child classes to use functions on void Update()
        */
    }

    public override async Task<int> DealDamage(int dmg){
        int overkill = dmg;
        int damageDealt = 0;
        List<Task> tasksTmp = new List<Task>();

        if(shield > 0){
            dmg = (int)Mathf.Round(dmg*attackIntoShieldMultiplier);
            tasksTmp.Add(ShowDamageText(dmg));
            damageDealt = (int)Mathf.Min((float)dmg, (float)this.shield);
            overkill = (int)Mathf.Clamp(dmg-this.shield, 0.0f, float.MaxValue);
            // if(!damageTesting) await DepleteShield(dmg);
        }else tasksTmp.Add(ShowDamageText(dmg));

        if(overkill > 0) {
            damageDealt += (int)Mathf.Min((float)overkill, (float)this.healthPoints);
            if(!this.damageTesting) tasksTmp.Add(DepleteHp(overkill));
            await Task.WhenAll(tasksTmp);
            if(!this.damageTesting && this.healthPoints <= 0) this.Death();
        }
        return damageDealt;
    }

    public void KnockBack(int knockBackUnits){
        if(knockBackUnits < 0){
            Debug.LogError("Expected Knockback to be >= 0.");
            return;
        }
        knockBackUnits = (int)Mathf.Clamp((float)knockBackUnits, 0.0f, this.way);
        this.way -= knockBackUnits;
        transform.position -= new Vector3(-knockBackUnits, 0, 0);
    }

    public override void Crit(){
        if(!this.gameObject.activeSelf){
            Debug.Log("Crit on "+this.unitName);
            return;
        }
        this.critTaken = true;
    }

    public override async void MissedAttack(){
        if(!this.gameObject.activeSelf) {
            Debug.Log("Missed attack on "+this.unitName);
            return;
        }
        Text cText = DamageTexts[damageTextIndex];
        damageTextIndex = (damageTextIndex+1)%3;
        cText.text = "Miss!";
        await Task.Delay(this.textShowTime);
        cText.text = "";
    }

    public void Hide(bool enable){
        NameText.gameObject.SetActive(!enable);
        LevelText.gameObject.SetActive(!enable);
        HpSlider.gameObject.SetActive(!enable);
        ShieldSlider.gameObject.SetActive(!enable);
    }

    public void RollRandomLevel(int levelMin, int levelMax){
        this.level = Random.Range(levelMin, levelMax+1);
    }

    public void SetupStats(){
        if(level >= hpIncreaseStartLevel) {
            maxHealthPoints = baseHp + (hpIncrease * (level-hpIncreaseStartLevel));
            if(hpMaxSet && maxHealthPoints > hpIncreaseMax) maxHealthPoints = hpIncreaseMax; 
            healthPoints = maxHealthPoints;
        }

        if(level >= shieldIncreaseStartLevel) {
            maxShield = baseShield + (shieldIncrease * (level-shieldIncreaseStartLevel));
            if(shieldMaxSet && maxShield > shieldIncreaseMax) maxShield = shieldIncreaseMax; 
            shield = maxShield;
        }

        if(level >= atkMaxIncreaseStartLevel) {
            attackMax = baseAtkMax + (atkMaxIncrease * (level-atkMaxIncreaseStartLevel));
            if(atkMaxMaxSet && attackMax > atkMaxIncreaseMax) attackMax = atkMaxIncreaseMax; 
        }

        if(level >= atkMinIncreaseStartLevel) {
            attackMin = baseAtkMin + (atkMinIncrease * (level-atkMinIncreaseStartLevel));
            if(atkMinMaxSet && attackMin > atkMinIncreaseMax) attackMin = atkMinIncreaseMax; 
        }
            
        if(level >= spdIncreaseStartLevel) {
            moveSpeed = baseMoveSpeed + (spdIncrease * (level-spdIncreaseStartLevel));
            if(spdMaxSet && moveSpeed > spdIncreaseMax) moveSpeed = spdIncreaseMax; 
        }
              
        if(level >= wpDmgIncreaseStartLevel) {
            damageToWeapons = baseDamageToWeapons + (wpDmgIncrease * (level-wpDmgIncreaseStartLevel));
            if(wpDmgMaxSet && damageToWeapons > wpDmgIncreaseMax) damageToWeapons = wpDmgIncreaseMax; 
        }
              
        if(level >= cdIncreaseStartLevel) {
            killPrice = baseKillPrice + (cdIncrease * (level-cdIncreaseStartLevel));
            if(cdMaxSet && killPrice > cdIncreaseMax) killPrice = cdIncreaseMax; 
        }
              
        if(level >= expIncreaseStartLevel) {
            killExp = baseKillExp + (expIncrease * (level-expIncreaseStartLevel));
            if(expMaxSet && killExp > expIncreaseMax) killExp = expIncreaseMax; 
        }
    }

    public override void Death(){
        if(!this.deathTriggered){
            this.deathTriggered = true;
            BattleSystem.EnemyDies(this);
        }
    }

    public override async Task ShowDamageText(int amount){
        if(amount < 0) return;

        int cAmount = 0;
        Text cText = DamageTexts[damageTextIndex];
        damageTextIndex = (damageTextIndex+1)%3;

        this.CalculateTickRate(amount);
        int timeWaited = 0;

        while(cAmount < amount){
            await Task.Delay(this.tickRate);
            timeWaited += this.tickRate;

            cText.text = cAmount.ToString();
            if(this.critTaken) cText.text += "!";
            cAmount += this.pointsPerTick;
        }

        cText.text = amount.ToString();
        if(this.critTaken) cText.text += "!";
        this.critTaken = false;
        await Task.Delay((int)Mathf.Clamp(this.depletionTime - timeWaited, 0.0f, this.depletionTime));
        
        await Task.Delay(this.textShowTime);
        cText.text = "";
    }
}