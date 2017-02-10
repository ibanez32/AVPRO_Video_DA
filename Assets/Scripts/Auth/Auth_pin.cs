using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using LitJsonSrc;
using UGS;
using UnityEngine;
using System.Collections;


public class Auth_pin : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

        //LoadLogin();
        DownloadFile();

    }
  private    void DownloadFile()
{
    WebClient client = new WebClient();
    string    _absolutPath = Application.persistentDataPath +"/Simple.sd.mp4";
    //client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler( DownloadFileCompleted );
    //client.DownloadFileAsync (new Uri ("http://player.vimeo.com/external/198854287.sd.mp4?s=6ca3c81cdd82759e8b38a63f37ce7e9b1164a306&profile_id=165&oauth2_token_id=434493122"), _absolutPath);
        //client.DownloadFileAsync ((new Uri ("http://ServerInfo.net/moviefile.mp4")), Application.persistentDataPath + "/" + "moviefile.mp4");
      client.DownloadFile("http://player.vimeo.com/external/198854287.sd.mp4?s=6ca3c81cdd82759e8b38a63f37ce7e9b1164a306&profile_id=165&oauth2_token_id=434493122", _absolutPath);
}

    void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error == null)
        {
           Debug.Log("YES");
        }
        else
        {
            Debug.Log(e.Error.Data);
        }
    }
    private void LoadLogin()
    {
        UGS.UgsClient m_client_auth = new UgsClient("https://beta.dropadverts.com/player/auth");
        UGS.Auth_PIN aut = m_client_auth.Auth_pin(ShowNetworkInterfaces());
        StartCoroutine(LoginAsynk(aut));
    }
    private IEnumerator LoginAsynk(UGS.Auth_PIN auth)
    {
        yield return StartCoroutine(auth.Process());
        if (!auth.HasError)
        {

            Debug.Log(auth.Result);
            JsonData response = LitJsonSrc.JsonMapper.ToObject(auth.Result);
            if (response.ContainsKey("errNo") && response["errNo"].AsInt() == 1)
            {
                LoadShedule();
            }
        }
        else
        {

        }
    }

    private void LoadShedule()
    {
        UGS.UgsClient m_client_shedule = new UgsClient("https://beta.dropadverts.com/player/get-schedule");
        UGS.Auth_PIN shedule = m_client_shedule.Auth_pin(ShowNetworkInterfaces());
        StartCoroutine(LoadASheduleAsynk(shedule));
    }
    private IEnumerator LoadASheduleAsynk(UGS.Auth_PIN shedule)
    {
        yield return StartCoroutine(shedule.Process());
        if (!shedule.HasError)
        {
            Debug.Log("Shedule");

            
            JsonData response = LitJsonSrc.JsonMapper.ToObject(shedule.Result);
            if (response.ContainsKey("data"))
            {
                IDictionary tdMediasIdList = response["data"]["schedule"]["mediasIdList"] as IDictionary;
                IDictionary tdMedias = response["data"]["medias"] as IDictionary;
                Debug.Log(response["data"]["schedule"]["mediasIdList"].Count);
           
                int i = 0;
                foreach (string VARIABLE in tdMediasIdList.Keys)
                {
                    IDictionary ItemClip = tdMedias[tdMediasIdList[VARIABLE].ToString()] as IDictionary;
                    ItemDataschedule newDataschedule = new ItemDataschedule
                    {
                        number = i,
                        TimeStart = VARIABLE,
                        id = tdMediasIdList[VARIABLE].ToString(),
                        duration = ItemClip["duration"].ToString(),
                        PathLoad = ItemClip["path"].ToString(),
                        PathLocal = null,

                    };
                   DataSchedule.Instance.addItemDataSchedule(newDataschedule);
                    i++;
                   // Debug.Log(VARIABLE + "   " + tdMediasIdList[VARIABLE] + "   " + ItemClip["path"]);
                }
                DataSchedule.Instance.PrintDataSchedule();
            }
            else
            {
                Debug.Log("No");
            }
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public string ShowNetworkInterfaces()
    {
       
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = null;
        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();

            string mac = null;
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + ":");
                }
            }
            info += mac + "\n";

            info += "\n";
        }
        Debug.Log("MAC=  " + info);
        return info;
    }

}
