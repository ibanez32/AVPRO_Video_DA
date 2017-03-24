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

class ItemLoadMovie
{
    public bool isLoadsuccessfully;
    public string pathToGlobalPLayList;
    public string pathToLocallPLayList;
    public string pathToSegment;
    public string localPlayList;
    public string id;
    public Byte[] byteMovie;
    public string segment;
}
public class StateControllerAVPro : SingletonBehaviour<StateControllerAVPro>
{
    public FB_Controller FB_datebase;
    public ControllerAVPRO_Overlay ControllerVP;
    public GameObject Canvas;
    // private Context ContextState;
  
    //Dowload Movi
    private List<string> mediasSchedule;
    private List<string> saved_medias;
    private List<string> saving_medias;
    private List<string> prepare_medias;
    private List<string> delete_medias;
    private int numberDowloadClip;
    private int numberCurrentDowloadClip;
    private bool StopDowloadMoive;
    private bool isDowloadMovie;
   

   
    private WaitForSeconds waitForSeconds;
    
    private ManagerSelecteCurrentMovie SelectedManager;
    private Context_StrategySelectedMovies contextStrategySelectedMovies;
    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");

        StopDowloadMoive = false;
        isDowloadMovie = false;
        waitForSeconds = new WaitForSeconds(.01f);
        mediasSchedule = new List<string>();
        saved_medias = new List<string>();
        prepare_medias = new List<string>();
        delete_medias = new List<string>();
        saving_medias = new List<string>();
        deleteFolder(Application.streamingAssetsPath);

