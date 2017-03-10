using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;

public class TestLaderPlaylist : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
        //StartCoroutine(LoadPlayList());
	    StartCoroutine(LoadPlayList((isend) =>
	    {
	        if (isend)
	        {
	            Debug.Log("Yes");
	        }
	        else
	        {
	            Debug.Log("NO");
	        }

	    }));
	}

    
    IEnumerator LoadPlayList(Action <bool> isRun)
    {
       // string url = "https://skyfire.vimeocdn.com/1488813432-0x9e92890c2f06c95a4077ed755f27ff19591cec23/201943867/video/683769692/playlist.m3u8";
        string url = "https://player.vimeo.com/external/198854287.m3u8?s=968d75769538eafda055e69ccd113b5494011639&oauth2_token_id=434493122";
        //  _absolutPath = GetAbsolutPath(DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);

        WWW www_test = new WWW(url);

        while (false == www_test.isDone)
        {


            yield return null;

        }
        if (www_test.error == null)
        {
            //Debug.Log(www_test.text);
             Regex regex = new Regex(@"(#EXT.*?)\n([https://].*?)\/(\w+.m3u8)");
             Match match = regex.Match(www_test.text);
            string path = null;
             while (match.Success)
             {
                 // Т.к. мы выделили в шаблоне одну группу (одни круглые скобки),
                 // ссылаемся на найденное значение через свойство Groups класса Match
                
                 path = match.Groups[2].Value + "/" + match.Groups[3].Value;
                 // Переходим к следующему совпадению
                 match = match.NextMatch();
             }
             Debug.Log(path);
           // WriteStringToFile(www_test.text, "playlist");
        }
        
        else
        {

        }
        isRun (true);
    }
    IEnumerator LoadPartVideo()
    {
        string url = "";
        // (\b(RESOLUTION=)\b[0-9]+[x][0-9]+) \bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)
        //  _absolutPath = GetAbsolutPath(DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
        for (int i = 1; i < 605; i++)
        {
            url = "https://skyfire.vimeocdn.com/1488813432-0x9e92890c2f06c95a4077ed755f27ff19591cec23/201943867/video/683769692/chop/segment-"+i.ToString()+".ts";
            WWW www_test = new WWW(url);

            while (false == www_test.isDone)
            {


                yield return null;

            }
            if (www_test.error == null)
            {
                Debug.Log(www_test.text);
                string path = Application.streamingAssetsPath + "/chop/segment-" + i.ToString() + ".ts";
                File.WriteAllBytes(path,www_test.bytes);
               // WriteStringToFile(www_test.text, "playlist");
            }
            else
            {

            }
        }
        
    }
	// Update is called once per frame
	void Update () {
	
	}
    public void WriteStringToFile(string str, string filename)
    {
#if !WEB_BUILD
        // string path = PathForDocumentsFile(filename);
        string path = Application.streamingAssetsPath + "/" + filename+".m3u8";
        if (File.Exists(path))
        {
            //File.Delete(path);
        }
        FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);

        StreamWriter sw = new StreamWriter(file);
        sw.WriteLine(str);

        sw.Close();
        file.Close();
#endif
    }
}
