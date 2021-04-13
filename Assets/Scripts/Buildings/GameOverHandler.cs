using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameOverHandler : NetworkBehaviour
{
    private List<UnitBase> activeBases = new List<UnitBase>();

    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        activeBases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        activeBases.Remove(unitBase);

        // Remove defeated player from the game ?

        // If there is only one player left then they have won the game
        if (activeBases.Count == 1)
        {
            int playerId = activeBases[0].connectionToClient.connectionId;

            // Announce that the only player left is the winner
            RpcGameOver($"Player {playerId}");

            ServerOnGameOver?.Invoke();
        }
    }

    #endregion Server

    #region Client

    /// <summary>
    /// Client rpcs are functions that are called by the server to the clients
    /// In this case the server is sending the game over / announcing a winning
    /// player.
    /// </summary>
    /// <param name="winningPlayerName"></param>
    [ClientRpc]
    private void RpcGameOver(string winningPlayerName)
    {
        ClientOnGameOver?.Invoke(winningPlayerName);
    }

    #endregion Client
}
