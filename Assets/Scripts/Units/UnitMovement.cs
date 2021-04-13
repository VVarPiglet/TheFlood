using Mirror;
using System;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if(target != null)
        {
            float distanceBetweenSqrd = (target.transform.position - transform.position).sqrMagnitude;
            float chaseRangeSqrd = chaseRange * chaseRange;

            // sqrMagnitude is a more efficient way than Vector3.Distance since
            // .Distance needs to get the square root, and the former just gets the
            // square. However since it is the square we also need to square our 
            // chase range.
            if (distanceBetweenSqrd > chaseRangeSqrd)
            {
                // Chase target
                agent.SetDestination(target.transform.position);
            }
            else if(agent.hasPath)
            {
                // Stop chasing target
                agent.ResetPath();
            }
            return;
        }

        // Click to move to a new location
        if (!agent.hasPath)
            return;

        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 newPosition)
    {
        targeter.ClearTarget();

        // Check to see if the click position is a valid position. If it is not then return.
        if (!NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            return;

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }
    #endregion Server
}
