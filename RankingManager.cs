using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class RankingData
{
    public string username;
    public int total_score;
    public int user_rank;
}

[System.Serializable]
public class RankingResponse
{
    public bool success;
    public RankingData[] rankings;
}

public class RankingManager : ServerManager
{
    public IEnumerator GetRanking(Action<string> callback)
    {
        yield return StartCoroutine(SendRequest("/ranking", "GET", null, (response) =>
        {
            if (string.IsNullOrEmpty(response))
            {
                callback("서버 응답 없음.");
                return;
            }

            RankingResponse rankingResponse = JsonUtility.FromJson<RankingResponse>(response);
            if (rankingResponse == null || !rankingResponse.success)
            {
                callback("랭킹 데이터를 불러오는 데 실패했습니다.");
                return;
            }

            if (rankingResponse.rankings == null || rankingResponse.rankings.Length == 0)
            {
                callback("랭킹 데이터가 없습니다.");
                return;
            }

            string rankingText = "랭킹 정보:\n";
            foreach (var rank in rankingResponse.rankings)
            {
                rankingText += $"{rank.user_rank}위 - {rank.username} ({rank.total_score}점)\n";
            }
            callback(rankingText);
        }));
    }
}
