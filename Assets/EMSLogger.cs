using UnityEngine;
using System; // DateTime.UtcNow를 사용하기 위해 필요
using System.IO;
using System.Text;

/// <summary>
/// [싱글톤 버전] EMS 데이터를 CSV 파일에 기록합니다. (유닉스 타임스탬프 사용)
/// </summary>
public class EMSLogger : MonoBehaviour
{
    public static EMSLogger Instance { get; private set; }

    private StreamWriter csvWriter;
    private float trialStartTime = -1f; // 이 변수는 이제 타임스탬프 계산에 사용되지 않습니다.

    // 현재 테스트(Trial)의 컨텍스트(맥락) 정보
    private string currentTrialID = "N/A";
    private string currentPattern = "N/A";
    private string currentStimMode = "N/A";

    // 유닉스 Epoch 시간 (1970-01-01 UTC)
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
    }

    void Start()
    {
        // 파일 저장 경로 (예: 내 문서)
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string logPath = Path.Combine(folderPath, $"EMS_Log_{timestamp}.csv");

        try
        {
            // CSV 파일 열고 헤더 작성
            csvWriter = new StreamWriter(logPath, false, Encoding.UTF8);
            string header = "UnixTimestamp(s),TrialID,Pattern,StimMode,EventType,TargetChannel,TargetIntensity,TargetDuration(ms)";
            csvWriter.WriteLine(header);
            csvWriter.Flush();
            Debug.Log($"[EMSLogger] 로그 파일 생성 완료: {logPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EMSLogger] 파일 생성 실패: {e.Message}");
        }
    }

    public void StartNewTrial(string pattern, string stimMode)
    {
        this.trialStartTime = 0f; // -1f가 아님을 표시
        this.currentPattern = pattern;
        this.currentStimMode = stimMode;
        this.currentTrialID = $"{stimMode}_{pattern}_{DateTime.Now:HHmmss}";

        LogEvent("TRIAL_START");
    }

    /// <summary>
    /// PilotTestController가 테스트 종료 시 호출
    /// </summary>
    public void StopTrial()
    {
        if (trialStartTime < 0) return;
        LogEvent("TRIAL_END");
        trialStartTime = -1f; // 리셋
    }

    /// <summary>
    /// SendCommandToArduino에서 직접 호출
    /// </summary>
    public void LogEmsCommand(int channelID, int intensity, float durationMs)
    {
        if (trialStartTime < 0) return;
        LogEvent("EMS_SENT", channelID, intensity, durationMs);
    }


    private void LogEvent(string eventType, int channelID = 0, int intensity = 0, float durationMs = 0)
    {
        if (csvWriter == null) return;

        double unixTimestamp = (DateTime.UtcNow - UnixEpoch).TotalSeconds;

        string line = $"{unixTimestamp:F3},{currentTrialID},{currentPattern},{currentStimMode},{eventType},{channelID},{intensity},{(eventType == "EMS_SENT" ? durationMs.ToString("F0") : "")}";

        csvWriter.WriteLine(line);
        csvWriter.Flush();
    }

    void OnApplicationQuit()
    {
        csvWriter?.Close();
        Debug.Log("[EMSLogger] 자극 로그 저장됨.");
    }
}