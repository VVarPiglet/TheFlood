using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    /// <summary>
    /// Whenever the server updates the health it will call the client method to update the health
    /// </summary>
    [SyncVar(hook = nameof(HandleHealthUpdated))] //SyncVar is only updateable by the server
    private int currentHealth;

    public event Action ServerOnDie;

    /// <summary>
    /// Event just for clients
    /// </summary>
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth <= 0)
            return;

        //currentHealth -= damageAmount;
        //if (currentHealth < 0)
        //    currentHealth = 0;
        // The below code does the same thing as the above. Max() returns
        // whatever is greater
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        // If we don't have health anymore, raise the death event
        // The question mark stops the errors if no one is listening to the event
        if(currentHealth <= 0)
            ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId)
            return;

        DealDamage(currentHealth);
    }

    #endregion Server

    #region Client

    /// <summary>
    /// Update the health on the client by raising the client event
    /// </summary>
    /// <param name="oldHealth"></param>
    /// <param name="newHealth"></param>
    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }
    #endregion Client
}
