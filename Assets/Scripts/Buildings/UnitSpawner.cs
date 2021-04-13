using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    #region Server

    [Command]
    private void CmdSpawnUnit()
    {
        Vector3 spawnPos = new Vector3(unitSpawnPoint.position.x, 0, unitSpawnPoint.position.z);
        GameObject unitInstance = Instantiate(unitPrefab, spawnPos, unitSpawnPoint.rotation);

        // connectionToClient exists on the NetworkBehavior and is a way to identify the player that called this
        // and assign ownership
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
       //  NetworkServer.Destroy(gameObject);
    }

    #endregion Server

    #region Client
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!hasAuthority)
            return;

        CmdSpawnUnit();
    }
    #endregion Client
}
