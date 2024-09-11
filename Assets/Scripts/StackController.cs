using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask _hexagonLayerMask;
    [SerializeField] private LayerMask _gridHexagonLayerMask;
    [SerializeField] private LayerMask _groundLayerMask;
    private HexagonStack _currentStack;
    private Vector3 _currentStackInitialPosition;

    [Header("Data")]
    private GridCell _targetCell;

    [Header("Actions")]
    public static Action<GridCell> OnStackPlaced;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ManageControl();
    }

    private void ManageControl()
    {
        if (Input.GetMouseButtonDown(0))
            ManageMouseDown();
        else if (Input.GetMouseButton(0) && _currentStack != null)
            ManageMouseDrag();
        else if (Input.GetMouseButtonUp(0) && _currentStack != null)
            ManageMouseUp();
    }

    private void ManageMouseDown()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, _hexagonLayerMask);

        if (hit.collider == null)
        {
            Debug.Log("We have not detect any hexagon");
            return;
        }

        _currentStack = hit.collider.transform.parent.GetComponent<Hexagon>().HexagonStack;
        _currentStackInitialPosition = _currentStack.transform.position;
    }

    private void ManageMouseDrag()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, _gridHexagonLayerMask);

        if (hit.collider == null)
            DraggingAboveGround();
        else
            DraggingAboveGridCell(hit);
    }

    private void ManageMouseUp()
    {
        if (_targetCell == null)
        {
            _currentStack.transform.position = _currentStackInitialPosition;
            _currentStack = null;
            return;
        }

        _currentStack.transform.position = _targetCell.transform.position.With(y: 0.2f);
        _currentStack.transform.SetParent(_targetCell.transform);
        _currentStack.Place();

        _targetCell.AssignStack(_currentStack);

        OnStackPlaced?.Invoke(_targetCell);

        _targetCell = null;
        _currentStack = null;
    }

    private void DraggingAboveGridCell(RaycastHit hit)
    {
        GridCell gridCell = hit.collider.transform.parent.GetComponent<GridCell>();

        if (gridCell.IsOccupied)
            DraggingAboveGround();
        else
            DraggingAboveNonOccupiedGridCell(gridCell);

    }

    private void DraggingAboveNonOccupiedGridCell(GridCell gridCell)
    {
        Vector3 currentStackTargetPos = gridCell.transform.position.With(y: 2);

        _currentStack.transform.position = Vector3.MoveTowards(
            _currentStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        _targetCell = gridCell;
    }

    private void DraggingAboveGround()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, _groundLayerMask);

        if (hit.collider == null)
        {
            Debug.LogError("No ground detected, this is unusual...");
            return;
        }

        Vector3 currentStackTargetPos = hit.point.With(y: 2);

        _currentStack.transform.position = Vector3.MoveTowards(
            _currentStack.transform.position,
            currentStackTargetPos,
            Time.deltaTime * 30);

        _targetCell = null;
    }

    private Ray GetClickedRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
}
