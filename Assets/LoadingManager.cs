using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Animator animator { get { return GetComponent<Animator>(); } }

    public Slider progressBar;
    public Text stateText, percentsText;

    AsyncOperation op;

    public void LoadScene()
    {
        gameObject.SetActive(true);
        StartCoroutine(ILoading());
    }

    public IEnumerator ILoading()
    {
        animator.Play("SceneLoading");

        yield return new WaitForSeconds(20f / 60f);

        op = SceneManager.LoadSceneAsync("ModernEditor", LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            progressBar.value = op.progress;
            stateText.text = "Loading..";

            float percents = Mathf.RoundToInt(op.progress * 100f * 10f) / 10f;
            percentsText.text = percents + "%";

            yield return new WaitForEndOfFrame();
        }

        yield return ProjectManager.LoadAudioClip(LCData.project, (AudioClip clip) => { LCData.audioClip = clip; });

        OnLoaded();
    }
    public void OnLoaded()
    {
        StartCoroutine(ILoaded());
    }

    public IEnumerator ILoaded()
    {
        stateText.text = "Loaded";
        percentsText.text = "100%";
        progressBar.value = progressBar.maxValue;

        yield return new WaitForEndOfFrame();

        op.allowSceneActivation = true;
    }
}
