using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Boot splash / loading scene. Displays for a minimum time (and waits for the target scene to
/// finish loading) then switches to the Main Menu. Animated "LOADING..." dots. No buttons.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public string nextScene = "MainMenu";
    public float minDisplayTime = 2.5f;
    public TMP_Text loadingLabel;
    public string loadingBase = "LOADING";

    IEnumerator Start()
    {
        Time.timeScale = 1f;
        float t = 0f;

        var op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (t < minDisplayTime || op.progress < 0.9f)
        {
            t += Time.unscaledDeltaTime;
            if (loadingLabel != null)
            {
                int dots = Mathf.FloorToInt(Time.unscaledTime * 3f) % 4;
                loadingLabel.text = loadingBase + new string('.', dots);
            }
            yield return null;
        }

        op.allowSceneActivation = true;
    }
}
