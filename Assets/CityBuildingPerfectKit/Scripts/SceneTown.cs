using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          SceneTown
///   Description:    main class of town
///   Usage :
///   Author:         BraveElephant inc.
///   Version: 		  v1.0 (2015-11-15)
///-----------------------------------------------------------------------------------------
namespace BE
{

    public class SceneTown : MonoBehaviour
    {

        public static SceneTown instance;
        public Text textLevel;

        private bool bInTouch = false;
        private float ClickAfter = 0.0f;
        private bool bTemporarySelect = false;
        private Vector3 vCamRootPosOld = Vector3.zero;
        private Vector3 mousePosOld = Vector3.zero;
        private Vector3 mousePosLast = Vector3.zero;
        public GameObject goCamera = null;
        public GameObject goCameraRoot = null;
        public bool camPanningUse = true;
        public BEGround ground = null;
        private bool Dragged = false;

        private float zoomMax = 40;
        private float zoomMin = 16;
        private float zoomCurrent = 30.0f;
        private float zoomSpeed = 20;

        public float perspectiveZoomSpeed = 0.05f;  // The rate of change of the field of view in perspective mode.
        public float orthoZoomSpeed = 0.5f;         // The rate of change of the orthographic size in orthographic mode.

        [HideInInspector]
        public Plane xzPlane;


        // when game started, camera zoomin
        private bool InFade = true;
        private float FadeAge = 0.0f;

        private Building MouseClickedBuilding = null;
        private Text HouseInfo = null;

        public static bool isModalShow = false;
        public static Building buildingSelected = null;
        public static BENumber Exp;
        public static BENumber Gold;
        public static BENumber Elixir;
        public static BENumber Gem;
        public static BENumber Shield;
        public static int Level = 0;
        private static int ExpTotal = 0;

        public string mensagem;
        public bool chamarVice;





        void Awake()
        {
            instance = this;



            // initialize BENumber class and set ui element
            Exp = new BENumber(BENumber.IncType.VALUE, 0, 100000000, 0);
            Exp.AddUIImage(BEUtil.GetObject("PanelOverlay/LabelExp/Fill").GetComponent<Image>());
            Exp.AddUIText(BEUtil.GetObject("PanelOverlay/LabelExp/Text").GetComponent<Text>());
            //ExpTotal.

            Gold = new BENumber(BENumber.IncType.VALUE, 0, 200000, 1000); // initial gold count is 1000
            Gold.AddUIText(BEUtil.GetObject("PanelOverlay/LabelGold/Text").GetComponent<Text>());
            Gold.AddUIImage(BEUtil.GetObject("PanelOverlay/LabelGold/Fill").GetComponent<Image>());

            Elixir = new BENumber(BENumber.IncType.VALUE, 0, 300000, 1000); // initial elixir count is 1000
            Elixir.AddUIText(BEUtil.GetObject("PanelOverlay/LabelElixir/Text").GetComponent<Text>());
            Elixir.AddUIImage(BEUtil.GetObject("PanelOverlay/LabelElixir/Fill").GetComponent<Image>());

            Gem = new BENumber(BENumber.IncType.VALUE, 0, 100000000, 0);   // initial gem count is 100	0
            Gem.AddUIText(BEUtil.GetObject("PanelOverlay/LabelGem/Text").GetComponent<Text>());

            HouseInfo = BEUtil.GetObject("PanelOverlay/LabelHouse/Text").GetComponent<Text>();

            Shield = new BENumber(BENumber.IncType.TIME, 0, 100000000, 0);
            Shield.AddUIText(BEUtil.GetObject("PanelOverlay/LabelShield/Text").GetComponent<Text>());



            // For camera fade animation, set cameras initial positions
            goCameraRoot.transform.position = new Vector3(-5.5f, 0, -5);
            goCamera.transform.localPosition = new Vector3(0, 0, -128.0f);
            InFade = true;
            FadeAge = 0.0f;
        }

