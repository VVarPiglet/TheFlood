using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 2f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null)
            return;

        if (!CanFireAtTarget(target))
            return;

        // If we are able to fire at the target then rotate to face them.
        // We can figure out where we would like to aim, and then can
        // rotate between our current rotation and the target rotation
        Quaternion targetRotation = Quaternion.LookRotation(GetTargetSelfTransformDifference(target));

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Fire
        if(Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(targeter.GetTarget().GetAimAtPoint().position - projectileSpawnPoint.position);

            // We can now fire
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget(Targetable target)
    {
        float distanceBetweenSqrd = GetTargetSelfTransformDifference(target).sqrMagnitude;
        float fireRangeSqrd = fireRange * fireRange;

        // sqrMagnitude is a more efficient way than Vector3.Distance since
        // .Distance needs to get the square root, and the former just gets the
        // square. However since it is the square we also need to square our 
        // chase range.
        return distanceBetweenSqrd <= fireRangeSqrd;
    }

    [Server]
    private Vector3 GetTargetSelfTransformDifference(Targetable target)
    {
        return (target.transform.position - transform.position);
    }
}
