using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonMouseHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private bool playPointSound = true;
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool bumpButton = true;
    [SerializeField][ShowIf(nameof(bumpButton))] private float bumpScale = 1.2f;
    
    private Button _button;
    private Tween _mouseHoverTween;
    private Vector3 _originalScale;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        if (_mouseHoverTween.IsActive()) _mouseHoverTween.Kill();
        transform.localScale = _originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        // if (playPointSound)
        //     SoundManager.Instance.PlaySoundFX(SoundFXTypes.MousePointButton, out _);
        if (bumpButton)
        {
            if (_mouseHoverTween.IsActive()) _mouseHoverTween.Kill();
            _mouseHoverTween = transform.DOScale(bumpScale, 0.1f).SetUpdate(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        if (bumpButton)
        {
            if (_mouseHoverTween.IsActive()) _mouseHoverTween.Kill();
            _mouseHoverTween = transform.DOScale(1f, 0.1f).SetUpdate(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button is not PointerEventData.InputButton.Left) return;
        if (!_button.interactable) return;
        // if (playClickSound)
        //     SoundManager.Instance.PlaySoundFX(SoundFXTypes.MouseClickButton, out _);
    }
}

