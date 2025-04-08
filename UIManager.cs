using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글턴 인스턴스

    public TMP_InputField usernameInput, passwordInput, mbtiInput;
    public TMP_Text resultText;

    private AuthManager authManager;
    private CleaningManager cleaningManager;
    private RankingManager rankingManager;

    private void Awake()
    {
        // ✅ 인스턴스 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);  // 중복 방지
    }

    private void Start()
    {
        authManager = FindObjectOfType<AuthManager>();
        cleaningManager = FindObjectOfType<CleaningManager>();
        rankingManager = FindObjectOfType<RankingManager>();
    }

    public void ShowMessage(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();
        string mbti = mbtiInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(mbti))
        {
            resultText.text = "?? 모든 입력 필드를 채워주세요.";
            return;
        }

        StartCoroutine(authManager.Register(username, password, mbti, (response) => resultText.text = response));
    }

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "?? 아이디와 비밀번호를 입력하세요.";
            return;
        }

        StartCoroutine(authManager.Login(username, password, (response) =>
        {
            resultText.text = response;
            if (response.Contains("성공"))
            {
                resultText.text = "? 로그인 성공!";
            }
        }));
    }

    public void OnCleaningConfirmClick()
    {
        if (!UserSession.IsLoggedIn)
        {
            resultText.text = "❌ 로그인 후 이용 가능합니다.";
            return;
        }

        string[] cleanedItems = { "책상", "의자", "컴퓨터" };

        // ✅ userId 포함하여 호출
        StartCoroutine(cleaningManager.ConfirmCleaning(UserSession.userId, cleanedItems, (response) =>
        {
            resultText.text = response;
        }));
    }


    public void OnRankingClick()
    {
        StartCoroutine(rankingManager.GetRanking((response) => resultText.text = response));
    }
}