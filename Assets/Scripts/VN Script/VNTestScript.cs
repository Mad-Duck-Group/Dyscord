using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class VNTestScript : MonoBehaviour
{
    //Note 
    /// <summary>
    /// เหลือต้องทำให้ฟองแชทเก่าที่เกิน 2 ฟอง จะถูกลบออก และฟองแชทที่เหลือจะถูกเลื่อนไปที่จุด L1 หรือ R1 ตามฝั่งที่เหลือ
    /// โค๊ดส่วนการปรับขนาดอยู่ที่ AutoResizeTextBox.cs
    /// MessageController.cs ลบได้เลย
    /// ขอโทษที่ความสามารถน้อยเกินไปและกลายเป็นตัวถ่วงทีม
    /// </summary>
    
    public TMP_Text speakerText;
    public GameObject nextHintIcon;
    
    [SerializeField] private Transform pointL1;
    [SerializeField] private Transform pointL2;
    [SerializeField] private Transform pointR1;
    [SerializeField] private Transform pointR2;
    
    public GameObject chatBubblePrefabLeft;
    public GameObject chatBubblePrefabRight; 
    
    public GameObject leftCharacter;
    public GameObject rightCharacter;
    
    private List<Dialogue> dialogues;
    private int currentIndex = 0;

    private Queue<GameObject> leftBubbles = new Queue<GameObject>();
    private Queue<GameObject> rightBubbles = new Queue<GameObject>();
    
    void Start()
    {
        dialogues = new List<Dialogue>()
        {
            new Dialogue(1,"Player", "What are you doing here?"),
            new Dialogue(2,"NPC", "I'm just waiting for someone."),
            new Dialogue(3,"Player", "Really? Who?"),
            new Dialogue(4,"NPC", "Nope.")
        };

        ShowNextMessage();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextMessage();
        }
    }
    
    public void ShowNextMessage()
    {
        if (currentIndex < dialogues.Count)
        {
            var dialogue = dialogues[currentIndex];
            bool isPlayer = dialogue.speaker == "Player";
            AddChatBubble(dialogue.text, isPlayer);

            if (speakerText == null)
            {
                Debug.LogError("SpeakerText is not assigned in the inspector.");
                return;
            }
            speakerText.text = dialogue.speaker;

            SetCharacterOpacity(leftCharacter, isPlayer);
            SetCharacterOpacity(rightCharacter, !isPlayer);

            nextHintIcon.SetActive(currentIndex < dialogues.Count - 1);

            currentIndex++;
        }
        else
        {
            Debug.LogWarning("No more dialogues to display.");
        }
    }
    
    public void AddChatBubble(string message, bool isLeftSide)
    {
        if (isLeftSide)
        {
            if (leftBubbles.Count >= 2)
            {
                Destroy(leftBubbles.Dequeue()); // ลบฟองแชทที่เก่าที่สุด
                foreach (var bubble in leftBubbles)
                {
                    bubble.transform.position = pointL1.position; // ย้ายฟองแชทปัจจุบันไปจุด L1
                }
            }

            GameObject newBubble = Instantiate(chatBubblePrefabLeft, pointL2);
            newBubble.GetComponentInChildren<TextMeshProUGUI>().text = message;
            leftBubbles.Enqueue(newBubble);
        }
        else
        {
            if (rightBubbles.Count >= 2)
            {
                Destroy(rightBubbles.Dequeue());
                foreach (var bubble in rightBubbles)
                {
                    bubble.transform.position = pointR1.position;
                }
            }

            GameObject newBubble = Instantiate(chatBubblePrefabRight, pointR2);
            newBubble.GetComponentInChildren<TextMeshProUGUI>().text = message;
            rightBubbles.Enqueue(newBubble);
        }
    }
    
    public void SetCharacterOpacity(GameObject character, bool isSpeaking)
    {
        if (character == null)
        {
            Debug.LogError("Character is null. Cannot set opacity.");
            return;
        }
        
        CanvasGroup canvasGroup = character.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError($"Character {character.name} does not have a CanvasGroup component.");
            return;
        }
        canvasGroup.alpha = isSpeaking ? 1f : 0.5f;
    }
}

[System.Serializable]
public class Dialogue
{
    public int id;
    public string speaker;
    public string text;

    public Dialogue(int id, string speaker, string text)
    {
        this.id = id;
        this.speaker = speaker;
        this.text = text;
    }
}