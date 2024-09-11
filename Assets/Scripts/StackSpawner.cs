using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StackSpawner : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform _stackPositionsParent;
    [SerializeField] private Hexagon _hexagonPrefab;
    [SerializeField] private HexagonStack _hexagonStackPrefab;

    [Header("Settings")]
    [NaughtyAttributes.MinMaxSlider(2, 8)]
    [SerializeField] private Vector2Int _minMaxHexagonCount;
    [SerializeField] private Color[] _colors;
    private int _stackCounter; 

    private void Awake()
    {
        Application.targetFrameRate = 60;

        StackController.OnStackPlaced += StackPlacedCallBack;
    }

    private void OnDestroy()
    {
        StackController.OnStackPlaced -= StackPlacedCallBack;
    }

    private void StackPlacedCallBack(GridCell gridCell)
    {
        _stackCounter++;

        if (_stackCounter >= 3)
        {
            _stackCounter = 0;
            GenerateStacks();
        }
    }

    private void Start()
    {
        GenerateStacks();
    }

    private void GenerateStacks()
    {
        for (int i = 0; i < _stackPositionsParent.childCount; i++)
            GenerateStack(_stackPositionsParent.GetChild(i));
    }

    private void GenerateStack(Transform parent)
    {
        HexagonStack hexagonStack = Instantiate(_hexagonStackPrefab, parent.position, Quaternion.identity, parent);
        hexagonStack.name = $"Stack {parent.GetSiblingIndex()}";

        int amount = Random.Range(_minMaxHexagonCount.x, _minMaxHexagonCount.y);

        int firstColorHexagonCount = Random.Range(0, amount);

        Color[] colorArray = GetRandomColors();

        for (int i = 0; i < amount; i++)
        {
            Vector3 hexagonLocalPosition = Vector3.up * i * .2f;
            Vector3 spawnPosition = hexagonStack.transform.TransformPoint(hexagonLocalPosition);

            Hexagon hexagonInstance = Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity, hexagonStack.transform);
            hexagonInstance.Color = i < firstColorHexagonCount ? colorArray[0] : colorArray[1];

            hexagonInstance.Configure(hexagonStack);

            hexagonStack.Add(hexagonInstance);

        }
    }

    private Color[] GetRandomColors()
    {
        List<Color> colorList = new List<Color>();
        colorList.AddRange(_colors);

        if (colorList.Count <= 0)
        {
            Debug.LogError("No color found");
            return null;
        }

        Color firstColor = colorList.OrderBy(color => Random.value).First();

        colorList.Remove(firstColor);

        if (colorList.Count <= 0)
        {
            Debug.LogError("Only one color was found");
            return null;
        }

        Color secondColor = colorList.OrderBy(color => Random.value).First();

        return new Color[] { firstColor, secondColor };
    }
}
