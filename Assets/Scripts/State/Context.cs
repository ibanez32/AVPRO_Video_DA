using System.Resources;
using UnityEngine;
using System.Collections;

class Context:MonoBehaviour  {

    internal State State { get; set; }
   // private MonoBehaviour Controler;

    public Context(State state)
    {
        this.State = state;
        //this.Controler = controler;
    }

    public void FindOut(Mark mark)
    {
        State.HandleMark(this, mark);
    }
}
