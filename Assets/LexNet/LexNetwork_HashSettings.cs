using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LexNetwork_HashSettings 
{
 public  Dictionary<string, object> RoomHash { get; private set; }

   public LexNetwork_HashSettings() {
        RoomHash = new Dictionary<string, object>();   
    }

    public object GetRoomSetting(string key, object defaultValue) {
        if (!RoomHash.ContainsKey(key))
        {
            RoomHash.Add(key, defaultValue);
        }
        return RoomHash[key];
    }
    public void SetRoomSetting(string key, object value) {
        if (!RoomHash.ContainsKey(key))
        {
            RoomHash.Add(key, value);
        }
        else {
            RoomHash[key] = value;
        }
    }

}
