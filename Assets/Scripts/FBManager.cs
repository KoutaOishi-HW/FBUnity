using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FBManager : MonoBehaviour
{
	public Text userIdText;

	public Text userNick;
	public Text userBirth;
	public Text userAge;
	public Text userAddress;
	public Image userPicture;
	public Button fbLoginbtn;

	public Text debugLog;

	//起動時の処理 FBSDKのInitialization
	void Awake()
	{
		if(!FB.IsInitialized)
		{
			FB.Init();
		}
		else
		{
			FB.ActivateApp();
		}
	}

	// FBログイン処理
	public void Login()
	{
		var list = new List<string> {"user_birthday,user_location" };
		FB.LogInWithReadPermissions(list,callback:OnLogIn);	
	}

	//FBログイン時の処理
	private void OnLogIn(ILoginResult result)
	{
		if(FB.IsLoggedIn)
		{
			AccessToken token = AccessToken.CurrentAccessToken;
			userIdText.text = token.UserId;

			//ここでFBのAPIにアクセス　[me?fields=～]の～部分でフィールドを指定する　
			FB.API("me?fields=name", Facebook.Unity.HttpMethod.GET, GetName);
			FB.API("me?fields=birthday", Facebook.Unity.HttpMethod.GET, GetBirth);
			FB.API("me?fields=location", Facebook.Unity.HttpMethod.GET, GetLocation);

			OnGetMyPicture();

			Debug.Log("ログイン成功しました");
			debugLog.text = "ログイン成功しました";
		}
		else
		{
			Debug.Log("ログイン出来ませんでした");
			debugLog.text = "ログイン出来ませんでした";
		}
	}

	void GetName(Facebook.Unity.IGraphResult result)
	{
		userNick.text = result.ResultDictionary["name"].ToString();
	}

	//誕生日と年齢反映
	void GetBirth(Facebook.Unity.IGraphResult result)
	{
		DateTime birthDay = DateTime.Parse(result.ResultDictionary["birthday"].ToString());
		userBirth.text = ToJPPattern(birthDay);
		userAge.text = AgeCalculation(userBirth.text).ToString() + "歳";
	}

	//日付データを日本語形式に変換
	string ToJPPattern(DateTime date )
	{
		return date.ToString( "yyyy年MM月dd日" );
	}

	//誕生日と現在日時から年齢を計算
	int AgeCalculation(string userBirth)
	{
		int birthYear = int.Parse(userBirth.Substring(0,4));
		int birthMonth = int.Parse(userBirth.Substring(5,2));
		int birthDay = int.Parse(userBirth.Substring(8,2));

		DateTime thisDay = DateTime.Now;
		string thisDayData = ToJPPattern(thisDay);

		int toYear = int.Parse(thisDayData.Substring(0,4));
		int toMonth = int.Parse(thisDayData.Substring(5,2));
		int toDay = int.Parse(thisDayData.Substring(8,2));

		int age = toYear - birthYear - 1;
		if(toMonth >= birthMonth && toDay >= birthDay){
			age++;
		}

		return age;
	}

	//住所反映
	void GetLocation(Facebook.Unity.IGraphResult result)
	{
		var loc = result.ResultDictionary["location"] as Dictionary<string,object>;
		userAddress.text = loc["name"].ToString();
	}

	//リンクURLシェア
	public void Share()
	{
		FB.ShareLink (
			contentTitle:"Page Message",
			contentURL:new System.Uri("https://www.google.co.jp/"),
			contentDescription:"Google検索をシェア",
			callback:OnShare);
	}

	//シェア後の判定（エラー等）
	public void OnShare(IShareResult result)
	{
		if(result.Cancelled || !string.IsNullOrEmpty(result.Error))
		{
			Debug.Log("シェア失敗:"+result.Error);
			debugLog.text = "シェア失敗:";
		}
		else if(!string.IsNullOrEmpty(result.PostId))
		{
			Debug.Log(result.PostId);
		}
		else
		{
			Debug.Log("シェア成功");
			debugLog.text = "シェア成功";
		}
	}

	//ログイン時FBプロフィール画像を取得開始
	public void OnGetMyPicture()
	{	
		if (FB.IsLoggedIn) {
			StartCoroutine (MyPicture ());
		} else 
		{
			Debug.Log ("ログインしていません");
			debugLog.text = "ログインしていません";
		}
	}

	//FBプロフィール画像を取得・反映　画像取得は若干時間が掛かるのでコルーチンで行う
	IEnumerator  MyPicture()
	{
		WWW url = new WWW ("https" + "://graph.facebook.com/" + userIdText.text + "/picture?width=200&height=200"); //+ "?access_token=" + FB.AccessToken);
	
		Texture2D textFb2 = new Texture2D (200, 200, TextureFormat.ETC_RGB4, false); //TextureFormat must be DXT5
	
		yield return url;
		userPicture.sprite = Sprite.Create(textFb2, new Rect(0,0,200,200), Vector2.zero);;
		url.LoadImageIntoTexture (textFb2);

		yield return null;
	}
}