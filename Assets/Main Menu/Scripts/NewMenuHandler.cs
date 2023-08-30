using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class NewMenuHandler : MonoBehaviour
{
    private GameHandler GameHandler;
    private PlayerCharacter Player;

    private int currentButtonIndex = 0;
    private int currentMenuIndex = 0;

    [SerializeField] private TextMeshProUGUI PlayerNameText, CreditsText, HealthText;
    [SerializeField] private TextMeshProUGUI WeaponLvlText, AtkText, APRText, LifeDrainText, EnemyDotText, WeaponUpgradeCostText;
    [SerializeField] private TextMeshProUGUI NextWeaponLvlText, NextAtkText, NextAPRText, NextLifeDrainText, NextEnemyDotText;
    [SerializeField] private TextMeshProUGUI ArmorLvlText, HPText, ManaText, ManaRegText, ArmorUpgradeCostText;
    [SerializeField] private TextMeshProUGUI NextArmorLvlText, NextHPText, NextManaText, NextManaRegText;
    
    [SerializeField] private List<GameObject> ButtonList = new List<GameObject>();
    [SerializeField] private List<GameObject> MenuList = new List<GameObject>();

    private void Start()
    {
        StartSetup();
    }
    private void Update()
    {
        CheckPlayerInput();
    }
    
    private void StartSetup()
    {
        currentButtonIndex = 0;
        currentMenuIndex = 0;
        CreateButtonList();
        SelectOption(1);

        GameHandler = GameObject.Find("GameHandler").GetComponent<GameHandler>();
        Player = GameHandler.Player;
        UpdatePlayerTexts();
        UpdateWeaponTexts();
        UpdateArmorTexts();
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
            SelectOption(lastButtonIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SubmitOption();
        }
    }
    private void CreateButtonList()
    {
        ButtonList.Clear();
        
        GameObject MenuButtons = MenuList[currentMenuIndex].transform.Find("MenuButtons").gameObject;
        
        foreach(Transform Button in MenuButtons.transform)
        {
            ButtonList.Add(Button.gameObject);
        }

        //List<int> MyIntegerList = new List<int> { 1, 2, 3, 4, 5, 6 };
        //foreach(int currentInteger in MyIntegerList)
        //{
        //    Debug.Log(currentInteger);
        //}
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
        W.GetUpgrade(W.weaponLevel + 1);
        UpdateNextWeaponTexts();
        W.GetUpgrade(W.weaponLevel - 1);
        if (currentMenuIndex == 0 && currentButtonIndex != 1)
        {
            int lastButtonIndex = 1;
            SelectOption(lastButtonIndex);
        }
    }
    private void HoverArmorUpgrade()
    {
        Armor A = GameHandler.PlayerArmor;
        A.GetUpgrade(A.armorLevel + 1);
        UpdateNextArmorTexts();
        A.GetUpgrade(A.armorLevel - 1);
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
                //SubmitEnemyIndex();
                break;
            case 4:
                SubmitExitGame();
                break;
        }
    }
    private void SubmitStageSelect()
    {
        MenuList[currentMenuIndex].SetActive(false);
        currentMenuIndex = 1;
        currentButtonIndex = 0;
        MenuList[currentMenuIndex].SetActive(true);
        CreateButtonList();
    }
    private void SubmitWeaponUpgrade()
    {
        Weapon W = GameHandler.PlayerWeapon;
        if (GameHandler.earnedCredits >= W.upgradeCost)
        {
            W.GetUpgrade(W.weaponLevel + 1);
            GameHandler.earnedCredits -= W.upgradeCost;
            UpdatePlayerTexts();
            UpdateWeaponTexts();
            if (W.weaponLevel != 5)
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
        if (GameHandler.earnedCredits >= A.upgradeCost)
        {
            A.GetUpgrade(A.armorLevel + 1);
            GameHandler.earnedCredits -= A.upgradeCost;
            UpdatePlayerTexts();
            UpdateArmorTexts();
            if (A.armorLevel != 5)
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
        Application.Quit();
    }
}
