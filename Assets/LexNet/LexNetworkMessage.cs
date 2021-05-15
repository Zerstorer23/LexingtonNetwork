﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static LexNetworkConnection;

public class LexNetworkMessage 
{
    List<object> paramQueue;
    Queue<string> receivedQueue;
    public LexNetworkMessage() {
        paramQueue = new List<object>();
    }
    public LexNetworkMessage(params object[] strings)
    {
        paramQueue = new List<object>(strings);
    }
    public void Add(string s) {
        paramQueue.Add(s);
    }
    public void Add(params object[] strings) {
        foreach (string s in strings)
        {
            paramQueue.Add(s);
        }
    }
    public string Build() {
        int count = paramQueue.Count + 2;/// signature and size 
        string message = NET_SIG + NET_DELIM + count;
        foreach(object s in paramQueue) { 
            message += NET_DELIM + s.ToString();
        }
        return message+ NET_DELIM;
        //3#hello
        //2#hello

        
        //3#hello#2#hello#
    }

    internal void EncodeParameters(DataType[] dataTypes, object[] parameters)
    {
        if (dataTypes == null)
        {
            paramQueue.Add("0");
            return;
        }
        else {
            paramQueue.Add(dataTypes.Length);
        }

        Debug.Assert(dataTypes.Length == parameters.Length);
        for (int i = 0; i < parameters.Length; i++)
        {
            paramQueue.Add((int)dataTypes[i]);
            paramQueue.Add(parameters[i]);
        }
    }

    internal bool Merge(int max_buffer, LexNetworkMessage nextMessage)
    {
        throw new NotImplementedException();
    }

    public void Split(string message) {
        receivedQueue = new Queue<string>();
        string[] tokens = message.Split('#');
        foreach (string s in tokens) {
            receivedQueue.Enqueue(s);
        }
    }
    public string GetNext() {
        return receivedQueue.Dequeue();
    }
    public int GetReceivedSize() {
        return receivedQueue.Count;
    }
    public bool HasNext() {
        return receivedQueue.Count > 0;
    }
}
