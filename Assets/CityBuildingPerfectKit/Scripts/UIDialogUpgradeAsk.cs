using UnityEngine;
using UnityEngine.UI;
using System.Collections;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          UIDialogUpgradeAsk
///   Description:    class for building upgrade ask dialog
///   Usage :		  
///   Author:         BraveElephant inc.                    
///   Version: 		  v1.0 (2015-11-15)
///-----------------------------------------------------------------------------------------
namespace BE {

	// what kind of building info
	public enum BDInfo {
		None				= -1,
		CapacityGold		= 0, 	
		CapacityElixir		= 1, 	
		Capacity			= 2, 	
		ProductionRate		= 3,	
		HitPoint			= 4,
		StorageCapacity		= 5,
	}

	// use this class to show progress value
	[System.Serializable]
	public class ProgressInfo {
		public	GameObject 	gameObject;
		public	Image 		imageIcon;
		public	Text 		textInfo;
		public	Image 		imageMiddle;
		public	Image 		imageFront;
	}

	public class UIDialogUpgradeAsk : UIDialogBase {

		private static UIDialogUpgradeAsk instance;

		private Building 		building = null;
		private BuildingType 	bt = null;
		//private BuildingDef 	bd = null;
		private BuildingDef 	bdNext = null;
		//private BuildingDef 	bdLast = null;
		private bool 			Available = true;
		private bool 			TownLevelOk = true;

		public	Text 			textTitle;
		//public	Text 			textLevel;
		public	Image 			imgIcon;
		public	ProgressInfo []	progresses;
		public	GameObject 		goNote;
		public	Text 			NoteInfo;
		public	GameObject 		goNormal;
		public	Text 			textBuildTime;
		public	Image 			PriceIcon;
		public	Text 			Price;
		public	Text 			PriceGem;

		private int				GemCount = 0;


		void Awake () {
			instance=this;
		}
		
		void Start () {
			gameObject.SetActive(false);
		}
		
		void Update () {

			if (!UIDialogMessage.IsShow() && Input.GetKeyDown(KeyCode.Escape)) { 
				_Hide();
			}

			if(bdNext != null)
				Available = bdNext.PriceInfoCheck(Price);
		}

		void Reset () {
			//bd = null;

			bt = TBDatabase.GetBuildingType(building.Type);
			//bd = bt.GetDefine(building.Level);
			bdNext = bt.GetDefine(building.Level+1);
			//bdLast = bt.GetDefLast();
			
			textTitle.text = "Atualizar "+bt.Name+" para o Nivel "+(building.Level+1).ToString ()+" ?";
			imgIcon.sprite = Resources.Load<Sprite>("Icons/Building/"+bt.Name);

			for(int i=0 ; i < progresses.Length ; ++i) 
				progresses[i].gameObject.SetActive(true);
			
			// display progresses of building by building type
			if(bt.ID == 0) {
				// incase building si town hall, show gold capacity, elixir capacit and hitpoint
				building.UIFillProgressWithNext(progresses[0], BDInfo.CapacityGold);
				building.UIFillProgressWithNext(progresses[1], BDInfo.CapacityElixir);
				building.UIFillProgressWithNext(progresses[2], BDInfo.HitPoint);
			}
			else if(bt.ID == 1) {
				// incase building is house, only show hitpoint, and disable other progresses
				building.UIFillProgressWithNext(progresses[0], BDInfo.HitPoint);
				progresses[1].gameObject.SetActive(false);
				progresses[2].gameObject.SetActive(false);
			}
            // Ainda Falta add a Evolucao dos objetos
            else if (bt.ID >= 2 && bt.ID < 20) {
                building.UIFillProgressWithNext(progresses[0], BDInfo.CapacityGold);
                building.UIFillProgressWithNext(progresses[1], BDInfo.CapacityElixir);
                progresses[2].gameObject.SetActive(false);
            }			
			else {}

			// get townhall to check upgrade requires next townhall
			Building buildingTown = BEGround.instance.Buildings[0][0];
			if(bdNext.TownHallLevelRequired > buildingTown.Level) {
				goNote.SetActive(true);
				NoteInfo.text = "Para atualizar este edificio , primeiro você precisa da Prefeitura no Nível " + bdNext.TownHallLevelRequired.ToString ()+"!";
				TownLevelOk = false;
				goNormal.SetActive(false);
			}
			else {
				goNote.SetActive(false);
				TownLevelOk = true;
				goNormal.SetActive(true);

				// set infos about upgrade
				bdNext.PriceInfoApply(PriceIcon, Price);
				textBuildTime.text = BENumber.SecToString(bdNext.BuildTime);
                // Controla o valor de atualizao instantanea para o item descontar os SuperCoraçoes

                GemCount = (bdNext.BuildTime + 1)/10; // 1 gem per minute                
                if (GemCount < 2) GemCount = 3; // Se resultado for menor que 2 atribui 3 como valor
                PriceGem.text = GemCount.ToString ("#,##0");
				PriceGem.color = (SceneTown.Gem.Target() < GemCount) ? Color.red : Color.white;
			}
		}

		// when user clicked upgrade button
		public void OnButtonOk() {

			// if upgrade is available, then upgrade
			if(Available && TownLevelOk) {
				if(building.Upgrade()) {
					// set this building to worker
					BEWorkerManager.instance.SetWorker(building);
					SceneTown.instance.BuildingSelect(null);
				}
				
				_Hide();
			}
			else {
				UIDialogMessage.Show("Mais recursos são necessários", "Ok", "Erro");
			}
		}
		
		public void OnButtonInstant() {
			if(!TownLevelOk) {
				// upgrade not available
				return;
			}

			// checkuser has enough gem count
			if(SceneTown.Gem.Target() < GemCount) {
				// you need more gem
                //
				UIDialogMessage.Show("Mais Corações do 15 são necessários", "Ok", "Erro", TBDatabase.GetPayTypeIcon(PayType.Gem));

				return;
			}

			// decrease gem count and upgrade immediately
            // Desconta SuperCoracoes
            SceneTown.Gem.ChangeDelta(-GemCount);
            building.UpgradeEnd();
            SceneTown.instance.BuildingSelect(null);

            _Hide();
		}
		
		public void _Show(Building script) {
			
			building = script;
			
			Reset();
			ShowProcess();
		}
		
		public static void Show(Building script) 	{ instance._Show(script); }
		public static void Hide() 					{ instance._Hide(); }
	}
}