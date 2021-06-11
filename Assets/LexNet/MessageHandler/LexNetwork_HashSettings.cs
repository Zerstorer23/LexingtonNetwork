using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexNetwork_HashSettings 
{
 public  Dictionary<RoomProperty, string> RoomHash { get; private set; }

   public LexNetwork_HashSettings() {
        RoomHash = new Dictionary<RoomProperty, string>();   
    }

    public object GetRoomSetting(RoomProperty key, string defaultValue) {
        if (!RoomHash.ContainsKey(key))
        {
            RoomHash.Add(key, defaultValue);
        }
        return RoomHash[key];
    }
    public void SetRoomSetting(LexHashTable hash) {


    }

}
