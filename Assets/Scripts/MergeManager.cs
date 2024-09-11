using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MergeManager : MonoBehaviour
{
    [Header (" Elements ")]
    private List<GridCell> updatedCells = new List<GridCell>();

    private void Awake()
    {
        StackController.OnStackPlaced += StackPlacedCallBack;
    }

    private void OnDestroy()
    {
        StackController.OnStackPlaced -= StackPlacedCallBack;
    }

    private void StackPlacedCallBack(GridCell gridCell)
    {
        StartCoroutine(StackPlacedCoroutine(gridCell));
    }

    IEnumerator StackPlacedCoroutine(GridCell gridCell)
    {
        updatedCells.Add(gridCell);

        while (updatedCells.Count > 0)
            yield return CheckForMerge(updatedCells[0]);
    }

    IEnumerator CheckForMerge(GridCell gridCell)
    {
        updatedCells.Remove(gridCell);

        if(!gridCell.IsOccupied)
            yield break;

        // Does this cell has neighbors ?
        List<GridCell> neighborGridCells = GetNeighborGridCells(gridCell);

        if (neighborGridCells.Count <= 0)
        {
            Debug.Log("No neighbors for this cell");
            yield break;
        }

        // At this point, we have a list of the neighbor grid cells, that are occupied
        Color gridCellTopHexagonColor = gridCell.HexagonStack.GetTopHexagonColor();

        // Do these neighbors have the same top hex color ?
        List<GridCell> similarNeighborGridCells = GetSimilarNeighborGridCells(gridCellTopHexagonColor, neighborGridCells.ToArray());

        if (similarNeighborGridCells.Count <= 0)
        {
            Debug.Log("No similar neighbors for this cell");
            yield break;
        }

        updatedCells.AddRange(similarNeighborGridCells);

        // At this point, we have a list of similar neighbors
        List<Hexagon> hexagonsToAdd = GetHexagonsToAdd(gridCellTopHexagonColor, similarNeighborGridCells.ToArray());

        // Remove the hexagons from their stacks
        RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighborGridCells.ToArray());

        // At this point, we have removed the stacks we don't need anymore
        // We need to merge !

        MoveHexagons(gridCell, hexagonsToAdd);

        yield return new WaitForSeconds(.2f + (hexagonsToAdd.Count + 1) * .01f);

        // Merge everything inside of this cell

        // Is the stack on this cell complete ?
        // Does it have 10 or more similar hexagons ?
        yield return CheckForCompleteStack(gridCell, gridCellTopHexagonColor);

        // Check the updated cells
        // Repeat
    }

    private List<GridCell> GetNeighborGridCells(GridCell gridCell)
    {
        LayerMask gridCellMask = 1 << gridCell.gameObject.layer;

        List<GridCell> neighborGridCells = new List<GridCell>();

        Collider[] neighborGridCellColliders = Physics.OverlapSphere(gridCell.transform.position, 2, gridCellMask);

        // At this point, we have the grid cell collider neighbors
        foreach (Collider col in neighborGridCellColliders)
        {
            GridCell neighborGridCell = col.transform.parent.GetComponent<GridCell>();

            if (!neighborGridCell.IsOccupied)
                continue;

            if (neighborGridCell == gridCell)
                continue;

            neighborGridCells.Add(neighborGridCell);
        }

        return neighborGridCells;
    }

    private List<GridCell> GetSimilarNeighborGridCells(Color gridCellTopHexagonColor, GridCell[] neighborGridCells)
    {
        List<GridCell> similarNeighborGridCells = new List<GridCell>();

        foreach (GridCell neighborGridCell in neighborGridCells)
        {
            Color neighborGridCellTopHexagonColor = neighborGridCell.HexagonStack.GetTopHexagonColor();

            if (gridCellTopHexagonColor == neighborGridCellTopHexagonColor)
                similarNeighborGridCells.Add(neighborGridCell);
        }

        return similarNeighborGridCells;
    }

    private List<Hexagon> GetHexagonsToAdd(Color gridCellTopHexagonColor, GridCell[] similarNeighborGridCells)
    {
        List<Hexagon> hexagonsToAdd = new List<Hexagon>();

        foreach (GridCell neighborCell in similarNeighborGridCells)
        {
            HexagonStack neighborCellHexStack = neighborCell.HexagonStack;

            for (int i = neighborCellHexStack.Hexagons.Count - 1; i >= 0; i--)
            {
                Hexagon hexagon = neighborCellHexStack.Hexagons[i];

                if (hexagon.Color != gridCellTopHexagonColor)
                    break;

                hexagonsToAdd.Add(hexagon);
                hexagon.SetParent(null);
            }
        }
        
        return hexagonsToAdd;
    }

    private void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, GridCell[] similarNeighborGridCells)
    {
        foreach (GridCell neighborCell in similarNeighborGridCells)
        {
            HexagonStack stack = neighborCell.HexagonStack;

            foreach (Hexagon hexagon in hexagonsToAdd)
            {
                if (stack.Contains(hexagon))
                    stack.Remove(hexagon);
            }
        }
    }

    private void MoveHexagons(GridCell gridCell, List<Hexagon> hexagonsToAdd)
    {
        float initialY = gridCell.HexagonStack.Hexagons.Count * .2f;

        for (int i = 0; i < hexagonsToAdd.Count; i++)
        {
            Hexagon hexagon = hexagonsToAdd[i];

            float targetY = initialY + i * .2f;
            Vector3 targetLocalPosition = Vector3.up * targetY;

            gridCell.HexagonStack.Add(hexagon);
            hexagon.MoveToLocal(targetLocalPosition);
        }
    }

    private IEnumerator CheckForCompleteStack(GridCell gridCell, Color topColor)
    {
        if (gridCell.HexagonStack.Hexagons.Count < 10)
            yield break;

        List<Hexagon> similarHexagons = new List<Hexagon>();

        for (int i = gridCell.HexagonStack.Hexagons.Count - 1; i >= 0; i--)
        {
            Hexagon hex = gridCell.HexagonStack.Hexagons[i];

            if (hex.Color != topColor)
                break;

            similarHexagons.Add(hex);
        }

        // At this point, we have a list of similar hexagons
        // How many ?

        int similarHexagonCount = similarHexagons.Count;

        if (similarHexagons.Count < 10)
            yield break;

        float delay = 0f;

        while(similarHexagons.Count > 0)
        {
            similarHexagons[0].SetParent(null);
            similarHexagons[0].Vanish(delay);

            delay += .01f;

            gridCell.HexagonStack.Remove(similarHexagons[0]);
            similarHexagons.RemoveAt(0);
        }

        updatedCells.Add(gridCell);

        yield return new WaitForSeconds(.2f + (similarHexagonCount + 1) * .01f);
    }
}
