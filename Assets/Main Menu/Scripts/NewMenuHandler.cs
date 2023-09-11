using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject GameHandlerPrefab; 
    private GameHandler GameHandler;
    private PlayerCharacter Player;

    private int currentButtonIndex = 0;
    private int currentMenuIndex = 0;

    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] GameObject WeaponPrefab;
    [SerializeField] GameObject ArmorPrefab;

    [SerializeField] private TextMeshProUGUI PlayerNameText, CreditsText, HealthText;
    [SerializeField] private TextMeshProUGUI WeaponLvlText, AtkText, APRText, LifeDrainText, EnemyDotText, WeaponUpgradeCostText;
    [SerializeField] private TextMeshProUGUI NextWeaponLvlText, NextAtkText, NextAPRText, NextLifeDrainText, NextEnemyDotText;
    [SerializeField] private TextMeshProUGUI ArmorLvlText, HPText, ManaText, ManaRegText, ArmorUpgradeCostText;
    [SerializeField] private TextMeshProUGUI NextArmorLvlText, NextHPText, NextManaText, NextManaRegText;
    
    [SerializeField] private List<GameObject> ButtonList = new List<GameObject>();
    [SerializeField] private List<GameObject> MenuList = new List<GameObject>();

    void Awake(){
        GameObject GHGO = GameObject.Find("GameHandler");

        if(GHGO == null){
            if(this.GameHandlerPrefab == null){
                Debug.LogError("No Game Handler Prefab Set!");
                return;
            }

            GHGO = Instantiate(this.GameHandlerPrefab);
            GHGO.name = "GameHandler";
        }
    }

    void Update(){
        CheckPlayerInput();
    }
    
    public void StartSetup(GameHandler GH)
    {
        this.GameHandler = GH;
        if(this.GameHandler.Player == null){
            CreateNewPlayer();
        }else{
            Player = GameHandler.Player;
        }

        CreateButtonList();
        SelectOption(1);

        UpdatePlayerTexts();
        UpdateWeaponTexts();
        UpdateArmorTexts();
    }

    private void CreateNewPlayer() 
    {
        Debug.Log("Creating new player");
        
        Player = Instantiate(PlayerPrefab).GetComponent<PlayerCharacter>();

        Weapon PlayerWeapon = Instantiate(WeaponPrefab).GetComponent<Weapon>();
        this.GameHandler.PlayerWeapon = PlayerWeapon;
        Armor PlayerArmor = Instantiate(ArmorPrefab).GetComponent<Armor>();
        this.GameHandler.PlayerArmor = PlayerArmor;

        Player.MenuSetup(PlayerWeapon, PlayerArmor);
        this.GameHandler.SetPlayer(Player);
    }

    private void CreateButtonList()
    {
        currentButtonIndex = 0;
        currentMenuIndex = 0;

        ButtonList.Clear();
        
        GameObject MenuButtons = MenuList[currentMenuIndex].transform.Find("MenuButtons").gameObject;
        
        foreach(Transform Button in MenuButtons.transform)
        {
            ButtonList.Add(Button.gameObject);
        }
    }

    private void UpdatePlayerTexts()
    {
        PlayerNameText.text = GameHandler.Player.unitName;
        CreditsText.text = GameHandler.earnedCredits.ToString() + " Cd";
        HealthText.text = GameHandler.Player.healthPoints.ToString();
    }

    private void UpdateWeaponTexts()
    {
        WeaponLvlText.text = "Level: " + Player.GetWeaponLevel().ToString();
        AtkText.text = "Atk: " + Player.GetAttackMin().ToString() + " - " + Player.GetAttackMax().ToString();
        APRText.text = "APR: " + Player.GetActionsPerRound().ToString();
        LifeDrainText.text = "Life Drain: " + Player.GetLifeDrain().ToString();
        EnemyDotText.text = "Enemy DOT: " + Player.GetEnemyDOT().ToString();
        WeaponUpgradeCostText.text ="Upgrade Cost: " + Player.GetWeaponUpgradeCost().ToString();
    }

    private void UpdateNextWeaponTexts()
    {
        NextWeaponLvlText.text = Player.GetWeaponLevel().ToString();
        NextAtkText.text = Player.GetAttackMin().ToString() + " - " + Player.GetAttackMax().ToString();
        NextAPRText.text = Player.GetActionsPerRound().ToString();
        NextLifeDrainText.text = Player.GetLifeDrain().ToString();
        NextEnemyDotText.text = Player.GetEnemyDOT().ToString();
    }

    private void UpdateArmorTexts()
    {
        ArmorLvlText.text = "Level: " + Player.GetArmorLevel().ToString();
        HPText.text = "HP: " + Player.GetArmorHealth().ToString();
        ManaText.text = "Mana: " + Player.GetArmorMana().ToString();
        ManaRegText.text = "Mana Regen: " + Player.GetArmorManaRegen().ToString();
        ArmorUpgradeCostText.text = "Upgrade Cost: " + Player.GetArmorUpgradeCost().ToString();
    }

    private void UpdateNextArmorTexts()
    {
        NextArmorLvlText.text = Player.GetArmorLevel().ToString();
        NextHPText.text = Player.GetArmorHealth().ToString();
        NextManaText.text = Player.GetArmorMana().ToString();
        NextManaRegText.text = Player.GetArmorManaRegen().ToString();
    }

    private void CheckPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            int lastButtonIndex = currentButtonIndex;
            currentButtonIndex = (currentButtonIndex + 1) % ButtonList.Count;
            GameHandler.PlaySwitchMenuOptionSound();
            SelectOption(lastButtonIndex);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            int lastButtonIndex = currentButtonIndex;
            currentButtonIndex = (currentButtonIndex - 1);
            if (currentButtonIndex <= -1)
            {
                currentButtonIndex = ButtonList.Count - 1;
            }
            GameHandler.PlaySwitchMenuOptionSound();
            SelectOption(lastButtonIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SubmitOption();
        }
    }

    private void SelectOption(int lastButtonIndex)
    {
        Animator CurrentAnimator = ButtonList[currentButtonIndex].GetComponent<Animator>();
        Animator LastAnimator = ButtonList[lastButtonIndex].GetComponent<Animator>();
        CurrentAnimator.SetBool("Selected", true);
        LastAnimator.SetBool("Selected", false);

        if (currentMenuIndex == 0 && currentButtonIndex == 1)
        {
            UnhoverArmorUpgrade();
            HoverWeaponUpgrade();
            return;
        }
        else if (currentMenuIndex == 0 && currentButtonIndex == 2)
        {
            UnhoverWeaponUpgrade();
            HoverArmorUpgrade();
            return;
        }
        else
        {
            UnhoverWeaponUpgrade();
            UnhoverArmorUpgrade();
        }
    }

    private void HoverWeaponUpgrade()
    {
        Weapon W = GameHandler.PlayerWeapon;
        if (W.weaponLevel >= W.maxWeaponLevel) 
        {
            return;
        }
        W.UpgradeWeapon();
        UpdateNextWeaponTexts();
        W.DowngradeWeapon();
        if (currentMenuIndex == 0 && currentButtonIndex != 1)
        {
            int lastButtonIndex = 1;
            SelectOption(lastButtonIndex);
        }
    }

    private void HoverArmorUpgrade()
    {
        Armor A = GameHandler.PlayerArmor;
        if (A.armorLevel >= A.maxArmorLevel)
        {
            return;
        }
        A.UpgradeArmor();
        UpdateNextArmorTexts();
        A.DowngradeArmor();
        if (currentMenuIndex == 0 && currentButtonIndex != 2)
        {
            int lastButtonIndex = 2;
            SelectOption(lastButtonIndex);
        }
    }

    private void UnhoverWeaponUpgrade()
    {
        NextWeaponLvlText.text = "";
        NextAtkText.text = "";
        NextAPRText.text = "";
        NextLifeDrainText.text = "";
        NextEnemyDotText.text = "";
    }

    private void UnhoverArmorUpgrade()
    {
        NextArmorLvlText.text = "";
        NextHPText.text = "";
        NextManaText.text = "";
        NextManaRegText.text = "";
    }

    private void SubmitOption()
    {
        GameHandler.PlaySelectMenuOptionSound();
        switch (currentButtonIndex)
        {
            case 0:
                SubmitStageSelect();
                break;
            case 1:
                SubmitWeaponUpgrade();
                break;
            case 2:
                SubmitArmorUpgrade();
                break;
            case 3:
                GameHandler.PlayBlockedMenuOptionSound();
                //SubmitEnemyIndex();
                break;
            case 4:
                SubmitExitGame();
                break;
        }
    }

    private void SubmitStageSelect()
    {
        // MenuList[currentMenuIndex].SetActive(false);
        // currentMenuIndex = 1;
        // currentButtonIndex = 0;
        // MenuList[currentMenuIndex].SetActive(true);
        // CreateButtonList();
        GameHandler.LoadBattleScene();
    }

    private void SubmitWeaponUpgrade()
    {
        Weapon W = GameHandler.PlayerWeapon;
        if (W.weaponLevel >= W.maxWeaponLevel)
        {
            return;
        }
        if (GameHandler.earnedCredits >= W.upgradeCost)
        {
            GameHandler.earnedCredits -= W.upgradeCost;
            W.UpgradeWeapon();
            UpdatePlayerTexts();
            UpdateWeaponTexts();
            if (W.weaponLevel != W.maxWeaponLevel)
            {
                HoverWeaponUpgrade();
            }
            else
            {
                UnhoverWeaponUpgrade();
            }
        }
    }

    private void SubmitArmorUpgrade()
    {
        Armor A = GameHandler.PlayerArmor;
        if (A.armorLevel >= A.maxArmorLevel)
        {
            return;
        }
        if (GameHandler.earnedCredits >= A.upgradeCost)
        {
            GameHandler.earnedCredits -= A.upgradeCost;
            A.UpgradeArmor();
            UpdatePlayerTexts();
            UpdateArmorTexts();
            if (A.armorLevel != A.maxArmorLevel)
            {
                HoverArmorUpgrade();
            }
            else
            {
                UnhoverArmorUpgrade();
            }
        }
    }

    private void SubmitExitGame()
    {
        GameHandler.LoadStartMenu();
    }
}
