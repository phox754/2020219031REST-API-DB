using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;

public class ServerManager : MonoBehaviour
{
    protected const string BASE_URL = "http://localhost:3000";  // 서버 주소

    protected IEnumerator SendRequest(string endpoint, string method, string json, Action<string> callback, bool useAuth = false)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(BASE_URL + endpoint, method))
        {
            if (!string.IsNullOrEmpty(json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            if (useAuth && !string.IsNullOrEmpty(UserSession.authToken))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + UserSession.authToken);
            }

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                callback(webRequest.downloadHandler.text);
            }
            else
            {
                callback("요청 실패: " + webRequest.error);
            }
        }
    }
}
