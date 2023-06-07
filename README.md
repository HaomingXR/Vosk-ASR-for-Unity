# VOSK ASR for Unity
[中文|[English](README_EN.md)]

這是一個透過 Vosk 實作 語音轉文字 的 Unity 專案，改善自官網上的[範例](https://github.com/alphacep/vosk-unity-asr)。

## VOSK 是什麼
**[Vosk](https://alphacephei.com/vosk/)** 是一個語音辨識的套件。它的優點包含:
- 支援 20+ 語言與方言
- 可離線使用，甚至在輕型裝置上
- 有適合行動裝置的 `~50 MB` 小型模型 ； 以及適合伺服器的 `~2 GB` 大型模型
- 可以手動設定單字以提升準確度

## 準備
- 從[官網](https://alphacephei.com/vosk/models)下載一個模型 
    - 本專案已包含一個中文模型
- 把模型**解壓縮**至專案的 `Application.streamingAssetsPath`
- 大的模型較為準確，但需要更長時間進行載入
- **(推薦)** 安裝 [Newtonsoft Json Unity Package](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html) 

## 如何使用
- 在需要使用 ASR 的程式中加入 `using Vosk.APIs;`
- 呼叫 `VoskASR.Init` 
- 訂閱 `VoskASR.OnTranscriptionResult` 以獲取辨識結果
- 回傳的結果是 Json 格式，故推薦使用 `Newtonsoft.Json`
- 可參考 `Demo.cs` 的範例
- 可以使用 `LoudnessMeter` 來視覺化輸入音量
- 可以使用 `ChineseUtil` 來進行簡體與繁體的轉換
    - 中文模型大多為簡體

### 參數
- **`caller`:** 傳入 `MonoBehaviour` 讓 Unity 呼叫 `StartCoroutine`
- **`modelName`:** 傳入模型的資料夾名稱
- **`autoStart`:** 使否在呼叫 `Init` 後即開始辨識
- **`maxAlternatives`:** 該辨識幾組可能結果
- **`microphoneIndex`:** 麥克風的序號
- **`keyPhrases`:** 手動輸入特定單字來辨識