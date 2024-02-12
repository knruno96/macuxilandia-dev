using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

///-----------------------------------------------------------------------------------------
///   Namespace:      BE
///   Class:          UIInfo
///   Description:    class for diaplay building info & progress
///   Usage :		  
///   Author:         BraveElephant inc.                    
///   Version: 		  v1.0 (2015-11-15)
///-----------------------------------------------------------------------------------------
namespace BE {
	
	public class UIInfo : MonoBehaviour {
		
		public 	CanvasGroup groupRoot;
		public 	CanvasGroup groupInfo;
		public 	Text 		Name;
		public 	Text 		Level;

		public 	CanvasGroup groupProgress;
		public 	Text 		TimeLeft;
		public 	Image 		Progress;
		public 	Image 		Icon;

		public 	CanvasGroup groupCollect;
		public 	Image 		CollectDialog;
		public 	Image 		CollectIcon;

		public	Building 	building = null;


		void Update () {
            //building.Collect();
            // especifica Tipo de predio e faz o if para coletar da prefeitura
            /*
            List<Building> buildings = BEGround.instance.GetBuildingsByType(0);
            for (int i = 0; i < buildings.Count; i++)
            {
                // Atualiza a prefeitura
                buildings[i].Collect();
                Debug.Log(buildings[i].Upgrade());
            }
            */
        }

		// when user clicked collect dialog
		public void OnButtonCollect() {
			// do collect
			building.Collect();
		}
		
	}
	
}