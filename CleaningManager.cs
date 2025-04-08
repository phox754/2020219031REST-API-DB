using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class CleaningRequest
{
    public int userId;
    public string[] cleanedItems;
}

[System.Serializable]
public class CleaningResponse
{
    public bool success;
    public string message;
    public int pointsEarned;
}

public class CleaningManager : ServerManager
{
    public IEnumerator ConfirmCleaning(int userId, string[] cleanedItems, Action<string> callback)
    {
        if (userId <= 0 || cleanedItems == null || cleanedItems.Length == 0)
        {
            callback?.Invoke("❌ 유효하지 않은 데이터입니다.");
            yield break;
        }

        CleaningRequest request = new CleaningRequest { userId = userId, cleanedItems = cleanedItems };
        string json = JsonUtility.ToJson(request);

        yield return StartCoroutine(SendRequest("/confirm-cleaning", "POST", json, (response) =>
        {
            CleaningResponse cleaningResponse = JsonUtility.FromJson<CleaningResponse>(response);
            if (cleaningResponse.success)
            {
                callback?.Invoke($"✅ {UserSession.username}님이 청소 완료! 획득한 포인트: {cleaningResponse.pointsEarned}");
            }
            else
            {
                callback?.Invoke($"⚠️ 청소 확인 실패: {cleaningResponse.message}");
            }
        }, useAuth: true));
    }
}