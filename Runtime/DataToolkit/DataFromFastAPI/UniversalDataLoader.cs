
using System.Threading.Tasks;
using Cameo;
public class UniversalDataLoader : IDataLoader<string>
{
    /// <summary>
    /// Cameo fast api 用於讀取植樹案的google sheet表單
    /// </summary>

    //string BaseDataUrl { get { return BaseAPIUrl + AddAPIUrl; } }

    public async Task<string> LoadWithParams(string SheetID, string user, string token)
    {
        DownloadInfo downloadInfo = DownloadInfoManager.Instance.DownloadInfo(SheetID);

        string url = string.Format("{0}/?{1}={2}&{3}={4}&{5}={6}.sheet&{7}={8}", Settings.BaseDataUrl,
            Settings.AccountKey, user,
            Settings.TokenKey, token,
            Settings.SpreadSheetKey, downloadInfo.SpreadSheet,
            Settings.WorkSheetKey, downloadInfo.WorkSheet);

        string jsonString = await FileRequestHelper.Instance.LoadJsonString(url);

        return jsonString;
    }
}
