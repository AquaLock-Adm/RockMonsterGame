using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Menu { MAIN, FINALACTION, FULLQUEUE, OTHER }

public class PlayerControl : MonoBehaviour
{
    private PlayerCharacter Player;

    private Menu CurrentMenu;

    private List<Action> CurrentMenuAttacks = new List<Action>();

    [SerializeField] private bool stopInputs = false;

    [SerializeField] private bool blockInputW = false;
    [SerializeField] private bool blockInputA = false;
    [SerializeField] private bool blockInputS = false;
    [SerializeField] private bool blockInputD = false;

    private int basicAbilitiesCount = 0;

    private List<string> MainMenuTexts = new List<string>();

    private bool queueAttackRushW = false;
    private bool queueAttackRushA = false;
    private bool queueAttackRushS = false;

    private bool attackRushInputsEnabled = false;

    [Header("HUD References")]

    [SerializeField] private GameObject InputDarkFilter;
    [SerializeField] private List<TextMeshProUGUI> ButtonTexts = new List<TextMeshProUGUI>();



    public void Setup(PlayerCharacter Player, GameObject DarkFilter, List<TextMeshProUGUI> ButtonTexts)
    {
        this.Player = Player;
        this.InputDarkFilter = DarkFilter;
        this.InputDarkFilter.SetActive(false);
        this.ButtonTexts = ButtonTexts;

        SetupButtonTexts();
        LoadMainMenu();

        BattleUpdateLoop();
    }

    private void SetupButtonTexts()
    {

        InstantiateAttackButtons();

        foreach (TextMeshProUGUI t in this.ButtonTexts)
        {
            this.MainMenuTexts.Add(t.text);
        }
    }

    private void InstantiateAttackButtons()
    {
        this.basicAbilitiesCount = 0;

        List<Action> PlayerAbilities = Player.GetAbilityList();

        for (int i = 0; i < this.ButtonTexts.Count; i++)
        {

            if (i == 3) this.ButtonTexts[i].text = "Cancel";

            else if (i < PlayerAbilities.Count && PlayerAbilities[i].comboLevel == 1)
            {
                this.basicAbilitiesCount++;
                this.ButtonTexts[i].text = PlayerAbilities[i].cover;
                this.CurrentMenuAttacks.Add(PlayerAbilities[i]);
            }

            else this.ButtonTexts[i].text = "";
        }
    }

    public void EnableAttackRushInputs(bool on)
    {
        this.InputDarkFilter.SetActive(!on);
        this.attackRushInputsEnabled = on;
    }

    private async void BattleUpdateLoop()
    {
        while (!Player.deathTriggered && Application.isPlaying)
        {
            if (!this.stopInputs) CheckInputs();
            else this.InputDarkFilter.SetActive(true);

            await Task.Yield();
        }
    }

    public async void BlockAllInputsFor(int time_ms)
    {
        this.stopInputs = true;

        await Task.Delay(time_ms);

        this.stopInputs = false;
    }

    public void BlockPlayerInputs(List<bool> playerInputBlockList)
    {
        this.blockInputW = playerInputBlockList[0];
        this.blockInputA = playerInputBlockList[1];
        this.blockInputS = playerInputBlockList[2];
        this.blockInputD = playerInputBlockList[3];

        switch (this.CurrentMenu)
        {
            case Menu.MAIN:
                LoadMainMenu();
                break;
            case Menu.FINALACTION:
                LoadComboAbilityMenu();
                break;
            case Menu.FULLQUEUE:
                LoadRoundOverMenu();
                break;
        }
    }


    #region Player Input Functions
    private void CheckInputs()
    {
        switch (Player.state)
        {
            case PlayerState.START:
            case PlayerState.PLAYERTURN:
                this.InputDarkFilter.SetActive(false);
                BattleInputs();
                break;

            case PlayerState.ATTACKRUSH:
                if (this.attackRushInputsEnabled) AttackRushInputs();
                else this.InputDarkFilter.SetActive(true);
                break;

            default:
                this.InputDarkFilter.SetActive(true);
                break;
        }
    }

    private void BattleInputs()
    {
        if (Player.defendModeActive)
        {
            if (this.CurrentMenu == Menu.MAIN || this.CurrentMenu == Menu.FINALACTION)
            {
                NormalAttackCastInputs();
            }
            else if (CurrentMenu == Menu.FULLQUEUE)
            {
                FullQueueInputs();
            }
        }
        else
        {
            QueueAttackRushInputs();

            if (this.CurrentMenu == Menu.MAIN)
            {
                if (Player.state == PlayerState.START && !Player.HeatChargeIsDone() && Player.GetCurrentActionCount() <= 0)
                {
                    HeatChargeInputs();
                }
                else if (Player.GetCurrentActionCount() <= 0)
                {
                    HeatDischargeInputs();
                }
                NormalAttackCastInputs();
            }
            else if (this.CurrentMenu == Menu.FINALACTION)
            {
                FinalActionInputs();
            }
            else if (CurrentMenu == Menu.FULLQUEUE)
            {
                FullQueueInputs();
            }
        }
    }

