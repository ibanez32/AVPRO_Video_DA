using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using SimpleFirebaseUnity;
using SimpleFirebaseUnity.MiniJSON;
using UnityEngine;
using System.Collections;

public class FB_Controller : MonoBehaviour {
    private string datebaseUrl;

    private Firebase action;

    // Use this for initialization
    void Awake()
    {
        datebaseUrl = "dropadverts-148909.firebaseio.com/player/" + ShowNetworkInterfaces();
      
        StartCoroutine(Tests());
    }
    public string ShowNetworkInterfaces()
    {
        
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string mac = null;
        foreach (NetworkInterface adapter in nics)
        {

            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
              //  Debug.Log("Adapter=" + adapter.NetworkInterfaceType);

                byte[] bytes = address.GetAddressBytes();


                for (int i = 0; i < bytes.Length; i++)
                {
                    mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                    if (i != bytes.Length - 1)
                    {
                        mac = string.Concat(mac + ":");
                    }
                }
                break;
            }


        }
        // Debug.Log("MAC=  " + mac);
        return mac;
    }
    void GetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Get from key: <" + sender.FullKey + ">");
        DoDebug("[OK] Raw Json: " + snapshot.RawJson);

        Dictionary<string, object> dict = snapshot.Value<Dictionary<string, object>>();
        List<string> keys = snapshot.Keys;

        if (keys != null)
            foreach (string key in keys)
            {
                if (key=="sync")
                {
                    if (dict[key].ToString() == "DO")
                    {
                        Debug.Log("DO");
                        //StateController.Instance.ChangeState(Mark.GetSchedule);
                        StateControllerAVPro.Instance.ChangeState(Mark.GetSchedule);
                    }
                }
                DoDebug(key + " = " + dict[key].ToString());
            }
    }
   
    void GetFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Get from key: <" + sender.FullKey + ">,  " + err.Message + " (" + (int)err.Status + ")");
    }

    void SetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Set from key: <" + sender.FullKey + ">");
    }

    void SetFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Set from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void UpdateOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Update from key: <" + sender.FullKey + ">");
    }

    void UpdateFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Update from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void DelOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Del from key: <" + sender.FullKey + ">");
    }

    void DelFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Del from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void PushOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Push from key: <" + sender.FullKey + ">");
    }

    void PushFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Push from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void GetRulesOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] GetRules");
        DoDebug("[OK] Raw Json: " + snapshot.RawJson);
    }

    void GetRulesFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] GetRules,  " + err.Message + " (" + (int)err.Status + ")");
    }

    void GetTimeStamp(Firebase sender, DataSnapshot snapshot)
    {
        long timeStamp = snapshot.Value<long>();
        DateTime dateTime = Firebase.TimeStampToDateTime(timeStamp);

        DoDebug("[OK] Get on timestamp key: <" + sender.FullKey + ">");
        DoDebug("Date: " + timeStamp + " --> " + dateTime.ToString());
    }

    void DoDebug(string str)
    {
        Debug.Log(str);
       
    }

    IEnumerator Tests()
    {
       
        Firebase firebase = Firebase.CreateNew(datebaseUrl, "P860mYzzDiNtxNjlD6O1B5m9vMgaNocYyKUGK4et");

        // Init callbacks
        firebase.OnGetSuccess += GetOKHandler;
        firebase.OnGetFailed += GetFailHandler;
        firebase.OnSetSuccess += SetOKHandler;
        firebase.OnSetFailed += SetFailHandler;
        firebase.OnUpdateSuccess += UpdateOKHandler;
        firebase.OnUpdateFailed += UpdateFailHandler;
        firebase.OnPushSuccess += PushOKHandler;
        firebase.OnPushFailed += PushFailHandler;
        firebase.OnDeleteSuccess += DelOKHandler;
        firebase.OnDeleteFailed += DelFailHandler;

        // Get child node from firebase, if false then all the callbacks are not inherited.
         action = firebase.Child("action", true);
        
       FirebaseObserver observer = new FirebaseObserver(action, 1f);
       observer.OnChange += (Firebase sender, DataSnapshot snapshot) =>
       {
           DoDebug("[OBSERVER] Last updated changed to: " + snapshot.Value<long>());
           action.GetValue();
       };
       observer.Start();
      

        // Unnecessarily skips a frame, really, unnecessary.
        yield return null;
       // action.GetValue();
        //StateController.Instance.StartController();
        StateControllerAVPro.Instance.StartController();
        
   
    }

    public void SetActionDONE()
    {
        // Create a FirebaseQueue
        FirebaseQueue firebaseQueue = new FirebaseQueue();


        firebaseQueue.AddQueueUpdate(action, "{\"sync\": \"DONE\"}", true);
    }
    
}