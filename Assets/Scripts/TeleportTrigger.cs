using UnityEngine;
using System.Collections.Generic;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private bool copyDestinationRotation = false;
    [SerializeField] private float teleportCooldownSeconds = 0.2f;
    [SerializeField] private bool enableDebugLogs = true;

    private static readonly Dictionary<int, float> PlayerCooldownUntil = new Dictionary<int, float>();

    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs)
        {
            Debug.Log("TeleportTrigger(3D): trigger with " + other.name + " | tag=" + other.tag, this);
        }

        TryTeleport(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enableDebugLogs)
        {
            Debug.Log("TeleportTrigger(2D): trigger with " + other.name + " | tag=" + other.tag, this);
        }

        TryTeleport(other.gameObject);
    }

    private void TryTeleport(GameObject triggerObject)
    {
        if (triggerObject == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("TeleportTrigger: targetObject is null.", this);
            }

            return;
        }

        Transform target = ResolveTeleportTarget(triggerObject.transform);
        if (target == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("TeleportTrigger: unable to resolve teleport target from trigger object " + triggerObject.name, this);
            }

            return;
        }

        GameObject targetObject = target.gameObject;
        if (!targetObject.CompareTag("Player"))
        {
            if (enableDebugLogs)
            {
                Debug.Log("TeleportTrigger: ignored object (not Player): " + targetObject.name + " | tag=" + targetObject.tag + " | trigger source=" + triggerObject.name, this);
            }

            return;
        }

        if (destination == null)
        {
            Debug.LogWarning("TeleportTrigger: destination is not assigned.", this);
            return;
        }

        int playerId = targetObject.GetInstanceID();
        if (PlayerCooldownUntil.TryGetValue(playerId, out float cooldownUntil) && Time.time < cooldownUntil)
        {
            if (enableDebugLogs)
            {
                Debug.Log("TeleportTrigger: cooldown active for " + targetObject.name + " (" + (cooldownUntil - Time.time).ToString("F2") + "s remaining)", this);
            }

            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log("TeleportTrigger: teleporting " + targetObject.name + " from " + target.position + " -> " + destination.position, this);
        }

        Teleport(target);
        PlayerCooldownUntil[playerId] = Time.time + Mathf.Max(0f, teleportCooldownSeconds);

        if (enableDebugLogs)
        {
            Debug.Log("TeleportTrigger: teleport complete for " + targetObject.name + " | now at " + target.position, this);
        }
    }

    private static Transform ResolveTeleportTarget(Transform source)
    {
        if (source == null)
        {
            return null;
        }

        Rigidbody2D rb2d = source.GetComponentInParent<Rigidbody2D>();
        if (rb2d != null)
        {
            return rb2d.transform;
        }

        CharacterController cc = source.GetComponentInParent<CharacterController>();
        if (cc != null)
        {
            return cc.transform;
        }

        TopDownPlayerController topDownController = source.GetComponentInParent<TopDownPlayerController>();
        if (topDownController != null)
        {
            return topDownController.transform;
        }

        PlayerController playerController = source.GetComponentInParent<PlayerController>();
        if (playerController != null)
        {
            return playerController.transform;
        }

        return source;
    }

    private void Teleport(Transform target)
    {
        Vector3 destinationPosition = destination.position;

        CharacterController cc = target.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            target.SetPositionAndRotation(destinationPosition, copyDestinationRotation ? destination.rotation : target.rotation);

            cc.enabled = true;
            return;
        }

        Rigidbody2D rb2d = target.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.simulated = false;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            target.SetPositionAndRotation(destinationPosition, copyDestinationRotation ? destination.rotation : target.rotation);
            rb2d.position = new Vector2(destinationPosition.x, destinationPosition.y);
            if (copyDestinationRotation) rb2d.rotation = destination.eulerAngles.z;
            rb2d.simulated = true;
            Physics2D.SyncTransforms();

            return;
        }

        target.SetPositionAndRotation(destinationPosition, copyDestinationRotation ? destination.rotation : target.rotation);
    }
}