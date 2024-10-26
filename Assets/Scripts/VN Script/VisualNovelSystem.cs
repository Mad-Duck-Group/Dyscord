using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VisualNovelSystem : MonoBehaviour
{
    // UI elements for dialogue and speaker
    public TMP_Text dialogueText;      // ช่องข้อความสนทนา
    public TMP_Text speakerText;       // ชื่อผู้พูด
    public GameObject nextHintIcon;    // ไอคอนสำหรับบอกผู้เล่นให้คลิกต่อ
    
    // Character objects for setting opacity
    public GameObject leftCharacter;   // ตัวละครฝั่งซ้าย (ผู้เล่น)
    public GameObject rightCharacter;  // ตัวละครฝั่งขวา (NPC)

    // Chat bubble prefab and containers
    public GameObject chatBubblePrefab;    // Prefab สำหรับสร้างฟองแชท
    public Transform chatContainerLeft;    // พื้นที่สำหรับฟองแชทฝั่งซ้าย
    public Transform chatContainerRight;   // พื้นที่สำหรับฟองแชทฝั่งขวา

    // Scroll view for chat bubbles
    public ScrollRect chatScrollView;

    private List<Dialogue> dialogues;      // บทสนทนาทั้งหมด
    private int currentIndex = 0;          // ตำแหน่งบทสนทนาปัจจุบัน

    void Start()
    {
        // Initialize dialogue
        dialogues = new List<Dialogue>()
        {
            new Dialogue(1,"Player", "What are you doing here?"),
            new Dialogue(2,"NPC", "I'm just waiting for someone."),
            new Dialogue(3,"Player", "Really? Who?"),
            new Dialogue(4,"NPC", "Nope.")
        };

        // Display the first dialogue
        ShowNextMessage();
    }

    void Update()
    {
        // Show next message when left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextMessage();
        }
    }

    // Show the next dialogue message
    public void ShowNextMessage()
    {
        if (currentIndex < dialogues.Count)
        {
            var dialogue = dialogues[currentIndex];

            // Add chat bubble based on speaker
            bool isPlayer = dialogue.speaker == "Player";
            AddChatBubble(dialogue.text, isPlayer);

            // Update speaker text
            if (speakerText == null)
            {
                Debug.LogError("SpeakerText is not assigned in the inspector.");
                return;
            }
            speakerText.text = dialogue.speaker;

            // Update character opacity
            SetCharacterOpacity(leftCharacter, isPlayer);
            SetCharacterOpacity(rightCharacter, !isPlayer);

            // Scroll to the bottom
            ScrollToBottom();

            // Show or hide next hint icon based on dialogue progression
            nextHintIcon.SetActive(currentIndex < dialogues.Count - 1);

            currentIndex++;
        }
        else
        {
            Debug.LogWarning("No more dialogues to display.");
        }
    }

    // Add a chat bubble to the UI
    public void AddChatBubble(string message, bool isLeftSide)
    {
        GameObject newChatBubble = Instantiate(chatBubblePrefab);

        if (isLeftSide)
        {
            newChatBubble.transform.SetParent(chatContainerLeft, false);  // วางใน Parent ซ้าย
        }
        else
        {
            newChatBubble.transform.SetParent(chatContainerRight, false); // วางใน Parent ขวา
        }

        TextMeshProUGUI chatText = newChatBubble.GetComponentInChildren<TextMeshProUGUI>();
        chatText.text = message;

        // อัปเดต Layout ใหม่หลังจากเพิ่มฟองแชท
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainerLeft.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContainerRight.GetComponent<RectTransform>());
    
        // เลื่อน Scroll View ลงไปที่ด้านล่าง
        ScrollToBottom();
    }

    // Set the opacity of a character
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

    // Scroll to the bottom of the chat
    public void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }

    IEnumerator ScrollToBottomCoroutine()
    {
        // รอหนึ่งเฟรมเพื่อให้ Unity อัปเดตการจัดวางฟองแชท
        yield return null;

        // เลื่อน Scroll View ไปที่ด้านล่าง
        chatScrollView.verticalNormalizedPosition = 0f;
    }
}



