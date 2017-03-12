using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ItemDataschedule
{
    public int number;
    public string TimeStart;
    public string id;
    public string duration;
    public string PathLoad;
    public string PathLoadGlobal;
    public string PathLocal;
    public bool isLocal;
}
public class DataSchedule  {

	 private static DataSchedule instance;
    private List<ItemDataschedule> DataClips;

    private DataSchedule()
    {
        DataClips=new List<ItemDataschedule>();
    }

     public static DataSchedule Instance
   {
      get 
      {
         if (instance == null)
         {
             instance = new DataSchedule();
         }
         return instance;
      }
   }

    public void CreateItemDataSchedule(ItemDataschedule dataschedule)
    {
     DataClips.Add(dataschedule);   
    }

    public void addItemDataSchedule(ItemDataschedule data)
    {
        DataClips.Add(data);
    }
    public void PrintDataSchedule()
    {
        foreach (ItemDataschedule clip in DataClips)
        {
            Debug.Log("No=" + clip.number + "    Time=" + clip.TimeStart + "   id=" + clip.id + "   PathLoad=" + clip.PathLoad + "   PathLocal=" + clip.PathLocal);
        }
    }

    public List<ItemDataschedule> GetDataschedules()
    {
        return DataClips;
    }

    public void ClearDataschedules()
    {
        DataClips.Clear();
    }
}