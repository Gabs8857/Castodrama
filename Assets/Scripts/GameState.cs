using UnityEngine;

public enum GameMode { Free, Dialogue, Question, Result }

public static class GameState
{
    public static GameMode Mode = GameMode.Free;

    public static ChatManager chatManager;
    public static StatsUIManager statsUIManager;
    public static DayManager dayManager;

    // =====================
    // JOUR ACTUEL
    // =====================
    public static int currentDay = 1; // 1, 2 ou 3
    public static bool isInHut = false; // true quand le joueur est dans la hutte
    public static bool hasSeenBilan = false; // a parlé au NPC ce jour

    // =====================
    // QUIZ DATA — Jour 1 (5 questions)
    // =====================
    public static int quizScore_day1 = 0;
    public static string q1d1 = ""; public static string r1d1 = ""; public static string e1d1 = "";
    public static string q2d1 = ""; public static string r2d1 = ""; public static string e2d1 = "";
    public static string q3d1 = ""; public static string r3d1 = ""; public static string e3d1 = "";
    public static string q4d1 = ""; public static string r4d1 = ""; public static string e4d1 = "";
    public static string q5d1 = ""; public static string r5d1 = ""; public static string e5d1 = "";

    // =====================
    // QUIZ DATA — Jour 2 (6 questions)
    // =====================
    public static int quizScore_day2 = 0;
    public static string q1d2 = ""; public static string r1d2 = ""; public static string e1d2 = "";
    public static string q2d2 = ""; public static string r2d2 = ""; public static string e2d2 = "";
    public static string q3d2 = ""; public static string r3d2 = ""; public static string e3d2 = "";
    public static string q4d2 = ""; public static string r4d2 = ""; public static string e4d2 = "";
    public static string q5d2 = ""; public static string r5d2 = ""; public static string e5d2 = "";
    public static string q6d2 = ""; public static string r6d2 = ""; public static string e6d2 = "";

    // =====================
    // QUIZ DATA — Jour 3 (6 questions)
    // =====================
    public static int quizScore_day3 = 0;
    public static string q1d3 = ""; public static string r1d3 = ""; public static string e1d3 = "";
    public static string q2d3 = ""; public static string r2d3 = ""; public static string e2d3 = "";
    public static string q3d3 = ""; public static string r3d3 = ""; public static string e3d3 = "";
    public static string q4d3 = ""; public static string r4d3 = ""; public static string e4d3 = "";
    public static string q5d3 = ""; public static string r5d3 = ""; public static string e5d3 = "";
    public static string q6d3 = ""; public static string r6d3 = ""; public static string e6d3 = "";

    // =====================
    // COMPAT — variables génériques utilisées par DialogueManager/StreamQuestionUI
    // =====================
    public static int quizScore = 0;
    public static string question_1 = ""; public static string reponse_1 = ""; public static string explication_q1 = "";
    public static string question_2 = ""; public static string reponse_2 = ""; public static string explication_q2 = "";
    public static string question_3 = ""; public static string reponse_3 = ""; public static string explication_q3 = "";
    public static string question_4 = ""; public static string reponse_4 = ""; public static string explication_q4 = "";
    public static string question_5 = ""; public static string reponse_5 = ""; public static string explication_q5 = "";
    public static string question_6 = ""; public static string reponse_6 = ""; public static string explication_q6 = "";

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

    // Sauvegarde les variables génériques dans les variables du bon jour
    public static void SaveDayResults()
    {
        switch (currentDay)
        {
            case 1:
                quizScore_day1 = quizScore;
                q1d1 = question_1; r1d1 = reponse_1; e1d1 = explication_q1;
                q2d1 = question_2; r2d1 = reponse_2; e2d1 = explication_q2;
                q3d1 = question_3; r3d1 = reponse_3; e3d1 = explication_q3;
                q4d1 = question_4; r4d1 = reponse_4; e4d1 = explication_q4;
                q5d1 = question_5; r5d1 = reponse_5; e5d1 = explication_q5;
                break;
            case 2:
                quizScore_day2 = quizScore;
                q1d2 = question_1; r1d2 = reponse_1; e1d2 = explication_q1;
                q2d2 = question_2; r2d2 = reponse_2; e2d2 = explication_q2;
                q3d2 = question_3; r3d2 = reponse_3; e3d2 = explication_q3;
                q4d2 = question_4; r4d2 = reponse_4; e4d2 = explication_q4;
                q5d2 = question_5; r5d2 = reponse_5; e5d2 = explication_q5;
                q6d2 = question_6; r6d2 = reponse_6; e6d2 = explication_q6;
                break;
            case 3:
                quizScore_day3 = quizScore;
                q1d3 = question_1; r1d3 = reponse_1; e1d3 = explication_q1;
                q2d3 = question_2; r2d3 = reponse_2; e2d3 = explication_q2;
                q3d3 = question_3; r3d3 = reponse_3; e3d3 = explication_q3;
                q4d3 = question_4; r4d3 = reponse_4; e4d3 = explication_q4;
                q5d3 = question_5; r5d3 = reponse_5; e5d3 = explication_q5;
                q6d3 = question_6; r6d3 = reponse_6; e6d3 = explication_q6;
                break;
        }
    }

    // Remet les variables génériques à zéro pour le prochain jour
    public static void ResetDayVars()
    {
        quizScore = 0;
        question_1 = ""; reponse_1 = ""; explication_q1 = "";
        question_2 = ""; reponse_2 = ""; explication_q2 = "";
        question_3 = ""; reponse_3 = ""; explication_q3 = "";
        question_4 = ""; reponse_4 = ""; explication_q4 = "";
        question_5 = ""; reponse_5 = ""; explication_q5 = "";
        question_6 = ""; reponse_6 = ""; explication_q6 = "";
        hasSeenBilan = false;
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