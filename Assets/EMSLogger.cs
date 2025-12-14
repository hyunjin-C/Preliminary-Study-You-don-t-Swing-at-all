using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// [Singleton] EMS 로그를 Unix time 기준 CSV로 기록
/// - 모드(Task+GuideType+Location)별 버튼 클릭 index 관리
/// </summary>
public class EMSLogger : MonoBehaviour
{
    public static EMSLogger Instance { get; private set; }

    private StreamWriter csvWriter;
    private float trialStartTime = -1f;

    // Trial context
    private string currentTrialID = "N/A";
    private string currentPattern = "N/A";
    private string currentTask = "N/A";       // SF / MF
    private string currentGuideType = "N/A";  // OP / PR
    private string currentLocation = "N/A";   // OF / FF

    // ===== 핵심 =====
    // 모드별 버튼 클릭 횟수 관리
    private Dictionary<string, int> modePressCount = new Dictionary<string, int>();
    private int currentButtonIndex = -1;

    private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        string folderPath = @"D:\hyunjin\Research\You don't Swing at all\[Preliminary Study] Experiement";
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string logPath = Path.Combine(folderPath, $"EMS_Log_{timestamp}.csv");

        try
        {
            csvWriter = new StreamWriter(logPath, false, Encoding.UTF8);

            string header =
                "UnixTimestamp(s),TrialID,Pattern,Task,GuideType,Location,EventType,ButtonIndex,TargetChannel,TargetIntensity,TargetDuration(ms)";

            csvWriter.WriteLine(header);
            csvWriter.Flush();

            Debug.Log($"[EMSLogger] 로그 파일 생성 완료: {logPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EMSLogger] 파일 생성 실패: {e.Message}");
        }
    }

    // =========================
    // Trial 시작
    // =========================
    public void StartNewTrial(
        string pattern,
        string task,        // SF / MF
        string guideType,   // OP / PR
        string location     // OF / FF
    )
    {
        trialStartTime = 0f;

        currentPattern = pattern;
        currentTask = task;
        currentGuideType = guideType;
        currentLocation = location;

        // 여기서 modeKey 기준 버튼 인덱스 증가
        string modeKey = $"{task}_{guideType}_{location}";
        if (!modePressCount.ContainsKey(modeKey))
            modePressCount[modeKey] = 0;

        modePressCount[modeKey] += 1;
        currentButtonIndex = modePressCount[modeKey];

        currentTrialID =
            $"{task}_{guideType}{location}_{pattern}_{DateTime.Now:HHmmss}";

        LogEvent("TRIAL_START");
    }

    public void StopTrial()
    {
        if (trialStartTime < 0) return;

        LogEvent("TRIAL_END");
        trialStartTime = -1f;
    }


    // =========================
    // EMS 명령 기록
    // =========================
    public void LogEmsCommand(int channelID, int intensity, float durationMs)
    {
        if (trialStartTime < 0) return;

        LogEvent("EMS_SENT", channelID, intensity, durationMs);
    }

    private void LogEvent(
        string eventType,
        int channelID = 0,
        int intensity = 0,
        float durationMs = 0
    )
    {
        if (csvWriter == null) return;

        double unixTimestamp =
            (DateTime.UtcNow - UnixEpoch).TotalSeconds;

        string line =
            $"{unixTimestamp:F3}," +
            $"{currentTrialID}," +
            $"{currentPattern}," +
            $"{currentTask}," +
            $"{currentGuideType}," +
            $"{currentLocation}," +
            $"{eventType}," +
            $"{currentButtonIndex}," +
            $"{channelID}," +
            $"{intensity}," +
            $"{(eventType == "EMS_SENT" ? durationMs.ToString("F0") : "")}";

        csvWriter.WriteLine(line);
        csvWriter.Flush();
    }

    void OnApplicationQuit()
    {
        csvWriter?.Close();
        Debug.Log("[EMSLogger] EMS 로그 저장됨.");
    }
}
