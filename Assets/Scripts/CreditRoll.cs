using UnityEngine;
using UnityEngine.UI;

public class CreditRoll : MonoBehaviour
{

    [Header("Références canvas")]
    [SerializeField] CanvasGroup canvaControl;

    [Header("UI (auto-bind possible)")]
    [SerializeField] Button     reelButton;
    [SerializeField] GameObject joystickInput;

    public float scrollSpeed = 70f;
    private RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hide(canvaControl);
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        AutoBindUI();
        joystickInput?.SetActive(false);
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
    }
    
    void AutoBindUI()
    {
        if (!reelButton)
        {
            foreach (var b in Object.FindObjectsOfType<Button>(true))
            {
                if (b.name.ToLower().Contains("reel"))
                {
                    reelButton = b;
                    break;
                }
            }
        }

        if (!joystickInput)
        {
            joystickInput = GameObject.Find("JoystickInput") ??
                            GameObject.Find("Fixed Joystick") ??
                            GameObject.Find("Joystick");
        }
    }

    static void Hide(CanvasGroup cg)
    {
        if (!cg) return;

        cg.alpha = 0f;
        cg.blocksRaycasts = cg.interactable = false;
        cg.gameObject.SetActive(false);
    }

}
