using UnityEngine;
using System.Collections;

public enum Mark
{
    GetPin,
    GetSchedule
}

 abstract  class State {

    public virtual void HandleMark( Context context, Mark mark)
    {
        ChangeState(context, mark);
    } 
 
        protected abstract void ChangeState(Context context, Mark mark); 
}
