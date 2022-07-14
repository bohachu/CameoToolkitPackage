
using System.Collections.Generic;
namespace Cameo
{
    public class JsonTool
    {

        public static List<T> ListJsonConverter<T>(string jsonString)
               where T : class
        {

            var data = LitJson.JsonMapper.ToObject(jsonString);

            var result = new List<T>();

            for (int i = 0; i < data.Count; i++)
            {
                var obj = LitJson.JsonMapper.ToObject<T>(data[i].ToJson());
                result.Add(obj);
            }

            return result;
        }
        public static List<Dictionary<string, string>> JsonToListDictionary(string jsonString)
        {

            var data = LitJson.JsonMapper.ToObject(jsonString);

            var result = new List<Dictionary<string, string>>();

            for (int i = 0; i < data.Count; i++)
            {
                var obj = LitJson.JsonMapper.ToObject<Dictionary<string, object>>(data[i].ToJson());

                var element = new Dictionary<string, string>();
                foreach (var one in obj)
                    element.Add(one.Key, one.Value.ToString());
                result.Add(element);
            }

            return result;
        }
        /// <summary>
        /// 以特定key作為 index, 將原本的表單dic list轉為 dic的型態
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="keyTitle"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> JsonToIndexedDictionary(string jsonString, string keyTitle)
        {
            List<Dictionary<string, string>> originData = JsonToListDictionary(jsonString);
            Dictionary<string, Dictionary<string, string>> thisDataDict =
            new Dictionary<string, Dictionary<string, string>>();
            foreach (Dictionary<string, string> mDict in originData)
            {
                string mKey = mDict[keyTitle];
                thisDataDict.Add(mKey, mDict);
            }
            return thisDataDict;
        }
    }


}