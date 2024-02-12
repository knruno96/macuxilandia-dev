using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControleMenu : MonoBehaviour
{
    public GameObject Teresa15;
    public GameObject Friendsoft;
    void Start()
    {
        StartCoroutine(Carregando());

    }

    IEnumerator Carregando()
    {               
        yield return new WaitForSeconds(3);
        Friendsoft.SetActive(false);
        Teresa15.SetActive(true);        
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("MenuLogin");
    }       


}