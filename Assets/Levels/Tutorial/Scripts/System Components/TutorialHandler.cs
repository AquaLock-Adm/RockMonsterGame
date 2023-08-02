using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialHandler : MonoBehaviour
{
    private BattleSystem BattleSystem;

    private int tutorialInstanceIndex = 0;
    private int dialogueIndex = 0;

    private int inputDelay_ms = 300;

    [SerializeField] private GameObject DialogueBoxes;
    [SerializeField] private List<GameObject> DialogueBoxList;
    private int lastDialogueBoxIndex = 0;

    [SerializeField] private List<Image> BasicInputTestButtonsList;
    [SerializeField] private Color BasicButtonPressedColor;

    private List<List<string>> DialogueTexts; // DialogueTexts[0] texts for instance one

    public void Setup(BattleSystem BS){
        this.BattleSystem = BS;
        SetupDialogueBoxList();
        SetupDialogueTexts();
        SetupBasicInputTestButtons();
    }

    private void SetupDialogueBoxList(){
        this.DialogueBoxList = new List<GameObject>();

        foreach(Transform t in this.DialogueBoxes.transform){
            this.DialogueBoxList.Add(t.gameObject);
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
        await Task.Delay(this.inputDelay_ms);

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
        /*
        Start up normal game but disable player inputs W A & D
        */
        BattleSystem.NextWave();
        
        while(Application.isPlaying && BattleSystem.Player.GetCurrentActionCount() < 1){
            await Task.Yield();
        }
        /*
         Block all Player input
        */
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
    }
}
