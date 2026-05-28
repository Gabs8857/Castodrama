using UnityEngine;
using UnityEngine.U2D.Animation;

public class LeakAnimator : MonoBehaviour
{
    [SerializeField] private float frameSwitchSpeed = 0.15f;
    [SerializeField] private string leakCategoryName = "Leak";
    [SerializeField] private string[] leakFrameNames = { "Frame1", "Frame2", "Frame3" };
    [SerializeField] private bool debugLogs = true;

    private SpriteResolver spriteResolver;
    private float timeSinceLastSwitch = 0f;
    private int currentFrameIndex = 0;
    private bool isLeaking = false;

    private void Start()
    {
        spriteResolver = GetComponent<SpriteResolver>();
        if (spriteResolver == null)
        {
            Debug.LogError($"[LeakAnimator] {gameObject.name} - No SpriteResolver found!");
            enabled = false;
            return;
        }
        if (debugLogs)
            Debug.Log($"[LeakAnimator] {gameObject.name} - Initialized");
        UpdateFrame(0);
    }

    private void Update()
    {
        if (!isLeaking) return;

        timeSinceLastSwitch += Time.deltaTime;
        if (timeSinceLastSwitch >= frameSwitchSpeed)
        {
            timeSinceLastSwitch = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % leakFrameNames.Length;
            UpdateFrame(currentFrameIndex);
            if (debugLogs)
                Debug.Log($"[LeakAnimator] {gameObject.name} - Frame {currentFrameIndex + 1}/{leakFrameNames.Length}");
        }
    }

    public void StartLeaking()
    {
        isLeaking = true;
        currentFrameIndex = 0;
        timeSinceLastSwitch = 0f;
        UpdateFrame(0);
        if (debugLogs)
            Debug.Log($"[LeakAnimator] {gameObject.name} - StartLeaking()");
    }

    public void StopLeaking()
    {
        isLeaking = false;
        currentFrameIndex = 0;
        UpdateFrame(0);
        if (debugLogs)
            Debug.Log($"[LeakAnimator] {gameObject.name} - StopLeaking()");
    }

    private void UpdateFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= leakFrameNames.Length) return;
        spriteResolver.SetCategoryAndLabel(leakCategoryName, leakFrameNames[frameIndex]);
        if (debugLogs)
            Debug.Log($"[LeakAnimator] {gameObject.name} - UpdateFrame({frameIndex}) - {leakCategoryName}/{leakFrameNames[frameIndex]}");
    }

    public void SetLeaking(bool leaking)
    {
        if (leaking) StartLeaking();
        else StopLeaking();
    }
}

