using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConheceEquipe : MonoBehaviour {

    public void ConhecaEquipe()
    {
      SceneManager.LoadScene("Creditos");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MenuLogin");
    }
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Town");
    }
}
