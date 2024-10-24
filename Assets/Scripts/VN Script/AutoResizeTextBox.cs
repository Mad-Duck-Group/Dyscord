using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoResizeTextBox : MonoBehaviour
{
    public TMP_Text dialogueText;  // ข้อความที่จะถูกแสดงในกล่อง
    public RectTransform textBoxRect;  // RectTransform ของกล่องข้อความ

    void Update()
    {
        // ปรับขนาดกล่องข้อความตามขนาดของข้อความที่แสดง
        float newHeight = dialogueText.preferredHeight;
        textBoxRect.sizeDelta = new Vector2(textBoxRect.sizeDelta.x, newHeight);
    }
}
