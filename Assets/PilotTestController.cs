using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro Namespace
using System.Net.Sockets; // TCP/IP Communication
using System.IO;         // Data Stream
using System.Collections; // Coroutines

public class PilotTestController : MonoBehaviour
{
    [Header("Network Settings")]
    public string arduinoIpAddress = "192.168.7.200";
    public int arduinoPort = 80;

    // --- [!! 수정됨 1.1: 5채널 + 개별 Save/Test/Adjust UI !!] ---
    [Header("UI Elements - Calibration Control")]
    public GameObject calibrationPanel;
    public TMP_Text calibrationInstructionText; // 상단 지시문
    public Button passButton;                 // 테스트 패스 버튼
    public Button resetButton;                // EMS 즉시 정지 버튼

    [Header("UI Elements - Channel 1")]
    public TMP_Text currentIntensityText_Ch1; // Ch1 현재 조절값
    public Button increaseIntensityButton_Ch1; // Ch1 +
    public Button decreaseIntensityButton_Ch1; // Ch1 -
    public TMP_Text minPerceptionValueText_Ch1;
    public Button saveMinPerceptionButton_Ch1;
    public Button testMinPerceptionButton_Ch1;
    public TMP_Text minActuationValueText_Ch1;
    public Button saveMinActuationButton_Ch1;
    public Button testMinActuationButton_Ch1;
    public TMP_Text maxActuationValueText_Ch1;
    public Button saveMaxActuationButton_Ch1;
    public Button testMaxActuationButton_Ch1;

    [Header("UI Elements - Channel 2")]
    public TMP_Text currentIntensityText_Ch2; // Ch2 현재 조절값
    public Button increaseIntensityButton_Ch2; // Ch2 +
    public Button decreaseIntensityButton_Ch2; // Ch2 -
    public TMP_Text minPerceptionValueText_Ch2;
    public Button saveMinPerceptionButton_Ch2;
    public Button testMinPerceptionButton_Ch2;
    public TMP_Text minActuationValueText_Ch2;
    public Button saveMinActuationButton_Ch2;
    public Button testMinActuationButton_Ch2;
    public TMP_Text maxActuationValueText_Ch2;
    public Button saveMaxActuationButton_Ch2;
    public Button testMaxActuationButton_Ch2;

    [Header("UI Elements - Channel 3")]
    public TMP_Text currentIntensityText_Ch3; // Ch3 현재 조절값
    public Button increaseIntensityButton_Ch3; // Ch3 +
    public Button decreaseIntensityButton_Ch3; // Ch3 -
    public TMP_Text minPerceptionValueText_Ch3;
    public Button saveMinPerceptionButton_Ch3;
    public Button testMinPerceptionButton_Ch3;
    public TMP_Text minActuationValueText_Ch3;
    public Button saveMinActuationButton_Ch3;
    public Button testMinActuationButton_Ch3;
    public TMP_Text maxActuationValueText_Ch3;
    public Button saveMaxActuationButton_Ch3;
    public Button testMaxActuationButton_Ch3;

    [Header("UI Elements - Channel 4")]
    public TMP_Text currentIntensityText_Ch4; // Ch4 현재 조절값
    public Button increaseIntensityButton_Ch4; // Ch4 +
    public Button decreaseIntensityButton_Ch4; // Ch4 -
    public TMP_Text minPerceptionValueText_Ch4;
    public Button saveMinPerceptionButton_Ch4;
    public Button testMinPerceptionButton_Ch4;
    public TMP_Text minActuationValueText_Ch4;
    public Button saveMinActuationButton_Ch4;
    public Button testMinActuationButton_Ch4;
    public TMP_Text maxActuationValueText_Ch4;
    public Button saveMaxActuationButton_Ch4;
    public Button testMaxActuationButton_Ch4;

    [Header("UI Elements - Channel 5")]
    public TMP_Text currentIntensityText_Ch5; // Ch5 현재 조절값
    public Button increaseIntensityButton_Ch5; // Ch5 +
    public Button decreaseIntensityButton_Ch5; // Ch5 -
    public TMP_Text minPerceptionValueText_Ch5;
    public Button saveMinPerceptionButton_Ch5;
    public Button testMinPerceptionButton_Ch5;
    public TMP_Text minActuationValueText_Ch5;
    public Button saveMinActuationButton_Ch5;
    public Button testMinActuationButton_Ch5;
    public TMP_Text maxActuationValueText_Ch5;
    public Button saveMaxActuationButton_Ch5;
    public Button testMaxActuationButton_Ch5;

    [Header("UI Elements - Channel 6")]
    public TMP_Text currentIntensityText_Ch6; // Ch5 현재 조절값
    public Button increaseIntensityButton_Ch6; // Ch5 +
    public Button decreaseIntensityButton_Ch6; // Ch5 -
    public TMP_Text minPerceptionValueText_Ch6;
    public Button saveMinPerceptionButton_Ch6;
    public Button testMinPerceptionButton_Ch6;
    public TMP_Text minActuationValueText_Ch6;
    public Button saveMinActuationButton_Ch6;
    public Button testMinActuationButton_Ch6;
    public TMP_Text maxActuationValueText_Ch6;
    public Button saveMaxActuationButton_Ch6;
    public Button testMaxActuationButton_Ch6;

    [Header("UI Elements - Pilot Test")]
    public GameObject pilotTestPanel;
    public TMP_Text testInstructionText;

    public Button modeOneFingerButton;  // 'One Finger' (3채널 로직) 선택 버튼
    public Button modeFourFingerButton; // 'Four Finger' (5채널 로직) 선택 버튼

    public Button straightPresetButton;
    public Button swingPresetButton;
    // ... (모든 리듬/모드 버튼들) ...
    public Button modePressReleaseButton;
    public Button modeOnlyFingerButton;
    public Button modeFingerForearmButton;
    public Button modeFingerForearmPressReleaseButton;
    public Button timingEighthButton;
    public Button timingFourthButton;
    public Button timingTripletButton;
    public Button length1to1Button;
    public Button length1_5to1Button;
    public Button length2to1Button;
    public Button velocityWeakWeakButton;
    public Button velocityWeakStrongButton;
    public Button velocityStrongStrongButton;


    [Header("System Status")]
    public TMP_Text statusText;

    [Header("EMS Parameters")]
    public int intensityStep = 1;
    public float pulseDurationSec = 0.5f; // 아두이노로 보내는 자극 기본 지속 시간
    public int testBPM = 50;

    // --- Internal Variables ---
    private TcpClient client;
    private StreamWriter writer;
    private bool isConnected = false;

    // --- [!! 수정됨 1.2: 6채널 변수 배열 !!] ---
    private const int ChannelCount = 6;
    // 영구 저장되는 값
    private int[] minPerceptions = new int[ChannelCount];
    private int[] minActuations = new int[ChannelCount];
    private int[] maxActuations = new int[ChannelCount];

    // +/- 버튼으로 조절되는 '임시 값'
    private int[] currentIntensities = new int[ChannelCount];

    // UI 텍스트 배열 (관리를 위해)
    private TMP_Text[] currentIntensityTexts = new TMP_Text[ChannelCount];
    private TMP_Text[] minPerceptionValueTexts = new TMP_Text[ChannelCount];
    private TMP_Text[] minActuationValueTexts = new TMP_Text[ChannelCount];
    private TMP_Text[] maxActuationValueTexts = new TMP_Text[ChannelCount];

    private enum RhythmPlaybackMode { OneFinger, FourFinger }
    private RhythmPlaybackMode currentRhythmMode = RhythmPlaybackMode.OneFinger;

    // 4핑거 모드 (Ch1-4: Fingers, Ch5: Forearm)
    private enum FourFingerStimMode { PressRelease, OnlyFinger, FingerForearm, FingerForearmPressRelease }
    private FourFingerStimMode currentFourFingerMode = FourFingerStimMode.OnlyFinger;

    // 1핑거 모드 (Ch1: Press, Ch2: Forearm, Ch3: Release)
    private enum OneFingerStimMode { PressRelease, OnlyFinger, FingerForearm, FingerForearmPressRelease }
    private OneFingerStimMode currentOneFingerMode = OneFingerStimMode.PressRelease;

