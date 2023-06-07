# VOSK ASR for Unity
This is a project that implements **ASR** (**A**utomatic **S**peech **R**ecognition) inside Unity via the Vosk libraries. 
This project was built and improved upon the [Sample](https://github.com/alphacep/vosk-unity-asr) linked on the official website. 

## What is VOSK
[**Vosk**]((https://alphacephei.com/vosk/)) is a speech recognition toolkit. Some advantages of Vosk include:
- Supports 20+ languages and dialects
- Works offline, even on lightweight devices
- Model comes in `~50 MB` size for Mobile & `~2 GB` size for Server
- Allows manual configuration of keywords for better accuracy

## Getting Started
- Download a language model from the [Official List](https://alphacephei.com/vosk/models)
- **Extract** the model to the `Application.streamingAssetsPath` of the project
- **(Optionally)** Install [Newtonsoft Json Unity Package](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html) 

## How to Use
- Inside the script that uses the ASR, add `using Vosk.APIs;`
- Call `VoskASR.Init` with the necessary arguments
- Subscribe to `VoskASR.OnTranscriptionResult` to process the results
- The results are sent in a Json format. Hence why `Newtonsoft.Json` is recommended.
- Refer to `Demo.cs` for examples