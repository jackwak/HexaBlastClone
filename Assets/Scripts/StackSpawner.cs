using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackSpawner : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Transform _stackPositionsParent;
    [SerializeField] private GameObject _hexagonPrefab;
    [SerializeField] private GameObject _hexagonStackPrefab;

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
        GameObject hexagonStack = Instantiate(_hexagonStackPrefab, parent.position, Quaternion.identity, parent);
        hexagonStack.name = $"Stack {parent.GetSiblingIndex()}";

        int amount = Random.Range(2, 7);
        for (int i = 0; i < amount; i++)
        {
            Vector3 hexagonLocalPosition = Vector3.up * i * .2f;
            Vector3 spawnPosition = hexagonStack.transform.TransformPoint(hexagonLocalPosition);
            GameObject hexagonInstance = Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity, hexagonStack.transform);
        }
    }
}
