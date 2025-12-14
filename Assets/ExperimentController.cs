using UnityEngine;
using System;
using Melanchall.DryWetMidi.Core;        // NoteOnEvent, NoteOffEvent
using Melanchall.DryWetMidi.Common;     // SevenBitNumber
using Melanchall.DryWetMidi.Multimedia; // IOutputDevice, OutputDevice

public class ExperimentController : MonoBehaviour
{
    [Header("EMS Logger Object")]
    public GameObject emsControllerObject;

    private EMSLogger emsLogger;
    private IOutputDevice _outputDevice;

    // Ableton Live에 보낼 MIDI 마커 노트 번호
    private const byte MarkerNoteNumber = 48;

    [Header("Trial Metadata (set these before Start)")]
    public string pattern = "DefaultPattern"; // 예: "Swing_2_1"
    public string task = "SF";                // "SF" or "MF"
    public string guideType = "OP";           // "OP" or "PR"
    public string location = "OF";            // "OF" or "FF"

    private static readonly DateTime UnixEpoch =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    void Start()
    {
        // -------------------------------
        // [1] EMSLogger 가져오기
        // -------------------------------
        if (emsControllerObject == null)
        {
            Debug.LogError("emsControllerObject가 Inspector에 연결되지 않았습니다!");
            return;
        }

        emsLogger = emsControllerObject.GetComponent<EMSLogger>();

        if (emsLogger == null)
        {
            Debug.LogError("emsControllerObject에서 EMSLogger 컴포넌트를 찾을 수 없습니다!");
        }

        // -------------------------------
        // [2] LoopBe1 가상 MIDI 포트 연결
        // -------------------------------
        try
        {
            _outputDevice = OutputDevice.GetByName("LoopBe Internal MIDI");
            Debug.Log(">>> LoopBe1 (LoopBe Internal MIDI) 연결 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError("!!! LoopBe1 가상 MIDI 포트를 찾을 수 없습니다!");
            Debug.LogError("!!! LoopBe1 설치 여부 및 이미 다른 프로그램이 점유 중인지 확인하세요.");
            Debug.LogError(ex.Message);
        }
    }


    // ===============================================================
    //  [UI 버튼] 실험 Start 버튼을 누르면 호출될 함수
    // ===============================================================
    public void OnStartExperimentButtonPress()
    {
        Debug.Log("========================================");
        Debug.Log(">>> 실험 시작 버튼 클릭됨");
        Debug.Log("========================================");

        // -------------------------------
        // (A) Global Unix Time 기록
        // -------------------------------
        double globalStartTimeUnix =
            (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        // EMSLogger에도 기록 (원하면 EMSLogger에 별도 메서드 추가 가능)
        Debug.Log($"[ExperimentController] Global Unix Time = {globalStartTimeUnix:F6}");

        // -------------------------------
        // (B) EMS Trial 시작
        // -------------------------------
        if (emsLogger != null)
        {
            emsLogger.StartNewTrial(
                pattern: pattern,
                task: task,
                guideType: guideType,
                location: location
            );

            Debug.Log($">>> EMSLogger StartNewTrial() 호출 완료: {task}_{guideType}{location}_{pattern}");
        }
        else
        {
            Debug.LogError("EMSLogger가 null이라 Trial 시작 불가!");
        }

        // -------------------------------
        // (C) MIDI 마커 전송 (Ableton)
        // -------------------------------
        if (_outputDevice != null)
        {
            try
            {
                _outputDevice.SendEvent(
                    new NoteOnEvent(
                        (SevenBitNumber)MarkerNoteNumber,
                        (SevenBitNumber)100
                    )
                );

                _outputDevice.SendEvent(
                    new NoteOffEvent(
                        (SevenBitNumber)MarkerNoteNumber,
                        (SevenBitNumber)0
                    )
                );

                Debug.Log(">>> Ableton Live에 MIDI 마커(C2) 전송 성공!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"!!! MIDI 마커 전송 실패: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("!!! MIDI 장치가 연결되지 않아 마커를 전송하지 못했습니다.");
        }

        Debug.Log("========================================");
    }


    // ===============================================================
    // 종료 시 MIDI 장치 연결 종료
    // ===============================================================
    private void OnApplicationQuit()
    {
        _outputDevice?.Dispose();
        Debug.Log("LoopBe1 MIDI 장치 Dispose 완료");
    }
}
