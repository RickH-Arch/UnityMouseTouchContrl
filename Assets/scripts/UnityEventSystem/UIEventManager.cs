using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIEventManager
{
    private static Dictionary<UICmd, UnityEvent<UICmdMessage>> eventDictionary = new Dictionary<UICmd, UnityEvent<UICmdMessage>>();
    private static object locker = new object();
    
    public static void StartListening(UICmd cmd, UnityAction<UICmdMessage> action,bool allowDuplicate = false)
    {
        UnityEvent<UICmdMessage> thisEvent = null;
       if(eventDictionary.TryGetValue(cmd,out thisEvent))
        {
            if (allowDuplicate)
            {
                thisEvent.AddListener(action);
            }
            else
            {
                lock (locker)
                {
                    thisEvent.RemoveListener(action);
                    thisEvent.AddListener(action);
                }
            }
        }
        else
        {
            thisEvent = new UnityEvent<UICmdMessage>();
            thisEvent.AddListener(action);
            eventDictionary.Add(cmd, thisEvent);
        }
    }

    public static void StopListening(UICmd cmd, UnityAction<UICmdMessage> listener)
    {
        UnityEvent<UICmdMessage> thisEvent = null;
        if(eventDictionary.TryGetValue(cmd,out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(UICmdMessage msg)
    {
        UnityEvent<UICmdMessage> thisEvent = null;
        if(eventDictionary.TryGetValue(msg.cmd,out thisEvent)){

            //callStart
            if(msg.callStarted != null)
            {
                try
                {
                    msg.callStarted.Invoke();
                }
                catch(Exception ex)
                {
                    Debug.Log(msg.cmd.ToString() + " failed to run call start.");
                    Debug.LogException(ex);
                }
            }

            //main
            try
            {
                thisEvent.Invoke(msg);
                UnityAction succeed = msg.callSucceeded;
                if(succeed != null)
                {
                    try
                    {
                        succeed.Invoke();
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(msg.cmd.ToString() + " failed to run call succeed.");
                        Debug.LogException(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log(msg.cmd.ToString() + " failed to run main code");
                Debug.LogException(ex);
                UnityAction failed = msg.callFailed;
                if(failed != null)
                {
                    try
                    {
                        failed.Invoke();
                    }
                    catch (Exception ex1)
                    {
                        Debug.Log(msg.cmd.ToString() + " failed to run call failed.");
                        Debug.LogException(ex1);
                        throw;
                    }
                }
            }

            //finished
            UnityAction finished = msg.callFinished;
            if (finished != null)
            {
                try
                {
                    finished.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.Log(msg.cmd.ToString() + " failed to run call finished.");
                    Debug.LogException(ex);
                }
            }


        }

    }
}