    private enum TimingMode { Eighth, Fourth, Triplet }
    private enum LengthMode { Straight, Swing1_5, Swing2_0 }
    private enum VelocityMode { WeakWeak, WeakStrong, StrongStrong }

    private TimingMode currentTimingMode = TimingMode.Eighth;
    private LengthMode currentLengthMode = LengthMode.Straight;
    private VelocityMode currentVelocityMode = VelocityMode.WeakWeak;

    private enum State
    {
        Idle,
        Connecting,
        Calibrating, // 캘리브레이션 모드 (단일 상태)
        ReadyToTest,
        Testing
    }
    private State currentState = State.Idle;
    private Coroutine currentTestCoroutine = null;

    [Header("Data Logging")]
    public EMSLogger emsLogger;

    float pressDuration1, releaseDuration1;
    float pressDuration2, releaseDuration2;

    // --- Unity Lifecycle Functions ---
    void Start()
    {
        UpdateStatus("Connecting to Arduino...");
        ConnectToArduino();
        PopulateUIArrays(); // UI 배열 초기화
        SetupUIListeners();
        LoadCalibrationData(); // 모든 채널의 저장된 값 로드 및 UI 표시
        SetState(isConnected ? State.Calibrating : State.Idle);
    }

    void OnApplicationQuit()
    {
        if (isConnected)
        {
            SendEmsCommand(0, 0, 0, 0, 0, 0); // 5채널 OFF
            writer?.Close();
            client?.Close();
        }
    }

    // --- [!! 신규 1.4: UI 배열 초기화 함수 !!] ---
    // (Unity 에디터에서 값을 다 연결한 후, 코드가 사용하기 쉽게 배열로 묶음)
    void PopulateUIArrays()
    {
        // '현재 값' 텍스트 배열
        currentIntensityTexts[0] = currentIntensityText_Ch1;
        currentIntensityTexts[1] = currentIntensityText_Ch2;
        currentIntensityTexts[2] = currentIntensityText_Ch3;
        currentIntensityTexts[3] = currentIntensityText_Ch4;
        currentIntensityTexts[4] = currentIntensityText_Ch5;
        currentIntensityTexts[5] = currentIntensityText_Ch6;

        // 'MinP' 텍스트 배열
        minPerceptionValueTexts[0] = minPerceptionValueText_Ch1;
        minPerceptionValueTexts[1] = minPerceptionValueText_Ch2;
        minPerceptionValueTexts[2] = minPerceptionValueText_Ch3;
        minPerceptionValueTexts[3] = minPerceptionValueText_Ch4;
        minPerceptionValueTexts[4] = minPerceptionValueText_Ch5;
        minPerceptionValueTexts[5] = minPerceptionValueText_Ch6;

        // 'MinA' 텍스트 배열
        minActuationValueTexts[0] = minActuationValueText_Ch1;
        minActuationValueTexts[1] = minActuationValueText_Ch2;
        minActuationValueTexts[2] = minActuationValueText_Ch3;
        minActuationValueTexts[3] = minActuationValueText_Ch4;
        minActuationValueTexts[4] = minActuationValueText_Ch5;
        minActuationValueTexts[5] = minActuationValueText_Ch6;

        // 'MaxA' 텍스트 배열
        maxActuationValueTexts[0] = maxActuationValueText_Ch1;
        maxActuationValueTexts[1] = maxActuationValueText_Ch2;
        maxActuationValueTexts[2] = maxActuationValueText_Ch3;
        maxActuationValueTexts[3] = maxActuationValueText_Ch4;
        maxActuationValueTexts[4] = maxActuationValueText_Ch5;
        maxActuationValueTexts[5] = maxActuationValueText_Ch6;
    }

    // --- Network Connection (동일) ---
    void ConnectToArduino()
    {
        try
        {
            client = new TcpClient();
            client.Connect(arduinoIpAddress, arduinoPort);
            writer = new StreamWriter(client.GetStream());
            isConnected = true;
            Debug.Log($"Arduino connected at {arduinoIpAddress}:{arduinoPort}");
            UpdateStatus("Arduino Connected. Start Calibration.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error connecting to Arduino: {e.Message}");
            UpdateStatus("Arduino Connection Failed!");
            isConnected = false;
        }
    }

