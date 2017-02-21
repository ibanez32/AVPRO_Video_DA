using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using LitJsonSrc;
using UGS;

 class GetSchedule : State {

	 private MonoBehaviour Context;
     internal GetSchedule(MonoBehaviour context)
    {
        this.Context = context;
        SetState();
    }

     protected override void ChangeState(Context context, Mark mark)
     {
         switch (mark)
         {
             case Mark.GetSchedule:
                 {
                     context.State = new GetSchedule(Context);
                     break;

                 }
             case Mark.GetPin:
             {
                 context.State=new GetPinState(Context);
                 break;
             }
         }
     }
     private void SetState()
     {

         LoadShedule();
     }
     private void LoadShedule()
     {
         UGS.UgsClient m_client_shedule = new UgsClient("https://beta.dropadverts.com/player/get-schedule");
         UGS.Auth_PIN shedule = m_client_shedule.Auth_pin(ShowNetworkInterfaces());
        Context.StartCoroutine(LoadASheduleAsynk(shedule));
     }
     private IEnumerator LoadASheduleAsynk(UGS.Auth_PIN shedule)
     {
         yield return Context.StartCoroutine(shedule.Process());
         if (!shedule.HasError)
         {
             Debug.Log("Shedule="+shedule.Result);
            StateController.Instance.StopSelectNumberClip();
           StateController.Instance.SetstopDowloadMovie(true);
            // StateController.Instance.StopDeleteClip();
             //StateController.Instance.StoptDowloadMoive();
             DataSchedule.Instance.ClearDataschedules();
       
             
             JsonData response = LitJsonSrc.JsonMapper.ToObject(shedule.Result);
             if (response.ContainsKey("data"))
             {

                 if (response["data"]["schedule"].ContainsKey("mediasIdList"))
                 {
                     IDictionary tdMediasIdList = response["data"]["schedule"]["mediasIdList"] as IDictionary;
                     IDictionary tdMedias = response["data"]["medias"] as IDictionary;
                     StateController.Instance.GetMediasSchedule().Clear();
                     foreach (var item in tdMedias.Keys)
                     {
                         IDictionary I_Clip = tdMedias[item] as IDictionary;
                         StateController.Instance.GetMediasSchedule().Add(I_Clip["id"].ToString());
                        

                     }
                 //    Debug.Log(response["data"]["schedule"]["mediasIdList"].Count);

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
                             isLocal = false,

                         };
                         DataSchedule.Instance.addItemDataSchedule(newDataschedule);
                         i++;
                         // Debug.Log(VARIABLE + "   " + tdMediasIdList[VARIABLE] + "   " + ItemClip["path"]);
                     }
                  //  foreach (ItemDataschedule itemDataschedule in DataSchedule.Instance.GetDataschedules())
                  //  {
                  //      string path = Application.persistentDataPath + "/" + itemDataschedule.id.ToString() + ".mp4";
                  //      if (File.Exists(path))
                  //      {
                  //          itemDataschedule.PathLocal = path;
                  //      }
                  //  }

                     // DataSchedule.Instance.PrintDataSchedule();

                     StateController.Instance.SetAction();
                   
                     StateController.Instance.Canvas.SetActive(false);
                     StateController.Instance.SetCurrentClip(-1);
                     StateController.Instance.SetIsFirstDowload(true);
                     //StateController.Instance.PrepareMediasList();
                    
                    StateController.Instance.StartSelectNumberClip();
                   // StateController.Instance.SetSelectedClip(true);
                   //  StateController.Instance.StartDowloadMoive();
                 }
                 else
                 {
                     StateController.Instance.StopPlayer();
                     StateController.Instance.SetAction();
                     StateController.Instance.Canvas.SetActive(false);
                 }
                 
                 
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
 }
