using System.Globalization;
using UnityEngine;

namespace Cameo
{
    /// <summary>
    /// 上稿表／API 回傳值轉數字的共用解析工具。
    ///
    /// 動機：試算表儲存格是自由文字，空白、全形數字、被 Google Sheets 判定成日期的
    /// "1/2"（會變成 2022-01-02T00:00:00）都可能出現。直接呼叫 int.Parse 會拋出
    /// 只有型別名稱、沒有任何上下文的例外，往上冒之後整個關卡流程中斷，
    /// 畫面只剩空白，回報者與工程師都無從判斷是哪一格資料有問題。
    ///
    /// 兩種語意，依欄位性質選用：
    ///   Required — 正確性關鍵欄位（答案、正解編號）。解析失敗必須中斷，
    ///              但先印出可定位的訊息再拋，不要靜默代入預設值，
    ///              否則會變成「答案悄悄變成 0、學生永遠答錯」的無聲錯誤。
    ///   Optional — 版面、計時、權重等欄位。解析失敗記警告並採用預設值，
    ///              不該讓整個關卡掛掉。
    /// </summary>
    public static class Cameo_DataParse
    {
        /// <summary>正確性關鍵欄位：失敗時印出上下文後拋出。</summary>
        public static int RequiredInt(string raw, string context)
        {
            if (TryParseInt(raw, out int value))
                return value;

            string message = string.Format(
                "[Cameo_DataParse] 無法將資料轉為整數：{0}，實際內容 = \"{1}\"。" +
                "常見原因：儲存格空白、或內容被試算表判定成日期（例如 1/2 變成 2022-01-02T00:00:00）。",
                context, raw ?? "(null)");
            Debug.LogError(message);
            throw new CameoDataException(message);
        }

        /// <summary>非關鍵欄位：失敗時記警告並回傳預設值。</summary>
        public static int OptionalInt(string raw, int fallback, string context)
        {
            if (TryParseInt(raw, out int value))
                return value;

            Debug.LogWarning(string.Format(
                "[Cameo_DataParse] 無法將資料轉為整數：{0}，實際內容 = \"{1}\"，改用預設值 {2}。",
                context, raw ?? "(null)", fallback));
            return fallback;
        }

        /// <summary>非關鍵欄位（浮點數）：失敗時記警告並回傳預設值。</summary>
        public static float OptionalFloat(string raw, float fallback, string context)
        {
            if (TryParseFloat(raw, out float value))
                return value;

            Debug.LogWarning(string.Format(
                "[Cameo_DataParse] 無法將資料轉為數值：{0}，實際內容 = \"{1}\"，改用預設值 {2}。",
                context, raw ?? "(null)", fallback));
            return fallback;
        }

        public static bool TryParseInt(string raw, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            return int.TryParse(raw.Trim(), NumberStyles.Integer,
                                CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseFloat(string raw, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            return float.TryParse(raw.Trim(), NumberStyles.Float,
                                  CultureInfo.InvariantCulture, out value);
        }
    }

    /// <summary>
    /// 上稿資料格式錯誤。與程式邏輯錯誤區分開來，
    /// 讓關卡載入的邊界能夠只攔這一類、顯示「此關卡資料有誤」而不吞掉真正的 bug。
    /// </summary>
    public class CameoDataException : System.Exception
    {
        public CameoDataException(string message) : base(message) { }
    }
}
