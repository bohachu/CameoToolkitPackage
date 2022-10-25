using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameo.UI
{
    public class PageHistoryManager
    {
        private static Dictionary<string, Stack<PageSwitchRecord>> historyMapping = new Dictionary<string, Stack<PageSwitchRecord>>();

        public static int Count(string managerID)
        {
            return historyMapping.ContainsKey(managerID) ? historyMapping[managerID].Count : 0;
        }

        public static PageSwitchRecord Peek(string managerID)
        {
            return historyMapping.ContainsKey(managerID) ? historyMapping[managerID].Peek() : null;
        }

        public static PageSwitchRecord Pop(string managerID)
        {
            return historyMapping.ContainsKey(managerID) ? historyMapping[managerID].Pop() : null;
        }

        public static void Push(string managerID, PageSwitchRecord switchRecord)
        {
            if(!historyMapping.ContainsKey(managerID))
            {
                historyMapping.Add(managerID, new Stack<PageSwitchRecord>());
            }
            historyMapping[managerID].Push(switchRecord);
        }

        public static void Clear(string managerID)
        {
            if (historyMapping.ContainsKey(managerID))
            {
                historyMapping[managerID].Clear();
                historyMapping.Remove(managerID);
            }
        }
    }

    public class PageSwitchRecord
    {
        public string PageID;
        public Dictionary<string, object> ParamMapping;

        public PageSwitchRecord(string pageID, Dictionary<string, object> paramMapping = null)
        {
            PageID = pageID;
            ParamMapping = paramMapping;
        }
    }
}
