using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class QuizDataSender : MonoBehaviour
{
    // ⭐ À MODIFIER ICI
    private const int NB_REPONSES = 17;

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

        // Crée le payload dynamiquement
        var payload = new QuizPayload
        {
            player_id = playerId,
            score = GameState.quizScore
        };

        // Ajoute les réponses dynamiquement via reflexion
        var payloadType = payload.GetType();
        for (int i = 1; i <= NB_REPONSES; i++)
        {
            string fieldName = "reponse_" + i;
            string gameStateFieldName = "reponse_" + i;

            // Récupère la valeur de GameState
            var gsField = typeof(GameState).GetField(gameStateFieldName, 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            if (gsField != null)
            {
                string value = (string)gsField.GetValue(null) ?? "";
                var payloadField = payloadType.GetField(fieldName);
                if (payloadField != null)
                {
                    payloadField.SetValue(payload, value);
                }
            }
        }

        string json = JsonUtility.ToJson(payload);
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

    [System.Serializable]
    private class QuizPayload
    {
        public string player_id;
        public int score;
        
        // ⭐ Les 17 réponses
        public string reponse_1 = "";
        public string reponse_2 = "";
        public string reponse_3 = "";
        public string reponse_4 = "";
        public string reponse_5 = "";
        public string reponse_6 = "";
        public string reponse_7 = "";
        public string reponse_8 = "";
        public string reponse_9 = "";
        public string reponse_10 = "";
        public string reponse_11 = "";
        public string reponse_12 = "";
        public string reponse_13 = "";
        public string reponse_14 = "";
        public string reponse_15 = "";
        public string reponse_16 = "";
        public string reponse_17 = "";
    }
}