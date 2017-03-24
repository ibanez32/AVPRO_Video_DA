using System;
using UnityEngine;
using System.Collections;

public abstract class StrategySelectedMovie
{
    public abstract void SelectedTime(int msec, ref  int number);
    public abstract void NextMovie(ref int number);
    public abstract void FirstStart(ref int number);
}
//-------------------------------------------
// Manager Selected Current Movies for AVPRO
//-------------------------------------------
public class StrategyMainPlayList : StrategySelectedMovie
{
    public StrategyMainPlayList()
    {
        Debug.Log("StrategyMainPlayList");
    }
    public override void SelectedTime(int mSec, ref int number)
    {
        var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);

        int offset = 0;
        if (Item != null && Item.isTimeSet && Item.number != number)
        {
            offset = mSec - Int32.Parse(Item.TimeStart);
            number = Item.number;
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(Item.PathLocal))
            {
                Debug.Log("StrategyMainPlayList SelectedTime Local");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                Debug.Log("StrategyMainPlayList SelectedTime URL");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, true);

            }
        }
    }

    public override void NextMovie(ref int number)
    {
        if (number + 1 < DataSchedule.Instance.GetDataschedules().Count && !DataSchedule.Instance.GetDataschedules()[number + 1].isTimeSet)
        {
            number = DataSchedule.Instance.GetDataschedules()[number + 1].number;
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[number].PathLocal))
            {
                Debug.Log("StrategyMainPlayList NextMovie Local");
              //  StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, false);

            }
            else
            {
                Debug.Log("StrategyMainPlayList NextMovie URL");
              //  StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, true);

            }
        }
        else
        {
            StateControllerAVPro.Instance.SetOverlay(true);
        }

    }

    public override void FirstStart(ref int number)
    {
        Debug.Log("FirstStart");
        DateTime localDate = DateTime.Now;
        int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
        var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);
        Debug.Log("mSec="+mSec);

        int offset = 0;
        if (Item != null)
        {
            offset = mSec - Int32.Parse(Item.TimeStart);
            number = Item.number;
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(Item.PathLocal))
            {
                Debug.Log("StrategyMainPlayList FirstStart Local");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                Debug.Log("StrategyMainPlayList FirstStart URL");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, true);

            }
           
            Debug.Log("CurrentNumberClip=" + number + "    PathLocal=" + pathLoad + "    offset=" + offset);
        }
        else
        {
            Debug.Log("NULL");
            StateControllerAVPro.Instance.GetSimpleController()._mediaPlayer.Stop();
            StateControllerAVPro.Instance.SetOverlay(true);
        }
    }
}

public class StrategyMainPlayList_No_Internet : StrategySelectedMovie
{
    public override void SelectedTime(int mSec, ref int number)
    {
        var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);

        int offset = 0;
        if (Item != null && Item.isTimeSet && Item.number != number)
        {
            offset = mSec - Int32.Parse(Item.TimeStart);
            number = Item.number;
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(Item.PathLocal))
            {
                Debug.Log("StrategyMainPlayList_No_Internet SelectedTime Local");
              //  StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                Debug.Log("StrategyMainPlayList_No_Internet SelectedTime URL");
              //  StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, true);

            }
        }
    }

    public override void NextMovie(ref int number)
    {
        if (number + 1 < DataSchedule.Instance.GetDataschedules().Count && !DataSchedule.Instance.GetDataschedules()[number + 1].isTimeSet)
        {
            number = DataSchedule.Instance.GetDataschedules()[number + 1].number;
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[number].PathLocal))
            {
                Debug.Log("StrategyMainPlayList_No_Internet NextMovie Local");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, false);

            }
            else
            {
                Debug.Log("StrategyMainPlayList_No_Internet NextMovie URL");
               // StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, true);

            }
        }
        else
        {
            StateControllerAVPro.Instance.SetOverlay(true);
        }

    }

    public override void FirstStart(ref int number)
    {
       // Debug.Log("FirstStart");
        if (StateControllerAVPro.Instance.GetSimpleController()._mediaPlayer.Control.IsPlaying() 
            && !DataSchedule.Instance.GetDataschedules()[number].isLocal &&
            DataSchedule.Instance.GetDataschedules()[number].PathLocal != null)
        {
            DateTime localDate = DateTime.Now;
            int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
            string pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;

            Debug.Log("StrategyMainPlayList_No_Internet FirstStart Local");
            int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[number].TimeStart);
          //  StateControllerAVPro.Instance.SetOverlay(false);
            DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
            StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);
           
        }
        
        
    }
}

public class StategyShortPlayList: StrategySelectedMovie
{
    public override void SelectedTime(int msec, ref int number)
    {
        
    }

    public override void NextMovie(ref int number)
    {
        int count = 0;
        number++;
        if (number == DataSchedule.Instance.GetDataschedules().Count)
        {
            number = 0;
        }
        while (DataSchedule.Instance.GetDataschedules()[number].PathLocal == null && count < DataSchedule.Instance.GetDataschedules().Count)
        {
            count++;
            number++;
            if (number == DataSchedule.Instance.GetDataschedules().Count)
            {
                number = 0;
            }
        }
        string pathLoad = null;
        //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
        if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[number].PathLocal))
        {
            Debug.Log("StategyShortPlayList NextMovie Local");
           // StateControllerAVPro.Instance.SetOverlay(false);
            pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;
            DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
            StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, false);

        }
        else
        {
            StateControllerAVPro.Instance.SetOverlay(true);
        }



    }

    public override void FirstStart(ref int number)
    {
        Debug.Log("NUmber_2="+number);
        if (StateControllerAVPro.Instance.GetSimpleController()._mediaPlayer.Control.IsPlaying())
        {
            if (DataSchedule.Instance.GetDataschedules()[number].PathLocal != null)
            {
                Debug.Log("Local");
                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                string pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;

                Debug.Log("StategyShortPlayList FirstStart Local");
                int offset = mSec - Int32.Parse(DataSchedule.Instance.GetDataschedules()[number].TimeStart);
                Debug.Log("offset=" + offset);
             //   StateControllerAVPro.Instance.SetOverlay(false);
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                Debug.Log("StategyShortPlayList FirstStart Local-2");
                NextMovie(ref number);
            }
        }
        else
        {
            Debug.Log("StategyShortPlayList FirstStart Local-3   "+number);
            NextMovie(ref number);
        }
        
       
    }
}

public class ManagerSelecteCurrentMovie
{
    private StrategySelectedMovie strategy;
    public int currentNumberMovie;
    public ManagerSelecteCurrentMovie(StrategySelectedMovie strategy)
    {
        this.strategy = strategy;
        currentNumberMovie = -1;
    }

    public void SetStrategy(StrategySelectedMovie strategy)
    {
        this.strategy = strategy;
    }
    public void SelectedTime(int msec)
    {
        strategy.SelectedTime(msec, ref currentNumberMovie);
    }

    public void NextMovie()
    {
        strategy.NextMovie(ref currentNumberMovie);
    }

    public void FirstStart()
    {
        
        strategy.FirstStart(ref currentNumberMovie);
    }
}
