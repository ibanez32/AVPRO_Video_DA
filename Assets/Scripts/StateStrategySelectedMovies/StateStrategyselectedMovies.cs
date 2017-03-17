using UnityEngine;
using System.Collections;


public enum MarkStrategy
{
    Internet_No,
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

internal class StateStategy_Internet_No : StateStrategyselectedMovies
{
    internal StateStategy_Internet_No()
    {
        Debug.Log("Internet_No");
        StateControllerAVPro.Instance.GetStrategyselectedMovies().SetStrategy(new StategyShortPlayList());
        if (StateControllerAVPro.Instance.GetSimpleController()._mediaPlayer.Control.IsPlaying() &&
               !DataSchedule.Instance.GetDataschedules()[StateControllerAVPro.Instance.GetCurrentClip()].isLocal)
        {
            StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
        }
    }

    protected override void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    {
        switch (mark)
        {
            case MarkStrategy.Internet_Is_available:
                {
                    strategy.State = new StateStategy_Internet_Is_available();
                    break;
                }
            case MarkStrategy.Internet_No:
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
        StateControllerAVPro.Instance.GetStrategyselectedMovies().FirstStart();
    }

    protected override void ChangeState(Context_StrategySelectedMovies strategy, MarkStrategy mark)
    {
        switch (mark)
        {
            case MarkStrategy.Internet_Is_available:
                {
                    break;
                }
            case MarkStrategy.Internet_No:
                {
                    strategy.State = new StateStategy_Internet_No();
                    break;
                }
        }
    }
}

