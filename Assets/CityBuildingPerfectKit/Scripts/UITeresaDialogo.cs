using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace BE {

    public class UITeresaDialogo : UIDialogBase
    {
        public static UITeresaDialogo instance;

        void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }

      
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _Hide();
            }
        }

        void _Show()
        {
            Time.timeScale = 0;
            ShowProcess();
        }

        public static void Show() {
            instance._Show();
            //print("chamou tela teresa");
        }
        public static void Hide() {
            instance._Hide();
        }

        // Chama o a Classe UIDialogoTeresaMensagem
        // Atribui a mensagem que será chamada na tela do facebook,
        public void MostrarProximaMensagem ()
        {
            Hide();
            //UITeresaFacebook.Show("Tenho tralhado para cuidar das pessoas, mas tem muito para se fazer", "Proxima", null, null, true);
            UIDialogoTeresaMensagem.Show("Temos trabalhado para cuidar das pessoas, mas tem muito para se fazer." +
                                        "\n Você deve cumprir as missões durante o jogo." +
                                         "\n Sua primeira missão é contruir 6 Casas." +
                                         "\n Com o passar do tempo lembre-se de evoluir as casas de seus habitantes", 
                                         
                                         "Proxima", null, null);
            UIDialogoTeresaMensagem.ProximaMensagemtxt = "Vamos Iniciar!!!"+
                "\n Compartilhe com seus amigos no Facebook e você ganhará Super Corações para acelerar o desenvolvimento da Cidade";
        }
    }

}
