using UnityEngine;

public enum GameMode { Free, Dialogue, Question, Result }

public static class GameState
{
    public static GameMode Mode = GameMode.Free;

    public static ChatManager chatManager;
    public static StatsUIManager statsUIManager;
    public static DayManager dayManager;
    public static GrassSpawner grassSpawner;

    // =====================
    // JOUR ACTUEL
    // =====================
    public static int currentDay = 1;
    public static bool isInHut = false;
    public static bool hasSeenBilan = false;

    // =====================
    // SCORE
    // =====================
    public static int quizScore = 0;

    public static int quizScore_day1 = 0;
    public static int quizScore_day2 = 0;
    public static int quizScore_day3 = 0;

    // =====================
    // QUIZ DATA
    // Jour 1 : question_1  a question_5
    // Jour 2 : question_6  a question_11
    // Jour 3 : question_12 a question_17
    // =====================

    // Jour 1
    public static string question_1  = ""; public static string reponse_1  = ""; public static string explication_q1  = "";
    public static string question_2  = ""; public static string reponse_2  = ""; public static string explication_q2  = "";
    public static string question_3  = ""; public static string reponse_3  = ""; public static string explication_q3  = "";
    public static string question_4  = ""; public static string reponse_4  = ""; public static string explication_q4  = "";
    public static string question_5  = ""; public static string reponse_5  = ""; public static string explication_q5  = "";

    // Jour 2
    public static string question_6  = ""; public static string reponse_6  = ""; public static string explication_q6  = "";
    public static string question_7  = ""; public static string reponse_7  = ""; public static string explication_q7  = "";
    public static string question_8  = ""; public static string reponse_8  = ""; public static string explication_q8  = "";
    public static string question_9  = ""; public static string reponse_9  = ""; public static string explication_q9  = "";
    public static string question_10 = ""; public static string reponse_10 = ""; public static string explication_q10 = "";
    public static string question_11 = ""; public static string reponse_11 = ""; public static string explication_q11 = "";

    // Jour 3
    public static string question_12 = ""; public static string reponse_12 = ""; public static string explication_q12 = "";
    public static string question_13 = ""; public static string reponse_13 = ""; public static string explication_q13 = "";
    public static string question_14 = ""; public static string reponse_14 = ""; public static string explication_q14 = "";
    public static string question_15 = ""; public static string reponse_15 = ""; public static string explication_q15 = "";
    public static string question_16 = ""; public static string reponse_16 = ""; public static string explication_q16 = "";
    public static string question_17 = ""; public static string reponse_17 = ""; public static string explication_q17 = "";

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

    public static void SaveDayResults()
    {
        switch (currentDay)
        {
            case 1: quizScore_day1 = quizScore; break;
            case 2: quizScore_day2 = quizScore; break;
            case 3: quizScore_day3 = quizScore; break;
        }
    }

    public static void ResetDayVars()
    {
        quizScore = 0;
        hasSeenBilan = false;

        // Remet les réponses relatives à zéro pour le prochain jour
        reponse_1 = ""; reponse_2 = ""; reponse_3 = "";
        reponse_4 = ""; reponse_5 = ""; reponse_6 = "";
        question_1 = ""; question_2 = ""; question_3 = "";
        question_4 = ""; question_5 = ""; question_6 = "";
        explication_q1 = ""; explication_q2 = ""; explication_q3 = "";
        explication_q4 = ""; explication_q5 = ""; explication_q6 = "";
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