    private void AttackRushInputs()
    {
        if (Input.GetKeyDown(KeyCode.W) && !this.blockInputW)
        {
            Player.GetCurrentEnemy().AttackRushHit();
        }
        else if (Input.GetKeyDown(KeyCode.A) && !this.blockInputA)
        {
            Player.GetCurrentEnemy().AttackRushHit();
        }
        else if (Input.GetKeyDown(KeyCode.S) && !this.blockInputS)
        {
            Player.GetCurrentEnemy().AttackRushHit();
        }
    }

    private void NormalAttackCastInputs()
    {
        int abilityIndex = -1;
        if (Input.GetKeyDown(KeyCode.D) && !this.blockInputD && Player.GetCurrentActionCount() > 0 && Player.HeatChargeIsDone())
        {
            StopAttackRushQueue();
            Player.CancelLastAction();
            LoadMainMenu();
            return;
        }
        else if (Input.GetKeyDown(KeyCode.W) && !this.blockInputW && this.basicAbilitiesCount > 2)
        {
            if (!queueAttackRushW) StopAttackRushQueue();
            if (IsChargeCheck('W')) return;
            abilityIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.A) && !this.blockInputA && this.basicAbilitiesCount > 1)
        {
            if (!queueAttackRushA) StopAttackRushQueue();
            if (IsChargeCheck('A')) return;
            abilityIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.S) && !this.blockInputS && this.basicAbilitiesCount > 0)
        {
            if (!queueAttackRushS) StopAttackRushQueue();
            if (IsChargeCheck('S')) return;
            abilityIndex = 0;
        }
        else return;

        if (Player.defendModeActive) Player.CastBlock(abilityIndex);
        else Player.CastAttack(abilityIndex);
    }

    private void FinalActionInputs()
    {
        if (Input.GetKeyDown(KeyCode.D) && !this.blockInputD)
        {
            StopAttackRushQueue();
            Player.CancelLastAction();
            LoadMainMenu();
        }
        else if (Input.GetKeyDown(KeyCode.W) && !this.blockInputW && this.basicAbilitiesCount > 2)
        {
            if (!this.queueAttackRushW) StopAttackRushQueue();
            if (IsChargeCheck('W')) return;
            Player.CastAttack(Player.GetAbilityIndexByString(this.CurrentMenuAttacks[2].name));
        }
        else if (Input.GetKeyDown(KeyCode.A) && !this.blockInputA && this.basicAbilitiesCount > 1)
        {
            if (!this.queueAttackRushA) StopAttackRushQueue();
            if (IsChargeCheck('A')) return;
            Player.CastAttack(Player.GetAbilityIndexByString(this.CurrentMenuAttacks[1].name));
        }
        else if (Input.GetKeyDown(KeyCode.S) && !this.blockInputS && this.basicAbilitiesCount > 0)
        {
            if (!this.queueAttackRushS) StopAttackRushQueue();
            if (IsChargeCheck('S')) return;
            Player.CastAttack(Player.GetAbilityIndexByString(this.CurrentMenuAttacks[0].name));
        }
    }

    private void FullQueueInputs()
    {
        if (Input.GetKeyDown(KeyCode.D) && !this.blockInputD)
        {
            StopAttackRushQueue();
            Player.CancelLastAction();
            if (Player.GetCurrentActionCount() <= 1) LoadMainMenu();
            else LoadComboAbilityMenu();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            StopAttackRushQueue();
            Player.PassRound();
            LoadMainMenu();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            StopAttackRushQueue();
            Player.PassRound();
            LoadMainMenu();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StopAttackRushQueue();
            Player.PassRound();
            LoadMainMenu();
        }
    }

    private void QueueAttackRushInputs()
    {
        if (Input.GetKeyDown(KeyCode.W) && !this.blockInputW)
        {
            if (IsChargeCheck('W')) return;
            this.queueAttackRushW = true;
            Player.StartAttackRushQueue();
        }
        else if (Input.GetKeyUp(KeyCode.W) && !this.blockInputW && this.queueAttackRushW)
        {
            StopAttackRushQueue();
        }
        else if (Input.GetKeyDown(KeyCode.A) && !this.blockInputA && !this.queueAttackRushW && !this.queueAttackRushS)
        {
            if (IsChargeCheck('A')) return;
            this.queueAttackRushA = true;
            Player.StartAttackRushQueue();
        }
        else if (Input.GetKeyUp(KeyCode.A) && !this.blockInputA && this.queueAttackRushA)
        {
            StopAttackRushQueue();
        }
        else if (Input.GetKeyDown(KeyCode.S) && !this.blockInputS && !this.queueAttackRushW && !this.queueAttackRushA)
        {
            if (IsChargeCheck('S')) return;
            this.queueAttackRushS = true;
            Player.StartAttackRushQueue();
        }
        else if (Input.GetKeyUp(KeyCode.S) && !this.blockInputS && this.queueAttackRushS)
        {
            StopAttackRushQueue();
        }
    }

    private void HeatChargeInputs()
    {
        if (Input.GetKeyDown(KeyCode.D) && !this.blockInputD)
        {
            StopAttackRushQueue();
            Player.StartHeatChargeLoop();
        }
        else if (Input.GetKeyUp(KeyCode.D) && !this.blockInputD)
        {
            Player.StopHeatChargeLoop();
        }
    }

    private void HeatDischargeInputs()
    {
        if (Input.GetKeyDown(KeyCode.D) && !this.blockInputD)
        {
            StopAttackRushQueue();
            Player.StartHeatDischargeLoop();
        }
        else if (Input.GetKeyUp(KeyCode.D) && !this.blockInputD)
        {
            StopAttackRushQueue();
            Player.StopHeatDischargeLoop();
        }
    }
    #endregion


    #region Player Input Helper Functions
    private void StopAttackRushQueue()
    {
        this.queueAttackRushW = false;
        this.queueAttackRushA = false;
        this.queueAttackRushS = false;
        Player.StopAttackRushQueue();
    }

    private bool IsChargeCheck(char key)
    {
        return (Player.IsInHeatCharge() || Player.IsInHeatDischarge());
    }
    #endregion



    #region Menu Loaders
    public void LoadMainMenu()
    {
        this.CurrentMenu = Menu.MAIN;

        if (Player.defendModeActive)
        {
            LoadBlocksMenu();
        }
        else
        {
            LoadAttacksMenu();
        }
    }

    private void LoadBlocksMenu()
    {
        List<Action> BlocksList = Player.GetWeapon().Blocks;

        for (int i = 0; i < ButtonTexts.Count - 1; i++)
        {
            ButtonTexts[i].text = BlocksList[i].name;
        }
        ButtonTexts[3].text = "Cancel";
    }

    private void LoadAttacksMenu()
    {
        foreach (TextMeshProUGUI T in this.ButtonTexts)
        {
            T.text = "";
        }

        if (!this.blockInputS && Player.GetAbilityList().Count > 0) ButtonTexts[0].text = this.MainMenuTexts[0];
        if (!this.blockInputA && Player.GetAbilityList().Count > 1) ButtonTexts[1].text = this.MainMenuTexts[1];
        if (!this.blockInputW && Player.GetAbilityList().Count > 2) ButtonTexts[2].text = this.MainMenuTexts[2];

        if (!this.blockInputD)
        {
            if (!Player.HeatChargeIsDone() && Player.IsHeatChargeAvailable()) this.ButtonTexts[3].text = "Charge";
            else if (Player.HeatChargeIsDone() && Player.GetCurrentActionCount() <= 0 && Player.IsHeatChargeAvailable()) this.ButtonTexts[3].text = "Discharge";
            else this.ButtonTexts[3].text = "Cancel";
        }
    }

    public void LoadComboAbilityMenu()
    {
        if (Player.defendModeActive)
        {
            LoadBlocksMenu();
        }
        else
        {
            LoadComboAttacksMenu();
        }
        this.CurrentMenu = Menu.FINALACTION;
    }

    private void LoadComboAttacksMenu()
    {
        List<Action> PlayerAbilities = Player.GetAbilityList();

        if (!this.blockInputS && Player.GetAbilityList().Count > 0) this.CurrentMenuAttacks[0] = Player.PredictComboAbilityWith(PlayerAbilities[0]);
        if (!this.blockInputA && Player.GetAbilityList().Count > 1) this.CurrentMenuAttacks[1] = Player.PredictComboAbilityWith(PlayerAbilities[1]);
        if (!this.blockInputW && Player.GetAbilityList().Count > 2) this.CurrentMenuAttacks[2] = Player.PredictComboAbilityWith(PlayerAbilities[2]);

        ChangeToNewActionButtons();
    }

    private void ChangeToNewActionButtons()
    {

        foreach (TextMeshProUGUI T in this.ButtonTexts)
        {
            T.text = "";
        }

        if (!this.blockInputS && Player.GetAbilityList().Count > 0) this.ButtonTexts[0].text = this.CurrentMenuAttacks[0].cover;
        if (!this.blockInputA && Player.GetAbilityList().Count > 1) this.ButtonTexts[1].text = this.CurrentMenuAttacks[1].cover;
        if (!this.blockInputW && Player.GetAbilityList().Count > 2) this.ButtonTexts[2].text = this.CurrentMenuAttacks[2].cover;

        if (!this.blockInputD)
        {
            if (!Player.HeatChargeIsDone() && Player.IsHeatChargeAvailable()) this.ButtonTexts[3].text = "Charge";
            else if (Player.HeatChargeIsDone() && Player.GetCurrentActionCount() <= 0 && Player.IsHeatChargeAvailable()) this.ButtonTexts[3].text = "Discharge";
            else this.ButtonTexts[3].text = "Cancel";
        }
    }

    public void LoadRoundOverMenu()
    {
        for (int i = 0; i < 3; i++)
        {
            this.ButtonTexts[i].text = "End Round";
        }
        this.ButtonTexts[3].text = "Cancel";
        this.CurrentMenu = Menu.FULLQUEUE;
    }
    #endregion

}