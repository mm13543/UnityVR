using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    // Replace these with your actual Play.ht API Key and User ID
    private const string ApiKey = "34ded232961b4ee2a332175f5623b410";
    private const string UserId = "m35aIk3Bn7UcpDtHk8P2eihq2KJ2";
    private const string PlayHtEndpoint = "https://play.ht/api/v1/convert";

    public void MakeAudioRequest(string message)
    {
        StartCoroutine(RequestAudioFromPlayHT(message));
    }

    private IEnumerator RequestAudioFromPlayHT(string message)
    {
        // Construct the JSON payload
        var payload = new
        {
            content = new[] { message },
            voice = "en_us_male",
            speed = 1.0
        };
        string jsonData = JsonUtility.ToJson(payload);

        // Set up the UnityWebRequest for the Play.ht API
        using (UnityWebRequest www = new UnityWebRequest(PlayHtEndpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {ApiKey}");
            www.SetRequestHeader("X-User-ID", UserId);

            // Send the request and wait for the response
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending request: {www.error}");
            }
            else
            {
                // Parse the JSON response to extract the audio URL
                var responseJson = www.downloadHandler.text;
                var audioUrl = ParseAudioUrlFromResponse(responseJson);

                if (!string.IsNullOrEmpty(audioUrl))
                {
                    // Download and play the audio from the extracted URL
                    yield return StartCoroutine(DownloadAndPlayAudio(audioUrl));
                }
                else
                {
                    Debug.LogError("Failed to retrieve audio URL from response.");
                }
            }
        }
    }

    private IEnumerator DownloadAndPlayAudio(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading audio: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    private string ParseAudioUrlFromResponse(string jsonResponse)
    {
        // Implement JSON parsing logic to extract the 'audio_url' from Play.ht API response
        // You may use a third-party JSON library like Newtonsoft.Json if needed
        // Example:
        // var jsonObj = JsonUtility.FromJson<YourResponseModel>(jsonResponse);
        // return jsonObj.audio_url;

        // Placeholder for actual implementation
        return "extracted_audio_url_from_json";
    }
}