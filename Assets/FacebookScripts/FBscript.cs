using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Facebook.MiniJSON;


namespace BE
{
    public class FBscript : MonoBehaviour
    {

        public GameObject DialogLoggedIn;
        public GameObject DialogLoggedOut;
        public GameObject DialogUserName;
        public GameObject DialogProfilePic;

        public GameObject ScoreEntryPanel;
        public GameObject ScoreScrollList;
        public List<object> scoreList = null;


        private Dictionary<string, string> profile = null;

        void Awake()
        {
            FacebookManager.Instance.InitFB();
            DealWithFBMenus(FB.IsLoggedIn);

        }

        void SetInit()
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("FB esta Logado");
            }
            else
            {
                Debug.Log("FB não esta Logado");
            }

            DealWithFBMenus(FB.IsLoggedIn);
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
        public void FBLogin()
        {
            List<string> permissions = new List<string>();
            permissions.Add("public_profile");

            FB.LogInWithReadPermissions(permissions, AuthCallBack);
        }

        //

        void AuthCallBack(IResult resultado)
        {

            if (resultado.Error != null)
            {
                Debug.Log(resultado.Error);
            }
            else
            {
                if (FB.IsLoggedIn)
                {
                    FacebookManager.Instance.IsLoggedIn = true;
                    FacebookManager.Instance.GetProfile();
                    Debug.Log("FB está Logado");
                }
                else
                {
                    Debug.Log("FB nao Está Logado!");
                }

                DealWithFBMenus(FB.IsLoggedIn);
            }

        }

        void DealWithFBMenus(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                DialogLoggedIn.SetActive(true);
                DialogLoggedOut.SetActive(false);

                if (FacebookManager.Instance.ProfileName != null)
                {
                    Text UserName = DialogUserName.GetComponent<Text>();
                    UserName.text = "Olá, " + FacebookManager.Instance.ProfileName;
                }
                else
                {
                    StartCoroutine("WaitForProgileName");
                }

                if (FacebookManager.Instance.ProfilePic != null)
                {
                    Image ProfilePic = DialogProfilePic.GetComponent<Image>();
                    ProfilePic.sprite = FacebookManager.Instance.ProfilePic;
                }
                else
                {
                    StartCoroutine("WaitForProgilePic");
                }

            }
            else
            {
                DialogLoggedIn.SetActive(false);
                DialogLoggedOut.SetActive(true);


            }
        }

        IEnumerator WaitForProgileName()
        {
            while (FacebookManager.Instance.ProfileName == null)
            {
                yield return null;
            }

            DealWithFBMenus(FB.IsLoggedIn);
        }

        IEnumerator WaitForProgilePic()
    {
        while (FacebookManager.Instance.ProfilePic == null)
        {
            yield return null;
        }

        DealWithFBMenus(FB.IsLoggedIn);
    }

        public void Share()
        {
            FacebookManager.Instance.Share();
        }

        public void Invite()
        {
            FacebookManager.Instance.Invite();
        }

        public void ShareWithUsers()
        {
            FacebookManager.Instance.ShareWithUsers();
        }

        /*
        public void QueryScores()
        {
            FacebookManager.Instance.QueryScores();
        }

        public void SetScore()
        {
            FacebookManager.Instance.SetScore();
        }
        */


        /*
         void ScoresCallback(IResult resultado)
         {
             var dict = Json.Deserialize(resultado.RawResult) as Dictionary<string, object>;

             object dataH;
             var i = 0;
             var users = new List<object>();
             if (dict.TryGetValue("data", out dataH))
             {
                 users = (List<object>)dataH;
                 foreach (Transform child in ScoreScrollList.transform)
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