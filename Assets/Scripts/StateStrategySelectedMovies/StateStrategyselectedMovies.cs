using UnityEngine;
using System.Collections;

//-------------------------------------------
// Manager State Strategy Selected Movies for AVPRO
//-------------------------------------------
public enum MarkStrategy
{
    Internet_No_Full_PlayList,
    Internet_No_Short_PlayList,
    Internet_Is_available
}
public class Context_StrategySelectedMovies
{
    internal StateStrategyselectedMovies State { get; set; }

    public Context_StrategySelectedMovies(StateStrategyselectedMovies state)
    {
        State = state;
    }

    public void FindOut(MarkStrategy mark)
    {
        State.HandleMark(this, mark);
    }
}
public abstract class StateStrategyselectedMovies
{

    internal virtual void HandleMark(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    { ChangeState(strategy, mark); }

    protected abstract void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark);
}

internal class StateStategy_Internet_No_Full_PLay_List : StateStrategyselectedMovies
{
    internal StateStategy_Internet_No_Full_PLay_List()
    {
        Debug.Log("Internet_No_Full");
        StateControllerAVPro.Instance.GetStrategyselectedMovies().SetStrategy(new StrategyMainPlayList_No_Internet());
     
    }

    protected override void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    {
        switch (mark)
        {
            case MarkStrategy.Internet_Is_available:
                {
                    strategy.State = new StateStategy_Internet_Is_available();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy.Internet_No_Full_PlayList:
                {
                    break;
                }
            case MarkStrategy.Internet_No_Short_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Short_PLay_List();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
        }
    }
}
internal class StateStategy_Internet_No_Short_PLay_List : StateStrategyselectedMovies
{
    internal StateStategy_Internet_No_Short_PLay_List()
    {
        Debug.Log("Internet_No_Shor");
        StateControllerAVPro.Instance.GetStrategyselectedMovies().SetStrategy(new StategyShortPlayList());
        Debug.Log("number=" + StateControllerAVPro.Instance.GetStrategyselectedMovies().currentNumberMovie);
        // if (StateControllerAVPro.Instance.GetSimpleController()._mediaPlayer.Control.IsPlaying() &&
        //        !DataSchedule.Instance.GetDataschedules()[StateControllerAVPro.Instance.GetCurrentClip()].isLocal)
        // {
        //     StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
        // }
    }

    protected override void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    {
        switch (mark)
        {
            case MarkStrategy.Internet_Is_available:
                {
                    strategy.State = new StateStategy_Internet_Is_available();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy.Internet_No_Full_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Full_PLay_List();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy.Internet_No_Short_PlayList:
                {
                    break;
                }
        }
    }
}
internal class StateStategy_Internet_Is_available : StateStrategyselectedMovies
{
    internal StateStategy_Internet_Is_available()
    {
        Debug.Log("Is_available");
        StateControllerAVPro.Instance.GetStrategyselectedMovies().SetStrategy(new StrategyMainPlayList());
      //  StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
    }

    protected override void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    {
        switch (mark)
        {
            case MarkStrategy.Internet_Is_available:
                {
                    break;
                }
            case MarkStrategy.Internet_No_Full_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Full_PLay_List();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy.Internet_No_Short_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Short_PLay_List();
                    StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
        }
    }
}

