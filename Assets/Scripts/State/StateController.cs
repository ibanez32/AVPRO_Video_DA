using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using LitJsonSrc;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;
using UGS;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class StateController : SingletonBehaviour<StateController>
{
    public FB_Controller FB_datebase;
    public UMP_Controller ControllerVP;
    public GameObject Canvas;
    public Text PIN_text;
   // private Context ContextState;
    private int CurrentNumberClip;
    //Dowload Movi
    private List<string> mediasSchedule;
    private List<string> saved_medias;
    private List<string> saving_medias;
    private List<string> prepare_medias;
    private List<string> delete_medias;
    private int numberDowloadClip;
    private int numberCurrentDowloadClip;
    
   private int totalDownloaded;
   
    private int contentLength;
    private int ItemSizeLoad;
    private FileStream filestream;
    private WWW www_test;
    private Stream stream=null;
    private string _absolutPath;
    private bool isFirstDowloadClip;
    private WaitForSeconds waitForSeconds;
    private WaitForSeconds waitForSecondsGC;
    private bool StopDowloadMoive;
    private bool isDowloadMovie;
    private bool isWWW;
    private bool isDeleteMovie;
    private bool SelectedClip;
    
    // Use this for initialization
    void Start()
    {
        isWWW = false;
        SelectedClip = false;
        StopDowloadMoive = false;
        isDowloadMovie = false;
        waitForSeconds = new WaitForSeconds(.01f);
        waitForSecondsGC=new WaitForSeconds(60f);
        isFirstDowloadClip = false;
        mediasSchedule = new List<string>();
        saved_medias = new List<string>();
        prepare_medias = new List<string>();
        delete_medias = new List<string>();
        saving_medias = new List<string>();
        //StartCoroutine(_CoroutinaGC());
    }

    // Update is called once per frame
    void Update()
    {
       // SelectedNumberClip();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        //if (Time.frameCount % 30 == 0)
        //{
        //    System.GC.Collect();
        //}
        //-------Select time interval

        //-------END Select time interval


        //------Writing Movi


        //------End Writing Movi
    }

    public void SetSelectedClip(bool set)
    {
        SelectedClip = set;
    }
    public void SetstopDowloadMovie(bool set)
    {
        StopDowloadMoive = set;
        
    }

    public bool GetIsDowloadMovie()
    {
        return isDowloadMovie;
    }
    public void StartController()
    {
       
        numberDowloadClip = 0;
      
        CurrentNumberClip = -1;
        //ContextState = new Context(new GetPinState(this));
        LoadShedule();
        
    }

    public void StopPlayer()
    {
        ControllerVP._mediaPlayer.Stop();
    }
    public void SetCurrentClip(int num)
    {
        CurrentNumberClip = num;
    }

    public int GetCurrentClip()
    {
        return CurrentNumberClip;
    }
    public void SetAction()
    {
        FB_datebase.SetActionDONE();
    }
    public void ChangeState(Mark mark)
    {
       // ContextState.FindOut(mark);
        if (mark==Mark.GetSchedule)
        {
            LoadShedule();
        }
    }
    //--------Select Number Clip
    public void StartSelectNumberClip()
    {
        StartCoroutine(_CoroutinaSelectNUmberClip());
    }

    public void StopSelectNumberClip()
    {
        StopCoroutine(_CoroutinaSelectNUmberClip());
    }

    public void SetIsFirstDowload(bool isDow)
    {
        isFirstDowloadClip = isDow;
    }

    public bool GetIsFirstDowloadClip()
    {
        return isFirstDowloadClip;
    }

    public void SelectedNumberClip()
    {
        if (SelectedClip&&DataSchedule.Instance.GetDataschedules().Count > 0)
        {
            DateTime localDate = DateTime.Now;
            int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
            var Item = DataSchedule.Instance.GetDataschedules()
            .Find(
                elm =>
                    Int32.Parse(elm.TimeStart) <= mSec &&
                    (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);
            int offset = mSec - Int32.Parse(Item.TimeStart);
                
            if (DataSchedule.Instance.GetDataschedules().IndexOf(Item) >= 0)
            {
                CurrentNumberClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
                // SetIsFirstDowload(true);
                string pathLoad = null;
                //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
                if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal))
                {
                    pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                    ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
                }
                else
                {
                    pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                    ControllerVP.ReplacePlayVideo(pathLoad, offset, true);
                }
                            
               
                Debug.Log("Path" + pathLoad);
                Debug.Log("offset" + offset);
                SelectedClip = false;
            }

            
        }
    }

    IEnumerator _CoroutinaGC()
    {
        GC.Collect();
        Debug.Log("+++++++++++GC");

        yield return waitForSecondsGC;
        StartCoroutine(_CoroutinaGC());
    }
    IEnumerator _CoroutinaSelectNUmberClip()
    {
      
        for (; ; )
        {
            
            if (DataSchedule.Instance.GetDataschedules().Count > 0)
            {
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);

                int offset = 0;
                if (Item!=null)
                {
                    offset = mSec - Int32.Parse(Item.TimeStart);
                }
                if (DataSchedule.Instance.GetDataschedules().IndexOf(Item) >= 0&&DataSchedule.Instance.GetDataschedules().IndexOf(Item) != CurrentNumberClip)
                {
                    Debug.Log("Enter");
                    if (CurrentNumberClip==-1)
                    {
                        Debug.Log("Enter2");
                        //SetIsFirstDowload(true);
                        if (!isDowloadMovie)
                        {
                            Debug.Log("Enter3");
                            PrepareMediasList();
                            CurrentNumberClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
                            string pathLoad = null;
                            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
                            if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal))
                            {
                                pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                                ControllerVP.ReplacePlayVideo(pathLoad, offset,false);
                            }
                            else
                            {
                                pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                               ControllerVP.ReplacePlayVideo(pathLoad, offset,true);
                            }
                            
                            
                            Debug.Log("Path" + pathLoad);
                           
                          //  Debug.Log("offset" + offset);
                        }
                       
                    }
                    else
                    {
                        CurrentNumberClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
                        string pathLoad = null;
                        //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
                        if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal))
                        {
                            pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                            DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                            ControllerVP.ReplacePlayVideo(pathLoad, offset,false);
                            
                        }
                        else
                        {
                            pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                            DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                            ControllerVP.ReplacePlayVideo(pathLoad, offset,true);
                           
                        }
                        
                       
                        Debug.Log("Path" + pathLoad);
                        Debug.Log("offset" + offset);
                    }

                    
                }
                if (DataSchedule.Instance.GetDataschedules().IndexOf(Item) < 0&&isFirstDowloadClip)
                {
                    if (ControllerVP._mediaPlayer.IsPlaying)
                    {
                        StopPlayer();
                    }
                   
                    if (!isDowloadMovie)
                    {
                        isFirstDowloadClip = false;
                        PrepareMediasList();
                        SetstopDowloadMovie(false);
                        StartDeleteClip();
                    }
                    
                }

                //Debug.Log("Current" + mSec );
                //  Debug.Log("Current" + mSec + "     TimeStart=" + Item);
                //   Debug.Log("Current" + mSec +  "    index=" + Item.id);
            }
            yield return waitForSeconds;
        }
        
       
    }
    //--------End Select Number Clip
    //----------------- Dowload movi
    public List<string> GetMediasSchedule()
    {
        return mediasSchedule;
    }
    public void PrepareMediasList()
    {

       // StopCoroutine(_CroutineDownloadMoive());
       //
       saved_medias.Clear();
       prepare_medias.Clear();
       delete_medias.Clear();
       saving_medias.Clear();
        if (!String.IsNullOrEmpty(ReadStringFromFile("SavedMediaList")))
        {
            JsonData response = JsonMapper.ToObject(ReadStringFromFile("SavedMediaList"));
            IList list = response as IList;
            for (int i = 0; i < list.Count; i++)
            {

                saved_medias.Add(list[i].ToString());
            }
        }
       // Debug.Log("SavedMediaList  ----------------------------");


      //  foreach (var VARIABLE in saved_medias)
      //  {
      //      Debug.Log(VARIABLE);
      //  }
     //   Debug.Log("MediasSchedule  ----------------------------");
     //
     //
     //   foreach (var VARIABLE in mediasSchedule)
     //   {
     //       Debug.Log(VARIABLE);
     //   }
     //   Debug.Log("Saving  ----------------------------");
        var intersect_medias = mediasSchedule.Intersect(saved_medias);
        foreach (string media in intersect_medias)
        {
            saving_medias.Add(media);
        }
    //   foreach (var VARIABLE in saving_medias)
    //   {
    //       Debug.Log(VARIABLE);
    //   }
     //   Debug.Log("Delete  ----------------------------");
        var except_medias = saved_medias.Except(saving_medias);
        foreach (string media in except_medias)
        {
            delete_medias.Add(media);
            //Debug.Log(media);
        }
     //   IEnumerable<string> distinctAges = delete_medias.Distinct();

       // delete_medias.Clear();

      //  foreach (string media in distinctAges)
      //  {
      //      delete_medias.Add(media);
      //  }
     //   Debug.Log("+++++++++++  ----------------------------");
     //   foreach (var VARIABLE in distinctAges)
     //   {
      //      Debug.Log(VARIABLE);

    //    }
        string str = JsonMapper.ToJson(saving_medias);

        WriteStringToFile(str, "SavedMediaList");
     //   Debug.Log("Prepare  ----------------------------");
        var except_prepare = mediasSchedule.Except(saving_medias);
        foreach (string media in except_prepare)
        {
            prepare_medias.Add(media);
        }
     //   foreach (var VARIABLE in prepare_medias)
      //  {
     //       Debug.Log(VARIABLE);
     //   }
        //-------- writing to DataClip
        foreach (ItemDataschedule item in DataSchedule.Instance.GetDataschedules())
        {
            item.PathLocal = null;
        }
        foreach (string media in saving_medias)
        {
            foreach (ItemDataschedule item in DataSchedule.Instance.GetDataschedules())
            {
                if (item.id == media)
                {
                   // item.PathLocal = GetAbsolutPath(media);
                    item.PathLocal = item.id+".mp4";
                }
            }
        }
         //numberCurrentDowloadClip++;
       
        
        
        
       // CurrentNumberClip = -1;
        // DataSchedule.Instance.PrintDataSchedule();
        //StartDeletingMoive();
    }
    public void StartDowloadMoive()
    {
        if (prepare_medias.Count()>0)
        {
            isDowloadMovie = true;
            DateTime localDate = DateTime.Now;
            int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
            var Item = DataSchedule.Instance.GetDataschedules()
            .Find(
                elm =>
                    Int32.Parse(elm.TimeStart) <= mSec &&
                    (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);
            numberCurrentDowloadClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
            numberDowloadClip = 0;
            if (numberCurrentDowloadClip < 0)
            {
                numberCurrentDowloadClip = 0;

            }
            if (numberCurrentDowloadClip == DataSchedule.Instance.GetDataschedules().Count)
            {
                numberCurrentDowloadClip = 0;
            }
           // Debug.Log("   number=" + numberCurrentDowloadClip);
          //  Debug.Log("_________ mSec=" + mSec + "   number=" + numberCurrentDowloadClip + "   start" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].TimeStart + "   id=" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
       
            StartCoroutine(_CroutineDownloadMoive());
        }
        else
        {
            Debug.Log("NO DOWLOAD CLIP");
            if (ControllerVP._mediaPlayer.IsPlaying && !DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal && DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal != null)
            {
                Debug.Log("Point_2");
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                string pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].TimeStart);
                //ControllerVP.ReplacePlayVideo(pathLoad, offset,false);
            }
            
        }
        

    }
    public void StoptDowloadMoive()
    {
        StopCoroutine(_CoroutineWritingMovi());
        StopCoroutine(_CroutineDownloadMoive());

        if (filestream != null && filestream.CanWrite)
        {
            filestream.Close();
        }
        if (stream != null && stream.CanRead)
        {
            stream.Close();
        }
       

    }

    public void CheckContinueDowloadMoive()
    {
       Debug.Log("Start CheckContinueDowloadMoive");
        if (!StopDowloadMoive)
        {
            
          
                //DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal = _absolutPath;
                DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + ".mp4";
                saving_medias.Add(DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
                string str = JsonMapper.ToJson(saving_medias);
                WriteStringToFile(str, "SavedMediaList");

                foreach (ItemDataschedule itemDataschedule in DataSchedule.Instance.GetDataschedules())
                {
                    if (itemDataschedule.id == DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id)
                    {
                        //itemDataschedule.PathLocal = _absolutPath;
                        itemDataschedule.PathLocal = itemDataschedule.id + ".mp4";
                    }
                }
                //   Debug.Log("Point_1");

                if (ControllerVP._mediaPlayer.IsPlaying && !DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal && DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal != null)
                {
                    // Debug.Log("Point_2");
                    DateTime localDate = DateTime.Now;
                    int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                    string pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;

                    int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].TimeStart);
                    //ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
                }
       
            
            
            numberDowloadClip++;
            numberCurrentDowloadClip++;
            if (numberCurrentDowloadClip == DataSchedule.Instance.GetDataschedules().Count)
            {
                numberCurrentDowloadClip = 0;
            }
            while (DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal != null && numberDowloadClip < DataSchedule.Instance.GetDataschedules().Count)
            {
                numberDowloadClip++;
                numberCurrentDowloadClip++;
                if (numberCurrentDowloadClip == DataSchedule.Instance.GetDataschedules().Count)
                {
                    numberCurrentDowloadClip = 0;
                }
            }
            if (numberDowloadClip < DataSchedule.Instance.GetDataschedules().Count)
            {
               // Debug.Log("Finish 1 CheckContinueDowloadMoive");
                StartCoroutine(_CroutineDownloadMoive());
                
            }
            else
            {
               // Debug.Log("ALL COMPLETE");
              //  DataSchedule.Instance.PrintDataSchedule();
                isDowloadMovie = false;
            }
        }
        else
        {
            if (File.Exists(_absolutPath))
            {
                try
                {
                    File.Delete(_absolutPath);
                }
                catch (System.IO.IOException e)
                {
                    Debug.Log(e.Data);

                }

            }
            isDowloadMovie = false;
           // StopDowloadMoive = false;
         //  SetIsFirstDowload(false);
          // PrepareMediasList();
          // StartDeleteClip();
        }
        
        
    }

    public void CheckContinuewritingMovie()
    {
        Debug.Log("Start CheckContinuewritingMovie");
        if (!StopDowloadMoive)
        {
            StartCoroutine(_CoroutineWritingMovi());
         
             
        }
        else
        {
            isDowloadMovie = false;
            // StopDowloadMoive=false;

            // SetIsFirstDowload(false);
            //  PrepareMediasList();
            //  StartDeleteClip();

        }
    }
    IEnumerator _CroutineDownloadMoive()
    {


        Debug.Log("Start Dowload");
        isWWW = true;
        //ogg format does not support mobile, you must change url to your video file download url(mp4)

        string url = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoad;
        _absolutPath = GetAbsolutPath(DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);

        www_test = new WWW(url);

        while (false == www_test.isDone)
        {

            
            yield return null;
            if (StopDowloadMoive)
            {
                Debug.Log("Stop Dowload");
                break;
            }
        }
        if (www_test.error==null)
        {
            isWWW = false;
          
            
        Debug.Log("Finish1 Dowload");
        CheckContinuewritingMovie();
           // CheckContinueDowloadMoive();
        }
        else
        {
             Debug.Log("Finish1 Dowload ERROR");
            if (StopDowloadMoive)
            {
               CheckContinuewritingMovie();
               // CheckContinueDowloadMoive();
            }
            else
            {
                StartCoroutine(_CroutineDownloadMoive());
   
            }
        }
        

    }

    IEnumerator _CoroutineWritingMovi()
    {
       
       Debug.Log("Start Writing");
    
       WeakReference wrefFileStream = null;
       WeakReference wrefMemoryStream = null;
       WeakReference wrefWWW = null;
       totalDownloaded = 0;
       byte[] buffer = new byte[16*1024 * 1024];
       while (true)
       {
           try
           {
               filestream = new FileStream(_absolutPath, FileMode.Create);
               
               break;
           }
           catch (Exception )
           {
               
               
           }
           
       }
       using (Stream ms = new MemoryStream(www_test.bytes))
       {
           int read;
           while ((read = ms.Read(buffer, 0, buffer.Length)) > 0)
           {
               filestream.Write(buffer, 0, read);
               yield return waitForSeconds;
               if (StopDowloadMoive)
               {
                   break;
               }
           }
           wrefMemoryStream=new WeakReference(ms);
       }
     filestream.Flush();
       filestream.Close();
       totalDownloaded = 0;
        buffer = null;
       wrefFileStream=new WeakReference(filestream);
       
       _absolutPath = null;
        filestream = null;
        wrefWWW=new WeakReference(www_test.bytes);
        www_test = null;
        www_test.Dispose();
       System.GC.Collect();
       System.GC.WaitForPendingFinalizers();
       System.GC.Collect();
       
      CheckContinueDowloadMoive();



     // while (true)
     // {
     //     try
     //     {
     //         stream = new MemoryStream(www_test.bytes);
     //
     //         break;
     //     }
     //     catch (Exception)
     //     {
     //
     //
     //     }
     //
     // }

      //  if (www_test.bytes.Length%buffer.Length==0)
      //  {
      //      int number = (int)Math.Truncate((float)www_test.bytes.Length / (float)buffer.Length);
      //      for (int i = 0; i < number; i++)
      //      {
      //          int k = 0;
      //          for (int j = i * buffer.Length; j < (i + 1) * buffer.Length; j++)
      //          {
      //              buffer[k] = www_test.bytes[j];
      //              k++;
      //          }
      //          filestream.Write(buffer, 0, buffer.Length);
      //          yield return waitForSeconds;
      //      }
      //    // Debug.Log("buffer="+buffer.Length);
      //    // Debug.Log("www=" + www_test.bytes.Length);
      //    // Debug.Log("number=" + www_test.bytes.Length / buffer.Length);
      //  }
      //  else
      //  {
      //      int number=(int)Math.Truncate((float)www_test.bytes.Length /(float) buffer.Length);
      //      for (int i = 0; i < number; i++)
      //      {
      //          int k = 0;
      //          for (int j = i * buffer.Length; j < (i+1) * buffer.Length; j++)
      //          {
      //              buffer[k] = www_test.bytes[j];
      //              k++;
      //          }
      //          filestream.Write(buffer, 0, buffer.Length);
      //          yield return waitForSeconds;
      //      }
      //      buffer = new byte[www_test.bytes.Length - number * buffer.Length];
      //      int t = 0;
      //      for (int i = number * buffer.Length; i < www_test.bytes.Length; i++)
      //      {
      //           buffer[t] = www_test.bytes[i];
      //          t++;
      //      }
      //      filestream.Write(buffer, 0, buffer.Length);
      //      yield return waitForSeconds;
      //     // Debug.Log("buffer="+buffer.Length);
      //     // Debug.Log("www=" + www_test.bytes.Length);
      //     // Debug.Log("number=" + Math.Truncate((float)www_test.bytes.Length /(float) buffer.Length));
      //     // int part = www_test.bytes.Length -
      //     //           (int) Math.Truncate((float) www_test.bytes.Length/(float) buffer.Length)*buffer.Length;
      //     // Debug.Log("part=" + part);
      //  
      //  }
     
        
        
      // filestream = new FileStream(_absolutPath, FileMode.Create);
      // stream = new MemoryStream(www_test.bytes);
      // totalDownloaded = 0;
      // byte[] buffer = new byte[1024 * 1024];
      // while (totalDownloaded < www_test.bytes.Length)
      // {
      //     
      //     int read = stream.Read(buffer, 0, buffer.Length);
      //     totalDownloaded += read;
      //     filestream.Write(buffer, 0, read);
      //     // int percent = (int)((totalDownloaded / www_test.bytes.Length) * 100);
      //     //  Debug.Log("Downloaded: " + totalDownloaded + " of " + www_test.bytes.Length + " bytes ..." + percent);
      //     yield return waitForSeconds;
      // }
        //filestream.Flush();
       
       // stream.Flush();
       // stream.Close();
       
      
       // yield return waitForSeconds;
      //  Debug.Log("Write Complete1");
       


    }
    //----- End Dowload Movi
    //------- Delete Clip
    public void StartDeleteClip()
    {
        if (delete_medias != null && delete_medias.Any())
        {
            StartCoroutine(_CoroutinaDeleteClip());
        }
        else
        {
          //  Debug.Log("NO DELETE");
            StartDowloadMoive();
        }
    }

    public void StopDeleteClip()
    {
        StopCoroutine(_CoroutinaDeleteClip());
    }
    IEnumerator _CoroutinaDeleteClip()
    {
     int totalDeleted = 0;
     while (totalDeleted < delete_medias.Count)
        {
            if (File.Exists(GetAbsolutPath(delete_medias[totalDeleted])))
            {
                try
                {
                    Debug.Log("Delete= " + GetAbsolutPath(delete_medias[totalDeleted]));
                    File.Delete(GetAbsolutPath(delete_medias[totalDeleted]));
                }
                catch (System.IO.IOException e)
                {
                    Debug.Log("NO Delete= " + GetAbsolutPath(delete_medias[totalDeleted]));
                    Debug.Log("EXEP= "+e.Data);

                }

            }
            if (File.Exists(GetAbsolutPath(delete_medias[totalDeleted])+".meta"))
            {
                try
                {
                    Debug.Log("Delete= " + GetAbsolutPath(delete_medias[totalDeleted]) + ".meta");
                    File.Delete(GetAbsolutPath(delete_medias[totalDeleted]));
                }
                catch (System.IO.IOException e)
                {
                    Debug.Log("NO Delete= " + GetAbsolutPath(delete_medias[totalDeleted]) + ".meta");
                    Debug.Log("EXEP= " + e.Data);

                }

            }
            totalDeleted++;
            yield return waitForSeconds;
        }
         StartDowloadMoive();
       
    }
    //--------END Delete CLIP
    private string GetAbsolutPath(string id)
    {
        string _absolutPath = "";
        //_absolutPath = Application.persistentDataPath + "/" + id + ".mp4";
       _absolutPath = PathForDocumentsFile(id) + ".mp4";
       // _absolutPath = Application.dataPath + "/StreamingAssets/" + id + ".mp4";
        // Debug.Log("PATH= "+_absolutPath);
        return _absolutPath;
    }

    

    public string ReadStringFromFile(string filename)
    {
#if !WEB_BUILD
        string path = PathForDocumentsFile(filename);

        if (File.Exists(path))
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);

            string str = null;
            str = sr.ReadLine();

            sr.Close();
            file.Close();

            return str;
        }

        else
        {
            return null;
        }
#else
return null;
#endif
    }
    public void WriteStringToFile(string str, string filename)
    {
#if !WEB_BUILD
        string path = PathForDocumentsFile(filename);
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
    private string PathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }

        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
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
            Debug.Log("Shedule=" + shedule.Result);
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
