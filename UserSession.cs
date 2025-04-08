using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserSession
{
    public static int userId;
    public static string username;
    public static string authToken;

    public static bool IsLoggedIn => !string.IsNullOrEmpty(authToken); // ✅ 추가
}
