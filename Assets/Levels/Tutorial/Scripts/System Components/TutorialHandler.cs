using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialHandler : MonoBehaviour
{
    private BattleSystem_Tutorial BattleSystem;

    [SerializeField] private int tutorialInstanceIndex = 0;
    [SerializeField] private int dialogueIndex = 0;

    private int inputDelay_ms = 300;

    [SerializeField] private GameObject DialogueBoxes;
    [SerializeField] private List<GameObject> DialogueBoxList;
    private int lastDialogueBoxIndex = 0;

    [SerializeField] private List<Image> BasicInputTestButtonsList;
    [SerializeField] private Color BasicButtonPressedColor;

    private List<List<string>> DialogueTexts; // DialogueTexts[0] texts for instance one

    private List<string> AwaitedActionsNames = new List<string>();
    private bool awaitedActionsListActive = false;
    private bool awaitedActionsContainsOnly = false;

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

    public void UpdateAwaitedActions(List<Action> CurrentActions){
        if(this.awaitedActionsListActive) this.awaitedActionsListActive = false;

        if(CurrentActions.Count < this.AwaitedActionsNames.Count) return;

        if(awaitedActionsContainsOnly){
            int awaitedFound = 0;
            int currentActionsIndex = 0;

            while(awaitedFound < this.AwaitedActionsNames.Count && currentActionsIndex < CurrentActions.Count){
                if(CurrentActions[currentActionsIndex].name == this.AwaitedActionsNames[awaitedFound]){
                    awaitedFound++;
                }
                currentActionsIndex++;
            }
            if(awaitedFound == this.AwaitedActionsNames.Count){
                this.awaitedActionsListActive = true;
            }
        }else {
            for(int i=0; i< this.AwaitedActionsNames.Count; i++){
                if(this.AwaitedActionsNames[i] == "Any") continue;
                else if(this.AwaitedActionsNames[i] != CurrentActions[i].name) return;
            }

            this.awaitedActionsListActive = true;
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

            case 15:
                LoadComboIntroDialogue();
            break;

            case 16:
                LoadComboEndDialogue();
            break;

            case 17:
                LoadModeSwitchIntroDialogue();
            break;

            case 18:
                LoadEnemyAttackDialogue();
            break;

            case 19:
                LoadDefendInputsDialogue();
            break;

            case 20:
                LoadBlockSuccessfulDialogue();
            break;

            case 21:
                LoadInputChangedBackDialogue();
            break;

            case 22:
                LoadTutorialEndDialogue();
            break;

            case 23:
                Debug.Log("Tutorial over");
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



#region Change WatchDog Functions
    private async void WaitForTutorialContinue(){
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

    private async Task WaitForPlayerContinueInput(){
        BattleSystem.SwitchDialogueState(true);
        await Task.Delay(this.inputDelay_ms);

        if(BattleSystem.state != BattleState.DIALOGUE){
            Debug.Log(BattleSystem.state);
        }

        while(Application.isPlaying){
            if(Input.GetKeyDown(KeyCode.S)){
                return;
            }else await Task.Yield();
        }
    }

    private async void WaitForBasicInputTestInputs(){
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

    private async void WaitForFirstAttackInput(){
        await Task.Delay(this.inputDelay_ms);
        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, true});
        BattleSystem.SwitchDialogueState(false);
        BattleSystem.NextWave();
        
        while(Application.isPlaying && BattleSystem.Player.GetCurrentActionCount() < 1){
            await Task.Yield();
        }
        
        Continue();
    }

    private async void WaitForNextRound(){
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

    private async void WaitForExecutedActions(List<string> ActionsNamesList, bool containsActions = false){
        this.AwaitedActionsNames = ActionsNamesList;
        this.awaitedActionsListActive = false;
        this.awaitedActionsContainsOnly = containsActions;
        BattleSystem.WaitForExecutedActions(true);

        await Task.Delay(this.inputDelay_ms);
        BattleSystem.SwitchDialogueState(false);

        while(!this.awaitedActionsListActive){
            while( (BattleSystem.Player.state == PlayerState.START || BattleSystem.Player.state == PlayerState.PLAYERTURN) && Application.isPlaying){
                await Task.Yield();
            }
            await Task.Yield();
        }

        BattleSystem.WaitForExecutedActions(false);
        this.awaitedActionsContainsOnly = false;
        this.awaitedActionsListActive = false;

        while(BattleSystem.Player.state == PlayerState.QUEUE && Application.isPlaying){
            await Task.Yield();
        }

        Continue();
    }

    private async void WaitForQueuedActions(List<string> ActionsNamesList, bool containsActions = false){
        this.AwaitedActionsNames = ActionsNamesList;
        this.awaitedActionsListActive = false;
        this.awaitedActionsContainsOnly = containsActions;
        BattleSystem.WaitForQueuedActions(true);

        await Task.Delay(this.inputDelay_ms);
        BattleSystem.SwitchDialogueState(false);

        while(!this.awaitedActionsListActive){
            await Task.Yield();
        }
        
        // Debug.Log("Awaited Actions detected!");
        BattleSystem.WaitForQueuedActions(false);
        this.awaitedActionsContainsOnly = false;
        this.awaitedActionsListActive = false;

        Continue();
    }
#endregion



#region Dialogue Box Loaders
    private void LoadDialogueBox(int boxIndex, bool advanceToNextInstance = true){
        this.DialogueBoxList[this.lastDialogueBoxIndex].SetActive(false);
        this.DialogueBoxList[boxIndex].SetActive(true);
        this.lastDialogueBoxIndex = boxIndex;

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count){
            string info = this.DialogueTexts[this.tutorialInstanceIndex][this.dialogueIndex];
            this.DialogueBoxList[boxIndex].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = info;
            this.dialogueIndex++;
        }

        if(this.dialogueIndex >= this.DialogueTexts[this.tutorialInstanceIndex].Count && advanceToNextInstance){
            NextInstance();
        }
    }

    private void LoadStartDialogue(){
        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, true, true} );
        LoadDialogueBox(0);
        WaitForTutorialContinue();
    }

    private void LoadBasicInputTestDialogue(){
        LoadDialogueBox(1);
        WaitForBasicInputTestInputs();
    }

    private void LoadAfterBasicInputDialogue(){
        LoadDialogueBox(0);
        WaitForTutorialContinue();
    }

    private void LoadFirstAttackDialogue(){
        LoadDialogueBox(2);

        WaitForFirstAttackInput();
    }

    private void LoadQueueFullDialogue(){
        LoadDialogueBox(3);

        WaitForTutorialContinue();
    }

    private void LoadCurrentHeatDialogue(){
        LoadDialogueBox(4);

        WaitForTutorialContinue();
    }

    private void LoadRoundEndDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, false});
        BattleSystem.BlockPlayerBattleModeSwitch(true);

        WaitForNextRound();
    }

    private void LoadAttackNoDamageDialogue(){
        LoadDialogueBox(0);

        WaitForTutorialContinue();
    }

    private void LoadBlockInfoDialogue(){
        LoadDialogueBox(5);

        WaitForTutorialContinue();
    }

    private void LoadTryTwoAttacksDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {true, true, false, false});

        WaitForNextRound();
    }

    private void LoadFirstEnemyKilledDialogue(){
        LoadDialogueBox(5);

        WaitForTutorialContinue();
    }

    private void LoadNewAttacksDialogue(){
        LoadDialogueBox(2);

        BattleSystem.BlockPlayerInputs(new List<bool> {false, false, false, false});

        WaitForExecutedActions(new List<string> {"Light Attack"}, true);
    }

    private void LoadHeavyBlockFoundDialogue(){
        LoadDialogueBox(5);

        WaitForTutorialContinue();
    }

    private void LoadNewMaxComboLevelDialogue(){
        LoadDialogueBox(2);

        WaitForTutorialContinue();
    }

    private void LoadAttacksAttributesDialogue(){
        LoadDialogueBox(2, false);
        
        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count) WaitForTutorialContinue();
        else {
            BattleSystem.SetPlayerMaxActionLevel(3);
            BattleSystem.SetPlayerComboLevel(3);
            NextInstance(); // <-- Important because it was skipped in LoadDialogueBox
            WaitForQueuedActions(new List<string> {"Light Attack", "Heavy Attack"});
        }
    }

    private void LoadComboIntroDialogue(){
        LoadDialogueBox(2, false);

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count) WaitForTutorialContinue();
        else {
            BattleSystem.BlockPlayerInputs(new List<bool> {false, true, true, true});
            NextInstance(); // <-- Important because it was skipped in LoadDialogueBox
            WaitForExecutedActions(new List<string> {"Any", "Any", "Combo A"});
        }
    }

    private async void LoadComboEndDialogue(){
        LoadDialogueBox(0, false);

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count) WaitForTutorialContinue();
        else {
            await WaitForPlayerContinueInput();
            this.DialogueBoxList[this.lastDialogueBoxIndex].SetActive(false);
            BattleSystem.BlockPlayerInputs(new List<bool> {false, false, false, false});
            BattleSystem.BlockPlayerBattleModeSwitch(false);
            NextInstance(); // <-- Important because it was skipped in LoadDialogueBox
            WaitForNextRound();
        }
    }

    private void LoadModeSwitchIntroDialogue(){
        LoadDialogueBox(0);

        WaitForTutorialContinue();
    }

    private void LoadEnemyAttackDialogue(){
        LoadDialogueBox(6);

        WaitForTutorialContinue();
    }

    private void LoadDefendInputsDialogue(){
        LoadDialogueBox(2, false);

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count) WaitForTutorialContinue();
        else {
            BattleSystem.BlockPlayerInputs(new List<bool> {false, false, false, false});
            NextInstance(); // <-- Important because it was skipped in LoadDialogueBox
            WaitForNextRound();
        }
    }

    private void LoadBlockSuccessfulDialogue(){
        LoadDialogueBox(0);

        WaitForTutorialContinue();
    }

    private void LoadInputChangedBackDialogue(){
        LoadDialogueBox(2);

        WaitForTutorialContinue();
    }

    private async void LoadTutorialEndDialogue(){
        LoadDialogueBox(0, false);

        if(this.dialogueIndex < this.DialogueTexts[this.tutorialInstanceIndex].Count) WaitForTutorialContinue();
        else {
            NextInstance(); // <-- Important because it was skipped in LoadDialogueBox
            await WaitForPlayerContinueInput();
            this.DialogueBoxList[this.lastDialogueBoxIndex].SetActive(false);

            BattleSystem.SwitchDialogueState(false);
            Continue();
        }
    }