        void Start()
        {

            Time.timeScale = 1;
            isModalShow = false;
            xzPlane = new Plane(new Vector3(0f, 1f, 0f), 0f);

            // load game data from xml file
            Load();

            //if user new to this game add initial building
            if (bFirstRun)
            {
                // add town hall
                {
                    UITeresaDialogo.Show();


                    Building script = BEGround.instance.BuildingAdd(0, 1);

                    script.Move(new Vector3(4, -2, -2));

                    BuildingSelect(script);
                    BuildingLandUnselect();
                }
                // add hut
                {
                    Building script = BEGround.instance.BuildingAdd(1, 1);
                    script.Move(new Vector3(7, 0, 0));
                    BuildingSelect(script);
                    BuildingLandUnselect();
                }
            }

            //  Chama toda vez que o jogo é reiniciado
            //  Exibe na tela as missoes para o jogador 
            //Debug.Log(TBDatabase.GetLevel(ExpTotal));
            //if (TBDatabase.GetLevel(ExpTotal) != 1)
                MensagemNivel(TBDatabase.GetLevel(ExpTotal));


            GainExp(0); // chamam isso de uma vez para calcular o nível

            //set resource's capacity
            CapacityCheck();


            // Cria trabalhadores por conta hut
            int HutCount = BEGround.instance.GetBuildingCount(1);
            BEWorkerManager.instance.CreateWorker(HutCount);
            BEGround.instance.SetWorkingBuildingWorker();
        }

        // result of quit messagebox
        public void MessageBoxResult(int result)
        {
            BEAudioManager.SoundPlay(6);
            if (result == 0)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
            }
        }

