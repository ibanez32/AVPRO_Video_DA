using System.Collections;
using UnityEngine;

public class Network
{
    private static Network _instance;
    public static Network Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Network();
            return _instance;
        }
    }

    private const string SERVER_PATH = "https://beta.dropadverts.com/player/auth";

    public IEnumerator SendRequest(NetRequest request)
    {
        Debug.Log(string.Concat(SERVER_PATH, "?", request.GetParamsString()));
        var www = new WWW(string.Concat(SERVER_PATH, "?", request.GetParamsString()));
        while (!www.isDone)
            yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            //Logger.Log("[Network] SendRequest ERROR:" + www.error);
            yield return new NetResponse("error=" + www.error);
            yield break;
        }

        //Logger.Log("[Network] input data:{0}", www.text);
        yield return new NetResponse(www.text);
    }

    /// <summary>
    /// Обновляем данные
    /// </summary>
    public IEnumerator SendUpdateInfo(int points)
    {
        var request = new NetRequest(PacketTypes.updateinfo);
        request.AddParam("points", points);

        return SendRequest(request).ContinueWith(response =>
        {
            if (response.IsError)
            {
                //Logger.Log("SendUpdateInfoError:", response.GetError);
            }
            else
            {
                //OK
            }
        });

    }
}