    // --- [!! 수정됨 2.1: UI 리스너 (총 10 + 15 + 15 = 40개 버튼) !!] ---
    void SetupUIListeners()
    {
        // 공통 캘리브레이션 버튼
        passButton.onClick.AddListener(OnPassButtonClicked);
        resetButton.onClick.AddListener(ResetStimulation); // EMS 즉시 정지

        // 1. 채널별 '+ / -' 버튼 (총 10개)
        if (increaseIntensityButton_Ch1 != null) increaseIntensityButton_Ch1.onClick.AddListener(() => IncreaseIntensity(1));
        if (decreaseIntensityButton_Ch1 != null) decreaseIntensityButton_Ch1.onClick.AddListener(() => DecreaseIntensity(1));
        if (increaseIntensityButton_Ch2 != null) increaseIntensityButton_Ch2.onClick.AddListener(() => IncreaseIntensity(2));
        if (decreaseIntensityButton_Ch2 != null) decreaseIntensityButton_Ch2.onClick.AddListener(() => DecreaseIntensity(2));
        if (increaseIntensityButton_Ch3 != null) increaseIntensityButton_Ch3.onClick.AddListener(() => IncreaseIntensity(3));
        if (decreaseIntensityButton_Ch3 != null) decreaseIntensityButton_Ch3.onClick.AddListener(() => DecreaseIntensity(3));
        if (increaseIntensityButton_Ch4 != null) increaseIntensityButton_Ch4.onClick.AddListener(() => IncreaseIntensity(4));
        if (decreaseIntensityButton_Ch4 != null) decreaseIntensityButton_Ch4.onClick.AddListener(() => DecreaseIntensity(4));
        if (increaseIntensityButton_Ch5 != null) increaseIntensityButton_Ch5.onClick.AddListener(() => IncreaseIntensity(5));
        if (decreaseIntensityButton_Ch5 != null) decreaseIntensityButton_Ch5.onClick.AddListener(() => DecreaseIntensity(5));
        if (increaseIntensityButton_Ch6 != null) increaseIntensityButton_Ch6.onClick.AddListener(() => IncreaseIntensity(6));
        if (decreaseIntensityButton_Ch6 != null) decreaseIntensityButton_Ch6.onClick.AddListener(() => DecreaseIntensity(6));

        // 2. 값별 'Save' 버튼 (총 15개)
        if (saveMinPerceptionButton_Ch1 != null) saveMinPerceptionButton_Ch1.onClick.AddListener(SaveMinPerception_Ch1);
        if (saveMinActuationButton_Ch1 != null) saveMinActuationButton_Ch1.onClick.AddListener(SaveMinActuation_Ch1);
        if (saveMaxActuationButton_Ch1 != null) saveMaxActuationButton_Ch1.onClick.AddListener(SaveMaxActuation_Ch1);

        if (saveMinPerceptionButton_Ch2 != null) saveMinPerceptionButton_Ch2.onClick.AddListener(SaveMinPerception_Ch2);
        if (saveMinActuationButton_Ch2 != null) saveMinActuationButton_Ch2.onClick.AddListener(SaveMinActuation_Ch2);
        if (saveMaxActuationButton_Ch2 != null) saveMaxActuationButton_Ch2.onClick.AddListener(SaveMaxActuation_Ch2);

        if (saveMinPerceptionButton_Ch3 != null) saveMinPerceptionButton_Ch3.onClick.AddListener(SaveMinPerception_Ch3);
        if (saveMinActuationButton_Ch3 != null) saveMinActuationButton_Ch3.onClick.AddListener(SaveMinActuation_Ch3);
        if (saveMaxActuationButton_Ch3 != null) saveMaxActuationButton_Ch3.onClick.AddListener(SaveMaxActuation_Ch3);

        if (saveMinPerceptionButton_Ch4 != null) saveMinPerceptionButton_Ch4.onClick.AddListener(SaveMinPerception_Ch4);
        if (saveMinActuationButton_Ch4 != null) saveMinActuationButton_Ch4.onClick.AddListener(SaveMinActuation_Ch4);
        if (saveMaxActuationButton_Ch4 != null) saveMaxActuationButton_Ch4.onClick.AddListener(SaveMaxActuation_Ch4);

        if (saveMinPerceptionButton_Ch5 != null) saveMinPerceptionButton_Ch5.onClick.AddListener(SaveMinPerception_Ch5);
        if (saveMinActuationButton_Ch5 != null) saveMinActuationButton_Ch5.onClick.AddListener(SaveMinActuation_Ch5);
        if (saveMaxActuationButton_Ch5 != null) saveMaxActuationButton_Ch5.onClick.AddListener(SaveMaxActuation_Ch5);

        if (saveMinPerceptionButton_Ch6 != null) saveMinPerceptionButton_Ch6.onClick.AddListener(SaveMinPerception_Ch6);
        if (saveMinActuationButton_Ch6 != null) saveMinActuationButton_Ch6.onClick.AddListener(SaveMinActuation_Ch6);
        if (saveMaxActuationButton_Ch6 != null) saveMaxActuationButton_Ch6.onClick.AddListener(SaveMaxActuation_Ch6);

        // 3. 값별 'Test' 버튼 (총 15개)
        if (testMinPerceptionButton_Ch1 != null) testMinPerceptionButton_Ch1.onClick.AddListener(TestMinPerception_Ch1);
        if (testMinActuationButton_Ch1 != null) testMinActuationButton_Ch1.onClick.AddListener(TestMinActuation_Ch1);
        if (testMaxActuationButton_Ch1 != null) testMaxActuationButton_Ch1.onClick.AddListener(TestMaxActuation_Ch1);

        if (testMinPerceptionButton_Ch2 != null) testMinPerceptionButton_Ch2.onClick.AddListener(TestMinPerception_Ch2);
        if (testMinActuationButton_Ch2 != null) testMinActuationButton_Ch2.onClick.AddListener(TestMinActuation_Ch2);
        if (testMaxActuationButton_Ch2 != null) testMaxActuationButton_Ch2.onClick.AddListener(TestMaxActuation_Ch2);

        if (testMinPerceptionButton_Ch3 != null) testMinPerceptionButton_Ch3.onClick.AddListener(TestMinPerception_Ch3);
        if (testMinActuationButton_Ch3 != null) testMinActuationButton_Ch3.onClick.AddListener(TestMinActuation_Ch3);
        if (testMaxActuationButton_Ch3 != null) testMaxActuationButton_Ch3.onClick.AddListener(TestMaxActuation_Ch3);

        if (testMinPerceptionButton_Ch4 != null) testMinPerceptionButton_Ch4.onClick.AddListener(TestMinPerception_Ch4);
        if (testMinActuationButton_Ch4 != null) testMinActuationButton_Ch4.onClick.AddListener(TestMinActuation_Ch4);
        if (testMaxActuationButton_Ch4 != null) testMaxActuationButton_Ch4.onClick.AddListener(TestMaxActuation_Ch4);

        if (testMinPerceptionButton_Ch5 != null) testMinPerceptionButton_Ch5.onClick.AddListener(TestMinPerception_Ch5);
        if (testMinActuationButton_Ch5 != null) testMinActuationButton_Ch5.onClick.AddListener(TestMinActuation_Ch5);
        if (testMaxActuationButton_Ch5 != null) testMaxActuationButton_Ch5.onClick.AddListener(TestMaxActuation_Ch5);

        if (testMinPerceptionButton_Ch6 != null) testMinPerceptionButton_Ch6.onClick.AddListener(TestMinPerception_Ch6);
        if (testMinActuationButton_Ch6 != null) testMinActuationButton_Ch6.onClick.AddListener(TestMinActuation_Ch6);
        if (testMaxActuationButton_Ch6 != null) testMaxActuationButton_Ch6.onClick.AddListener(TestMaxActuation_Ch6);

        if (modeOneFingerButton != null)
            modeOneFingerButton.onClick.AddListener(() =>
            {
                currentRhythmMode = RhythmPlaybackMode.OneFinger;
                Debug.Log("Master Mode Set: OneFinger (3-Channel Logic)");
                UpdateUIForState();
            });
        if (modeFourFingerButton != null)
            modeFourFingerButton.onClick.AddListener(() =>
            {
                currentRhythmMode = RhythmPlaybackMode.FourFinger;
                Debug.Log("Master Mode Set: FourFinger (5-Channel Logic)");
                UpdateUIForState();
            });

        // --- 공통 서브 모드 버튼 (OnlyFinger, FingerForearm) ---
        if (modeOnlyFingerButton != null)
            modeOnlyFingerButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.OnlyFinger;
                    Debug.Log("OneFinger Mode Set: Only Finger (Ch1->OFF)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.OnlyFinger;
                    Debug.Log("FourFinger Mode Set: Only Finger (Ch1-4)");
                }
            });
        if (modeFingerForearmButton != null)
            modeFingerForearmButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.FingerForearm;
                    Debug.Log("OneFinger Mode Set: Finger & Forearm (Ch1+Ch5)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.FingerForearm;
                    Debug.Log("FourFinger Mode Set: Finger & Forearm (Ch1-4 + Ch5)");
                }
            });

        // --- 공통 서브 모드 버튼 (PressRelease, FingerForearmPressRelease) ---
        if (modePressReleaseButton != null)// --- 공통 서브 모드 버튼 (OnlyFinger, FingerForearm) ---
            if (modeOnlyFingerButton != null)
                modeOnlyFingerButton.onClick.AddListener(() =>
                {
                    if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                    {
                        currentOneFingerMode = OneFingerStimMode.OnlyFinger;
                        Debug.Log("OneFinger Mode Set: Only Finger (Ch1->OFF)");
                    }
                    else
                    {
                        currentFourFingerMode = FourFingerStimMode.OnlyFinger;
                        Debug.Log("FourFinger Mode Set: Only Finger (Ch1-4)");
                    }
                });
        if (modeFingerForearmButton != null)
            modeFingerForearmButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.FingerForearm;
                    Debug.Log("OneFinger Mode Set: Finger & Forearm (Ch1+Ch5)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.FingerForearm;
                    Debug.Log("FourFinger Mode Set: Finger & Forearm (Ch1-4 + Ch5)");
                }
            });

        // --- 공통 서브 모드 버튼 (PressRelease, FingerForearmPressRelease) ---
        if (modePressReleaseButton != null)
            modePressReleaseButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.PressRelease;
                    Debug.Log("OneFinger Mode Set: Press & Release (Ch1->Ch6)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.PressRelease;
                    Debug.Log("FourFinger Mode Set: Only Finger + Press & Release (Ch1-4 -> Ch6)");
                }
            });
        if (modeFingerForearmPressReleaseButton != null)
            modeFingerForearmPressReleaseButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.FingerForearmPressRelease;
                    Debug.Log("OneFinger Mode Set: Finger & Forearm + Press & Release (Ch1+Ch5->Ch6)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.FingerForearmPressRelease;
                    Debug.Log("FourFinger Mode Set: Finger & Forearm + Press & Release (Ch1-4+Ch5 -> Ch6)");
                }
            });
        modePressReleaseButton.onClick.AddListener(() =>
        {
            if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
            {
                currentOneFingerMode = OneFingerStimMode.PressRelease;
                Debug.Log("OneFinger Mode Set: Press & Release (Ch1->Ch6)");
            }
            else
            {
                currentFourFingerMode = FourFingerStimMode.PressRelease;
                Debug.Log("FourFinger Mode Set: Only Finger + Press & Release (Ch1-4 -> Ch6)");
            }
        });
        if (modeFingerForearmPressReleaseButton != null)
            modeFingerForearmPressReleaseButton.onClick.AddListener(() =>
            {
                if (currentRhythmMode == RhythmPlaybackMode.OneFinger)
                {
                    currentOneFingerMode = OneFingerStimMode.FingerForearmPressRelease;
                    Debug.Log("OneFinger Mode Set: Finger & Forearm + Press & Release (Ch1+Ch5->Ch6)");
                }
                else
                {
                    currentFourFingerMode = FourFingerStimMode.FingerForearmPressRelease;
                    Debug.Log("FourFinger Mode Set: Finger & Forearm + Press & Release (Ch1-4+Ch5 -> Ch6)");
                }
            });
        if (straightPresetButton != null)
        {
            straightPresetButton.onClick.AddListener(() =>
            {
                currentTimingMode = TimingMode.Eighth;
                currentLengthMode = LengthMode.Straight;
                currentVelocityMode = VelocityMode.WeakWeak;
                string rhythmType = "Straight_1_1";
                string modeStr = (currentRhythmMode == RhythmPlaybackMode.OneFinger) ? currentOneFingerMode.ToString() : currentFourFingerMode.ToString();
                if (emsLogger != null) emsLogger.StartNewTrial(rhythmType, modeStr);


                Debug.Log("Preset Set: Straight (8th, 1:1, WeakWeak)");
                StartRhythmTest();
            });
        }

        if (swingPresetButton != null)
        {
            swingPresetButton.onClick.AddListener(() =>
            {
                currentTimingMode = TimingMode.Eighth;
                currentLengthMode = LengthMode.Swing2_0;
                currentVelocityMode = VelocityMode.WeakStrong;
                string rhythmType = "Swing_2_1";
                string modeStr = (currentRhythmMode == RhythmPlaybackMode.OneFinger) ? currentOneFingerMode.ToString() : currentFourFingerMode.ToString();
                if (emsLogger != null) emsLogger.StartNewTrial(rhythmType, modeStr);

                Debug.Log("Preset Set: Swing (8th, 2:1, WeakStrong)");
                StartRhythmTest();
            });
        }

        timingEighthButton.onClick.AddListener(() => { currentTimingMode = TimingMode.Eighth; StartRhythmTest(); });
        timingFourthButton.onClick.AddListener(() => { currentTimingMode = TimingMode.Fourth; StartRhythmTest(); });
        timingTripletButton.onClick.AddListener(() => { currentTimingMode = TimingMode.Triplet; StartRhythmTest(); });
        length1to1Button.onClick.AddListener(() => { currentLengthMode = LengthMode.Straight; StartRhythmTest(); });
        length1_5to1Button.onClick.AddListener(() => { currentLengthMode = LengthMode.Swing1_5; StartRhythmTest(); });
        length2to1Button.onClick.AddListener(() => { currentLengthMode = LengthMode.Swing2_0; StartRhythmTest(); });
        velocityWeakWeakButton.onClick.AddListener(() => { currentVelocityMode = VelocityMode.WeakWeak; StartRhythmTest(); });
        velocityWeakStrongButton.onClick.AddListener(() => { currentVelocityMode = VelocityMode.WeakStrong; StartRhythmTest(); });
        velocityStrongStrongButton.onClick.AddListener(() => { currentVelocityMode = VelocityMode.StrongStrong; StartRhythmTest(); });
    }

    void InitializeCurrentIntensities()
    {
        for (int i = 0; i < ChannelCount; i++)
        {
            currentIntensities[i] = 0;
            if (currentIntensityTexts[i] != null) currentIntensityTexts[i].text = "0";
        }
    }

    // --- 모든 채널 UI 텍스트 업데이트 (저장된 값) ---
    void UpdateAllCalibrationUI()
    {
        for (int i = 0; i < ChannelCount; i++)
        {
            if (minPerceptionValueTexts[i] != null) minPerceptionValueTexts[i].text = minPerceptions[i] > -1 ? minPerceptions[i].ToString() : "0";
            if (minActuationValueTexts[i] != null) minActuationValueTexts[i].text = minActuations[i] > -1 ? minActuations[i].ToString() : "0";
            if (maxActuationValueTexts[i] != null) maxActuationValueTexts[i].text = maxActuations[i] > -1 ? maxActuations[i].ToString() : "";
        }
    }

    void SetState(State newState)
    {
        currentState = newState;
        UpdateUIForState();
    }

    // --- [!! 수정됨 2.2: 단순화된 UI 상태 로직 !!] ---
    void UpdateUIForState()
    {
        calibrationPanel.SetActive(currentState == State.Calibrating || currentState == State.ReadyToTest);
        pilotTestPanel.SetActive(currentState == State.ReadyToTest || currentState == State.Testing);

        // 캘리브레이션 중일 때만 +/- 및 Save 버튼 활성화
        bool isCalibrating = (currentState == State.Calibrating);
        // 테스트 버튼은 테스트 중(Testing)이 아닐 때 항상 활성화
        bool canTest = (currentState != State.Testing);

        // 모든 버튼의 활성화 상태를 루프로 설정
        if (increaseIntensityButton_Ch1 != null) increaseIntensityButton_Ch1.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch1 != null) decreaseIntensityButton_Ch1.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch1 != null) saveMinPerceptionButton_Ch1.interactable = isCalibrating;
        if (saveMinActuationButton_Ch1 != null) saveMinActuationButton_Ch1.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch1 != null) saveMaxActuationButton_Ch1.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch1 != null) testMinPerceptionButton_Ch1.interactable = canTest;
        if (testMinActuationButton_Ch1 != null) testMinActuationButton_Ch1.interactable = canTest;
        if (testMaxActuationButton_Ch1 != null) testMaxActuationButton_Ch1.interactable = canTest;

        if (increaseIntensityButton_Ch2 != null) increaseIntensityButton_Ch2.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch2 != null) decreaseIntensityButton_Ch2.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch2 != null) saveMinPerceptionButton_Ch2.interactable = isCalibrating;
        if (saveMinActuationButton_Ch2 != null) saveMinActuationButton_Ch2.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch2 != null) saveMaxActuationButton_Ch2.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch2 != null) testMinPerceptionButton_Ch2.interactable = canTest;
        if (testMinActuationButton_Ch2 != null) testMinActuationButton_Ch2.interactable = canTest;
        if (testMaxActuationButton_Ch2 != null) testMaxActuationButton_Ch2.interactable = canTest;

        if (increaseIntensityButton_Ch3 != null) increaseIntensityButton_Ch3.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch3 != null) decreaseIntensityButton_Ch3.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch3 != null) saveMinPerceptionButton_Ch3.interactable = isCalibrating;
        if (saveMinActuationButton_Ch3 != null) saveMinActuationButton_Ch3.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch3 != null) saveMaxActuationButton_Ch3.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch3 != null) testMinPerceptionButton_Ch3.interactable = canTest;
        if (testMinActuationButton_Ch3 != null) testMinActuationButton_Ch3.interactable = canTest;
        if (testMaxActuationButton_Ch3 != null) testMaxActuationButton_Ch3.interactable = canTest;

        if (increaseIntensityButton_Ch4 != null) increaseIntensityButton_Ch4.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch4 != null) decreaseIntensityButton_Ch4.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch4 != null) saveMinPerceptionButton_Ch4.interactable = isCalibrating;
        if (saveMinActuationButton_Ch4 != null) saveMinActuationButton_Ch4.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch4 != null) saveMaxActuationButton_Ch4.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch4 != null) testMinPerceptionButton_Ch4.interactable = canTest;
        if (testMinActuationButton_Ch4 != null) testMinActuationButton_Ch4.interactable = canTest;
        if (testMaxActuationButton_Ch4 != null) testMaxActuationButton_Ch4.interactable = canTest;

        if (increaseIntensityButton_Ch5 != null) increaseIntensityButton_Ch5.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch5 != null) decreaseIntensityButton_Ch5.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch5 != null) saveMinPerceptionButton_Ch5.interactable = isCalibrating;
        if (saveMinActuationButton_Ch5 != null) saveMinActuationButton_Ch5.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch5 != null) saveMaxActuationButton_Ch5.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch5 != null) testMinPerceptionButton_Ch5.interactable = canTest;
        if (testMinActuationButton_Ch5 != null) testMinActuationButton_Ch5.interactable = canTest;
        if (testMaxActuationButton_Ch5 != null) testMaxActuationButton_Ch5.interactable = canTest;

        if (increaseIntensityButton_Ch6 != null) increaseIntensityButton_Ch6.interactable = isCalibrating;
        if (decreaseIntensityButton_Ch6 != null) decreaseIntensityButton_Ch6.interactable = isCalibrating;
        if (saveMinPerceptionButton_Ch6 != null) saveMinPerceptionButton_Ch6.interactable = isCalibrating;
        if (saveMinActuationButton_Ch6 != null) saveMinActuationButton_Ch6.interactable = isCalibrating;
        if (saveMaxActuationButton_Ch6 != null) saveMaxActuationButton_Ch6.interactable = isCalibrating;
        if (testMinPerceptionButton_Ch6 != null) testMinPerceptionButton_Ch6.interactable = canTest;
        if (testMinActuationButton_Ch6 != null) testMinActuationButton_Ch6.interactable = canTest;
        if (testMaxActuationButton_Ch6 != null) testMaxActuationButton_Ch6.interactable = canTest;


        // 파일럿 테스트 버튼들
        bool testingButtonsActive = (currentState == State.ReadyToTest);

        if (modeOnlyFingerButton != null) modeOnlyFingerButton.gameObject.SetActive(testingButtonsActive);
        if (modeFingerForearmButton != null) modeFingerForearmButton.gameObject.SetActive(testingButtonsActive);
        if (modePressReleaseButton != null) modePressReleaseButton.gameObject.SetActive(testingButtonsActive);
        if (modeFingerForearmPressReleaseButton != null) modeFingerForearmPressReleaseButton.gameObject.SetActive(testingButtonsActive);

        // 마스터 모드 선택 버튼
        if (modeOneFingerButton != null) modeOneFingerButton.interactable = testingButtonsActive;
        if (modeFourFingerButton != null) modeFourFingerButton.interactable = testingButtonsActive;

        if (modePressReleaseButton != null) modePressReleaseButton.interactable = testingButtonsActive;
        if (modeOnlyFingerButton != null) modeOnlyFingerButton.interactable = testingButtonsActive;
        if (modeFingerForearmButton != null) modeFingerForearmButton.interactable = testingButtonsActive;
        if (modeFingerForearmPressReleaseButton != null) modeFingerForearmPressReleaseButton.interactable = testingButtonsActive;
        if (straightPresetButton != null) straightPresetButton.interactable = testingButtonsActive;
        if (swingPresetButton != null) swingPresetButton.interactable = testingButtonsActive;
        if (timingEighthButton != null) timingEighthButton.interactable = testingButtonsActive;
        if (timingFourthButton != null) timingFourthButton.interactable = testingButtonsActive;
        if (timingTripletButton != null) timingTripletButton.interactable = testingButtonsActive;
        if (length1to1Button != null) length1to1Button.interactable = testingButtonsActive;
        if (length1_5to1Button != null) length1_5to1Button.interactable = testingButtonsActive;
        if (length2to1Button != null) length2to1Button.interactable = testingButtonsActive;
        if (velocityWeakWeakButton != null) velocityWeakWeakButton.interactable = testingButtonsActive;
        if (velocityWeakStrongButton != null) velocityWeakStrongButton.interactable = testingButtonsActive;
        if (velocityStrongStrongButton != null) velocityStrongStrongButton.interactable = testingButtonsActive;

        // 상태별 지시 텍스트
        switch (currentState)
        {
            case State.Connecting: statusText.text = "Status: Connecting..."; break;
            case State.Calibrating:
                calibrationInstructionText.text = "Adjust intensity using +/- buttons and press 'Save' for the desired value.";
                break;
            case State.ReadyToTest:
                statusText.text = "Status: Ready for Pilot Test.";
                testInstructionText.text = "Press a button to test the pattern.";
                break;
            case State.Testing:
                statusText.text = "Status: Testing in progress...";
                testInstructionText.text = "Feeling the pattern...";
                break;
            default: statusText.text = isConnected ? "Status: Idle" : "Status: Connection Failed!"; break;
        }
    }

    void OnPassButtonClicked()
    {
        Debug.Log("Pass button clicked. Loading saved values and skipping to test.");
        LoadCalibrationData(); // 저장된 값 로드
        SetState(State.ReadyToTest); // 파일럿 테스트 모드로
    }

    // --- [!! 신규 3.1: 개별 테스트 함수 (총 15개) !!] ---
    // 공통 테스트 로직
    private void TestValue(int channel, int value)
    {
        if (value < 0 || currentState == State.Testing)
        {
            Debug.LogWarning($"Cannot test CH{channel}. Value not set ({value}) or test in progress.");
            return;
        }
        Debug.Log($"Testing CH{channel} value: {value}");

        int[] intensities = new int[ChannelCount];
        intensities[channel - 1] = value; // 1-based to 0-based
        SendEmsCommand(intensities[0], intensities[1], intensities[2], intensities[3], intensities[4], intensities[5], pulseDurationSec);
        StartCoroutine(StopStimulationAfterDelay(pulseDurationSec));
    }

    // Channel 1
    void TestMinPerception_Ch1() { TestValue(1, minPerceptions[0]); }
    void TestMinActuation_Ch1() { TestValue(1, minActuations[0]); }
    void TestMaxActuation_Ch1() { TestValue(1, maxActuations[0]); }
    // Channel 2
    void TestMinPerception_Ch2() { TestValue(2, minPerceptions[1]); }
    void TestMinActuation_Ch2() { TestValue(2, minActuations[1]); }
    void TestMaxActuation_Ch2() { TestValue(2, maxActuations[1]); }
    // Channel 3
    void TestMinPerception_Ch3() { TestValue(3, minPerceptions[2]); }
    void TestMinActuation_Ch3() { TestValue(3, minActuations[2]); }
    void TestMaxActuation_Ch3() { TestValue(3, maxActuations[2]); }
    // Channel 4
    void TestMinPerception_Ch4() { TestValue(4, minPerceptions[3]); }
    void TestMinActuation_Ch4() { TestValue(4, minActuations[3]); }
    void TestMaxActuation_Ch4() { TestValue(4, maxActuations[3]); }
    // Channel 5
    void TestMinPerception_Ch5() { TestValue(5, minPerceptions[4]); }
    void TestMinActuation_Ch5() { TestValue(5, minActuations[4]); }
    void TestMaxActuation_Ch5() { TestValue(5, maxActuations[4]); }
    // Ch6
    void TestMinPerception_Ch6() { TestValue(6, minPerceptions[5]); }
    void TestMinActuation_Ch6() { TestValue(6, minActuations[5]); }
    void TestMaxActuation_Ch6() { TestValue(6, maxActuations[5]); }


    // --- [!! 신규 3.2: 채널별 Intensity 조절 !!] ---
    void IncreaseIntensity(int channel)
    {
        if (currentState != State.Calibrating) return; // 캘리브레이션 모드에서만 작동

        int index = channel - 1;
        currentIntensities[index] += intensityStep;
        currentIntensityTexts[index].text = currentIntensities[index].ToString();

        // 즉시 테스트 자극 전송
        SendTestPulse(channel, currentIntensities[index]);
    }

    void DecreaseIntensity(int channel)
    {
        if (currentState != State.Calibrating) return;

        int index = channel - 1;
        currentIntensities[index] -= intensityStep;
        if (currentIntensities[index] < 0) currentIntensities[index] = 0;
        currentIntensityTexts[index].text = currentIntensities[index].ToString();

        // 즉시 테스트 자극 전송
        SendTestPulse(channel, currentIntensities[index]);
    }

    // +/- 버튼 클릭 시 호출되는 공통 테스트 자극 함수
    void SendTestPulse(int channel, int intensity)
    {
        int[] intensities = new int[ChannelCount];
        intensities[channel - 1] = intensity; // 1-based to 0-based
        SendEmsCommand(intensities[0], intensities[1], intensities[2], intensities[3], intensities[4], intensities[5], 1.0f);
        StartCoroutine(StopStimulationAfterDelay(1.0f));
    }


    void ResetStimulation()
    {
        if (currentState == State.Testing) return;
        // 모든 '현재 값'과 텍스트를 0으로 리셋
        for (int i = 0; i < ChannelCount; i++)
        {
            currentIntensities[i] = 0;
            if (currentIntensityTexts[i] != null) currentIntensityTexts[i].text = "0";
        }
        SendEmsCommand(0, 0, 0, 0, 0, 0); // All off
        Debug.Log("Stimulation RESET to 0 by button.");
    }

    IEnumerator StopStimulationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 캘리브레이션 중이거나 테스트 준비 상태일 때만 자동 정지
        if (currentState == State.Calibrating || currentState == State.ReadyToTest)
        {
            SendEmsCommand(0, 0, 0, 0, 0, 0); // All off
        }
    }

    // --- [!! 신규 3.3: 개별 저장 함수 (총 15개) !!] ---
    // 공통 저장 로직
    private void SaveValue(int channel, string valueType)
    {
        int index = channel - 1;
        int valueToSave = currentIntensities[index]; // 현재 조절 중인 값을 가져옴

        if (valueType == "MinPerception") minPerceptions[index] = valueToSave;
        else if (valueType == "MinActuation") minActuations[index] = valueToSave;
        else if (valueType == "MaxActuation") maxActuations[index] = valueToSave;

        PlayerPrefs.SetInt($"{valueType}_Ch{channel}", valueToSave);
        PlayerPrefs.Save();
        Debug.Log($"CH{channel} {valueType} Saved: {valueToSave}");

        UpdateAllCalibrationUI(); // 모든 UI 텍스트 업데이트 (저장된 값 포함)
        SendEmsCommand(0, 0, 0, 0, 0, 0); // 자극 정지
    }

    // Channel 1
    void SaveMinPerception_Ch1() { SaveValue(1, "MinPerception"); }
    void SaveMinActuation_Ch1() { SaveValue(1, "MinActuation"); }
    void SaveMaxActuation_Ch1() { SaveValue(1, "MaxActuation"); }
    // Channel 2
    void SaveMinPerception_Ch2() { SaveValue(2, "MinPerception"); }
    void SaveMinActuation_Ch2() { SaveValue(2, "MinActuation"); }
    void SaveMaxActuation_Ch2() { SaveValue(2, "MaxActuation"); }
    // Channel 3
    void SaveMinPerception_Ch3() { SaveValue(3, "MinPerception"); }
    void SaveMinActuation_Ch3() { SaveValue(3, "MinActuation"); }
    void SaveMaxActuation_Ch3() { SaveValue(3, "MaxActuation"); }
    // Channel 4
    void SaveMinPerception_Ch4() { SaveValue(4, "MinPerception"); }
    void SaveMinActuation_Ch4() { SaveValue(4, "MinActuation"); }
    void SaveMaxActuation_Ch4() { SaveValue(4, "MaxActuation"); }
    // Channel 5
    void SaveMinPerception_Ch5() { SaveValue(5, "MinPerception"); }
    void SaveMinActuation_Ch5() { SaveValue(5, "MinActuation"); }
    void SaveMaxActuation_Ch5() { SaveValue(5, "MaxActuation"); }
    // Ch6
    void SaveMinPerception_Ch6() { SaveValue(6, "MinPerception"); }
    void SaveMinActuation_Ch6() { SaveValue(6, "MinActuation"); }
    void SaveMaxActuation_Ch6() { SaveValue(6, "MaxActuation"); }


    void StartTestCoroutine(IEnumerator testCoroutine)
    {
        if (currentTestCoroutine != null)
        {
            StopCoroutine(currentTestCoroutine);
            SendEmsCommand(0, 0, 0, 0, 0, 0);
        }
        currentTestCoroutine = StartCoroutine(testCoroutine);
    }

    void StartRhythmTest()
    {
        // 마스터 모드에 따라 분기
        if (currentRhythmMode == RhythmPlaybackMode.FourFinger)
        {
            // --- 4-Finger 모드 캘리브레이션 체크 ---
            for (int i = 0; i < 4; i++) // Ch1-4
            {
                if (minActuations[i] < 0) { Debug.LogWarning($"Cannot start test: MinActuation (Ch{i + 1}) not calibrated."); return; }
                if ((currentVelocityMode == VelocityMode.WeakStrong || currentVelocityMode == VelocityMode.StrongStrong) && maxActuations[i] < 0)
                { Debug.LogWarning($"Cannot start test: MaxActuation (Ch{i + 1}) not calibrated."); return; }
            }
            if (currentFourFingerMode == FourFingerStimMode.FingerForearm || currentFourFingerMode == FourFingerStimMode.FingerForearmPressRelease)
            {
                if (minActuations[4] < 0 || maxActuations[4] < 0) { Debug.LogWarning($"Cannot start test: Ch5 (Forearm) not calibrated."); return; }
            }
            // [!! 신규 !!] 4핑거 + 릴리즈 모드일 경우 Ch6 (릴리즈) 캘리브레이션 추가 확인
            if (currentFourFingerMode == FourFingerStimMode.PressRelease || currentFourFingerMode == FourFingerStimMode.FingerForearmPressRelease)
            {
                if (minActuations[5] < 0) { Debug.LogWarning("Cannot start test: Ch6 (Release) MinActuation not calibrated."); return; }
                if (currentVelocityMode != VelocityMode.WeakWeak && maxActuations[5] < 0) { Debug.LogWarning("Cannot start test: Ch6 (Release) MaxActuation not calibrated."); return; }
            }

            Debug.Log($"--- Starting FourFinger Rhythm Test ---");
            Debug.Log($"Mode: {currentFourFingerMode} | Timing: {currentTimingMode} | Length: {currentLengthMode} | Velocity: {currentVelocityMode}");
            StartTestCoroutine(PlayRhythmPattern_FourFinger());
        }
        else // currentRhythmMode == RhythmPlaybackMode.OneFinger
        {
            // --- 1-Finger 모드 캘리브레이션 체크 (Ch1, Ch5, Ch6) ---
            // Ch1 (Press)
            if (minActuations[0] < 0) { Debug.LogWarning("Cannot start test: MinActuation (Ch1) not calibrated."); return; }
            if (currentVelocityMode != VelocityMode.WeakWeak && maxActuations[0] < 0) { Debug.LogWarning("Cannot start test: MaxActuation (Ch1) not calibrated."); return; }

            // Ch5 (Forearm) - 필요한 모드일 때만
            if (currentOneFingerMode == OneFingerStimMode.FingerForearm || currentOneFingerMode == OneFingerStimMode.FingerForearmPressRelease)
            {
                if (minActuations[4] < 0) { Debug.LogWarning("Cannot start test: Ch5 (Forearm) MinActuation not calibrated."); return; }
                if (currentVelocityMode != VelocityMode.WeakWeak && maxActuations[4] < 0) { Debug.LogWarning("Cannot start test: Ch5 (Forearm) MaxActuation not calibrated."); return; }
            }
            // Ch6 (Release) - 필요한 모드일 때만
            if (currentOneFingerMode == OneFingerStimMode.PressRelease || currentOneFingerMode == OneFingerStimMode.FingerForearmPressRelease)
            {
                if (minActuations[5] < 0) { Debug.LogWarning("Cannot start test: Ch6 (Release) MinActuation not calibrated."); return; }
                if (currentVelocityMode != VelocityMode.WeakWeak && maxActuations[5] < 0) { Debug.LogWarning("Cannot start test: Ch6 (Release) MaxActuation not calibrated."); return; }
            }

            Debug.Log($"--- Starting OneFinger Rhythm Test ---");
            Debug.Log($"Mode: {currentOneFingerMode} | Timing: {currentTimingMode} | Length: {currentLengthMode} | Velocity: {currentVelocityMode}");
            StartTestCoroutine(PlayRhythmPattern_OneFinger());
        }
    }

    // --- [!! 수정됨 4.2: 4핑거 리듬 테스트 로직 (Ch6 릴리즈값 추가) !!] ---
    IEnumerator PlayRhythmPattern_FourFinger()
    {
        SetState(State.Testing);

        float beatDuration = 60f / testBPM;
        float noteDuration;
        switch (currentTimingMode)
        {
            case TimingMode.Fourth: noteDuration = beatDuration; break;
            case TimingMode.Triplet: noteDuration = beatDuration / 3f; break;
            case TimingMode.Eighth: default: noteDuration = beatDuration / 2f; break;
        }

        int[] fingerIntensities = new int[4];
        int[] forearmIntensities = new int[2];
        int[] releaseIntensities = new int[2]; //] Ch6 (Release) 강도

        switch (currentVelocityMode)
        {
            case VelocityMode.WeakStrong:
                fingerIntensities[0] = minActuations[0];
                fingerIntensities[2] = minActuations[2];
                fingerIntensities[1] = maxActuations[1];
                fingerIntensities[3] = maxActuations[3];
                forearmIntensities[0] = minActuations[4];
                forearmIntensities[1] = maxActuations[4];
                releaseIntensities[0] = minActuations[5]; // Ch6 MinA
                releaseIntensities[1] = maxActuations[5]; // Ch6 MaxA
                break;
            case VelocityMode.StrongStrong:
                fingerIntensities[0] = maxActuations[0];
                fingerIntensities[1] = maxActuations[1];
                fingerIntensities[2] = maxActuations[2];
                fingerIntensities[3] = maxActuations[3];
                forearmIntensities[0] = maxActuations[4];
                forearmIntensities[1] = maxActuations[4];
                releaseIntensities[0] = maxActuations[5]; // Ch6 MaxA
                releaseIntensities[1] = maxActuations[5]; // Ch6 MaxA
                break;
            case VelocityMode.WeakWeak:
            default:
                fingerIntensities[0] = minActuations[0];
                fingerIntensities[1] = minActuations[1];
                fingerIntensities[2] = minActuations[2];
                fingerIntensities[3] = minActuations[3];
                forearmIntensities[0] = minActuations[4];
                forearmIntensities[1] = minActuations[4];
                releaseIntensities[0] = minActuations[5]; // Ch6 MinA
                releaseIntensities[1] = minActuations[5]; // Ch6 MinA
                break;
        }

        float duration1, duration2;
        float totalNotePairDuration = noteDuration * 2;
        switch (currentLengthMode)
        {
            case LengthMode.Swing1_5: duration1 = totalNotePairDuration * (1.5f / 2.5f); duration2 = totalNotePairDuration * (1.0f / 2.5f); break;
            case LengthMode.Swing2_0: duration1 = totalNotePairDuration * (2f / 3f); duration2 = totalNotePairDuration * (1f / 3f); break;
            case LengthMode.Straight: default: duration1 = totalNotePairDuration * 0.5f; duration2 = totalNotePairDuration * 0.5f; break;
        }
         
        if (currentTimingMode == TimingMode.Eighth)
        {
            yield return StartCoroutine(PlayNoteSequence_FourFinger(1, fingerIntensities[0], forearmIntensities[0], releaseIntensities[0], duration1));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(2, fingerIntensities[1], forearmIntensities[1], releaseIntensities[1], duration2));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(3, fingerIntensities[2], forearmIntensities[0], releaseIntensities[0], duration1));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(4, fingerIntensities[3], forearmIntensities[1], releaseIntensities[1], duration2));
        }
        else if (currentTimingMode == TimingMode.Fourth)
        {
            yield return StartCoroutine(PlayNoteSequence_FourFinger(1, fingerIntensities[0], forearmIntensities[0], releaseIntensities[0], duration1));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(2, fingerIntensities[1], forearmIntensities[1], releaseIntensities[1], duration2));
        }
        else if (currentTimingMode == TimingMode.Triplet)
        {
            float tripletDuration = beatDuration / 3f;
            yield return StartCoroutine(PlayNoteSequence_FourFinger(1, fingerIntensities[0], forearmIntensities[0], releaseIntensities[0], tripletDuration));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(2, fingerIntensities[1], forearmIntensities[1], releaseIntensities[1], tripletDuration));
            yield return StartCoroutine(PlayNoteSequence_FourFinger(3, fingerIntensities[2], forearmIntensities[0], releaseIntensities[0], tripletDuration));
        }

        SendEmsCommand(0, 0, 0, 0, 0, 0);
        if (emsLogger != null) emsLogger.StopTrial();
        SetState(State.ReadyToTest);
        currentTestCoroutine = null;
    }

    // --- [!! 수정됨 4.3: 4핑거 시퀀스 재생 (Release 로직 추가) !!] ---
    IEnumerator PlayNoteSequence_FourFinger(int noteIndex, int fingerIntensity, int forearmIntensity, int releaseCh6Intensity, float duration)
    {
        int fingerChannel = noteIndex;
        int ch1 = 0, ch2 = 0, ch3 = 0, ch4 = 0, ch5 = 0, ch6 = 0;

        if (fingerChannel == 1) ch1 = fingerIntensity;
        else if (fingerChannel == 2) ch2 = fingerIntensity;
        else if (fingerChannel == 3) ch3 = fingerIntensity;
        else if (fingerChannel == 4) ch4 = fingerIntensity;

        Debug.Log($"[RhythmTest] Sending to Channel: {fingerChannel}, Value: {fingerIntensity}, Duration: {duration * 1000f:F0} ms");

        if (currentFourFingerMode == FourFingerStimMode.FingerForearm || currentFourFingerMode == FourFingerStimMode.FingerForearmPressRelease)
        {
            ch5 = forearmIntensity;
            if (ch5 > 0)
            {
                Debug.Log($"[RhythmTest] Sending to Channel: 5 (Forearm), Value: {ch5}, Duration: {duration * 1000f:F0} ms");
            }
        }

        // 1. PRESS
        SendEmsCommand(ch1, ch2, ch3, ch4, ch5, 0, duration);
        yield return new WaitForSeconds(duration);

        // 2. RELEASE (또는 쉼표)
        bool isPressReleaseMode = (currentFourFingerMode == FourFingerStimMode.PressRelease ||
                                   currentFourFingerMode == FourFingerStimMode.FingerForearmPressRelease);

        if (isPressReleaseMode)
        {
            // [!! 수정 !!] 릴리즈 200ms + 쉼표 300ms (총 500ms 쉼)
            Debug.Log($"[RhythmTest] Sending to Channel: 6 (Release), Value: {releaseCh6Intensity}, Duration: 200 ms");
            SendEmsCommand(0, 0, 0, 0, 0, releaseCh6Intensity, 0.2f); // 200ms 릴리즈
            yield return new WaitForSeconds(0.2f);

            SendEmsCommand(0, 0, 0, 0, 0, 0, 0.3f); // 300ms 끄기
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            // [!! 기존 !!] 쉼표 모드: 0.5초간 끔
            SendEmsCommand(0, 0, 0, 0, 0, 0, 0.5f); // All Off
            yield return new WaitForSeconds(0.5f); // 500ms 쉼표
        }
    }

    // --- [!! 수정됨 4.4: 1핑거 리듬 테스트 로직 (Ch1, Ch5, Ch6 매핑) !!] ---
    IEnumerator PlayRhythmPattern_OneFinger()
    {
        SetState(State.Testing);
        float beatDuration = 60f / testBPM;
        float noteDuration;
        int pairCountInMeasure;
        switch (currentTimingMode)
        {
            case TimingMode.Eighth: noteDuration = beatDuration / 2f; pairCountInMeasure = 2; break;
            case TimingMode.Triplet: noteDuration = beatDuration / 3f; pairCountInMeasure = 3; break;
            case TimingMode.Fourth: default: noteDuration = beatDuration; pairCountInMeasure = 1; break;
        }

        // Ch1(Press), Ch5(Forearm), Ch6(Release)
        int ch1_Intensity1, ch1_Intensity2; // Press (Ch1 -> index 0)
        int ch5_Intensity1, ch5_Intensity2; // Forearm (Ch5 -> index 4)
        int ch6_Intensity1, ch6_Intensity2; // Release (Ch6 -> index 5)

        switch (currentVelocityMode)
        {
            case VelocityMode.WeakStrong:
                ch1_Intensity1 = minActuations[0]; ch5_Intensity1 = minActuations[4]; ch6_Intensity1 = minActuations[5];
                ch1_Intensity2 = maxActuations[0]; ch5_Intensity2 = maxActuations[4]; ch6_Intensity2 = maxActuations[5];
                break;
            case VelocityMode.StrongStrong:
                ch1_Intensity1 = maxActuations[0]; ch5_Intensity1 = maxActuations[4]; ch6_Intensity1 = maxActuations[5];
                ch1_Intensity2 = maxActuations[0]; ch5_Intensity2 = maxActuations[4]; ch6_Intensity2 = maxActuations[5];
                break;
            case VelocityMode.WeakWeak:
            default:
                ch1_Intensity1 = minActuations[0]; ch5_Intensity1 = minActuations[4]; ch6_Intensity1 = minActuations[5];
                ch1_Intensity2 = minActuations[0]; ch5_Intensity2 = minActuations[4]; ch6_Intensity2 = minActuations[5];
                break;
        }

        float totalNotePairDuration = noteDuration * 2;
        float totalDuration1, totalDuration2;
        switch (currentLengthMode)
        {
            case LengthMode.Swing1_5: totalDuration1 = totalNotePairDuration * (1.5f / 2.5f); totalDuration2 = totalNotePairDuration * (1.0f / 2.5f); break;
            case LengthMode.Swing2_0: totalDuration1 = totalNotePairDuration * (2f / 3f); totalDuration2 = totalNotePairDuration * (1f / 3f); break;
            case LengthMode.Straight: default: totalDuration1 = totalNotePairDuration * 0.5f; totalDuration2 = totalNotePairDuration * 0.5f; break;
        }

        // 1-Finger 모드는 500ms 쉼표가 releaseDuration에 이미 포함되어 있음
        pressDuration1 = totalDuration1;
        releaseDuration1 = 0.5f;
        pressDuration2 = totalDuration2;
        releaseDuration2 = 0.5f;

        for (int i = 0; i < pairCountInMeasure; i++)
        {
            yield return StartCoroutine(PlaySingleNote_OneFinger(
                currentOneFingerMode,
                ch1_Intensity1, ch5_Intensity1, ch6_Intensity1,
                pressDuration1, releaseDuration1));

            yield return StartCoroutine(PlaySingleNote_OneFinger(
                currentOneFingerMode,
                ch1_Intensity2, ch5_Intensity2, ch6_Intensity2,
                pressDuration2, releaseDuration2));
        }

        SendEmsCommand(0, 0, 0, 0, 0, 0);
        if (emsLogger != null) emsLogger.StopTrial();
        SetState(State.ReadyToTest);
        currentTestCoroutine = null;
    }

    // --- [!! 수정됨 4.5: 1핑거 시퀀스 재생 (Ch1, Ch5, Ch6 매핑) !!] ---
    IEnumerator PlaySingleNote_OneFinger(OneFingerStimMode stimMode, int pressCh1, int pressCh5_Forearm, int releaseCh6, float pressDuration, float releaseDuration)
    {
        // 1. PRESS
        if (stimMode == OneFingerStimMode.PressRelease)
        {
            Debug.Log($"[RhythmTest] Sending to Channel: 1, Value: {pressCh1}, Duration: {pressDuration * 1000f:F0} ms");
            SendEmsCommand(pressCh1, 0, 0, 0, 0, 0, pressDuration); // Press (Ch1)
        }
        else if (stimMode == OneFingerStimMode.OnlyFinger)
        {
            Debug.Log($"[RhythmTest] Sending to Channel: 1, Value: {pressCh1}, Duration: {pressDuration * 1000f:F0} ms");
            SendEmsCommand(pressCh1, 0, 0, 0, 0, 0, pressDuration); // Press (Ch1)
        }
        else // FingerForearm, FingerForearmPress&Release
        {
            Debug.Log($"[RhythmTest] Sending to Channel: 1, Value: {pressCh1}, Duration: {pressDuration * 1000f:F0} ms");
            Debug.Log($"[RhythmTest] Sending to Channel: 5 (Forearm), Value: {pressCh5_Forearm}, Duration: {pressDuration * 1000f:F0} ms");
            SendEmsCommand(pressCh1, 0, 0, 0, pressCh5_Forearm, 0, pressDuration); // Press (Ch1 + Ch5)
        }
        yield return new WaitForSeconds(pressDuration);


        // 2. RELEASE (또는 쉼표)
        if (stimMode == OneFingerStimMode.FingerForearmPressRelease || stimMode == OneFingerStimMode.PressRelease)
        {
            // [!! 수정 !!] 릴리즈 200ms + 쉼표 300ms (총 500ms 쉼)
            Debug.Log($"[RhythmTest] Sending to Channel: 6 (Release), Value: {releaseCh6}, Duration: 200 ms");
            SendEmsCommand(0, 0, 0, 0, 0, releaseCh6, 0.2f); // 200ms 릴리즈
            yield return new WaitForSeconds(0.2f);

            SendEmsCommand(0, 0, 0, 0, 0, 0, 0.3f); // 300ms 끄기
            yield return new WaitForSeconds(0.3f);
        }
        else // OnlyFinger, FingerForearm 모드 (쉼표)
        {
            Debug.Log($"[RhythmTest] Resting for {releaseDuration * 1000f:F0} ms");
            SendEmsCommand(0, 0, 0, 0, 0, 0, releaseDuration); // Off
            if (releaseDuration > 0)
            {
                yield return new WaitForSeconds(releaseDuration);
            }
        }
    }


    // --- [!! 수정됨 5.1: 6채널 네트워크 함수 !!] ---
    void SendEmsCommand(int intensityCh1, int intensityCh2, int intensityCh3, int intensityCh4, int intensityCh5, int intensityCh6, float durationInSeconds = 0f)
    {
        if (!isConnected || writer == null)
        {
            Debug.LogWarning("SendEmsCommand: Not connected.");
            return;
        }

        int[] intensities = new int[] {
            Mathf.Clamp(intensityCh1, 0, 255), 
            Mathf.Clamp(intensityCh2, 0, 255),
            Mathf.Clamp(intensityCh3, 0, 255),
            Mathf.Clamp(intensityCh4, 0, 255),
            Mathf.Clamp(intensityCh5, 0, 255),
            Mathf.Clamp(intensityCh6, 0, 255) // Ch6 추가
        };

        int finalIntensity = 0;
        int finalChannel = 0;
        for (int i = 0; i < ChannelCount; i++) // ChannelCount는 6
        {
            if (intensities[i] > finalIntensity)
            {
                finalIntensity = intensities[i];
                finalChannel = i + 1;
            }
        }

        string[] ports = new string[6] {
            intensities[0].ToString("D3"),
            intensities[1].ToString("D3"),
            intensities[2].ToString("D3"),
            intensities[3].ToString("D3"),
            intensities[4].ToString("D3"),
            intensities[5].ToString("D3") // Ch6 값 사용
        };

        System.Text.StringBuilder commandBuilder = new System.Text.StringBuilder();
        foreach (string p in ports) commandBuilder.Append(p);
        commandBuilder.Append("\n");
        string command = commandBuilder.ToString();

        try
        {
            float durationInMs = durationInSeconds * 1000f; 


            Debug.Log($"[SendEmsCommand] Sending: {command.Trim()} | Duration: {durationInMs:F0} ms");
            emsLogger?.LogEmsCommand(finalChannel, finalIntensity, durationInMs);


            writer.Write(command);
            writer.Flush();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending command: {e.Message}");
            isConnected = false;
            UpdateStatus("Arduino Connection Lost!");
        }
    }

    void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = "Status: " + message;
        Debug.Log(message);
    }

    void LoadCalibrationData()
    {
        for (int i = 0; i < ChannelCount; i++) // ChannelCount는 6
        {
            int channel = i + 1;
            minPerceptions[i] = PlayerPrefs.GetInt($"MinPerception_Ch{channel}", -1);
            minActuations[i] = PlayerPrefs.GetInt($"MinActuation_Ch{channel}", -1);
            maxActuations[i] = PlayerPrefs.GetInt($"MaxActuation_Ch{channel}", -1);
        }
        Debug.Log("Loaded all 6-channel calibration data.");
        UpdateAllCalibrationUI();
    }
}