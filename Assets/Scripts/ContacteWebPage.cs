using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
public class ContacteWebPage : MonoBehaviour
{
    [SerializeField] 
    private string url = "https:gabrielmuller.dev/ContacteWebPage.html";
    [SerializeField]
    private string urlImage = "https:gabrielmuller.dev/ContacteWebPage.png";

    void Start()
    {
        StartCoroutine(GetRequest(url));
        StartCoroutine(GetRequest(urlImage));
        StartCoroutine(GetRequest("https://error.html"));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(uri + ": Network Error: " + webRequest.error);
                yield break;
            }
            else
            {
                Debug.Log(":\nReceived:" +webRequest.downloadHandler.text);
            }
        }
    }

}
