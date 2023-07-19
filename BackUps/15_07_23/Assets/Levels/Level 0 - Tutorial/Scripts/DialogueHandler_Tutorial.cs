using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueHandler_Tutorial : MonoBehaviour
{

    [SerializeField] private BattleSystem BattleSystem;

    [Header("Text Box References")]
    [SerializeField] private GameObject DialogueMainTextBox;
    [SerializeField] private GameObject DialogueHelpTextBox;
    [SerializeField] private TextMeshProUGUI MainText;
    [SerializeField] private TextMeshProUGUI HelpText;
    [SerializeField] private TextMeshProUGUI LeftSpeakerText;
    [SerializeField] private TextMeshProUGUI RightSpeakerText;

    private List<List<DialogueInstance>> DialogueInstances = new List<List<DialogueInstance>>();
    private int dialogueInstanceIndex = 0; // current Displayed Textbox is DialogueInstances[dialogueInstanceIndex][currentDialogueIndex]
    private int currentDialogueIndex = 0;

    private bool updateLoopOn = true;
    private bool spamBufferOn = false;

    private class DialogueInstance {
        public string lSpeaker;
        public string rSpeaker;
        public string mainText;
        public string helpText;
    }

#region Unity Functions
    void Start(){
        if(this.DialogueMainTextBox != null && this.DialogueMainTextBox.activeSelf){
            this.DialogueMainTextBox.SetActive(false);
        }

        if(this.DialogueHelpTextBox != null && this.DialogueHelpTextBox.activeSelf){
            this.DialogueHelpTextBox.SetActive(false);
        }

        SetDialogueInstances();
    }
    void OnApplicationQuit(){
        this.updateLoopOn = false;
    }
#endregion

    private async void DialogueUpdateLoop(){
        while(this.updateLoopOn){
            if(!this.spamBufferOn) CheckInputs();
            await Task.Yield();
        }
    }

    private void SetDialogueInstances(){
        this.DialogueInstances.Add(GetDialogueInstances1());
    }

    public void LoadDialogue(){
        BattleSystem.state = BattleState.DIALOGUE;

        if(this.dialogueInstanceIndex >= this.DialogueInstances.Count){
            Debug.Log("No More.");
            // Dialogue is over, in tuto case, the wave should be over as well so you can go back to menu
            return;
        }

       LoadDialogueInstances(this.DialogueInstances[this.dialogueInstanceIndex][this.currentDialogueIndex]);
       this.currentDialogueIndex++;

        SpamProtectionLoop();  
    }

    private void LoadDialogueInstances(DialogueInstance instance){
        SetRSpeakerText(instance.rSpeaker);
        SetLSpeakerText(instance.lSpeaker);
        SetMainText(instance.mainText);
        SetHelpText(instance.helpText);
    }

    private async void SpamProtectionLoop(){
        this.spamBufferOn = true;

        await Task.Delay(1000);

        this.spamBufferOn = false;
    }

    private void CheckInputs(){
        if(BattleSystem.state == BattleState.DIALOGUE){
            if(Input.GetKeyDown(KeyCode.A)){
                if(this.currentDialogueIndex >= this.DialogueInstances[this.dialogueInstanceIndex].Count){
                    Debug.Log("End");
                    // ContinueGame();
                }else {
                    this.dialogueInstanceIndex++;
                    this.currentDialogueIndex = 0;
                    LoadDialogue();
                }
            }
        }
    }

    private void SetRSpeakerText(string R){
        if(!this.RightSpeakerText.gameObject.activeSelf) this.RightSpeakerText.gameObject.SetActive(true);
        this.RightSpeakerText.text = R;
    }

    private void SetLSpeakerText(string L){
        if(!this.LeftSpeakerText.gameObject.activeSelf) this.LeftSpeakerText.gameObject.SetActive(true);
        this.LeftSpeakerText.text = L;
    }

    private void SetMainText(string M){
        if(!this.MainText.gameObject.activeSelf) this.MainText.gameObject.SetActive(true);
        this.MainText.text = M;
    }

    private void SetHelpText(string H){
        if(!this.HelpText.gameObject.activeSelf) this.HelpText.gameObject.SetActive(true);
        this.HelpText.text = H;
    }

    private List<DialogueInstance> GetDialogueInstances1(){
        List<DialogueInstance> list = new List<DialogueInstance>();

        DialogueInstance A = new DialogueInstance();
        A.lSpeaker = "Me";
        A.rSpeaker = " ";
        A.mainText = "Hello?\nTest is this on?";
        A.helpText = "";
        list.Add(A);

        DialogueInstance B = new DialogueInstance();
        B.lSpeaker = " ";
        B.rSpeaker = "Not Me";
        B.mainText = "I don't know!";
        B.helpText = "";
        list.Add(B);

        return list;
    }
}
