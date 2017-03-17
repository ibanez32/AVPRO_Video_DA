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

public class StateController : SingletonBehaviour<StateController>
{
    public FB_Controller_UMP FB_datebase;
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
        IsTime = true;
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
        //deleteFolder(Application.streamingAssetsPath);
        //StartCoroutine(_CoroutinaGC());
    }

    // Update is called once per frame
    void Update()
    {
      
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
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
                    // Debug.Log("Enter");
                    if (CurrentNumberClip == -1)
                    {
                        //  Debug.Log("Enter2");
                        if (!isDowloadMovie)
                        {
                            Debug.Log("Enter3");
                            PrepareMediasList();

                            string pathLoad = null;
                            CurrentNumberClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
                            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
                            if (
                                !string.IsNullOrEmpty(
                                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal))
                            {

                                pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad))
                                {

                                    pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                                    ControllerVP.ReplacePlayVideo(pathLoad, offset, true);
                                }
                                else
                                {
                                    CurrentNumberClip = -1;
                                }
                            }



                            Debug.Log("Path" + pathLoad);

                            //  Debug.Log("offset" + offset);
                        }

                    }
                    else
                    {

                        string pathLoad = null;
                        int tempCurrentNumberClip = CurrentNumberClip;
                        CurrentNumberClip = DataSchedule.Instance.GetDataschedules().IndexOf(Item);
                        //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
                        if (
                                !string.IsNullOrEmpty(
                                    DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal))
                        {

                            pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal;
                            DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = true;
                            ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad))
                            {

                                pathLoad = DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLoad;
                                DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal = false;
                                ControllerVP.ReplacePlayVideo(pathLoad, offset, true);
                            }
                            else
                            {
                                CurrentNumberClip = tempCurrentNumberClip;
                            }
                        }


                        Debug.Log("Path" + pathLoad);
                        Debug.Log("offset" + offset);
                    }


                }
                

                //Debug.Log("Current" + mSec );
                //  Debug.Log("Current" + mSec + "     TimeStart=" + Item);
                //   Debug.Log("Current" + mSec +  "    index=" + Item.id);
            }
            else
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
        Debug.Log("Prepare  ----------------------------");
        var except_prepare = mediasSchedule.Except(saving_medias);
        foreach (string media in except_prepare)
        {
            prepare_medias.Add(media);
        }
        foreach (var VARIABLE in prepare_medias)
        {
            Debug.Log(VARIABLE);
        }
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
            // Debug.Log("   number=" + numberCurrentDowloadClip);
            //  Debug.Log("_________ mSec=" + mSec + "   number=" + numberCurrentDowloadClip + "   start" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].TimeStart + "   id=" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);

            //StartCoroutine(_CroutineDownloadMoive());
            StartCoroutine(_CoroutinaLoadPlayList());
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
                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
            }

        }


    }
    

    
    IEnumerator _CoroutinaLoadPlayList()
    {
        Debug.Log("Start Dowload=" + DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id);
        string pathStreamingAssets = Application.streamingAssetsPath + "/";
        string ID_Folder = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id;
        //string urlGlobal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoad;

        //---- Download Global Play List
        string urlGlobal = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoadGlobal;
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

                // Regex regex = new Regex(@"(#EXT.*?)\n([https://].*?)\/(\w+.m3u8)");
                //  Match match = regex.Match(www_urlGlobal.text);

                string pathLoadPlayList = null;
                string pathLoadPartsVideo = null;
                // Debug.Log(Screen.width + " x " + Screen.height);
                Regex regex = new Regex(@"\bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)");
                Match match = regex.Match(www_urlGlobal.text);
                www_urlGlobal.Dispose();
                www_urlGlobal = null;
                int count = 0;
                while (match.Success)
                {
                    //  Debug.Log("List=" + match.Groups[1].Value + " X " + match.Groups[2].Value);
                    if (count == 0)
                    {
                        pathLoadPlayList = match.Groups[3].Value + "/" + match.Groups[4].Value;
                        pathLoadPartsVideo = match.Groups[3].Value;
                    }

                    if (Screen.width >= Int32.Parse(match.Groups[1].Value) && Screen.height >= Int32.Parse(match.Groups[2].Value))
                    {
                        pathLoadPlayList = match.Groups[3].Value + "/" + match.Groups[4].Value;
                        pathLoadPartsVideo = match.Groups[3].Value;
                        // Debug.Log("Yes");
                    }
                    else
                    {
                        // Debug.Log("NO");
                    }
                    count++;
                    // Переходим к следующему совпадению
                    match = match.NextMatch();
                }

                // while (match.Success)
                // {
                //
                //     pathLoadPlayList = match.Groups[2].Value + "/" + match.Groups[3].Value;
                //     pathLoadPartsVideo = match.Groups[2].Value;
                //
                //     match = match.NextMatch();
                // }
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
                                //  Debug.Log(pathLoadPartsVideo + "/" + matchLoacalPlayList.Groups[2].Value);

                                // Debug.Log(Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value) + "  " + Path.GetFileName(matchLoacalPlayList.Groups[2].Value));
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

            if (ControllerVP._mediaPlayer.IsPlaying && !DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].isLocal && DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal != null)
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

   
    IEnumerator _CoroutinaDeleteClip()
    {
        string pathStreamingAssets = Application.streamingAssetsPath + "/";
        int totalDeleted = 0;
        while (totalDeleted < delete_medias.Count)
        {
            deleteFolder(pathStreamingAssets + delete_medias[totalDeleted]);

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
  



    public string ReadStringFromFile(string filename)
    {
#if !WEB_BUILD
        // string path = PathForDocumentsFile(filename);
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
                    DateTime localDate = DateTime.Now;
                    int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                    foreach (string VARIABLE in tdMediasIdList.Keys)
                    {

                        IDictionary ItemClip = tdMedias[tdMediasIdList[VARIABLE].ToString()] as IDictionary;
                        if (Int32.Parse(VARIABLE) > mSec)
                        {
                            ItemDataschedule newDataschedule = new ItemDataschedule
                        {
                            number = i,
                            TimeStart = VARIABLE,
                            id = tdMediasIdList[VARIABLE].ToString(),
                            duration = ItemClip["duration"].ToString(),
                            PathLoadGlobal = ItemClip["path"].ToString(),
                            PathLoad = null,
                            PathLocal = null,
                            isLocal = false,

                        };
                            DataSchedule.Instance.addItemDataSchedule(newDataschedule);
                            i++;
                        }
                        if (Int32.Parse(VARIABLE) <= mSec && ((Int32.Parse(VARIABLE) + Int32.Parse(ItemClip["duration"].ToString()) * 1000) > mSec))
                        {
                            ItemDataschedule newDataschedule = new ItemDataschedule
                        {
                            number = i,
                            TimeStart = VARIABLE,
                            id = tdMediasIdList[VARIABLE].ToString(),
                            duration = ItemClip["duration"].ToString(),
                            PathLoadGlobal = ItemClip["path"].ToString(),
                            PathLoad = null,
                            PathLocal = null,
                            isLocal = false,

                        };
                            DataSchedule.Instance.addItemDataSchedule(newDataschedule);
                            i++;
                        }


                        // Debug.Log(VARIABLE + "   " + tdMediasIdList[VARIABLE] + "   " + ItemClip["path"]);
                    }

                    for (int j = 0; j < 1; j++)
                    {
                        if (string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[j].PathLoad))
                        {
                            string id = DataSchedule.Instance.GetDataschedules()[j].id;
                            string path = null;
                            // Debug.Log("PathLoad=" + ItemClip["path"].ToString());
                            WWW www_PlayList = new WWW(DataSchedule.Instance.GetDataschedules()[j].PathLoadGlobal);

                            while (false == www_PlayList.isDone)
                            {


                                yield return null;

                            }
                            if (www_PlayList.error == null)
                            {
                                // Debug.Log(Screen.width + " x " + Screen.height);
                                Regex regex = new Regex(@"\bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)");
                                Match match = regex.Match(www_PlayList.text);

                                int count = 0;
                                while (match.Success)
                                {
                                    //   Debug.Log("List=" + match.Groups[1].Value + " X " + match.Groups[2].Value);
                                    if (count == 0)
                                    {
                                        path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                                    }

                                    if (Screen.width >= Int32.Parse(match.Groups[1].Value) && Screen.height >= Int32.Parse(match.Groups[2].Value))
                                    {
                                        path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                                        //       Debug.Log("Yes");
                                    }
                                    else
                                    {
                                        //       Debug.Log("NO");
                                    }
                                    count++;
                                    // Переходим к следующему совпадению
                                    match = match.NextMatch();
                                }
                                // Debug.Log(path);
                                foreach (var VARIABLE in DataSchedule.Instance.GetDataschedules())
                                {
                                    if (VARIABLE.id == id)
                                    {
                                        VARIABLE.PathLoad = path;
                                    }
                                }
                            }

                            else
                            {

                            }
                        }

                    }
                    SetAction();

                    Canvas.SetActive(false);
                    SetCurrentClip(-1);
                    SetIsFirstDowload(true);
                    DataSchedule.Instance.PrintDataSchedule();
                    StartSelectNumberClip();

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