        public void Update()
        {
            // get delta time from BETime
            float deltaTime = BETime.deltaTime;

            // if user pressed escape key, show quit messagebox
            if (!UIDialogMessage.IsShow() && !isModalShow && Input.GetKeyDown(KeyCode.Escape))
            {
                UIDialogMessage.Show("Você quer sair do Jogo?", "Sim, Não", "Fechar?", null, (result) => { MessageBoxResult(result); });
            }

            // if in camera animation
            if (InFade)
            {
                //camera zoom in
                FadeAge += Time.deltaTime * 0.7f;
                if (FadeAge > 1.0f)
                {
                    InFade = false;
                    FadeAge = 1.0f;
                    zoomCurrent = 24.0f;
                }

                goCameraRoot.transform.position = Vector3.Lerp(new Vector3(-5.5f, 0, -5), Vector3.zero, FadeAge);
                goCamera.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, -128.0f), new Vector3(0, 0, -24.0f), FadeAge);
            }

            Exp.Update();
            Gold.Update();
            Elixir.Update();
            Gem.Update();
            Shield.ChangeTo(Shield.Target() - (double)deltaTime);
            Shield.Update();
            HouseInfo.text = BEWorkerManager.instance.GetAvailableWorkerCount().ToString() + "/" + BEGround.instance.GetBuildingCount(1).ToString();

            if (UIDialogMessage.IsShow() || isModalShow) return;
            //if(EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButton(0))
            {

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //Debug.Log("left-click over a GUI element!");
                    return;
                }

                //Click MouseButton
                if (!bInTouch)
                {
                    bInTouch = true;
                    ClickAfter = 0.0f;
                    bTemporarySelect = false;
                    Dragged = false;
                    mousePosOld = Input.mousePosition;
                    mousePosLast = Input.mousePosition;
                    vCamRootPosOld = goCameraRoot.transform.position;

                    //when a building was selected and user drag mouse on the map
                    //check mouse drag start is over selected building or not
                    //if not do not move selected building
                    Ray ray = Camera.main.ScreenPointToRay(mousePosOld);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && (hit.collider.gameObject.tag == "Building"))
                    {
                        MouseClickedBuilding = BuildingFromObject(hit.collider.gameObject);
                    }
                    else
                    {
                        MouseClickedBuilding = null;
                    }

                    //Debug.Log ("Update buildingSelected:"+((buildingSelected != null) ? buildingSelected.name : "none"));

                }
                else
                {
                    //Mouse Button is in pressed
                    //if mouse move certain diatance
                    if (Vector3.Distance(Input.mousePosition, mousePosLast) > 0.01f)
                    {

                        // set drag flag on
                        if (!Dragged)
                        {
                            Dragged = true;

                            // show tile grid
                            if ((buildingSelected != null) && (MouseClickedBuilding == buildingSelected))
                            {
                                BETween.alpha(ground.gameObject, 0.1f, 0.0f, 0.3f);
                                //Debug.Log ("ground alpha to 0.1");
                            }
                        }

                        mousePosLast = Input.mousePosition;

                        // if selected building exist
                        if ((buildingSelected != null) && (MouseClickedBuilding == buildingSelected))
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            float enter;
                            xzPlane.Raycast(ray, out enter);
                            Vector3 vTarget = ray.GetPoint(enter);
                            // move selected building
                            buildingSelected.Move(vTarget);
                        }
                        // else camera panning
                        // controla velocidade da camera
                        else
                        {
                            if (camPanningUse)
                            {
                                Vector3 vDelta = (Input.mousePosition - mousePosOld) * 0.008f;
                                Vector3 vForward = goCameraRoot.transform.forward; vForward.y = 0.0f; vForward.Normalize();
                                Vector3 vRight = goCameraRoot.transform.right; vRight.y = 0.0f; vRight.Normalize();
                                Vector3 vMove = -vForward * vDelta.y * 2f + -vRight * vDelta.x * 2f;
                                goCameraRoot.transform.position = vCamRootPosOld + vMove;
                            }
                        }
                    }
                    // Not Move
                    else
                    {

                        if (!Dragged)
                        {
                            ClickAfter += Time.deltaTime;
                            if (!bTemporarySelect && (ClickAfter > 0.5f))
                            {
                                bTemporarySelect = true;
                                //Debug.Log ("Update2 buildingSelected:"+((buildingSelected != null) ? buildingSelected.name : "none"));
                                Pick();
                            }
                        }
                    }

                }
            }
            else
            {

                //Release MouseButton
                if (bInTouch)
                {
                    bInTouch = false;

                    // if in drag state
                    if (Dragged)
                    {

                        // seleted building exist
                        if (buildingSelected != null)
                        {

                            // hide tile grid
                            if (MouseClickedBuilding == buildingSelected)
                                BETween.alpha(ground.gameObject, 0.1f, 0.3f, 0.0f);

                            if (buildingSelected.Landable && buildingSelected.OnceLanded)
                                BuildingLandUnselect();
                        }
                    }
                    else
                    {

                        if (bTemporarySelect)
                        {
                            // land building
                            if ((buildingSelected != null) && (MouseClickedBuilding != buildingSelected) && buildingSelected.OnceLanded)
                                BuildingLandUnselect();
                        }
                        else
                        {
                            // land building
                            if ((buildingSelected != null) && (MouseClickedBuilding != buildingSelected) && buildingSelected.OnceLanded)
                                BuildingLandUnselect();

                            //Debug.Log ("Update3 buildingSelected:"+((buildingSelected != null) ? buildingSelected.name : "none"));
                            Pick();
                        }
                    }
                }
            }

            //zoom
            if (!InFade)
            {
                zoomCurrent -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
                zoomCurrent = Mathf.Clamp(zoomCurrent, zoomMin, zoomMax);
                goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);
            }

            // pinch zoom for mobile touch input
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                zoomCurrent += deltaMagnitudeDiff * perspectiveZoomSpeed;
                zoomCurrent = Mathf.Clamp(zoomCurrent, zoomMin, zoomMax);
                goCamera.transform.localPosition = new Vector3(0, 0, -zoomCurrent);
            }

            
            
           
            // Mensagens Experiencias
        }

        public void Pick()
        {
            //Debug.Log ("Pick buildingSelected:"+((buildingSelected != null) ? buildingSelected.name : "none"));
            //GameObject goSelectNew = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log ("Pick"+hit.collider.gameObject.tag);
                if (hit.collider.gameObject.tag == "Ground")
                {
                    BuildingSelect(null);
                    return;
                }
                else if (hit.collider.gameObject.tag == "Building")
                {
                    Building buildingNew = BuildingFromObject(hit.collider.gameObject);
                    if (buildingNew.HasCompletedWork())
                        return;

                    BuildingSelect(buildingNew);
                    return;
                }
                else
                {
                }
            }
        }
        public void ExibirMissao()
        {
            MensagemNivel(Level);
        }

        public void MensagemNivel(int NivelPrefeitura)
        {

            switch (NivelPrefeitura)
            {                
                case 2:
                    mensagem = "Estamos fazendo um bom trabalho, Boa Vista está crescendo, por isso vamos construir ainda mais" +
                        "\n Missão: \n Construir 1 Escola Municipal e mais 6 Moradias";
                    chamarVice = false;
                    break;
                case 3:
                    mensagem = "Estamos no Nivel 3!!!\n"+
                        "Como prefeitura precisamos nos preocupar com as mamães e seus Bêbes." +
                        "\n Missão: \n Construir uma Casa Mãe e uma Unidade Básica de Saúde - UBS";
                    chamarVice = false;
                    break;
                case 4:
				mensagem = "Estamos no Nivel 4!!!\n"+
								"Nossa população está crescendo, Devemos melhorar as residências e predios públicos"+
								"\n Missão:\n Atualizar residências e predios públicos";
                    chamarVice = false;
                    break;
                    
                case 5:
				mensagem = " Ei! Que tal disponibilizar um pouco de tecnologia para a população?."+
					"\n Missão: \n Implantar internet gratuita, devemos atualizar a Casa Mãe e UBS, construir 4 casas da Tia Rita, construir e atualizar 1 casa do Tio João";
                    chamarVice = false;
                    break;
                case 6:
				mensagem = "Estamos no Nivel 6!!!\n"+ 
					"Não podemos perder tempo, Boa Vista está crescendo muito rapido. " +
                    "\n Missão: \n Construir 6 Moradias, 1 Padaria, 1 Escola, 3 Arbustos e 6 Árvores. \n Atualize suas Escolas";
                    chamarVice = false;
                    break;
                case 7:
				mensagem = "Estamos no Nivel 7!!!\n"+
					"Oi pessoal, como a cidade está crescendo precisamos investir na saúde. " +
                    "\n Missão: \n construir 1 UBS e 1 Drograria, e assim que puder vamos construir 1 Internet, 1 Moradia, 4 Buritizeiros e 2 Arbustos.";
				chamarVice = false;
                    break;
                case 8:
				mensagem = "Estamos no Nivel 8!!!\n"+
					"As Mães trabalhadoras de Boa Vista precisam de um lugar onde deixar suas crianças em quanto trabalham. " +
                    "\n Missão: \n Construir 1 Casa Mãe, 1 Padaria, 3 Arvores. Atualize suas UBS, Casas Mães e 2 casas Tio João.";
                    chamarVice = false;
                    break;
                case 9:
				mensagem = "Estamos no Nivel 9!!!\n"+
					"Boa Vista é linda por suas belezas naturais e a cada arvore plantada podemos garantir um futuro melhor. " +
                    "\n Missão: \n Construir 1 Buritizeiro, 3 Arvores, 6 Arbustoe  1 casa. Atualize as casas Tio Toim." +
                    "\n Melhore a Saúde da população.";
                    chamarVice = false;
                    break;
                case 10:
				mensagem = "Estamos no Nivel 10!!!\n"+
					"A cidade está crescendo, por isso devemos expandir nossas construções. " +
                    "\n Missão: \n Construir 2 Conj Habitacionais, 1 Padaria, 2 Shoppings, 2 Drogarias, 3 Buritizeiros e 9 arbustos.";
                    chamarVice = false;
                    break;
                case 11:
				mensagem = "Estamos no Nivel 11!!!\n"+
					"A cidade cresceu! Não esqueça de melhorar o acesso da população a Saúde. " +
                    "\n Missão: \n Construir 1 UBS. Atualize os Conjuntos Habitacionais, Escolas, Casas Mãe, Wifi e UBS.";
                    chamarVice = false;
                    break;
                case 12:
				mensagem = "Estamos no Nivel 12!!!\n"+
					"Não esqueça de melhorar as moradias já construídas!!!!. " +
                    "\n Missão: \n Construa 4 Arbustos e atualiza as casas da Tia Rita.";
                    chamarVice = false;
                    break;
                case 13:
				mensagem = "Estamos no Nivel 13!!!\n"+
					"Bom Tabalho! Agora continue a expansão da Cidade. " +
                    "\n Missão: \n Construa 1 Conj Habitacional, 1 Escola, 1 Casa Mãe, 1 WIFI e mais arbustos e Buritizeiro. Não esqueça de atualizá-los.";
				chamarVice = false;
                    break;
                case 14:
				mensagem = "Estamos no Nivel 14!!!\n"+
					"Estamos quase lá. Agora vamos dar um grande presente a nossa cidade: O Centro de Tecnologia :) " +
                    "\n Missão: \nConstrua o Centro de Tecnologia.";
				chamarVice = false;
                    break;
                case 15:
                    mensagem = "Que legal!!!! Conseguimos tornar nossa cidade um lugar ainda melhor para se viver. " +
                    "\n";
				chamarVice = false;
                    break;

                default:
                  // DEFAULT  
                    break;
            }

            // Exibe Mensagem de compartilhamento 
            if (NivelPrefeitura !=1)           
                UITeresaFacebook.Show(mensagem, "Compartilhe", null, null, chamarVice);

        }
        public void MensagemExperiencia()
        {
            // Nivel 1
            if (ExpTotal == 12)
            {
                //TBDatabase.GetBuildingType(1);
                UITeresaFacebook.Show("Aguarde até que as Casas gerem Recursos dos Impostos Municipais", "Compartilhe", null, null, false);
            } else
            
            // Nivel 2
            if (ExpTotal == 39)
            {
                UITeresaFacebook.Show("Lembre-se de Construir a Escola Municipal, o Investimento em Educação é Muito Importante!!!", "Compartilhe", null, null, false);
            } else

            //  Nivel 3
            if (ExpTotal == 90)
            {
                UITeresaFacebook.Show("Devemos Melhorar nossa Escola", "Compartilhe", null, null, false);
            } else
			if (ExpTotal == 363)
			{
				UITeresaFacebook.Show("Devemos Melhorar nossa Escola", "Compartilhe", null, null, false);
			} else
			if (ExpTotal == 439)
			{
				UITeresaFacebook.Show("Para avançar para o próximo nível, atualize as casas do Tio João", "Compartilhe", null, null, false);
			} else
			if (ExpTotal == 502)
			{
				UITeresaFacebook.Show("Amplie a qualidade da Internet atualizando a infraestrutura existente!", "Compartilhe", null, null, false);
			} else
			if (ExpTotal == 564)
			{
				UITeresaFacebook.Show("Atualize suas UBS, Casas Mães e 2 casas Tio João!", "Compartilhe", null, null, false);
			} else
			if (ExpTotal == 642)
			{
				UITeresaFacebook.Show("Atualize a casa do Tio Toim", "Compartilhe", null, null, false);
			} else
			if (ExpTotal == 747)
			{
				UITeresaFacebook.Show("Melhore a Saúde e Educação da população", "Compartilhe", null, null, false);
			}
        }
       

        // add exp
        public void GainExp(int exp)
        {
            //print ("Experiencia Total do Jogo: " + ExpTotal);
            ExpTotal += exp;            
            int NewLevel = TBDatabase.GetLevel(ExpTotal);
            int LevelExpToGet = TBDatabase.GetLevelExp(NewLevel);
            int LevelExpStart = TBDatabase.GetLevelExpTotal(NewLevel);

            //print("Nivel da Prefeitura: " + LevelExpStart);
            SceneTown.Exp.MaxSet(LevelExpToGet);
            int ExpLeft = ExpTotal - LevelExpStart;
            SceneTown.Exp.ChangeTo(ExpLeft);

            MensagemExperiencia();

            // Se o nível UP ocorreu
            if ((NewLevel > Level) && (Level != 0))
            {
                // Mostra mensagem de Level Up
                // Compartilha no facebook
                MensagemNivel(NewLevel);

                //Reposiciona a Cemera Focando na Prefeitura
                goCameraRoot.transform.position = new Vector3(8f, 0, -5);
                goCamera.transform.localPosition = new Vector3(0, 0, -128.0f);
                InFade = false;
                FadeAge = 0.0f;

                               


                // Retorna todas as buildings do tipo 0 (prefeitura)
                List<Building> buildings = BEGround.instance.GetBuildingsByType(0);
                for (int i = 0; i < buildings.Count; i++)
                {
                    // Atualiza a prefeitura
                    
                    Debug.Log(buildings[i].Upgrade());
                    //  buildings[i].Collect();
                }

            }
            Level = NewLevel;
            textLevel.text = NewLevel.ToString();

            // save game data
            // chama o set do facebook para pontuacao
            Save();
        }

        // get building script
        // if child object was hitted, check parent
        public Building BuildingFromObject(GameObject go)
        {
            Building buildingNew = go.GetComponent<Building>();
            if (buildingNew == null) buildingNew = go.transform.parent.gameObject.GetComponent<Building>();

            return buildingNew;
        }

        // select building
        public void BuildingSelect(Building buildingNew)
        {

            // if user select selected building again
            bool SelectSame = (buildingNew == buildingSelected) ? true : false;

            if (buildingSelected != null)
            {

                // if initialy created building, then pass
                if (!buildingSelected.OnceLanded) return;
                // building can't land, then pass
                if (!buildingSelected.Landed && !buildingSelected.Landable) return;

                // land building
                BuildingLandUnselect();
                UICommand.Hide();
            }

            if (SelectSame)
                return;

            buildingSelected = buildingNew;

            if (buildingSelected != null)
            {
                //Debug.Log ("Building Selected:"+buildingNew.gameObject.name+" OnceLanded:"+buildingNew.OnceLanded.ToString ());
                // set scale animation to newly selected building
                BETween bt = BETween.scale(buildingSelected.gameObject, 0.1f, new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.4f, 1.4f, 1.4f));
                bt.loopStyle = BETweenLoop.pingpong;
                // se tbuilding state unland
                buildingSelected.Land(false, true);
            }
        }

        public void BuildingLandUnselect()
        {
            if (buildingSelected == null) return;

            buildingSelected.Land(true, true);
            buildingSelected = null;
            Save();

            UICommand.Hide();
        }

        public void BuildingDelete()
        {
            if (buildingSelected == null) return;

            buildingSelected.Land(false, false);
            BEGround.instance.BuildingRemove(buildingSelected);
            Destroy(buildingSelected.gameObject);
            buildingSelected = null;
            Save();
        }

        //pause
        public void OnButtonAttack()
        {
            BEAudioManager.SoundPlay(6);
        }

        // user clicked shop button
        public void OnButtonShop()
        {
            BEAudioManager.SoundPlay(6);

            UIShop.Show(ShopType.Normal);
        }

        // user clicked gem button
        public void OnButtonGemShop()
        {
            BEAudioManager.SoundPlay(6);
            UITeresaFacebook.Show("Compartilhe e acelere o desenvolvimento da Cidade", "compartilhar", null, null);
            //UIShop.Show(ShopType.InApp);
        }

        // user clicked house button
        public void OnButtonHouse()
        {
            BEAudioManager.SoundPlay(6);
            UIShop.Show(ShopType.House);
        }

        // user clicked option button
        public void OnButtonOption()
        {
            BEAudioManager.SoundPlay(6);
            UIOption.Show();
        }


        // Chama mensagem da S
        public void OnTeresaMensagem()
        {
            BEAudioManager.SoundPlay(6);

            //UIOption.Show ();
            //print (TBDatabase.GetLevelExp(NewLevel));


        }

        public void CapacityCheck()
        {
            int GoldCapacityTotal = BEGround.instance.GetCapacityTotal(PayType.Gold);
            //Debug.Log ("iGoldCapacityTotal:"+GoldCapacityTotal.ToString ());
            int ElixirCapacityTotal = BEGround.instance.GetCapacityTotal(PayType.Elixir);
            //Debug.Log ("ElixirCapacityTotal:"+ElixirCapacityTotal.ToString ());

            Gold.MaxSet(GoldCapacityTotal);
            if (Gold.Target() > GoldCapacityTotal) Gold.ChangeTo(GoldCapacityTotal);
            Elixir.MaxSet(ElixirCapacityTotal);
            if (Elixir.Target() > ElixirCapacityTotal) Elixir.ChangeTo(ElixirCapacityTotal);

            BEGround.instance.DistributeByCapacity(PayType.Gold, (float)Gold.Target());
            BEGround.instance.DistributeByCapacity(PayType.Elixir, (float)Elixir.Target());
        }

        // related to save and load gamedata with xml format
        bool UseEncryption = false;
        bool bFirstRun = false;
        string configFilename = "Config.dat";
        int ConfigVersion = 1;
        public bool InLoading = false;

        // Do not save town when script quit.
        // save when action is occured
        // (for example, when building created, when start upgrade, when colled product, when training start)
        public void Save()
        {

            if (InLoading) return;

            string xmlFilePath = BEUtil.pathForDocumentsFile(configFilename);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<item><name>wrench</name></item>");
            {
                xmlDocument.DocumentElement.RemoveAll();

                // Version
                { XmlElement ne = xmlDocument.CreateElement("ConfigVersion"); ne.SetAttribute("value", ConfigVersion.ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("Time"); ne.SetAttribute("value", DateTime.Now.ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("ExpTotal"); ne.SetAttribute("value", ExpTotal.ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("Gem"); ne.SetAttribute("value", Gem.Target().ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("Gold"); ne.SetAttribute("value", Gold.Target().ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("Elixir"); ne.SetAttribute("value", Elixir.Target().ToString()); xmlDocument.DocumentElement.AppendChild(ne); }
                { XmlElement ne = xmlDocument.CreateElement("Shield"); ne.SetAttribute("value", Shield.Target().ToString()); xmlDocument.DocumentElement.AppendChild(ne); }

                Transform trDecoRoot = BEGround.instance.trDecoRoot;
                //List<GameObject> goTiles=new List<GameObject>();
                foreach (Transform child in trDecoRoot)
                {
                    Building script = child.gameObject.GetComponent<Building>();
                    if (script != null)
                    {
                        script.Save(xmlDocument);
                    }
                }

                // ####### Encrypt the XML #######
                // If you want to view the original xml file, turn of this piece of code and press play.
                if (xmlDocument.DocumentElement.ChildNodes.Count >= 1)
                {
                    if (UseEncryption)
                    {
                        string data = BEUtil.Encrypt(xmlDocument.DocumentElement.InnerXml);
                        xmlDocument.DocumentElement.RemoveAll();
                        xmlDocument.DocumentElement.InnerText = data;
                    }
                    xmlDocument.Save(xmlFilePath);
                }
                // ###############################
            }
        }

        public void Load()
        {

            string xmlFilePath = BEUtil.pathForDocumentsFile(configFilename);
            if (!File.Exists(xmlFilePath))
            {
                Save();
                bFirstRun = true;
            }
            else
            {
                bFirstRun = false;
            }

            InLoading = true;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlFilePath);

            // ####### Encrypt the XML #######
            // If the Xml is encrypted, so this piece of code decrypt it.
            if (xmlDocument.DocumentElement.ChildNodes.Count <= 1)
            {
                if (UseEncryption)
                {
                    string data = BEUtil.Decrypt(xmlDocument.DocumentElement.InnerText);
                    xmlDocument.DocumentElement.InnerXml = data;
                }
            }
            //################################


            if (xmlDocument != null)
            {
                XmlElement element = xmlDocument.DocumentElement;
                XmlNodeList list = element.ChildNodes;
                foreach (XmlElement ele in list)
                {
                    if (ele.Name == "ConfigVersion") { ConfigVersion = int.Parse(ele.GetAttribute("value")); }
                    else if (ele.Name == "Time")
                    {
                        DateTime dtNow = DateTime.Now;
                        DateTime dtSaved = DateTime.Parse(ele.GetAttribute("value"));
                        //Debug.Log ("dtNow:"+dtNow.ToString());
                        //Debug.Log ("dtSaved:"+dtSaved.ToString());
                        TimeSpan timeDelta = dtNow.Subtract(dtSaved);
                        //Debug.Log ("TimeSpan:"+timeDelta.ToString());
                        BETime.timeAfterLastRun = timeDelta.TotalSeconds;
                    }
                    else if (ele.Name == "ExpTotal") { ExpTotal = int.Parse(ele.GetAttribute("value")); }
                    else if (ele.Name == "Gem") { Gem.ChangeTo(double.Parse(ele.GetAttribute("value"))); }
                    else if (ele.Name == "Gold") { Gold.ChangeTo(double.Parse(ele.GetAttribute("value"))); }
                    else if (ele.Name == "Elixir") { Elixir.ChangeTo(double.Parse(ele.GetAttribute("value"))); }
                    else if (ele.Name == "Shield") { Shield.ChangeTo(double.Parse(ele.GetAttribute("value"))); }
                    else if (ele.Name == "Building")
                    {
                        int Type = int.Parse(ele.GetAttribute("Type"));
                        int Level = int.Parse(ele.GetAttribute("Level"));
                        //Debug.Log ("Building Type:"+Type.ToString()+" Level:"+Level.ToString());
                        Building script = BEGround.instance.BuildingAdd(Type, Level);
                        script.Load(ele);
                    }
                    else { }
                }
            }

            InLoading = false;
        }

    }

}
