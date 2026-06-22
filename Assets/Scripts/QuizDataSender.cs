using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class QuizDataSender : MonoBehaviour
{
    [Header("API")]
    public string apiUrl = "https://beaverse.alwaysdata.net/save_quiz.php";

    public void SendDayResults(int day)
    {
        StartCoroutine(PostDayResults(day));
    }

    IEnumerator PostDayResults(int day)
    {
        bool isDebugMode = PlayerPrefs.GetInt("debug_mode", 0) == 1;
        string playerId;

        if (isDebugMode)
        {
            playerId = "DEBUG";
        }
        else
        {
            playerId = PlayerPrefs.GetString("player_id", "");
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("player_id", playerId);
                PlayerPrefs.Save();
            }
        }

        // Lit directement les variables relatives du GameState (remises à 0 chaque jour)
        // Le PHP place ces réponses dans les bonnes colonnes globales selon le jour
        QuizPayload payload = new QuizPayload
        {
            player_id = playerId,
            day       = day,
            score     = GameState.quizScore,
            reponse_1 = GameState.reponse_1,
            reponse_2 = GameState.reponse_2,
            reponse_3 = GameState.reponse_3,
            reponse_4 = GameState.reponse_4,
            reponse_5 = GameState.reponse_5,
            reponse_6 = GameState.reponse_6
        };

        string json = JsonUtility.ToJson(payload);
        Debug.Log("[QuizDataSender] Envoi jour " + day + " : " + json);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("[QuizDataSender] ✅ Jour " + day + " envoyé : " + request.downloadHandler.text);
            else
                Debug.LogWarning("[QuizDataSender] ❌ Erreur : " + request.error);
        }
    }

    [Serializable]
    private class QuizPayload
    {
        public string player_id;
        public int    day;
        public int    score;
        public string reponse_1;
        public string reponse_2;
        public string reponse_3;
        public string reponse_4;
        public string reponse_5;
        public string reponse_6;
    }
}