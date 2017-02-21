using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
	yield return   new WaitForSeconds(2f);
    SceneManager.LoadScene("DA_player");
	   // StartCoroutine(Load());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator Load()
    {
        AsyncOperation async = Application.LoadLevelAdditiveAsync("DA_player");
        yield return async;
    }
}
