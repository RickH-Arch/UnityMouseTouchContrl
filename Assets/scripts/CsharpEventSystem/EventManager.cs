using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EventManager
{
    public interface IRegisterations { }

    public class Registerations<T> : IRegisterations
    {
        public Action<T> OnReceives = obj => { };
    }

    private static Dictionary<Type, IRegisterations> mTyperEventDic = new Dictionary<Type, IRegisterations>();

    public static void Register<T>(Action<T> onReceive)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if(mTyperEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives += onReceive;
        }
        else
        {
            var reg = new Registerations<T>();
            reg.OnReceives += onReceive;
            mTyperEventDic.Add(type, reg);
        }
    }

    public static void UnRegister<T>(Action<T> onReceive)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if(mTyperEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives -= onReceive;
        }
    }

    public static void Send<T>(T t)
    {
        var type = typeof(T);
        IRegisterations registerations = null;
        if(mTyperEventDic.TryGetValue(type,out registerations))
        {
            var reg = registerations as Registerations<T>;
            reg.OnReceives(t);
        }
    }
}
