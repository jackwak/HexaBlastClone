using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _hexagon;

    [Header(" Settings ")]
    [OnValueChanged("GenerateGrid")]
    [SerializeField] private int _gridSize;

    private void GenerateGrid()
    {
        transform.Clear();

        for (int x = -_gridSize; x <= _gridSize; x++)
        {
            for (int y = -_gridSize; y <= _gridSize; y++)
            {
                Vector3 spawnPosition = _grid.CellToWorld(new Vector3Int(x, y, 0));

                if (spawnPosition.magnitude > _grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * _gridSize)
                    continue;

                Instantiate(_hexagon, spawnPosition, Quaternion.identity, transform);
            }
        }
    }
}
