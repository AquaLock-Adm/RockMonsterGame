using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialHandler : MonoBehaviour
{
    private BattleSystem_Tutorial BattleSystem;

    private int tutorialInstanceIndex = 0;
    private int dialogueIndex = 0;

    private int inputDelay_ms = 300;

    [SerializeField] private GameObject DialogueBoxes;
    [SerializeField] private List<GameObject> DialogueBoxList;
    private int lastDialogueBoxIndex = 0;

    [SerializeField] private List<Image> BasicInputTestButtonsList;
    [SerializeField] private Color BasicButtonPressedColor;

    private List<List<string>> DialogueTexts; // DialogueTexts[0] texts for instance one

    void OnApplicationQuit(){
        foreach(GameObject B in this.DialogueBoxList){
            B.SetActive(false);
        }
    }

    public void Setup(BattleSystem_Tutorial BS){
        this.BattleSystem = BS;
        BS.SetPlayerMaxActionLevel(2);

        SetupDialogueBoxList();
        SetupDialogueTexts();
        SetupBasicInputTestButtons();
    }

    private void SetupDialogueBoxList(){
        this.DialogueBoxList = new List<GameObject>();

        foreach(Transform t in this.DialogueBoxes.transform){
            this.DialogueBoxList.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
    }

    private void SetupBasicInputTestButtons(){
        foreach(Transform t in this.DialogueBoxList[1].transform.Find("Test Buttons")){
            this.BasicInputTestButtonsList.Add(t.Find("Button - Inner").gameObject.GetComponent<Image>());
        }
    }

    public void Continue(){
        switch(this.tutorialInstanceIndex){
            case 0:
                LoadStartDialogue();
            break;

            case 1:
                LoadBasicInputTestDialogue();
            break;

            case 2:
                LoadAfterBasicInputDialogue();
            break;

            case 3:
                LoadFirstAttackDialogue();
            break;

            case 4:
                LoadQueueFullDialogue();
            break;

            case 5:
                LoadCurrentHeatDialogue();
            break;

            case 6:
                LoadRoundEndDialogue();
            break;

            case 7:
                LoadAttackNoDamageDialogue();
            break;

            case 8:
                LoadBlockInfoDialogue();
            break;

            case 9:
                LoadTryTwoAttacksDialogue();
            break;

            case 10:
                LoadFirstEnemyKilledDialogue();
            break;

            case 11:
                LoadNewAttacksDialogue();
            break;

            case 12:
                LoadHeavyBlockFoundDialogue();
            break;

            case 13:
                LoadNewMaxComboLevelDialogue();
            break;

            case 14:
                LoadAttacksAttributesDialogue();
            break;

            default:
                Debug.LogError("Tutorial Handler reached End of instance");
            break;
        }
    }

    private void NextInstance(){
        this.tutorialInstanceIndex++;
        this.dialogueIndex = 0;
    }

    private async void EnableTutorialDialogueTextInput(){
        BattleSystem.SwitchDialogueState(true);
        await Task.Delay(this.inputDelay_ms);

        if(BattleSystem.state != BattleState.DIALOGUE){
            Debug.Log(BattleSystem.state);
        }

        bool inputGot = false;
        while(Application.isPlaying && !inputGot){
            if(Input.GetKeyDown(KeyCode.S)){
                inputGot = true;
                Continue();
            }else await Task.Yield();
        }
    }

    private async void EnableBasicInputTestInputs(){
        await Task.Delay(this.inputDelay_ms);
        BattleSystem.SwitchDialogueState(true);

        bool wPressed = false;
        bool aPressed = false;
        bool sPressed = false;
        bool dPressed = false;

        while(Application.isPlaying && (
            !wPressed ||
            !aPressed ||
            !sPressed ||
            !dPressed)){
            if(Input.GetKeyDown(KeyCode.W)){
                wPressed = true;
                this.BasicInputTestButtonsList[0].color = this.BasicButtonPressedColor;
            }else if(Input.GetKeyDown(KeyCode.A)){
                aPressed = true;
                this.BasicInputTestButtonsList[1].color = this.BasicButtonPressedColor;
            }else if(Input.GetKeyDown(KeyCode.S)){
                sPressed = true;
                this.BasicInputTestButtonsList[2].color = this.BasicButtonPressedColor;
            }else if(Input.GetKeyDown(KeyCode.D)){
                dPressed = true;
                this.BasicInputTestButtonsList[3].color = this.BasicButtonPressedColor;
            }
            await Task.Yield();
        }
        Continue();
    }

    private async void EnableFirstAttackInput(){
        await Task.Delay(this.inputDelay_ms);
        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, true});
        BattleSystem.SwitchDialogueState(false);
        BattleSystem.NextWave();
        
        while(Application.isPlaying && BattleSystem.Player.GetCurrentActionCount() < 1){
            await Task.Yield();
        }
        
        Continue();
    }

    private async void EnableRoundOverInputs(){
        await Task.Delay(this.inputDelay_ms);
        BattleSystem.SwitchDialogueState(false);

        while( (BattleSystem.Player.state == PlayerState.START || BattleSystem.Player.state == PlayerState.PLAYERTURN) && Application.isPlaying){
            await Task.Yield();
        }

        while(BattleSystem.Player.state == PlayerState.QUEUE && Application.isPlaying){
            await Task.Yield();
        }

        Continue();
    }

    private void LoadDialogueBox(int boxIndex){
        this.DialogueBoxList[this.lastDialogueBoxIndex].SetActive(false);
        this.DialogueBoxList[boxIndex].SetActive(true);
        this.lastDialogueBoxIndex = boxIndex;

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count){
            string info = this.DialogueTexts[this.tutorialInstanceIndex][this.dialogueIndex];
            this.DialogueBoxList[boxIndex].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = info;
            this.dialogueIndex++;
        }

        if(this.dialogueIndex >= this.DialogueTexts[this.tutorialInstanceIndex].Count){
            NextInstance();
        }
    }

    private void LoadStartDialogue(){
        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, true, true} );
        LoadDialogueBox(0);
        EnableTutorialDialogueTextInput();
    }

    private void LoadBasicInputTestDialogue(){
        LoadDialogueBox(1);
        EnableBasicInputTestInputs();
    }

    private void LoadAfterBasicInputDialogue(){
        LoadDialogueBox(0);
        EnableTutorialDialogueTextInput();
    }

    private void LoadFirstAttackDialogue(){
        LoadDialogueBox(2);

        EnableFirstAttackInput();
    }

    private void LoadQueueFullDialogue(){
        LoadDialogueBox(3);

        EnableTutorialDialogueTextInput();
    }

    private void LoadCurrentHeatDialogue(){
        LoadDialogueBox(4);

        EnableTutorialDialogueTextInput();
    }

    private void LoadRoundEndDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, false});

        EnableRoundOverInputs();
    }

    private void LoadAttackNoDamageDialogue(){
        LoadDialogueBox(0);

        EnableTutorialDialogueTextInput();
    }

    private void LoadBlockInfoDialogue(){
        LoadDialogueBox(5);

        EnableTutorialDialogueTextInput();
    }

    private void LoadTryTwoAttacksDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, false});

        EnableRoundOverInputs();
    }

    private void LoadFirstEnemyKilledDialogue(){
        LoadDialogueBox(5);

        EnableTutorialDialogueTextInput();
    }

    private void LoadNewAttacksDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {false, false, false, false});
        Debug.Log("Todo!: Check for light attack first");

        EnableRoundOverInputs();
    }

    private void LoadHeavyBlockFoundDialogue(){
        LoadDialogueBox(5);

        EnableTutorialDialogueTextInput();
    }

    private void LoadNewMaxComboLevelDialogue(){
        LoadDialogueBox(2);

        EnableTutorialDialogueTextInput();
    }

    private void LoadAttacksAttributesDialogue(){
        LoadDialogueBox(2);

        EnableTutorialDialogueTextInput();
    }

    private void SetupDialogueTexts(){
        this.DialogueTexts = new List<List<string>>();
        List<string> StartTexts = new List<string>();

        StartTexts.Add("Hey! Welcome to Rock Monster Game Real Name TBA\n"
                    +  "This is the tutorial I made for people to give an idea of how the mechanics in this game work.\n"
                    +  "Press S to load the next text page.");

        this.DialogueTexts.Add(StartTexts);

        List<string> BasicInputTexts = new List<string>();

        BasicInputTexts.Add("For the rest of the demo you will only need WASD to control your character.\n"
                        +   "Press all 4 to continue:");

        this.DialogueTexts.Add(BasicInputTexts);

        List<string> AfterBasicInputTexts = new List<string>();

        AfterBasicInputTexts.Add("Rock Monster Game Real Name TBA is a turn based combat game.\n"
                                +"The combat is 1 v 1 so you and the enemy will be taking turns.\n"
                                +"Now let's have a look at the game interface.");

        this.DialogueTexts.Add(AfterBasicInputTexts);

        List<string> FirstAttackTexts = new List<string>();

        FirstAttackTexts.Add("Down here to the left we have a menu displaying all the actions you can take.\n"
                            +"With WASD you select the corresponding actions.\n"
                            +"For example: pressing S will select the Light attack.");

        this.DialogueTexts.Add(FirstAttackTexts);

        List<string> QueueFullTexts = new List<string>();

        QueueFullTexts.Add("As you can see your action was saved down here in the action queue.\n"
                        +  "It displays the current sequence of actions that you put in.\n"
                        +  "For now your action queue has only one slot.");

        this.DialogueTexts.Add(QueueFullTexts);

        List<string> CurrentHeatTexts = new List<string>();

        CurrentHeatTexts.Add("That is because your Action Heat Level, which is the number up here,\n"
                            +"Shows that your Action Queue is level 1. During combat your level will gradually rise\n"
                            +"everytime you land a good hit on the enemy. A higher level means more actions you can take in a turn");

        this.DialogueTexts.Add(CurrentHeatTexts);

        List<string> RoundOverTexts = new List<string>();

        RoundOverTexts.Add("Now that your queue is full all the action buttons on the left have turned into End Round buttons\n"
                        +  "Press any one of these to end your round. Otherwise you can press D to cancel your last action input.\n"
                        +  "End your turn now and your character will execute the action(s).");

        this.DialogueTexts.Add(RoundOverTexts);

        List<string> AttackNoDamageTexts = new List<string>();

        AttackNoDamageTexts.Add("You're actions were executed but they didn't deal any damage.\n"
                        +  "That's because the enemy blocked it!");

        this.DialogueTexts.Add(AttackNoDamageTexts);

        List<string> BlockInfoTexts = new List<string>();

        BlockInfoTexts.Add("See the grey bar right next to the enemy? It means that the enemy will always block\n"
                        +  "one of your attacks, while the grey color indicates that the block will be used up by\n"
                        +  "any kind of attack. Should the attack type not match the block, then the enemy will\n"
                        +  "block the attack without using up it's block.");
        BlockInfoTexts.Add("But since you attack matched the color, you have gained enough heat to reach level 2.\n"
                        +  "For now that will be your maximum.");

        this.DialogueTexts.Add(BlockInfoTexts);

        List<string> TryOutTwoAttacksTexts = new List<string>();

        TryOutTwoAttacksTexts.Add("If you queue up 2 actions at once now \n"
                                + "you should be able to break the block with the first and hit the enemy with the second.\n"
                                + "Try it out!");

        this.DialogueTexts.Add(TryOutTwoAttacksTexts);

        List<string> FirstEnemyKilledTexts = new List<string>();

        FirstEnemyKilledTexts.Add("That's more like it!\n"
                                + "Now that the enemy has died a new one has taken it's place.\n"
                                + "It's front block right now is white. That indicates that it can only be broken by a light attack.");
        FirstEnemyKilledTexts.Add("The little black square next to the front block tells you that the Enemy actually blocks 2 attacks.\n"
                                + "The color black does not relate to any attack type.\n"
                                + "It means that you don't know what that block will be yet.");
        FirstEnemyKilledTexts.Add("You'll have to see it to find out. So until then the only thing you can do is guess.\n"
                                + "I will unlock the other two attacks you can do for that.");

        this.DialogueTexts.Add(FirstEnemyKilledTexts);

        List<string> NewAttacksTexts = new List<string>();

        NewAttacksTexts.Add("Pressing A will queue a Heavy Attack and W will queue a Special Attack.\n"
                        +   "Start your next attack sequence with a light attack plus one more.");

        this.DialogueTexts.Add(NewAttacksTexts);

        List<string> HeavyBlockFoundTexts = new List<string>();

        HeavyBlockFoundTexts.Add("It looks like the second block is blue!\n"
                                + "This means it can be broken by a Heavy Attack.\n"
                                + "If it were red, then it would only be broken by a Special Attack.\n"
                                + "But Remember, normaly it also could be white for Light Attacks or grey for Any Attack.");
        HeavyBlockFoundTexts.Add("You can also see, that now that you know, what the second block is, the black square has turned blue.");

        this.DialogueTexts.Add(HeavyBlockFoundTexts);

        List<string> NewMaxComboLevelText = new List<string>();

        NewMaxComboLevelText.Add("To hurt this enemy you will need atleast 3 attacks.\n"
                                +  "So let me simply give you Action Heat Level 3.");

        this.DialogueTexts.Add(NewMaxComboLevelText);

        List<string> AttackAttributesTexts = new List<string>();

        AttackAttributesTexts.Add("It might also be interesting to know that the 3 Attacktypes have different attributes.");
        AttackAttributesTexts.Add("Special Attacks are faster and deal less Damage.\n"
                                + "Heavy Attacks are slower and deal more Damage.\n"
                                + "Light Attacks are a good middle ground.\n"
                                + "Now with these 3 Attacks try to defeat the enemy!");

        this.DialogueTexts.Add(AttackAttributesTexts);
    }
}
