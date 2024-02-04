using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Facebook.MiniJSON;
using UnityEngine.SceneManagement;


namespace BE
{
    public class FacebookManager : MonoBehaviour {

        private static FacebookManager _instancia;
        public string scoreDebug;
        private List<object> scoresList = null;
        public Text ScoresDebug;

        public static FacebookManager Instance
        {
            get
            {
                if (_instancia == null)
                {
                    GameObject fbm = new GameObject("FBManager");
                    fbm.AddComponent<FacebookManager>();
                }
                return _instancia;
            }
        }

        //public GameObject ScoreEntryPanel;
        //public GameObject ScoreScrollList;
        //public List<object> scoreList = null;

        public bool IsLoggedIn { get; set; }
        public string ProfileName { get; set; }
        public Sprite ProfilePic { get; set; }
        public string AppLinkURL { get; set; }

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            _instancia = this;
            IsLoggedIn = true;
        }      

        void SetInit()
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("FB esta Logado");
                GetProfile();
            }
            else
            {
                Debug.Log("FB não esta Logado");
            }
            IsLoggedIn = FB.IsLoggedIn;
        }


        void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 0;
            }
        }

        public void InitFB()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(SetInit, OnHideUnity);
            }
            else
            {
                IsLoggedIn = FB.IsLoggedIn;
            }
        }

        public void GetProfile()
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, DisplayUsername);
            FB.API("/me/picture?type=square&heigth=128&width=128", HttpMethod.GET, DisplayProfilePic);
            FB.GetAppLink(DealWithAppLink);
        }


        void DealWithAppLink(IAppLinkResult result)
        {
            if (!String.IsNullOrEmpty(result.Url))
            {
                AppLinkURL = "" + result.Url + "";
                Debug.Log(AppLinkURL);
            }
            else
            {
                AppLinkURL = "http:/google.com";
            }
        }

        void DisplayUsername(IResult resultado)
        {
            //Text Username = DialogUserName.GetComponent<Text>();

            if (resultado.Error == null)
            {
                ProfileName = "" + resultado.ResultDictionary["first_name"];
            }
            else
            {
                Debug.Log(resultado.Error);
            }
        }


        void DisplayProfilePic(IGraphResult resultado)
        {
            if (resultado.Texture != null)
            {
                ProfilePic = Sprite.Create(resultado.Texture, new Rect(0, 0, 128, 128), new Vector2());
            }
            else
            {
                Debug.Log(resultado.Error);
            }
        }


        public void Share()
	{
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Town"))
            {
                FB.FeedShare(
                string.Empty,
                new Uri("https://developers.facebook.com/"),
                "Parabéns, Sua Cidade está no Nível " + SceneTown.Level,
                "Boa Vista que eu quero",
                "Sua Cidade está no Nível " + SceneTown.Level + " no Boa Vista que eu quero!! Mais eventos sensacionais e explorações maravilhosas me aguardam!!",
				new Uri("https://scontent-gru2-1.xx.fbcdn.net/v/t39.2081-0/p128x128/16781533_442376296100779_498215321572737024_n.png?oh=17d8c4c7e7849e46f668a4411186c3db&oe=598EEBE2"),
                string.Empty,
                ShareCallBack
                );
            }
            else
            {

                FB.FeedShare(
                string.Empty,
                new Uri("https://developers.facebook.com/"),
                "Venha ajudar a construir Boa Vista uma cidade melhor",
                "Boa Vista que eu quero",
                "Venha ajudar a construir Boa Vista uma cidade melhor",
				new Uri("https://scontent-gru2-1.xx.fbcdn.net/v/t39.2081-0/p128x128/16781533_442376296100779_498215321572737024_n.png?oh=17d8c4c7e7849e46f668a4411186c3db&oe=598EEBE2"),
                string.Empty,
                ShareCallBack
                );
            }
        }

        void ShareCallBack(IResult result)
        {
            if (result.Cancelled)
            {
                Debug.Log("Share Cancelled");
            }
            else if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.Log("Error on share!");
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Town"))
                {
                    //SceneTown.Gem.Update();
                    Double addGema = SceneTown.Gem.Current();
                    //Debug.Log("Ponto Atual: " + addGema);
                    //Debug.Log(SceneTown.Level);
                    SceneTown.Gem.ChangeDelta(3);
                    //Debug.Log(SceneTown.Gem.ToString());
                    SceneTown.instance.Save();
                }
            }
        }

        public void Invite()
	{
		FB.Mobile.AppInvite(
			new Uri("https://developers.facebook.com"),
			new Uri ("https://scontent-gru2-1.xx.fbcdn.net/v/t39.2081-0/p128x128/16781533_442376296100779_498215321572737024_n.png?oh=17d8c4c7e7849e46f668a4411186c3db&oe=598EEBE2"),
			InviteCallBack
		);
	}

	void InviteCallBack(IResult result)
	{
		if (result.Cancelled) {
			Debug.Log ("Invite Cancelled");
		}else if(!string.IsNullOrEmpty(result.Error)){
			Debug.Log ("Error on Invite!");
		}else if(!string.IsNullOrEmpty(result.RawResult)){
			Debug.Log ("Success on invite");
		}
	}

	public void ShareWithUsers()
	{
		FB.AppRequest(
			"Come ond join me, I bet you can't beat my score!",
			null,
			new List<object>(){ "app_users" },
			null,
			null,
			null,
			null,
			ShareWithUsersCallback
		);
	}

	void ShareWithUsersCallback(IAppRequestResult result)
	{
		if (result.Cancelled) {
			Debug.Log ("Challenge Cancelled");
		}else if(!string.IsNullOrEmpty(result.Error)){
			Debug.Log ("Error on Challenge!");
		}else if(!string.IsNullOrEmpty(result.RawResult)){
			Debug.Log ("Success on Challenge");
		}
	}
        // Pontuacao
        /*
public void QueryScores()
{

    FB.API("/app/scores?fields=score,user.limit(5)", HttpMethod.GET, ScoresCallback);
}



void ScoresCallback(IResult resultado)
{
    var dict = Json.Deserialize(resultado.RawResult) as Dictionary<string, object>;

    object dataH;
    var i = 0;
    var users = new List<object>();
    if (dict.TryGetValue("data", out dataH))
    {
        users = (List<object>)dataH;
        foreach (Transform child in  ScoreScrollList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        while (i < users.Count)
        {
            var dataDict = (Dictionary<string, object>)users[i++];
            var user = (Dictionary<string, object>)dataDict["user"];

            GameObject ScorePanel;

            ScorePanel = Instantiate(ScoreEntryPanel) as GameObject;
            ScorePanel.transform.parent = ScoreScrollList.transform;

            Transform ThisScoreName = ScorePanel.transform.Find("FriendName");
            Transform ThisScoreScore = ScorePanel.transform.Find("FriendScore");

            Text ScoreName = ThisScoreName.GetComponent<Text>();
            Text ScoreScore = ThisScoreScore.GetComponent<Text>();

            Transform TheUserAvatar = ScorePanel.transform.Find("FriendsAvatar");
            Image UserAvatar = TheUserAvatar.GetComponent<Image>();

            FB.API("/" + user["id"].ToString() + "/picture?type=square&height=128&width=128", HttpMethod.GET, delegate (IGraphResult pictureResult)
            {
                if (pictureResult.Error != null)
                {
                    Debug.Log(pictureResult.Error);
                }
                else
                {
                    UserAvatar.sprite = Sprite.Create(pictureResult.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
                }
            });

            ScoreName.text = user["name"].ToString();
            ScoreScore.text = dataDict["score"].ToString();
        }
    }
}


public void SetScore()
{
    var scoreData = new Dictionary<string, string>();

    // scoreData["score"] = Random.Range(19, 200).ToString();
    scoreData["score"] = SceneTown.Level.ToString();
    FB.API("/me/scores", HttpMethod.POST, delegate (IGraphResult resultado)
    {
        Debug.Log("Score submit result: " + scoreData["score"]);
    }, scoreData);

}
*/

    }
}