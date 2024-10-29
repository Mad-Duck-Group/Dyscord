using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DynamicTextManager : MonoBehaviour
{

    public static DynamicTextData defaultData;
    public static GameObject canvasPrefab;
    public static Transform mainCamera;

    [SerializeField] private DynamicTextData _defaultData;
    [SerializeField] private GameObject _canvasPrefab;
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private DynamicTextData _damageData;
    [SerializeField] private DynamicTextData _ramData;
    [SerializeField] private DynamicTextData _shieldData;

    public static DynamicTextData damageData;
    public static DynamicTextData ramData;
    public static DynamicTextData shieldData;

    private void Awake()
    {
        defaultData = _defaultData;
        mainCamera = _mainCamera;
        canvasPrefab = _canvasPrefab;
        damageData = _damageData;
        ramData = _ramData;
        shieldData = _shieldData;
    }

    public static void CreateText2D(Vector2 position, string text, DynamicTextData data)
    {
        GameObject newText = Instantiate(canvasPrefab, position, Quaternion.identity);
        newText.transform.GetComponent<DynamicText2D>().Initialise(text, data);
    }

    public static void CreateText(Vector3 position, string text, DynamicTextData data)
    {
        GameObject newText = Instantiate(canvasPrefab, position, Quaternion.identity);
        newText.transform.GetComponent<DynamicText>().Initialise(text, data);
    }

}
