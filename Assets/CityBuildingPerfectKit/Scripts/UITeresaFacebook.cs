using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;


namespace BE
{
    public class UITeresaFacebook : UIDialogBase
    {

        private static UITeresaFacebook instance;



        public Image FotoVice;
        public Text Message;
        public Button Button;
        public RectTransform trDialog;
        public RectTransform trMessages;
        public RectTransform trButtons;
        public Image Arthur;

        public RectOffset Border;
        public float Spacing;
        public Vector2 IconSize;

        public bool Initialized = false;
        public bool InHiding = false;
        int ButtonCount;
        int result = -1;
        Action<int> onFinish = null;

        void Awake()
        {
            instance = this;
            SetModal = false;
            gameObject.SetActive(false);
        }

        void Start()
        {
        }

        void Update()
        {
            /*
			if (Input.GetKeyDown(KeyCode.Escape)) { 
				Hide();
			}*/

            if (Initialized || InHiding) return;
            Initialized = true;
            //Debug.Log ("UIDialogMessage::Update Initialized true");

            SetUpShape();
        }

        // change dialog size by button count & message box size
        void SetUpShape()
        {
            float MessageHeight = Message.GetComponent<RectTransform>().sizeDelta.y;
            MessageHeight = 320.0f;

            trMessages.sizeDelta = new Vector2(trMessages.sizeDelta.x, MessageHeight);

            float DialogHeight = 0.0f;

            if (trButtons != null) DialogHeight += trButtons.sizeDelta.y + Spacing;
            DialogHeight += MessageHeight;
            DialogHeight += Border.top + Border.bottom;
            trDialog.sizeDelta = new Vector2(trDialog.sizeDelta.x, DialogHeight);


        }

        public void mensagem()
        {
            //UIDialogTeresaCompartilhar.Show("dsds", "dad",null,null);

            Debug.Log("Fim");
        }

        void SetUpData(string message, Sprite spr)
        {

            Message.text = message;


            if (spr != null)
            {
                //FotoVice.sprite = spr;
                //FotoVice.gameObject.SetActive(true);
            }
            else
            {
                //FotoVice.gameObject.SetActive(false);
                RectTransform MessageIn = Message.GetComponent<RectTransform>();
                MessageIn.sizeDelta = new Vector2(trDialog.sizeDelta.x - 20.0f, MessageIn.sizeDelta.y);
                MessageIn.anchoredPosition = new Vector3(0, 0, 0);
            }
        }

        // create buttons by input string
        void SetUpButtons(string texts)
        {

            // delete previously created buttons
            for (int j = trButtons.transform.childCount - 1; j >= 0; j--)
            {
                if (trButtons.transform.GetChild(j).gameObject != Button.gameObject)
                    Destroy(trButtons.transform.GetChild(j).gameObject);
            }
            // tokenize string by , 
            string[] textsub = texts.Split(',');
            ButtonCount = textsub.Length;
            var button = Button.gameObject;
            for (int i = 0; i < ButtonCount; ++i)
            {
                int iTemp = i;
                // first button is already exist
                // fill value for that
                if (i == 0)
                {
                    //Text txt = button.transform.Find ("Text").GetComponent<Text>();
                    button.transform.Find("Text").GetComponent<Text>().text = textsub[0];
                    button.GetComponent<Button>().onClick.AddListener(() => { result = iTemp; Hide(); });
                }
                else
                {
                    // after 2nd button, instantiate button and fill data
                    var buttonNew = Instantiate(button) as GameObject;
                    buttonNew.transform.SetParent(button.transform.parent, false);
                    buttonNew.transform.Find("Text").GetComponent<Text>().text = textsub[i];
                    buttonNew.GetComponent<Button>().onClick.AddListener(() => { result = iTemp; Hide(); });
                }
            }
        }

        void _Show(string message, string buttons, Sprite spr, Action<int> onFinished, bool arthur)
        {
            if(arthur == true)
            {

                Arthur.gameObject.SetActive(true);
                //FotoVice.gameObject.SetActive(true);
                Debug.Log("Mostrar artur");
            } else
            {
                Arthur.gameObject.SetActive(false);
                //FotoVice.gameObject.SetActive(false);
                Debug.Log("Nao mostrar");
            }
            onFinish = onFinished;
            SetUpButtons(buttons);
            SetUpData(message, spr);
            Initialized = false;
            InHiding = false;
            result = -1;
            ShowProcess();
        }

        void Close()
        {
            Initialized = false;
            InHiding = true;
            if (onFinish != null)
                onFinish(result);

            _Hide();
        }

        public static void Show(string message, string buttons, Sprite spr = null, Action<int> onFinished = null, bool arthur = false) { instance._Show(message, buttons, spr, onFinished, arthur);  }
        public static void Hide() { instance.Close(); }
        public static bool IsShow() { return instance.Initialized ? true : false; }

    }
}