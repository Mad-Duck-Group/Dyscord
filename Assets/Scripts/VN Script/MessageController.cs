using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    public Text dialogueText;  // ข้อความที่จะแสดง
    public CanvasGroup canvasGroup;  // ใช้สำหรับจัดการโปร่งใส
    public RectTransform rectTransform;  // ใช้สำหรับเลื่อนกล่องข้อความ

    public float fadeDuration = 0.5f;  // ระยะเวลาสำหรับการทำ fade
    public float slideAmount = 20f;    // ระยะที่ข้อความจะเลื่อนขึ้น
    public float slideDuration = 0.5f; // ระยะเวลาที่ข้อความจะเลื่อน

    // ฟังก์ชันสำหรับเลื่อนข้อความและทำให้ทึบ
    public void SlideAndFadeOut()
    {
        StartCoroutine(FadeOutAndSlide());
    }

    private IEnumerator FadeOutAndSlide()
    {
        // ตั้งค่าเริ่มต้น
        float fadeStartTime = Time.time;
        Vector3 originalPosition = rectTransform.anchoredPosition;
        Vector3 targetPosition = originalPosition + new Vector3(0, slideAmount, 0);  // ตำแหน่งใหม่เลื่อนขึ้น

        while (Time.time < fadeStartTime + fadeDuration)
        {
            // คำนวณความโปร่งใส
            float elapsed = Time.time - fadeStartTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);  // ทำให้ข้อความค่อย ๆ ทึบลง

            // เลื่อนตำแหน่งข้อความขึ้น
            rectTransform.anchoredPosition = Vector3.Lerp(originalPosition, targetPosition, t);
            yield return null;
        }

        // ตั้งค่าความโปร่งใสเป็น 0 (ทึบสนิท)
        canvasGroup.alpha = 0f;
    }
}

