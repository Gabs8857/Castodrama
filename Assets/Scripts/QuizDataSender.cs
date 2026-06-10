using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class QuizDataSender : MonoBehaviour
{
    [Header("API")]
    public string apiUrl = "https://beaverse.alwaysdata.net/save_quiz.php";

    public void SendResults()
    {
        StartCoroutine(PostResults());
    }

   IEnumerator PostResults()
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

    string json = JsonUtility.ToJson(new QuizPayload
    {
        player_id = playerId,
        score = GameState.quizScore,
        reponse_1 = GameState.reponse_1,
        reponse_2 = GameState.reponse_2,
        reponse_3 = GameState.reponse_3,
        reponse_4 = GameState.reponse_4,
        reponse_5 = GameState.reponse_5
    });

    Debug.Log("[QuizDataSender] Envoi : " + json);

    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

    using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
    {
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("[QuizDataSender] ✅ Résultat envoyé : " + request.downloadHandler.text);
        else
            Debug.LogWarning("[QuizDataSender] ❌ Erreur : " + request.error);
    }
}

    [Serializable]
    private class QuizPayload
    {
        public string player_id;
        public int    score;
        public string reponse_1;
        public string reponse_2;
        public string reponse_3;
        public string reponse_4;
        public string reponse_5;

    }
}