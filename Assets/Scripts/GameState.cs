using UnityEngine;

public enum GameMode { Free, Dialogue, Question, Result }

public static class GameState
{
    public static GameMode Mode = GameMode.Free;

    public static ChatManager chatManager;
    public static StatsUIManager statsUIManager;

    // =====================
    // QUIZ DATA
    // =====================
    public static int quizScore = 0;

    public static string question_1 = "";
    public static string reponse_1  = "";
    public static string explication_q1 = "";

    public static string question_2 = "";
    public static string reponse_2  = "";
    public static string explication_q2 = "";

    public static string question_3 = "";
    public static string reponse_3  = "";
    public static string explication_q3 = "";

    public static string question_4 = "";
    public static string reponse_4  = "";
    public static string explication_q4 = "";

    public static string question_5 = "";
    public static string reponse_5  = "";
    public static string explication_q5 = "";
    // =====================
    // STATS STREAM
    // =====================
    public static int viewers = 200;
    public static int signatures = 0;
    public static int signaturesGoal = 1000;

    public static void AddViewers(int amount)
    {
        viewers += amount;
        if (statsUIManager != null) statsUIManager.UpdateUI();
    }

    public static void AddSignatures(int amount)
    {
        signatures += amount;
        if (signatures < 0) signatures = 0;
        if (statsUIManager != null) statsUIManager.UpdateUI();
    }

    public static bool CanStartDialogue() => Mode == GameMode.Free;
    public static bool CanStartQuestion() => Mode == GameMode.Free;
    public static bool IsBlockingUI()     => Mode == GameMode.Question || Mode == GameMode.Result;

    public static void Set(GameMode mode)
    {
        Mode = mode;
        Debug.Log("[GameState] Mode = " + mode);
        if (chatManager != null) chatManager.UpdateVisibility();
    }

    public static void Reset() => Set(GameMode.Free);
}