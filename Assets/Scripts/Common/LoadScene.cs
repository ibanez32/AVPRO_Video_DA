using System.Net.NetworkInformation;
using LitJsonSrc;
using UGS;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public GameObject splash;
    public GameObject pin_overlay;
    public Text PIN_text;
	// Use this for initialization
	IEnumerator Start () {
        splash.SetActive(true);
	yield return   new WaitForSeconds(2f);
	    LoadLogin();
	    //SceneManager.LoadScene("DA_player");

	}
	
	// Update is called once per frame
	void Update () {
	
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
                splash.SetActive(false);
                pin_overlay.SetActive(true);
                PIN_text.text = "PIN Ok!";
                SceneManager.LoadScene("DA_player");
                
            }
            if (response.ContainsKey("errNo") && response["errNo"].AsInt() == 0 && response.ContainsKey("code"))
            {
                splash.SetActive(false);
                pin_overlay.SetActive(true);
                string label = null;
                foreach (char c in response["code"].AsInt().ToString())
                {
                    label += c + " ";
                }
                //StateController.Instance.PIN_text.text = response["code"].AsInt().ToString();
              PIN_text.text = label;
                StartCoroutine(RepeatRequest());

            }
        }
        else
        {
            LoadLogin();
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
        //   Debug.Log("MAC=  " + mac);
        return mac;
    }
    
}
