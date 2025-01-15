using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator anim;
    public float transitionTime = 3f;
    float sceneDelay = 1f;
    // Update is called once per frame


    public void LoadNextLevel()
    {
        StartCoroutine("CrossFadeTransition", transitionTime);
    }

    IEnumerator CrossFadeTransition(float delay)
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
