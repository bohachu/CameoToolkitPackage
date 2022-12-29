using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo
{
    public static class GlobalDataMediator
    {
        //API for fetching data, If invoked while handlers are not registered, it will throw exception

        public static event Func<int> FetchPlayerLevelHandler;
        public static int PlayerLevel
        {
            get
            {
                if (FetchPlayerLevelHandler != null)
                {
                    return FetchPlayerLevelHandler.Invoke();
                }
                else
                {
                    throw new NullReferenceException("FetchPlayerLevelHandler have not been registered!");
                }
            }
        }

        public static event Action<int> AddPlayerLevelHandler;
        public static void AddPlayerLevel(int deltaLevel)
        {
            AddPlayerLevelHandler?.Invoke(deltaLevel);
        }

        public static event Func<string> FetchPlayerAccountHandler;
        public static string PlayerAccount
        {
            get
            {
                if (FetchPlayerAccountHandler != null)
                {
                    return FetchPlayerAccountHandler.Invoke();
                }
                else
                {
                    throw new NullReferenceException("FetchPlayerAccountHandler have not been registered!");
                }
            } 
        }

        public static event Func<string> FetchPlayerTokenHandler;
        public static string PlayerToken
        {
            get
            {
                if (FetchPlayerTokenHandler != null)
                {
                    return FetchPlayerTokenHandler.Invoke();
                }
                else
                {
                    throw new NullReferenceException("FetchPlayerTokenHandler have not been registered!");
                }
            }
        }

        //GlobalTimeManager
        public static event Func<DateTime> FetchGlobalDateTimeHandler;
        public static DateTime GlobalTimerNow
        {
            get
            {
                if (FetchGlobalDateTimeHandler != null)
                {
                    return FetchGlobalDateTimeHandler.Invoke();
                }
                else
                {
                    throw new NullReferenceException("FetchGlobalDateTimeHandler have not been registered!");
                }
            }
        }
    }
}

