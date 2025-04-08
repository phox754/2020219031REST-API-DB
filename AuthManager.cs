//using System.Collections;
//using UnityEngine;
//using System;

//[System.Serializable]
//public class RegisterRequest
//{
//    public string username;
//    public string password;
//    public string mbti;
//}

//[System.Serializable]
//public class LoginRequest
//{
//    public string username;
//    public string password;
//}

//[System.Serializable]
//public class ApiResponse
//{
//    public bool success;
//    public string message;
//    public string token;
//}

//public class AuthManager : ServerManager
//{
//    public IEnumerator Register(string username, string password, string mbti, Action<string> callback)
//    {
//        RegisterRequest request = new RegisterRequest { username = username, password = password, mbti = mbti };
//        string json = JsonUtility.ToJson(request);
//        yield return StartCoroutine(SendRequest("/register", "POST", json, callback));
//    }

//    public IEnumerator Login(string username, string password, Action<string> callback)
//    {
//        LoginRequest request = new LoginRequest { username = username, password = password };
//        string json = JsonUtility.ToJson(request);
//        yield return StartCoroutine(SendRequest("/login", "POST", json, (response) =>
//        {
//            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);
//            if (apiResponse.success)
//            {
//                authToken = apiResponse.token;  // ✅ 수정: static authToken 저장
//            }
//            callback(response);
//        }));
//    }
//}

using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class RegisterRequest
{
    public string username;
    public string password;
    public string mbti;
}

[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
    public string token;
    public int userId; // ✅ 서버에서 함께 내려주는 userId
}

public class AuthManager : ServerManager
{
    public IEnumerator Register(string username, string password, string mbti, Action<string> callback)
    {
        RegisterRequest request = new RegisterRequest
        {
            username = username,
            password = password,
            mbti = mbti
        };

        string json = JsonUtility.ToJson(request);
        yield return StartCoroutine(SendRequest("/register", "POST", json, callback));
    }

    public IEnumerator Login(string username, string password, Action<string> callback)
    {
        LoginRequest request = new LoginRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(request);
        yield return StartCoroutine(SendRequest("/login", "POST", json, (response) =>
        {
            ApiResponse apiResponse = JsonUtility.FromJson<ApiResponse>(response);

            if (apiResponse.success)
            {
                // ✅ 로그인 성공 시 세션 정보 저장
                UserSession.userId = apiResponse.userId;
                UserSession.username = username;
                UserSession.authToken = apiResponse.token;

                Debug.Log($"[로그인 성공] userId: {UserSession.userId}, username: {username}");
            }
            else
            {
                Debug.LogWarning($"[로그인 실패] {apiResponse.message}");
            }

            callback(response);
        }));
    }
}