        SelectedManager = new ManagerSelecteCurrentMovie(new StrategyMainPlayList());
        contextStrategySelectedMovies = new Context_StrategySelectedMovies(new StateStategy_Internet_Is_available());
    // if (Application.internetReachability != NetworkReachability.NotReachable)
    // {
    //     contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_Is_available);
    // }
    // else
    // {
    //     bool check = true;
    //         foreach (ItemDataschedule dataschedule in DataSchedule.Instance.GetDataschedules())
    //         {
    //             if (dataschedule.PathLocal==null)
    //             {
    //                 check = false;
    //             }
    //         }
    //     if (check)
    //     {
    //         contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_No_Full_PlayList);
    //     }
    //     else
    //     {
    //         contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_No_Short_PlayList);
    //     }
    //     
    // }
        //StartCoroutine(_CoroutinaGC());
    }
    public void InternetStatus()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            // Debug.Log("YES INTERNET");
            contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_Is_available);

        }
        else
        {
            // Debug.Log("NO INTERNET");
            bool check = true;
            foreach (ItemDataschedule dataschedule in DataSchedule.Instance.GetDataschedules())
            {
                if (dataschedule.PathLocal == null)
                {
                    check = false;
                }
            }
            if (check)
            {
                contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_No_Full_PlayList);
               
            }
            else
            {
                contextStrategySelectedMovies.FindOut(MarkStrategy.Internet_No_Short_PlayList);
                
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

     
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
    }

    public void SetPathForPlayer(string path, int offset, bool url)
    {
        Debug.Log("SEEK_1"+offset);
        ControllerVP.ReplacePlayVideo(path, offset, url);
    }

    public ControllerAVPRO_Overlay GetSimpleController()
    {
        return ControllerVP;
    }
    public void SetOverlay(bool rule)
    {
        Debug.Log("OVERLAY=" + rule);
        Canvas.SetActive(rule);
    }

    public ManagerSelecteCurrentMovie GetStrategyselectedMovies()
    {
        return SelectedManager;
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
        Debug.Log("Start_AVPRO");
        numberDowloadClip = 0;
        LoadShedule();

    }

    public void StopPlayer()
    {
        ControllerVP._mediaPlayer.Stop();
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

    

    public void StartFirstSelectedMovie()
    {
        SelectedManager.FirstStart();
    }

    public void NextMovie()
    {
        SelectedManager.NextMovie();
    }

    IEnumerator _CoroutinaSelectNUmberClip()
    {

        for (; ; )
        {
            InternetStatus();
            DateTime localDate = DateTime.Now;
            int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
            SelectedManager.SelectedTime(mSec);
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


            LoaderMovies();
        }
        else
        {
            Debug.Log("NO DOWLOAD CLIP");
            if (ControllerVP._mediaPlayer.Control.IsPlaying() && !DataSchedule.Instance.GetDataschedules()[SelectedManager.currentNumberMovie].isLocal && DataSchedule.Instance.GetDataschedules()[SelectedManager.currentNumberMovie].PathLocal != null)
            {
                Debug.Log("Point_2");
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                string pathLoad = DataSchedule.Instance.GetDataschedules()[SelectedManager.currentNumberMovie].PathLocal;
                DataSchedule.Instance.GetDataschedules()[SelectedManager.currentNumberMovie].isLocal = true;
                int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[SelectedManager.currentNumberMovie].TimeStart);
                ControllerVP.ReplacePlayVideo(pathLoad, offset, false);
            }

        }


    }

    void LoaderMovies()
    {
        Debug.Log("Start Loader");
        ItemLoadMovie itemLoadMovie = new ItemLoadMovie();
        itemLoadMovie.pathToGlobalPLayList = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLoad;
        itemLoadMovie.id = DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].id;
        StartCoroutine(_CoroutinaLoadGlobalPlayList(itemLoadMovie, (isLoad) =>
         {
             if (isLoad)
             {
                 Debug.Log("PATH= " + itemLoadMovie.pathToLocallPLayList);
                 StartCoroutine(_CoroutinaLoadLocalPlayList(itemLoadMovie, (isLoadLocal) =>
                 {
                     if (isLoadLocal)
                     {
                         Debug.Log("Local_Local= " + itemLoadMovie.localPlayList);
                         WritePlaylistM3U8(itemLoadMovie);
                         StartCoroutine(_CoroutinaLoadSegment(itemLoadMovie,
                                     (isLoadSegment) =>
                                     {
                                         if (isLoadSegment)
                                         {
                                             // WriteSegmentOfMovie(itemLoadMovie);

                                             Debug.Log("Load Compleet "+itemLoadMovie.id);
                                             SaveLocalPathToMovies(itemLoadMovie);
                                             SelectedNumberLoadedMovie(itemLoadMovie);

                                         }
                                         else
                                         {
                                             DeleteFolderMovie(itemLoadMovie);
                                         }
                                     }));

                     }
                     else
                     {
                         DeleteFolderMovie(itemLoadMovie);
                     }
                 }));
             }
             else
             {
                 DeleteFolderMovie(itemLoadMovie);
             }
         }));

    }


    private IEnumerator _CoroutinaLoadGlobalPlayList(ItemLoadMovie item, Action<bool> action)
    {


        bool endLoad = false;
        while (!endLoad)
        {
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return null;
            }
            WWW www_urlGlobal = new WWW(item.pathToGlobalPLayList);
            while (false == www_urlGlobal.isDone)
            {

                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = true;
                    break;
                }
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    break;

                }
                yield return null;

            }
            if (www_urlGlobal.isDone&&www_urlGlobal.error == null)
            {
              // Regex regex = new Regex(@"(#EXT.*?)\n([https://].*?)\/(\w+.m3u8)");
              // Match match = regex.Match(www_urlGlobal.text);
              // www_urlGlobal.Dispose();
              // www_urlGlobal = null;
              //
              // while (match.Success)
              // {
              //
              //     item.pathToLocallPLayList = match.Groups[2].Value + "/" + match.Groups[3].Value;
              //     item.pathToSegment = match.Groups[2].Value;
              //
              //     match = match.NextMatch();
              // }


                Regex regex = new Regex(@"\bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)");
                Match match = regex.Match(www_urlGlobal.text);
                    
                     int count = 0;
                     while (match.Success)
                     {
                         Debug.Log("List=" + match.Groups[1].Value + " X " + match.Groups[2].Value);
                         if (count==0)
                         {
                             item.pathToLocallPLayList = match.Groups[3].Value + "/" + match.Groups[4].Value;
                             item.pathToSegment = match.Groups[3].Value;
                         }
                
                         if (Screen.width >= Int32.Parse(match.Groups[1].Value)&&Screen.height >= Int32.Parse(match.Groups[2].Value))
                         {
                             item.pathToLocallPLayList = match.Groups[3].Value + "/" + match.Groups[4].Value;
                             item.pathToSegment = match.Groups[3].Value;
                             Debug.Log("Yes");
                         }
                         else
                         {
                             Debug.Log("NO");
                         }
                         match = match.NextMatch();
                     }
                item.isLoadsuccessfully = true;
                action(true);
                endLoad = true;
            }
            else
            {
                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = true;

                }
                else
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = false;
                }

            }
        }


    }


    IEnumerator _CoroutinaLoadLocalPlayList(ItemLoadMovie item, Action<bool> action)
    {
        bool endLoad = false;
        while (!endLoad)
        {
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return null;
            }
            WWW www_urlLocal = new WWW(item.pathToLocallPLayList);
            while (false == www_urlLocal.isDone)
            {

                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = true;
                    break;
                }
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    break;

                }
                yield return null;

            }
            if (www_urlLocal.isDone&&www_urlLocal.error == null)
            {
                item.localPlayList = www_urlLocal.text;
                item.isLoadsuccessfully = true;
                action(true);
                endLoad = true;
            }
            else
            {
                Debug.Log("Error Load LoadLocalPlayList");
                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = true;

                }
                else
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    endLoad = false;
                }

            }
        }
    }

    void WritePlaylistM3U8(ItemLoadMovie item)
    {
        try
        {
            // Determine whether the directory exists.
            if (Directory.Exists(PathToFolderSavePlayList() + item.id))
            {


            }
            else
            {
                // Try to create the directory.
                Directory.CreateDirectory(PathToFolderSavePlayList() + item.id);

            }

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally { }
        Debug.Log(PathToFolderSavePlayList() + item.id + "/playlist_.m3u8");

        // -- Start Writing playlist_.m3u8 to disk
        FileStream filePlayList = new FileStream(PathToFolderSavePlayList() + item.id + "/playlist.m3u8", FileMode.Create, FileAccess.Write);

        StreamWriter swPlayList = new StreamWriter(filePlayList);
        swPlayList.WriteLine(item.localPlayList);

        swPlayList.Close();
        filePlayList.Close();
    }

    IEnumerator _CoroutinaLoadSegment(ItemLoadMovie item, Action<bool> action)
    {

        Regex regexLoacalPlayList = new Regex(@"(#EXTINF:.*?)\n(.*)");
        Match matchLoacalPlayList = regexLoacalPlayList.Match(item.localPlayList);



        //Start Download segments of Local Play List
        WWW www_segment = null;
        while (matchLoacalPlayList.Success)
        {
            if (StopDowloadMoive)
            {
                item.isLoadsuccessfully = false;
                action(false);
                break;
            }
            //Start Download Item segment
            string url_segment = item.pathToSegment + "/" + matchLoacalPlayList.Groups[2].Value;
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return null;
            }
            www_segment = new WWW(url_segment);
            while (false == www_segment.isDone)
            {
               
                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    break;
                }
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    break;
                    
                }
                yield return null;

            }
            //End Download Item segment

            //Start Save Item segment
            if (www_segment.isDone && www_segment.error == null)
            {
                try
                {
                    // Determine whether the directory exists.
                    if (Directory.Exists(PathToFolderSavePlayList() + item.id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value)))
                    {


                    }
                    else
                    {
                        // Try to create the directory.
                        Directory.CreateDirectory(PathToFolderSavePlayList() + item.id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value));

                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
                finally { }
                File.WriteAllBytes(PathToFolderSavePlayList() + item.id + "/" + Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value) + "/" + Path.GetFileName(matchLoacalPlayList.Groups[2].Value), www_segment.bytes);
                www_segment.Dispose();
                www_segment = null;
                Debug.Log(item.id + "--" + matchLoacalPlayList.Groups[2].Value);

              //  Debug.Log(Path.GetDirectoryName(matchLoacalPlayList.Groups[2].Value) + "  " + Path.GetFileName(matchLoacalPlayList.Groups[2].Value));
                //End Save Item segment
                matchLoacalPlayList = matchLoacalPlayList.NextMatch();
            }
            else
            {
                if (StopDowloadMoive)
                {
                    item.isLoadsuccessfully = false;
                    action(false);
                    break;
                }
                Debug.Log("Error Load Segment");
            }

        }
        if (StopDowloadMoive)
        {
            item.isLoadsuccessfully = false;
            action(false);

        }
        else
        {
            item.isLoadsuccessfully = true;
            action(true);
        }

    }

    private void SaveLocalPathToMovies(ItemLoadMovie item)
    {
        if (!StopDowloadMoive)
        {
            DataSchedule.Instance.GetDataschedules()[numberCurrentDowloadClip].PathLocal = item.id + "/playlist.m3u8";
            saving_medias.Add(item.id);
            string str = JsonMapper.ToJson(saving_medias);
            WriteStringToFile(str, "SavedMediaList");

            foreach (ItemDataschedule itemDataschedule in DataSchedule.Instance.GetDataschedules())
            {
                if (itemDataschedule.id == item.id)
                {

                    itemDataschedule.PathLocal = item.id + "/playlist.m3u8";
                }
            }
        }
        else
        {
            DeleteFolderMovie(item);
        }

    }

    private void SelectedNumberLoadedMovie(ItemLoadMovie item)
    {
        if (!StopDowloadMoive)
        {
            //numberDowloadClip++;
            numberCurrentDowloadClip++;
            if (numberCurrentDowloadClip == DataSchedule.Instance.GetDataschedules().Count)
            {
                numberCurrentDowloadClip = 0;
            }
            numberDowloadClip = 0;
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
                LoaderMovies();

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
            DeleteFolderMovie(item);
        }

    }

    private void DeleteFolderMovie(ItemLoadMovie item)
    {
        Debug.Log("Delete folder=" + PathToFolderSavePlayList() + item.id);
        if (Directory.Exists(PathToFolderSavePlayList() + item.id))
        {
            deleteFolder(PathToFolderSavePlayList() + item.id);

            //Directory.Delete(pathStreamingAssets + ID_Folder);
        }
        isDowloadMovie = false;
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

    private string PathToFolderSavePlayList()
    {
        string path = Application.streamingAssetsPath + "/";
        return path;
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
            StopSelectNumberClip(); // Stop selected Movies
            SetstopDowloadMovie(true); // Stop DownLoad Movies

            while (isDowloadMovie) //Waiting for the end of the video upload process
            {
                yield return null;
            }

            DataSchedule.Instance.ClearDataschedules();  //Deleting the old schedule

            //Generation of an array of new schedules
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


                    //Marking the beginning of the clips
                    DataSchedule.Instance.GetDataschedules()[0].isTimeSet = true;
                    for (int j = 1; j < DataSchedule.Instance.GetDataschedules().Count; j++)
                    {
                        int timeStart = Int32.Parse(DataSchedule.Instance.GetDataschedules()[j - 1].TimeStart) +
                                         Int32.Parse(DataSchedule.Instance.GetDataschedules()[j - 1].duration) * 1000;
                        int timeEnd = Int32.Parse(DataSchedule.Instance.GetDataschedules()[j].TimeStart);
                        if (timeEnd > timeStart)
                        {
                            DataSchedule.Instance.GetDataschedules()[j].isTimeSet = true;
                        }
                    }
                    // Print of schedules
                  //  DataSchedule.Instance.PrintDataSchedule();

                    //Mark a database for a new schedule
                    SetAction();


                    SelectedManager.currentNumberMovie = -1;
                    SelectedManager.FirstStart();
                    //Canvas.SetActive(false);
                    StartSelectNumberClip();

                    SetstopDowloadMovie(false);
                    //Checking saved videos
                    PrepareMediasList();
                    StartDeleteClip();
                 
                   
                }
                else
                {
                    StopPlayer();
                    SetAction();
                    SetOverlay(true);
                    
                }


            }
            else
            {
                Debug.Log("No");
                LoadShedule();
            }
        }
        else
        {
            Debug.Log("No Connect");
            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return null;
            }
            LoadShedule();
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