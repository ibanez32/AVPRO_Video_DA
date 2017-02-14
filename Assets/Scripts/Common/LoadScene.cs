using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
	yield return   new WaitForSeconds(2f);
    SceneManager.LoadScene("Demo_imGui");
	   // StartCoroutine(Load());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator Load()
    {
        AsyncOperation async = Application.LoadLevelAdditiveAsync("Demo_imGui");
        yield return async;
    }
}