#endregion



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
                        +   "In your next Attack Sequence you will need atleast one Light Attack\n"
                        +   "to reach the second block.");

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

        List<string> ComboIntroTexts = new List<string>();

        ComboIntroTexts.Add("Do you see that your Special Attack has changed into Combo A?\n"
                        +   "That's because the system has detected, that you have almost all actions\n"
                        +   "for Combo A in your queue.");
        ComboIntroTexts.Add("That means Combo A is executed by chaining Light > Heavy > Special.\n"
                        +   "By pressing W now you will queue Combo A and all earlier actions\n"
                        +   "will turn into the parts of Combo A.");
        ComboIntroTexts.Add("Combo Attacks are always far more powerfull then their individual attacks.\n"
                        +   "But! Remember that the single attack parts still keep their attack type\n"
                        +   "and still break and get blocked by the same blocks!");
        ComboIntroTexts.Add("Here it is important that atleast the last attack of the combo\n"
                        +   "hits the enemy since it carries most of the damage.");
        ComboIntroTexts.Add("So for a combo to be effective, be sure, that at the end you get\n"
                        +   "through all of the blocks of the enemy.");
        ComboIntroTexts.Add("Note: The system will only check for combos at the END of your attack sequence.\n"
                        +   "Light>Heavy>Special>Light will NOT make Combo A>Light\n"
                        +   "But Light>Light>Heavy>Special will make Light>Combo A");
        ComboIntroTexts.Add("Lucky for you, right now is your chance to use Combo A!\n"
                        +   "Press W to queue up Combo A and see the enemies health bar melt!");

        this.DialogueTexts.Add(ComboIntroTexts);

        List<string> ComboEndTexts = new List<string>();

        ComboEndTexts.Add("See? That did a lot of damage!\n"
                        + "And it will only grow more damaging the longer the combo is.\n"
                        + "Of course to execute longer combos you will need enough slots in your Action Queue.");
        ComboEndTexts.Add("Also know, that the shortest combos are 3 Actions long, there are no 2 Action Combos.\n"
                        + "Now defeat the next enemy!");

        this.DialogueTexts.Add(ComboEndTexts);

        List<string> ModeSwitchIntroTexts = new List<string>();

        ModeSwitchIntroTexts.Add("But we're not quite done.\n"
                                +"Of course, now that you have attacked the enemy it's their turn to attack you!");

        this.DialogueTexts.Add(ModeSwitchIntroTexts);

        List<string> EnemyAttackInfoTexts = new List<string>();

        EnemyAttackInfoTexts.Add("See the letter up here? That is the sequence with which the enemy is going to attack.\n"
                                +"One slot means there is one attack that you'll have to block to avoid taking damage.");
        EnemyAttackInfoTexts.Add("The letter inside the box right now is just a random letter, which means you don't know\n"
                                +"what attack the enemy is going to do. Which again means, you're going to have to guess.");

        this.DialogueTexts.Add(EnemyAttackInfoTexts);

        List<string> DefendInputTexts = new List<string>();

        DefendInputTexts.Add("Now let's see what options you have.\n"
                            +"Your 3 Attacks have turned into 3 different Blocks.\n"
                            +"Low Block, Mid Block and High Block.");
        DefendInputTexts.Add("Once you have discovered what the Letters in\n"
                            +"the enemies Attack sequence above them stand for\n"
                            +"they will be replaced and can be matched by the equivalent Block.\n"
                            +"Lo > Low Block, Mi > Mid Block, Hi > High Block.");
        DefendInputTexts.Add("Now go on and try your hand at blocking the enemies attack!");

        this.DialogueTexts.Add(DefendInputTexts);

        List<string> BlockSuccessTexts = new List<string>();

        BlockSuccessTexts.Add("Did you manage to block it?\n"
                            + "Right now the visualization is quite lackluster\n"
                            + "but you can tell by your healthbar at the top right corner\n"
                            + "wether you received damage or not.");
        BlockSuccessTexts.Add("Should your Healthbar reach zero\n"
                            + "that would mean the end of your run.");
        BlockSuccessTexts.Add("But now that you have survived the enemies attack\n"
                            + "it is your turn once again to attack it.");

        this.DialogueTexts.Add(BlockSuccessTexts);

        List<string> InputBackTexts = new List<string>();

        InputBackTexts.Add("As you can see your input interface has changed back to attack mode\n"
                        +  "Should you not magage to kill the enemy with your attack\n"
                        +  "it will change back to defense mode, then back to attack etc.");

        this.DialogueTexts.Add(InputBackTexts);

        List<string> TutorialEndTexts = new List<string>();

        TutorialEndTexts.Add("Every time the enemy attacks there is a chance that it's\n"
                            +"Attack Sequence gets longer.");
        TutorialEndTexts.Add("Of course every enemy has it's maximum number of attacks\n"
                            +"but all attacks will usually be random.\n"
                            +"So it will be important to figure out which Attack Code(letter)\n"
                            +"stands for which attack.");
        TutorialEndTexts.Add("Some stronger Enemies will have multiple Attack Codes for the same\n"
                            +"attack and Some Enemies will have multiple Defensive stances\n"
                            +"which means, that their sequence of blocks may vary.");
        TutorialEndTexts.Add("One last thing:\n"
                            +"To make things more interesting, your Life total will be depleting\n"
                            +"while you have control over your Inputs.\n"
                            +"Right now in the Tutorial I have disabled that mechanic but later\n"
                            +"you'll have to be more carefull and quick.");
        TutorialEndTexts.Add("This could make your gameplay quite stressfull\n"
                            +"but try to stay calm and concentrate on the hints the enemy is giving you.");
        TutorialEndTexts.Add("And don't worry, by dealing damage to the enemy you will regain\n"
                            +"some life, based on how much damage you dealt.");
        TutorialEndTexts.Add("So act fast and try to deal as much damage as possible.\n"
                            +"You got this!\n"
                            +"Defeat this last enemy to finish the tutorial!\n"
                            +"Good Luck and Have Fun.");

        this.DialogueTexts.Add(TutorialEndTexts);
    }
}
