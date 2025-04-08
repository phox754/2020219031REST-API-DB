using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SurveyResponseSender : MonoBehaviour
{
    [Header("서버 주소 설정")]
    public string serverUrl = "http://localhost:3000/api/survey/submit";  // 실제 서버 주소로 바꿔주세요

    /// <summary>
    /// 버튼에서 호출할 메인 함수
    /// </summary>
    public void SendSurveyResponse(string questionKey, string answer)
    {
        // 현재 로그인 상태 확인
        if (UserSession.userId == 0)
        {
            Debug.LogWarning("로그인 먼저 해주세요.");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("로그인 후 이용해주세요!");
            }
            return;
        }

        StartCoroutine(SendSurveyResponseToServer(questionKey, answer));
    }

    IEnumerator SendSurveyResponseToServer(string questionKey, string answer)
    {
        string userId = UserSession.userId.ToString();  // 최신 userId 가져오기

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("question_key", questionKey);
        form.AddField("answer", answer);

        Debug.Log($"[설문 응답 전송] user_id={userId}, question_key={questionKey}, answer={answer}");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("응답 전송 실패: " + www.error);
            }
            else
            {
                Debug.Log("응답 저장 완료!");
            }
        }
    }

    // ✅ Unity 버튼에서 쓸 수 있도록 파라미터 없는 함수로 래핑
    public void SendQ1Yes() => SendSurveyResponse("btn_q1", "예");
    public void SendQ1No() => SendSurveyResponse("btn_q1", "아니오");
    public void SendQ2Yes() => SendSurveyResponse("btn_q2", "예");
    public void SendQ2No() => SendSurveyResponse("btn_q2", "아니오");
}
