using UnityEngine;
using System;
using System.IO;
using System.Text;

/// <summary>
/// Calibration 단계에서 Test 버튼 클릭 시 EMS calibration 로그를 별도 CSV로 저장.
/// 저장 컬럼:
/// UnixTimestamp(s), Channel, EMSValue, CalibType
/// CalibType: "min_perception" | "min_actuation" | "max_actuation"
/// </summary>
public class EMSCalibrationLogger : MonoBehaviour
{
    public static EMSCalibrationLogger Instance { get; private set; }

    private StreamWriter csvWriter;

    private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private string logPath;

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
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        logPath = Path.Combine(folderPath, $"EMS_Calibration_Log_{timestamp}.csv");

        try
        {
            csvWriter = new StreamWriter(logPath, false, Encoding.UTF8);
            csvWriter.WriteLine("UnixTimestamp(s),Channel,EMSValue,CalibType");
            csvWriter.Flush();
            Debug.Log($"[EMSCalibrationLogger] 로그 파일 생성 완료: {logPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EMSCalibrationLogger] 파일 생성 실패: {e.Message}");
        }
    }

    /// <summary>
    /// Calibration Test 버튼 눌렀을 때 호출
    /// </summary>
    public void LogCalibrationTest(int channel, int emsValue, string calibType)
    {
        if (csvWriter == null) return;

        double unixTimestamp = (DateTime.UtcNow - UnixEpoch).TotalSeconds;

        // CSV injection/쉼표 방지용 기본 sanitize
        calibType = (calibType ?? "N/A").Replace(",", "_").Trim();

        string line = $"{unixTimestamp:F3},{channel},{emsValue},{calibType}";
        csvWriter.WriteLine(line);
        csvWriter.Flush();
    }

    void OnApplicationQuit()
    {
        csvWriter?.Close();
        Debug.Log("[EMSCalibrationLogger] calibration 로그 저장됨.");
    }
}
