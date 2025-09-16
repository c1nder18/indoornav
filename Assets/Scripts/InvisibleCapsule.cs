

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InvisibleCapsule : MonoBehaviour
{
    public float maxDistance = 3f;      // Maximum distance from user
    public float normalDistance = 2.0f; // Normal distance from user the capsule should be at
    
    private NavMeshAgent agent;
    private Camera cam;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cam = Camera.main;
        
        GoTo(Vector3.zero);
    }

    private void Update()
    {
        GameObject next = NavigationManager.Instance.GetNextLocationInPath();
        if (next != null && agent.destination != next.transform.position) GoTo(next.transform.position);
        
        var distanceToUser = Vector3.Distance(cam.transform.position, transform.position);
        var distanceToDestination = Vector3.Distance(transform.position, agent.destination);
        var distanceOfUserToDestination = Vector3.Distance(cam.transform.position, agent.destination);
        
        // if too far from player, go back
        if (distanceToUser >= maxDistance) GoTo(cam.transform.position);
        // don't go too far from player, stop, but only if you are closer to destination than him
        else if (distanceToUser >= normalDistance && distanceToDestination < distanceOfUserToDestination) Stop();
    }

    public void GoTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
}
