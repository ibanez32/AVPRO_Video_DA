using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using LitJsonSrc;
using UGS;
 class GetPinState : State
{
    private MonoBehaviour Context;
    internal GetPinState(MonoBehaviour context)
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
        }
    }

    private void SetState()
    {
        StateController.Instance.Canvas.SetActive(false);
        LoadLogin();
        
    }
    private void LoadLogin()
    {
        UGS.UgsClient m_client_auth = new UgsClient("https://beta.dropadverts.com/player/auth");
        UGS.Auth_PIN aut = m_client_auth.Auth_pin(ShowNetworkInterfaces());
        Context.StartCoroutine(LoginAsynk(aut));
    }
    private IEnumerator LoginAsynk(UGS.Auth_PIN auth)
    {
        yield return Context.StartCoroutine(auth.Process());
        if (!auth.HasError)
        {

            Debug.Log(auth.Result);
            JsonData response = LitJsonSrc.JsonMapper.ToObject(auth.Result);
            if (response.ContainsKey("errNo") && response["errNo"].AsInt() == 1)
            {
                StateController.Instance.Canvas.SetActive(true);
                StateController.Instance.PIN_text.text ="PIN Ok!";
                StateController.Instance.ChangeState(Mark.GetSchedule);
            }
             if (response.ContainsKey("errNo") && response["errNo"].AsInt() == 0&&response.ContainsKey("code"))
            {
                StateController.Instance.Canvas.SetActive(true);
                string label = null;
                foreach (char c in response["code"].AsInt().ToString())
                {
                    label += c + " ";
                }
                //StateController.Instance.PIN_text.text = response["code"].AsInt().ToString();
                StateController.Instance.PIN_text.text = label;
                Context.StartCoroutine(RepeatRequest());

            }
        }
        else
        {
            
        }
    }

     private IEnumerator RepeatRequest()
     {
         yield return new WaitForSeconds(15f);
         LoadLogin();
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
        Debug.Log("MAC=  " + mac);
        return mac;
    }
}
