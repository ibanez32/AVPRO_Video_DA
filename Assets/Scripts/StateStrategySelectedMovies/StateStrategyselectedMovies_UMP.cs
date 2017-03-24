using UnityEngine;
using System.Collections;

//-------------------------------------------
// Manager State Strategy Selected Movies for UMP
//-------------------------------------------
public enum MarkStrategy_UMP
{
    Internet_No_Full_PlayList,
    Internet_No_Short_PlayList,
    Internet_Is_available
}
public class Context_StrategySelectedMovies_UMP
{
    internal StateStrategyselectedMovies_UMP State { get; set; }

    public Context_StrategySelectedMovies_UMP(StateStrategyselectedMovies_UMP state)
    {
        State = state;
    }

    public void FindOut(MarkStrategy_UMP mark)
    {
        State.HandleMark(this, mark);
    }
}
public abstract class StateStrategyselectedMovies_UMP
{

    internal virtual void HandleMark(Context_StrategySelectedMovies_UMP strategy, MarkStrategy_UMP mark)
    { ChangeState(strategy, mark); }

    protected abstract void ChangeState(Context_StrategySelectedMovies_UMP strategy, MarkStrategy_UMP mark);
}

internal class StateStategy_Internet_No_Full_PLay_List_UMP : StateStrategyselectedMovies_UMP
{
    internal StateStategy_Internet_No_Full_PLay_List_UMP()
    {
        Debug.Log("Internet_No_Full");
        State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().SetStrategy(new StrategyMainPlayList_No_Internet_UMP());

    }

    protected override void ChangeState(Context_StrategySelectedMovies_UMP strategy, MarkStrategy_UMP mark)
    {
        switch (mark)
        {
            case MarkStrategy_UMP.Internet_Is_available:
                {
                    strategy.State = new StateStategy_Internet_Is_available_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Full_PlayList:
                {
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Short_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Short_PLay_List_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
        }
    }
}
internal class StateStategy_Internet_No_Short_PLay_List_UMP : StateStrategyselectedMovies_UMP
{
    internal StateStategy_Internet_No_Short_PLay_List_UMP()
    {
        Debug.Log("Internet_No_Shor");
        State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().SetStrategy(new StategyShortPlayList_UMP());
        Debug.Log("number=" + State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().currentNumberMovie);
       
    }

    protected override void ChangeState(Context_StrategySelectedMovies_UMP strategy, MarkStrategy_UMP mark)
    {
        switch (mark)
        {
            case MarkStrategy_UMP.Internet_Is_available:
                {
                    strategy.State = new StateStategy_Internet_Is_available_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Full_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Full_PLay_List_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Short_PlayList:
                {
                    break;
                }
        }
    }
}
internal class StateStategy_Internet_Is_available_UMP : StateStrategyselectedMovies_UMP
{
    internal StateStategy_Internet_Is_available_UMP()
    {
        Debug.Log("Is_available");
        State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().SetStrategy(new StrategyMainPlayList_UMP());
        //  State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
    }

    protected override void ChangeState(Context_StrategySelectedMovies_UMP strategy, MarkStrategy_UMP mark)
    {
        switch (mark)
        {
            case MarkStrategy_UMP.Internet_Is_available:
                {
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Full_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Full_PLay_List_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
            case MarkStrategy_UMP.Internet_No_Short_PlayList:
                {
                    strategy.State = new StateStategy_Internet_No_Short_PLay_List_UMP();
                    State_Controller_UMP_Overlay.Instance.GetStrategyselectedMovies().FirstStart();
                    break;
                }
        }
    }
}

