using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SendSerialData : MonoBehaviour
{
    public TextMeshProUGUI err_text;  // エラーメッセージを表示するためのUIテキスト
    private AndroidJavaClass androidJavaClass_ = null;  // Android Javaクラスの参照
    private string errMsg_ = "";  // エラーメッセージの格納
    private object lockObject_ = new object();  // スレッド同期用のオブジェクト

    // Start is called before the first frame update
    void Start()
    {
        err_text.enabled = true;  // エラーテキストを非表示に設定

        TryConnect();
    }
    public void TryConnect()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = null;
        AndroidJavaObject context = null;
        AndroidJavaObject intent = null;
        try
        {
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            intent = activity.Call<AndroidJavaObject>("getIntent");
        }
        catch (Exception e)
        {
            err_text.text = "call error :" + e.Message;  // 例外が発生した場合のエラーメッセージを表示
        }

        androidJavaClass_ = new AndroidJavaClass("com.hoho.android.usbserial.wrapper.UsbSerialWrapper");
        if (activity == null || context == null || intent == null) return;
        try
        {
            androidJavaClass_.CallStatic("Initialize", context, activity, intent);  // 初期化
            bool ret = androidJavaClass_.CallStatic<bool>("OpenDevice", 115200);  // デバイスを開く
            if (!ret)
            {
                err_text.text = androidJavaClass_.CallStatic<string>("ErrorMsg");  // エラーメッセージを取得して表示
            }
            else
            {
                err_text.text = "Device opened successfully";  // 正常にデバイスが開かれたことを表示
            }
        }
        catch (Exception e)
        {
            err_text.text = "call error :" + e.Message;  // 例外が発生した場合のエラーメッセージを表示
        }
    }
    int count = 0;
    private void Update()
    {
        count++;
        if (count < 80) return;
        count = 0;
        TryConnect();
    }

    // シリアルデータを送信するメソッド
    public void SendData(string data)
    {
        lock (lockObject_)
        {
            bool isWriteSuccess = false;
            try
            {
                if (androidJavaClass_ != null)
                {
                    isWriteSuccess = androidJavaClass_.CallStatic<bool>("Write", data);  // データを送信
                }
                else
                {
                    errMsg_ = "androidJavaClass_ is null.";  // Javaクラスが初期化されていない場合
                }
            }
            catch (Exception e)
            {
                errMsg_ = "Exception :" + e.Message;  // 例外が発生した場合のエラーメッセージ
            }

            err_text.text = errMsg_;  // エラーメッセージを表示
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 必要に応じてエラーメッセージを更新するなどの処理を追加できます
    }
}
