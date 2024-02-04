using UnityEngine;
using UnityEngine.UI;
using System.Collections;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          UIDialogInfo
///   Description:    class for show building info
///   Usage :		  
///   Author:         BraveElephant inc.                    
///   Version: 		  v1.0 (2015-11-15)
///-----------------------------------------------------------------------------------------
namespace BE {

	public class UIDialogInfo : UIDialogBase {

		private static UIDialogInfo instance;

		private Building 		building = null;
		private BuildingType 	bt = null;
		//private BuildingDef 	bd = null;
		//private BuildingDef 	bdNext = null;
		//private BuildingDef 	bdLast = null;

		public	Text 			textTitle;
		public	Text 			textLevel;
		public	Image 			imgIcon;
		public	ProgressInfo []	progresses;
		public	Text 			textInfo;

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
		}

        void Reset()
        {
            //bd = null;
            bt = TBDatabase.GetBuildingType(building.Type);
            //bd = bt.GetDefine(building.Level);
            //bdNext = bt.GetDefine(building.Level+1);
            //bdLast = bt.GetDefLast();

            textTitle.text = bt.Name;
            textLevel.text = "Nivel " + building.Level.ToString();
            imgIcon.sprite = Resources.Load<Sprite>("Icons/Building/" + bt.Name);


            for (int i = 0; i < progresses.Length; ++i)
                progresses[i].gameObject.SetActive(true);


            // display progresses of building by building type
            if (bt.ID == 0)
            {
                // incase building si town hall, show gold capacity, elixir capacit and hitpoint
                textInfo.text = "Este é o coração da sua cidade. Atualize sua Prefeitura e desbloqueia novas moradias, serviços, urbanização e muito mais.";
                building.UIFillProgress(progresses[0], BDInfo.CapacityGold);
                building.UIFillProgress(progresses[1], BDInfo.CapacityElixir);
                progresses[2].gameObject.SetActive(false);
            }
            else if (bt.ID == 1)
            {
                // incase building is house, only show hitpoint, and disable other progresses
                textInfo.text = "Nada aqui é feito sem Agentes Publicos! Você pode contratar mais Agentes para iniciar vários projetos de construção , ou acelerar o seu trabalho usando Super Corações.";
                progresses[0].gameObject.SetActive(false);
                progresses[1].gameObject.SetActive(false);
                progresses[2].gameObject.SetActive(false);
            }            
            else if (bt.ID >= 2 && bt.ID < 20)
            {
				if (bt.ID >= 3 && bt.ID <= 6) textInfo.text = "As árvores ajudam a melhorar o clima da cidade, além de melhorar a qualidade de vida dos moradores.";
                else if (bt.ID >= 11 && bt.ID <=14) textInfo.text = "Moradias novas trazem qualidade de vida aos moradores de nossa cidade!!!";
                else if (bt.ID == 15) textInfo.text = "Casas Mãe possibilitam o aumento de vagas na Educação Infantil e precisamos sempre olhar com carinho à educação das crianças de nossa cidade!";
                else if (bt.ID == 16) textInfo.text = "Escolas Municipais  possibilitam o aumento de vagas na Educação Infantil e precisamos sempre olhar com carinho à educação das crianças de nossa cidade!";
                else if (bt.ID == 17) textInfo.text = "Na UBS, é possível receber atendimentos básicos e gratuitos em Pediatria, Ginecologia, Clínica Geral, Enfermagem e Odontologia.";
                else if (bt.ID == 18) textInfo.text = "As praças estarão sempre lotadas por conta do Internet, então compartilhe vida, compartilhe internet :)";
				else if (bt.ID == 19) textInfo.text = "As árvores ajudam a melhorar o clima da cidade, além de melhorar a qualidade de vida dos moradores.";
				else if (bt.ID >= 7 && bt.ID <= 9) textInfo.text = "Os empreendedores são importantes para o desenvolvimento economico da cidade.";
				else if (bt.ID == 10) textInfo.text = "O Centro de Tecnologia é a cidade rumo ao futuro.";
				//else if (bt.ID == 19)

                // Fazer os If elses da vida descrevendo todos os objetos
                building.UIFillProgress(progresses[0], BDInfo.Capacity);
                building.UIFillProgress(progresses[1], BDInfo.ProductionRate);
                progresses[2].gameObject.SetActive(false);
            }
            
            // Ainda Falta add a descricao dos objetos
            else { }           
        }
		
		public void OnButtonOk() {
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