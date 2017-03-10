using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using LitJsonSrc;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos;
using UGS;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class StateControllerAVPro : SingletonBehaviour<StateControllerAVPro>
{
    public FB_Controller FB_datebase;
    public SimpleController ControllerVP;
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
    private Stream stream = null;
    private string _absolutPath;
    private bool isFirstDowloadClip;
    private WaitForSeconds waitForSeconds;
    private WaitForSeconds waitForSecondsGC;
    private bool StopDowloadMoive;
    private bool isDowloadMovie;
    private bool isWWW;
    private bool isDeleteMovie;
    private bool SelectedClip;
    private int currentTime;
    private bool IsTime;
    // Use this for initialization
    void Start()
    {
        IsTime =true;
        isWWW = false;
        SelectedClip = false;
        StopDowloadMoive = false;
        isDowloadMovie = false;
        waitForSeconds = new WaitForSeconds(.01f);
        waitForSecondsGC = new WaitForSeconds(60f);
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
        //if (Time.frameCount % 3000 == 0)
        //{
        //    System.GC.Collect();
        //}
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


        Debug.Log("Start");
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
        if (mark == Mark.GetSchedule)
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

   

   
    IEnumerator _CoroutinaSelectNUmberClip()
    {

        for (; ; )
        {
            DateTime localDate = DateTime.Now;
            int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
            // Debug.Log("Current="+currentTime);
            // Debug.Log("mSec=" + mSec);
            if (mSec==0&&IsTime)
            {
                IsTime = false;
                ChangeState(Mark.GetSchedule);
            }
            if (mSec>0)
            {
                IsTime = true;
            }
            // if (mSec>currentTime)
            // {
            //     Debug.Log("GO");
            //     currentTime = mSec;
            // }
            // else
            // {
            //     //ChangeState(Mark.GetSchedule);
            //     Debug.Log("Reload");
            // }
            if (DataSchedule.Instance.GetDataschedules().Count > 0)
            {

                var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);

                int offset = 0;
                if (Item != null)
                {
                    offset = mSec - Int32.Parse(Item.TimeStart);
                }
                if (DataSchedule.Instance.GetDataschedules().IndexOf(Item) >= 0 && DataSchedule.Instance.GetDataschedules().IndexOf(Item) != CurrentNumberClip)
                {
                    Debug.Log("Enter numberCurrentDowloadClip=" + numberCurrentDowloadClip);
                    if (CurrentNumberClip == -1)
                    {
                        //  Debug.Log("Enter2");
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
                                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
                            }
                            else
                            {
                                pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                                ControllerVP.ReplacePlayVideo(pathLoad, offset, true);
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
                    }


                }
                if (DataSchedule.Instance.GetDataschedules().IndexOf(Item) < 0 && isFirstDowloadClip)
                {
                    if (ControllerVP._mediaPlayer.Control.IsPlaying())
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
                    item.PathLocal = item.id + "/playlist.m3u8";
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
        if (prepare_medias.Count() > 0)
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
            Debug.Log("   number=" + numberCurrentDowloadClip + "  id=" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
            //  Debug.Log("_________ mSec=" + mSec + "   number=" + numberCurrentDowloadClip + "   start" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].TimeStart + "   id=" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);

            //StartCoroutine(_CroutineDownloadMoive());
            StartCoroutine(_CoroutinaLoadPlayList());
        }
        else
        {
            Debug.Log("NO DOWLOAD CLIP");
            if (ControllerVP._mediaPlayer.Control.IsPlaying() && !DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal && DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal != null)
            {
                Debug.Log("Point_2");
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                string pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].TimeStart);
                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
            }

        }


    }
    

    

    
   

    IEnumerator _CoroutinaLoadPlayList()
    {
        Debug.Log("Start Dowload");
        string pathStreamingAssets = Application.streamingAssetsPath + "/";
        string ID_Folder = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id;
        //string urlGlobal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoad;

        //---- Download Global Play List
        string urlGlobal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoad;
        WWW www_urlGlobal = new WWW(urlGlobal);

        while (false == www_urlGlobal.isDone)
        {

            if (StopDowloadMoive)
            {
              break;
            }
            yield return null;

        }
        if (!StopDowloadMoive)
        {
            if (www_urlGlobal.error == null)
            {
                //--- Select Path to Local Play List

                Regex regex = new Regex(@"(#EXT.*?)\n([https://].*?)\/(\w+.m3u8)");
                Match match = regex.Match(www_urlGlobal.text);
                www_urlGlobal.Dispose();
                www_urlGlobal = null;
                string pathLoadPlayList = null;
                string pathLoadPartsVideo = null;
                while (match.Success)
                {

                    pathLoadPlayList = match.Groups[2].Value + "/" + match.Groups[3].Value;
                    pathLoadPartsVideo = match.Groups[2].Value;

                    match = match.NextMatch();
                }
                //---- Download Local Play List
                WWW www_urlLocal = new WWW(pathLoadPlayList);
                while (false == www_urlLocal.isDone)
                {
                    if (StopDowloadMoive)
                    {
                        break;
                    }
                    yield return null;
                }
                if (!StopDowloadMoive)
                {
                    if (www_urlLocal.error == null)
                    {

                        try
                        {
                            // Determine whether the directory exists.
                            if (Directory.Exists(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id))
                            {


                            }
                            else
                            {
                                // Try to create the directory.
                                Directory.CreateDirectory(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);

                            }

                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.ToString());
                        }
                        finally { }
                        Debug.Log(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/playlist_.m3u8");

                        // -- Start Writing playlist_.m3u8 to disk
                        FileStream filePlayList = new FileStream(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/playlist.m3u8", FileMode.Create, FileAccess.Write);

                        StreamWriter swPlayList = new StreamWriter(filePlayList);
                        swPlayList.WriteLine(www_urlLocal.text);

                        swPlayList.Close();
                        filePlayList.Close();
                        Regex regexLoacalPlayList = new Regex(@"(#EXTINF:.*?)\n(.*)");
                        Match matchLoacalPlayList = regexLoacalPlayList.Match(www_urlLocal.text);
                        www_urlLocal.Dispose();
                        www_urlLocal = null;
                        // -- End Writing playlist_.m3u8 to disk

                        //Start Download segments of Local Play List
                        WWW www_segment = null;
                        while (matchLoacalPlayList.Success)
                        {
                            if (StopDowloadMoive)
                            {
                                break;
                            }
                            //Start Download Item segment
                            string url_segment = pathLoadPartsVideo + "/" + matchLoacalPlayList.Groups[2].Value;
                            www_segment = new WWW(url_segment);
                            while (false == www_segment.isDone)
                            {

                                if (StopDowloadMoive)
                                {
                                    break;
                                }
                                yield return null;

                            }
                            //End Download Item segment

                            //Start Save Item segment
                            if (!StopDowloadMoive)
                            {
                                if (www_segment.error == null)
                                {
                                    try
                                    {
                                        // Determine whether the directory exists.
                                        if (Directory.Exists(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value)))
                                        {


                                        }
                                        else
                                        {
                                            // Try to create the directory.
                                            Directory.CreateDirectory(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value));

                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        Debug.Log(e.ToString());
                                    }
                                    finally { }
                                }
                                File.WriteAllBytes(pathStreamingAssets + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value) + "/" + Path.GetFileName(matchLoacalPlayList.Groups[2].Value), www_segment.bytes);
                                www_segment.Dispose();
                                www_segment = null;
                                Debug.Log(pathLoadPartsVideo + "/" + matchLoacalPlayList.Groups[2].Value);

                                Debug.Log(Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value) + "  " + Path.GetFileName(matchLoacalPlayList.Groups[2].Value));
                                //End Save Item segment
                                matchLoacalPlayList = matchLoacalPlayList.NextMatch();
                            }
                            
                        }


                    }
                    else
                    {

                    }
                }
                
            }
            else
            {

            }
        }
        
        if (!StopDowloadMoive)
        {


            //DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal = _absolutPath;
            DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id + "/playlist.m3u8";
            saving_medias.Add(DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
            string str = JsonMapper.ToJson(saving_medias);
            WriteStringToFile(str, "SavedMediaList");

            foreach (ItemDataschedule itemDataschedule in DataSchedule.Instance.GetDataschedules())
            {
                if (itemDataschedule.id == DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id)
                {
                    //itemDataschedule.PathLocal = _absolutPath;
                    itemDataschedule.PathLocal = itemDataschedule.id + "/playlist.m3u8";
                }
            }
            //   Debug.Log("Point_1");

            if (ControllerVP._mediaPlayer.Control.IsPlaying() && !DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal && DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal != null)
            {
                // Debug.Log("Point_2");
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                string pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;

                int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].TimeStart);
                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
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
                StartCoroutine(_CoroutinaLoadPlayList());

            }
            else
            {
                 Debug.Log("ALL COMPLETE");
                //  DataSchedule.Instance.PrintDataSchedule();
                isDowloadMovie = false;
            }
        }
        else
        {
            Debug.Log("Delete folder=" + pathStreamingAssets + ID_Folder);
            if (Directory.Exists(pathStreamingAssets + ID_Folder))
            {
                deleteFolder(pathStreamingAssets + ID_Folder);

                //Directory.Delete(pathStreamingAssets + ID_Folder);
            }
            isDowloadMovie = false;
        }

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
        string pathStreamingAssets = Application.streamingAssetsPath + "/";
        int totalDeleted = 0;
        while (totalDeleted < delete_medias.Count)
        {
            deleteFolder(pathStreamingAssets + delete_medias[totalDeleted]);
           // if (File.Exists(GetAbsolutPath(delete_medias[totalDeleted])))
          
            
            //if (File.Exists(GetAbsolutPath(delete_medias[totalDeleted]) + ".meta"))
            //{
            //    try
            //    {
            //        Debug.Log("Delete= " + GetAbsolutPath(delete_medias[totalDeleted]) + ".meta");
            //        File.Delete(GetAbsolutPath(delete_medias[totalDeleted]));
            //    }
            //    catch (System.IO.IOException e)
            //    {
            //        Debug.Log("NO Delete= " + GetAbsolutPath(delete_medias[totalDeleted]) + ".meta");
            //        Debug.Log("EXEP= " + e.Data);

            //    }

            //}
            
            yield return waitForSeconds;
            if (Directory.Exists(pathStreamingAssets + delete_medias[totalDeleted]))
            {
                deleteFolder(pathStreamingAssets + delete_medias[totalDeleted]);
            }
            totalDeleted++;
        }
        StartDowloadMoive();

    }
    private void deleteFolder(string folder)
    {
        try
        {
            //Класс DirectoryInfo как раз позволяет работать с папками. Создаём объект этого
            //класса, в качестве параметра передав путь до папки.
            DirectoryInfo di = new DirectoryInfo(folder);
            //Создаём массив дочерних вложенных директорий директории di
            DirectoryInfo[] diA = di.GetDirectories();
            //Создаём массив дочерних файлов директории di
            FileInfo[] fi = di.GetFiles();
            //В цикле пробегаемся по всем файлам директории di и удаляем их
            foreach (FileInfo f in fi)
            {
                f.Delete();
            }
            //В цикле пробегаемся по всем вложенным директориям директории di 
            foreach (DirectoryInfo df in diA)
            {
                //Как раз пошла рекурсия
                deleteFolder(df.FullName);
                //Если в папке нет больше вложенных папок и файлов - удаляем её,
                if (df.GetDirectories().Length == 0 && df.GetFiles().Length == 0) df.Delete();
            }
        }
        //Начинаем перехватывать ошибки
        //DirectoryNotFoundException - директория не найдена
        catch (DirectoryNotFoundException ex)
        {
            Debug.Log("Директория не найдена. Ошибка: " + ex.Message);
        }
        //UnauthorizedAccessException - отсутствует доступ к файлу или папке
        catch (UnauthorizedAccessException ex)
        {
            Debug.Log("Отсутствует доступ. Ошибка: " + ex.Message);
        }
        //Во всех остальных случаях
        catch (Exception ex)
        {
            Debug.Log("Произошла ошибка. Обратитесь к администратору. Ошибка: " + ex.Message);
        }
    }
    //--------END Delete CLIP
    private string GetAbsolutPath(string id)
    {
        string _absolutPath = "";
        //_absolutPath = Application.persistentDataPath + "/" + id + ".mp4";
        //_absolutPath = PathForDocumentsFile(id) + ".mp4";
        // _absolutPath = Application.dataPath + "/StreamingAssets/" + id + ".mp4";
        _absolutPath = Application.streamingAssetsPath + "/" + id + ".m3u8";
        // Debug.Log("PATH= "+_absolutPath);
        return _absolutPath;
    }



    public string ReadStringFromFile(string filename)
    {
#if !WEB_BUILD
        //string path = PathForDocumentsFile(filename);
        string path = Application.streamingAssetsPath + "/" + filename;
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
        // string path = PathForDocumentsFile(filename);
        string path = Application.streamingAssetsPath + "/" + filename;
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
            StopSelectNumberClip();
            SetstopDowloadMovie(true);
            // StopDeleteClip();
            //StoptDowloadMoive();
            DataSchedule.Instance.ClearDataschedules();


            JsonData response = LitJsonSrc.JsonMapper.ToObject(shedule.Result);
            if (response.ContainsKey("data"))
            {

                if (response["data"]["schedule"].ContainsKey("mediasIdList"))
                {
                    IDictionary tdMediasIdList = response["data"]["schedule"]["mediasIdList"] as IDictionary;
                    IDictionary tdMedias = response["data"]["medias"] as IDictionary;
                    GetMediasSchedule().Clear();
                    foreach (var item in tdMedias.Keys)
                    {
                        IDictionary I_Clip = tdMedias[item] as IDictionary;
                        GetMediasSchedule().Add(I_Clip["id"].ToString());


                    }
                    //    Debug.Log(response["data"]["schedule"]["mediasIdList"].Count);

                    int i = 0;
                    foreach (string VARIABLE in tdMediasIdList.Keys)
                    {
                        string path = null;
                        IDictionary ItemClip = tdMedias[tdMediasIdList[VARIABLE].ToString()] as IDictionary;
                     // Debug.Log("PathLoad=" + ItemClip["path"].ToString());
                     // WWW www_PlayList = new WWW(ItemClip["path"].ToString());
                     //
                     // while (false == www_PlayList.isDone)
                     // {
                     //
                     //
                     //     yield return null;
                     //
                     // }
                     // if (www_PlayList.error == null)
                     // {
                     //     Debug.Log(Screen.width+" x "+Screen.height);
                     //     Regex regex = new Regex(@"\bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)");
                     //     Match match = regex.Match(www_PlayList.text);
                     //    
                     //     int count = 0;
                     //     while (match.Success)
                     //     {
                     //         Debug.Log("List=" + match.Groups[1].Value + " X " + match.Groups[2].Value);
                     //         if (count==0)
                     //         {
                     //             path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                     //         }
                     //
                     //         if (Screen.width >= Int32.Parse(match.Groups[1].Value)&&Screen.height >= Int32.Parse(match.Groups[2].Value))
                     //         {
                     //             path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                     //             Debug.Log("Yes");
                     //         }
                     //         else
                     //         {
                     //             Debug.Log("NO");
                     //         }
                     //         // Переходим к следующему совпадению
                     //         match = match.NextMatch();
                     //     }
                     //     Debug.Log(path);
                     //     // WriteStringToFile(www_test.text, "playlist");
                     // }
                     //
                     // else
                     // {
                     //
                     // }
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

                    SetAction();

                    Canvas.SetActive(false);
                    SetCurrentClip(-1);
                    SetIsFirstDowload(true);
                    //PrepareMediasList();

                    StartSelectNumberClip();
                    // SetSelectedClip(true);
                    //  StartDowloadMoive();
                    DateTime localDate = DateTime.Now;
                    currentTime = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                }
                else
                {
                    StopPlayer();
                    SetAction();
                    Canvas.SetActive(false);
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