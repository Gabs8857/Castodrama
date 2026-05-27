using UnityEngine;

public enum GameMode
{
    Free,
    Dialogue,
    Question,
    Result
}

public static class GameState
{
    public static GameMode Mode = GameMode.Free;

    public static ChatManager chatManager;

    // =====================
    // QUIZ DATA
    // =====================
    public static int quizScore = 0;

    public static string firstAnswer = "";
    public static string secondAnswer = "";

    public static string q1Explanation = "";
    public static string q2Explanation = "";

    // =====================
    public static bool CanStartDialogue()
    {
        return Mode == GameMode.Free;
    }

    public static bool CanStartQuestion()
    {
        return Mode == GameMode.Free;
    }

    public static bool IsBlockingUI()
    {
        return Mode == GameMode.Question || Mode == GameMode.Result;
    }

    // =====================
    public static void Set(GameMode mode)
    {
        Mode = mode;
        Debug.Log("[GameState] Mode = " + mode);

        if (chatManager != null)
            chatManager.UpdateVisibility();
    }

    public static void Reset()
    {
        Set(GameMode.Free);
    }
}