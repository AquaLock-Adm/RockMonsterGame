using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacter : Unit
{
    [Header("Player References:")]
    public Text NameText;
    public Slider HpSlider;
    public Slider ShieldSlider;
    public Slider ManaSlider;
    public BattleSystem BattleSystem;

    [Header("Player Stats:")]

    public Weapon Weapon;
    public Armor Armor;

    public int maxMana;
    public int mana;
    public int manaRegenRate;

    public List<Action> Abilities = new List<Action>();

    public List<SpellElement> Elements = new List<SpellElement>();

    public List<Item> Items = new List<Item>();

    public int healItemCount = 0;
    public int repairItemCount = 0;

    [Header("Proficiencies:")]
    // -1 -> not able to wield weapon

    public int baseSwordExp = -1;
    public int baseScytheExp = -1;
    public int baseAxeExp = -1;
    public int baseDaggerExp = -1;

    [Header("")]
    public int proficiencyLevel = 0;

    private void Update(){
        UpdateHUDElements();
        UpdateArmorDurability();
    }

    private void UpdateHUDElements(){
        this.HpSlider.value = this.healthPoints;
        this.HpSlider.maxValue = this.maxHealthPoints;

        this.ShieldSlider.value = this.shield;
        this.ShieldSlider.maxValue = this.maxShield;

        this.ManaSlider.value = this.mana;
        this.ManaSlider.maxValue = this.maxMana;
    }

    public void UpdateArmorDurability(){

        this.Armor.durability = this.healthPoints;
    }

    public void Setup(BattleSystem BS){
        if(this.Weapon != null) {
            this.Weapon = Instantiate(this.Weapon.gameObject).GetComponent<Weapon>();
            this.Weapon.Setup(this);
        }else Debug.Log("Weapon is not set");
        if(this.Armor != null) {
            this.Armor = Instantiate(this.Armor.gameObject).GetComponent<Armor>();
            this.Armor.Setup(this);
        }else Debug.Log("Armor is not set");
        this.BattleSystem = BS;
        CheckElementsList();
        SetupHUDElements();
        this.NameText.text = this.unitName;
        ManaRegenLoop();
    }

    public void CopyFrom(PlayerCharacter P_IN){
        this.attackIntoShieldMultiplier = P_IN.attackIntoShieldMultiplier;
        this.depletionTime = P_IN.depletionTime;
        this.pointsPerTick = P_IN.pointsPerTick;
        this.tickRate = P_IN.tickRate;
        this.textShowTime = P_IN.textShowTime;
        
        this.unitName = P_IN.unitName;
        this.element = P_IN.element;
        
        this.level = P_IN.level;
        
        this.healthPoints = P_IN.healthPoints;
        this.maxHealthPoints = P_IN.maxHealthPoints;
        
        this.shield = P_IN.shield;
        this.maxShield = P_IN.maxShield;
        
        this.attackMin = P_IN.attackMin;
        this.attackMax = P_IN.attackMax;
            
        this.accuracy = P_IN.accuracy;
        this.crit = P_IN.crit;
        
        this.Weapon = P_IN.Weapon;
        this.Armor = P_IN.Armor;

        // this.spentActionPoints = 0;

        // this.startActionPoints = P_IN.startActionPoints;
        // this.roundStartActionPointGain = P_IN.roundStartActionPointGain;
        // this.actionPointGain = P_IN.actionPointGain;
        // this.currentActionPoints = P_IN.currentActionPoints;

        this.maxMana = P_IN.maxMana;
        this.mana = P_IN.maxMana;
        this.manaRegenRate = P_IN.manaRegenRate;

        this.Abilities = P_IN.Abilities;

        this.Elements = P_IN.Elements;

        this.Items = P_IN.Items;

        this.healItemCount = P_IN.healItemCount;
        this.repairItemCount = P_IN.repairItemCount;

        this.baseSwordExp = P_IN.baseSwordExp;
        this.baseScytheExp = P_IN.baseScytheExp;
        this.baseAxeExp = P_IN.baseAxeExp;
        this.baseDaggerExp = P_IN.baseDaggerExp;

        this.proficiencyLevel = P_IN.proficiencyLevel;
    }

    public void AddAbility(Action A){
        this.Abilities.Add(A);
        SortAbilityList();
    }

    public void SortAbilityList(){
        this.Abilities.Sort((A1, A2) => A1.comboLevel.CompareTo(A2.comboLevel));
        foreach(Action A in this.Abilities){
            if(A.name == "Element"){
                this.Abilities.Remove(A);
                this.Abilities.Insert(0,A);
                break;
            }
        }
    }

    public bool AbilitiesContain(Action A){
        foreach(Action a_h in this.Abilities){
            if(a_h.name == A.name) return true;
        }
        return false;
    }

    public void RemoveFromAbilities(Action A){
        foreach(Action a_h in this.Abilities){
            if(a_h.name == A.name) {
                this.Abilities.Remove(a_h);
                break;
            }
        }
    }

    private void CheckElementsList(){
        List<SpellElement> Filtered = new List<SpellElement>();

        foreach(SpellElement E in this.Elements){
            if(!Filtered.Contains(E)) Filtered.Add(E);
        }

        this.Elements = Filtered;
    }

    private void SetupHUDElements() {
        this.NameText = this.BattleSystem.PlayerNameText;
        this.HpSlider = this.BattleSystem.PlayerHpSlider;
        this.ShieldSlider = this.BattleSystem.PlayerShieldSlider;
        this.ManaSlider = this.BattleSystem.PlayerManaSlider;
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
            if(!damageTesting) await DepleteShield(dmg);
        }else tasksTmp.Add(ShowDamageText(dmg));

        if(overkill > 0) {
            damageDealt += (int)Mathf.Min((float)overkill, (float)this.healthPoints);
            if(!damageTesting) tasksTmp.Add(DepleteHp(overkill));
            await Task.WhenAll(tasksTmp);
            if(!this.damageTesting && this.healthPoints <= 0) this.Death();
        }
        return damageDealt;
    }

    public override async void MissedAttack(){
        Debug.Log("Attack missed!");
        await Task.Yield();
    }

    public override void Crit(){
        Debug.Log("Crit!");
    }

    private async void ManaRegenLoop(){
        while(true && this.healthPoints > 0){
            if(BattleSystem.state == BattleState.PLAYERTURN){
                await Task.Delay(manaRegenRate);
                if(mana < maxMana) mana++;
            }
            await Task.Yield();
        }
    }

    public async void DepleteMana(int amount){
        if(amount == 0) return;

        if(amount < 0){
            this.mana = (int)Mathf.Clamp(this.mana - amount, 0.0f, (float)this.maxMana);
            return;
        }

        int cDepletion = 0;

        CalculateTickRate(amount);
        int timeWaited = 0;

        while(cDepletion < amount && this.mana >= 0 && this.healthPoints > 0){
            await Task.Delay(this.tickRate);
            timeWaited += this.tickRate;
            this.mana -= this.pointsPerTick;
            cDepletion += this.pointsPerTick;
        }

        if(this.healthPoints > 0){
            if(cDepletion > amount) this.mana += cDepletion - amount;
            await Task.Delay((int)Mathf.Clamp(this.depletionTime - timeWaited, 0.0f, this.depletionTime));
        }
    }
  
    public void CountAllItems(){
        foreach(Item I in Items){
            switch(I.type){
                case ItemType.heal:
                    healItemCount++;
                break;
                case ItemType.repair:
                    repairItemCount++;
                break;
                default:
                    Debug.Log("New Item type detect in Unit.CountAllItems()");
                break;
            }
        }
    }

    public override void Death(){

        BattleSystem.PlayerDies();
    }

    public override async Task ShowDamageText(int n){
        await Task.Yield();
        /*int cAmount = 0;
        Text cText = DamageTexts[damageTextIndex];
        damageTextIndex++;

        while(cAmount <= n){
            cText.text = cAmount.ToString();
            if(this.critTaken) cText.text += "!";
            await Task.Delay(depletionRate);
            cAmount++;
        }
        this.critTaken = false;*/
    }

    public void PrintStatus(){
        Debug.Log("Status Player "+ this.unitName+":");
        string s = "\nHUD References:\n"
                +  "NameText Set: "+ (this.NameText != null).ToString()+"\n"
                // +  "ApText Set: " + (this.ApText != null).ToString()+"\n"
                +  "HpSlider Set: " + (this.HpSlider != null).ToString()+"\n"
                +  "ShieldSlider Set: " + (this.ShieldSlider != null).ToString()+"\n"
                +  "ManaSlider Set: " + (this.ManaSlider != null).ToString()+"\n"
                +  "BattleSystem Set: " + (this.BattleSystem != null).ToString()+"\n\n"

                +  "Player Stats:\n"
                +  "Weapon: " + this.Weapon.weaponName+"\n"
                +  "Armor: " + this.Armor.armorName+"\n\n"

                // +  "SpentActionPoints: " + this.spentActionPoints.ToString()+"\n"
                // +  "StartActionPoints: " + this.startActionPoints.ToString()+"\n"
                // +  "RoundStartActionPointGain: " + this.roundStartActionPointGain.ToString()+"\n"
                // +  "ActionPointGain: " + this.actionPointGain.ToString()+"\n"
                // +  "CurrentActionPoints: " + this.currentActionPoints.ToString()+"\n\n"

                +  "MaxMana: " + this.maxMana.ToString()+"\n"
                +  "Mana: " + this.mana.ToString()+"\n"
                +  "ManaRegenRate: " + this.manaRegenRate.ToString()+"\n";
        Debug.Log(s);
        PrintAbilities();
        PrintElements();
        PrintItems();

        s = "Proficiencies: \n\n"
            + "BaseSwordExp: " + this.baseSwordExp.ToString()+"\n"
            + "BaseScytheExp: " + this.baseScytheExp.ToString()+"\n"
            + "BaseAxeExp: " + this.baseAxeExp.ToString()+"\n"
            + "BaseDaggerExp: " + this.baseDaggerExp.ToString()+"\n\n"

            + "ProficiencyLevel: " + this.proficiencyLevel.ToString()+"\n";

        Debug.Log(s);
    }

    public void PrintAbilities(){
        string s = "\nAbilities:\n\n";
        if(this.Abilities.Count == 0){
            s += "EMPTY";
        } else {
            foreach(Action A in this.Abilities){
                s += A.ToString()+"\n";
            } 
        }
        Debug.Log(s);
    }

    public void PrintElements(){
        string s = "\nElements:\n\n";
        if(this.Elements.Count == 0){
            s += "EMPTY";
        } else {
            foreach(SpellElement E in this.Elements){
                s += E.ToString()+"\n";
            } 
        }
        Debug.Log(s); 
    }

    public void PrintItems(){
        string s = "\nItems:\n\n";
        if(this.Items.Count == 0){
            s += "EMPTY";
        } else {
            foreach(Item I in this.Items){
                s += I.ToString()+"\n";
            } 
        }
        Debug.Log(s); 
    }
}