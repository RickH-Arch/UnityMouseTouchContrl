using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UICmdMessage
{
    public UICmd cmd;
    public object Sender;
    public bool Bool1;
    public int Int1;
    public string String1;

    public UnityAction callStarted, callFinished, callFailed, callCanceled, callSucceeded;

    public UICmdMessage(UICmd cmd)
    {
        this.cmd = cmd;
    }
}
