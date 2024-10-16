﻿using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOutsider.EmgTx
{
    public class EmgTxCustom
    {
        public static void Log(object obj)
        {
            Log($"{obj}");
        }

        public static void Log(string msg)
        {
            string header = Assembly.GetCallingAssembly().FullName.Split(',')[0];
            Debug.Log($"[EmgTx=>{header}]{msg}");
        }

        public static void Log(string pattern, params object[] vars)
        {
            string header = Assembly.GetCallingAssembly().FullName.Split(',')[0];
            Debug.Log($"[EmgTx=>{header}]" + string.Format(pattern, vars));
        }
    }
}
