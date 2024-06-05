using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cameo.UI;
public class PaymentToUnlock : MonoBehaviour
{
    //玩家因才幣數量讀取，花費，與呼叫FastApi，與教育部串接更新因才幣
    public UI_ComfirmCloseBox prefab;
    [SerializeField]
     string BuyUnlock="你現在有[$]因才幣，是否花費[cost]枚解鎖新關卡呢？";
    [SerializeField]
    string NotEnough="你現在有[$]因才幣，不足[cost]枚，無法解鎖新關卡。";
    [SerializeField]
    string MesLoading="讀取你的因才幣資料中...";
    [SerializeField]
    int UnLockFee=1;//解鎖關卡所需金額
    protected UI_ComfirmCloseBox box;
    string msgEnough
    {
        get
        {
            return BuyUnlock.Replace("[$]", GetCoin().ToString()).Replace("[cost]", UnLockFee.ToString());
        }
    }
    string msgNotEnough
    {
        get
        {
            return NotEnough.Replace("[$]", GetCoin().ToString()).Replace("[cost]", UnLockFee.ToString());
        }
    }
    public void ShowPaymentBox(UnityAction OnOk)
    {
       
        int coin = GetCoin();
        box= UI_ComfirmCloseBox.ShowBox(prefab, () => { 
            SpendCoin(UnLockFee);
            OnOk.Invoke();
            Debug.Log("OK"); 
            }, () => { Debug.Log("Cancel"); }, MesLoading);
        box.SetOKBTNEnable(false);
        StartCoroutine(LoadCoinCount());
    }
    protected virtual int GetCoin()
    {
        return 0;//PlayerDataManager.Instance.AdaptiveLearningCoinCout;
    }
    protected virtual  IEnumerator LoadCoinCount()
    {
        /*
        //讀取即時的因材幣數量，並更新UI
        Debug.Log("讀取即時的因材幣數量");
        yield return PlayerDataManager.Instance.GetPlayerCoinCoroutine();
      
        if(PlayerDataManager.Instance.IsAdaptiveLearningAccountExist==false)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("因材幣尚未開通，請先登入因才幣帳號。");
            box.SetMsg("因材幣尚未開通，請先登入因才幣帳號。");
            yield break;
        }
        */

        if(GetCoin()>=UnLockFee)
        {
            box.SetOKBTNEnable(true);
            box.SetMsg(msgEnough);
        }
        else
        {
            box.SetOKBTNEnable(false);
            box.SetMsg(msgNotEnough);
        }
       yield return null;

    }
    protected virtual void SpendCoin(int SpendCoins)
    {
        //更新因才幣數量
       //  PlayerDataManager.Instance.AdaptiveLearningCoinCout-= SpendCoins;
       //  PlayerDataManager.Instance.PostPlayerPurchaseTask("解鎖高中英文關卡", SpendCoins);
    }
    public void LogPayment(string levelName)
    {
        AddPaymentLog(levelName, UnLockFee);
    }
    protected virtual void AddPaymentLog(string levelName, int spendCoins)
    {
        // LogManager.Instance.AppendLog(LogTableDefine.table_player.ToString(), LogDefine.game_coin.ToString(), new string[] { "目的:解鎖關卡" + levelName, "花費金幣:" + spendCoins.ToString() });
    }
}
