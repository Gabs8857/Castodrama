using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TeleportCameraController : MonoBehaviour
{
    [Serializable]
    public class TeleportCameraPreset
    {
        [Tooltip("Nom exact de la destination du TP, par ex. TP_INT ou TP_Faim")]
        public string destinationName;

        [Tooltip("Position caméra à appliquer pour cette destination")]
        public Vector3 cameraPosition = new Vector3(0f, 0f, -10f);

        [Tooltip("Si coché, la caméra est replacée avant le TP du joueur")]
        public bool snapBeforeTeleport = true;

        [Tooltip("État du TopDownCameraFollow après le TP")]
        public bool followEnabledAfterTeleport = false;
    }

    [Header("References")]
    [SerializeField] private TopDownCameraFollow topDownCameraFollow;

    [Header("Default State")]
    [SerializeField] private bool disableFollowByDefault = true;
    [SerializeField] private bool defaultFollowEnabledAfterTeleport = false;
    [SerializeField] private Vector3 defaultCameraPosition = new Vector3(0f, 0f, -10f);
    [SerializeField] private bool defaultSnapBeforeTeleport = false;

    [Header("Teleport Presets")]
    [SerializeField] private List<TeleportCameraPreset> presets = new List<TeleportCameraPreset>();

    private void Awake()
    {
        CacheReferences();
        ApplyDefaultFollowState();
    }

    private void Start()
    {
        CacheReferences();
        ApplyDefaultFollowState();
    }

    public void PrepareTeleport(Transform destination)
    {
        CacheReferences();

        TeleportCameraPreset preset = FindPreset(destination);
        if (preset == null)
        {
            if (disableFollowByDefault && topDownCameraFollow != null)
            {
                topDownCameraFollow.enabled = false;
            }

            if (defaultSnapBeforeTeleport)
            {
                transform.position = defaultCameraPosition;
            }

            return;
        }

        if (topDownCameraFollow != null)
        {
            topDownCameraFollow.enabled = false;
        }

        if (preset.snapBeforeTeleport)
        {
            transform.position = preset.cameraPosition;
        }
    }

    public void FinalizeTeleport(Transform destination)
    {
        CacheReferences();

        TeleportCameraPreset preset = FindPreset(destination);
        if (preset == null)
        {
            if (defaultFollowEnabledAfterTeleport)
            {
                EnableFollow();
            }
            else
            {
                DisableFollow();
            }

            if (!defaultSnapBeforeTeleport)
            {
                transform.position = defaultCameraPosition;
            }

            return;
        }

        if (!preset.snapBeforeTeleport)
        {
            transform.position = preset.cameraPosition;
        }

        if (preset.followEnabledAfterTeleport)
        {
            EnableFollow();
        }
        else
        {
            DisableFollow();
        }
    }

    private void CacheReferences()
    {
        if (topDownCameraFollow == null)
        {
            topDownCameraFollow = GetComponent<TopDownCameraFollow>();
        }
    }

    private void ApplyDefaultFollowState()
    {
        if (disableFollowByDefault)
        {
            DisableFollow();
        }
    }

    private void EnableFollow()
    {
        if (topDownCameraFollow != null)
        {
            topDownCameraFollow.enabled = true;
        }
    }

    private void DisableFollow()
    {
        if (topDownCameraFollow != null)
        {
            topDownCameraFollow.enabled = false;
        }
    }

    private TeleportCameraPreset FindPreset(Transform destination)
    {
        if (destination == null)
        {
            return null;
        }

        string destinationKey = NormalizeName(destination.name);
        for (int i = 0; i < presets.Count; i++)
        {
            TeleportCameraPreset preset = presets[i];
            if (preset == null || string.IsNullOrWhiteSpace(preset.destinationName))
            {
                continue;
            }

            if (NormalizeName(preset.destinationName) == destinationKey)
            {
                return preset;
            }
        }

        return null;
    }

    private static string NormalizeName(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
