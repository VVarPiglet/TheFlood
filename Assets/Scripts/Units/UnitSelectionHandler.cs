using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 dragStartPosition;
    private Camera mainCamera;
    private RTSPlayer player;

    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;

        Unit.AuthorityOnUnitDespawn += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawn -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        //temp fix
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            StartSelectionArea();
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
            ClearSelectionArea();
        else if (Mouse.current.leftButton.isPressed)
            UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        float areaWidth = currentMousePosition.x - dragStartPosition.x;
        float areaHeight = currentMousePosition.y - dragStartPosition.y;
        
        // divide the width and height of the selection area to get the anchor
        // because it is a square and the anchor is at its center.
        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = dragStartPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void StartSelectionArea()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
                selectedUnit.Deselect();

            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);
        dragStartPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if(unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool didHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask);
            if (!didHit)
                return;

            bool wasUnitHit = hit.collider.TryGetComponent<Unit>(out Unit unit);
            if (!wasUnitHit || !unit.hasAuthority)
                return;

            SelectedUnits.Add(unit);

            foreach (Unit selectedUnit in SelectedUnits)
                selectedUnit.Select();
        }

        // Calculate the min x and max x, min y and max y of the selection area.
        // Test units to see if they are inside the selection area. If they are
        // then treat them as selected.
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach(Unit unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit))
                continue;

            // Get convert the units world position to screen position
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            if(screenPosition.x > min.x && screenPosition.x < max.x && 
                screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string obj)
    {
        enabled = false;
    }
}
