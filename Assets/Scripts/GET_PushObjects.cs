using UnityEngine;
using System.Collections.Generic;

public class GET_PushObjects : MonoBehaviour
{
    public float pushForce = 5f; // Force applied when pushing with "E"
    public float pushRange = 2f; // Maximum distance for pushing with "E"
    public float collisionPushStrength = 3f; // Strength of push when colliding
    public Camera playerCamera; // Reference to the player's camera
    public float raycastLength = 2f; // Adjustable raycast length in the inspector
    private Renderer lastHighlightedRenderer;
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    private void Update()
    {
        if (Input.GetKey(KeyCode.E)) // Hold 'E' to push objects
        {
            TryPushObject();
        }
        HighlightObject();
    }

    void TryPushObject()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera not assigned!");
            return;
        }

        RaycastHit hit;
        Vector3 forward = playerCamera.transform.forward; // Use camera's forward direction
        Vector3 origin = playerCamera.transform.position; // Start ray from camera position

        if (Physics.Raycast(origin, forward, out hit, raycastLength))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            Rigidbody rb = hit.collider.attachedRigidbody;

            if (rb != null && !rb.isKinematic)
            {
                Vector3 forceDirection = hit.point - origin;
                forceDirection.y = 0; // Keep force horizontal
                rb.AddForce(forceDirection.normalized * pushForce, ForceMode.Impulse);
            }
            
            if (renderer != null && originalColors.ContainsKey(renderer))
            {
                renderer.material.SetColor("_BaseColor", originalColors[renderer]); // Reset to original color after push
            }
        }
    }

    void HighlightObject()
    {
        if (playerCamera == null)
            return;

        RaycastHit hit;
        Vector3 forward = playerCamera.transform.forward;
        Vector3 origin = playerCamera.transform.position;

        if (Physics.Raycast(origin, forward, out hit, raycastLength))
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (lastHighlightedRenderer != renderer)
                {
                    if (lastHighlightedRenderer != null && originalColors.ContainsKey(lastHighlightedRenderer))
                        lastHighlightedRenderer.material.SetColor("_BaseColor", originalColors[lastHighlightedRenderer]);

                    if (!originalColors.ContainsKey(renderer))
                        originalColors[renderer] = renderer.material.GetColor("_BaseColor");

                    renderer.material.SetColor("_BaseColor", Color.yellow); // Change to yellow
                    lastHighlightedRenderer = renderer;
                }
            }
        }
        else if (lastHighlightedRenderer != null && originalColors.ContainsKey(lastHighlightedRenderer))
        {
            lastHighlightedRenderer.material.SetColor("_BaseColor", originalColors[lastHighlightedRenderer]);
            lastHighlightedRenderer = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * raycastLength);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        Renderer renderer = hit.collider.GetComponent<Renderer>();

        if (rb != null && !rb.isKinematic)
        {
            Vector3 forceDirection = hit.moveDirection; // Use player's movement direction
            forceDirection.y = 0; // Keep force horizontal to avoid tilting

            rb.AddForce(forceDirection * collisionPushStrength, ForceMode.Impulse);
        }
        
        if (renderer != null && originalColors.ContainsKey(renderer))
        {
            renderer.material.SetColor("_BaseColor", originalColors[renderer]); // Reset to original color after collision
        }
    }
}