using UnityEngine;
using UnityEngine.UI;

public class QuitGame : MonoBehaviour
{
    public Button quitButton;

    void Start()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(() => Quit());
        else
            Debug.LogWarning("Quit button is not assigned in the inspector.");
    }

    private void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
