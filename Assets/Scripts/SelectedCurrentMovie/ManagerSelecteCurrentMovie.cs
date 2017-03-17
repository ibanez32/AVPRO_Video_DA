using System;
using UnityEngine;
using System.Collections;

public abstract class StrategySelectedMovie
{
    public abstract void SelectedTime(int msec, ref  int number);
    public abstract void NextMovie(ref int number);
    public abstract void FirstStart(ref int number);
}

public class StrategyMainPlayList : StrategySelectedMovie
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
            StateControllerAVPro.Instance.SetCurrentClip(number);
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(Item.PathLocal))
            {
                StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                StateControllerAVPro.Instance.SetOverlay(false);
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
            StateControllerAVPro.Instance.SetCurrentClip(number);
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[number].PathLocal))
            {
                StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = DataSchedule.Instance.GetDataschedules()[number].PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, 0, false);

            }
            else
            {
                StateControllerAVPro.Instance.SetOverlay(false);
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
            StateControllerAVPro.Instance.SetCurrentClip(number);
            string pathLoad = null;
            //  Debug.Log("CurrentNumberClip=" + CurrentNumberClip + "    PathLocal=" + DataSchedule.Instance.GetDataschedules()[CurrentNumberClip].PathLocal);
            if (!string.IsNullOrEmpty(Item.PathLocal))
            {
                StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLocal;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = true;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, false);

            }
            else
            {
                StateControllerAVPro.Instance.SetOverlay(false);
                pathLoad = Item.PathLoad;
                DataSchedule.Instance.GetDataschedules()[number].isLocal = false;
                StateControllerAVPro.Instance.SetPathForPlayer(pathLoad, offset, true);

            }
           
            Debug.Log("CurrentNumberClip=" + number + "    PathLocal=" + pathLoad + "    offset=" + offset);
        }
        else
        {
            Debug.Log("NULL");
        }
    }
}
public class StategyShortPlayListFor_Relative_URL : StrategySelectedMovie
{
    public override void SelectedTime(int msec, ref int number)
    {
        throw new System.NotImplementedException();
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
            StateControllerAVPro.Instance.SetOverlay(false);
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
        NextMovie(ref number);
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
            StateControllerAVPro.Instance.SetOverlay(false);
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
        NextMovie(ref number);
    }
}
public class StategyStaicImage : StrategySelectedMovie
{
    public override void SelectedTime(int msec, ref int number)
    {
        throw new System.NotImplementedException();
    }

    public override void NextMovie(ref int number)
    {
        throw new System.NotImplementedException();
    }

    public override void FirstStart(ref int number)
    {
        throw new System.NotImplementedException();
    }
}
public class ManagerSelecteCurrentMovie
{
    private StrategySelectedMovie strategy;
    private int currentNumberMovie;
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
        currentNumberMovie = -1;
        strategy.FirstStart(ref currentNumberMovie);
    }
}
