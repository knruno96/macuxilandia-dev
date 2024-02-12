using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


namespace BE
{
    public class DetectaColisao : MonoBehaviour
    {
        public static bool podeCriar = true;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /*
        void OnCollisionExit(Collision col)
        {
            if (col.gameObject.tag == "PodeCriar")
            {
                // Debug.Log("Colidiu com o trigger " + other.transform.name);
                podeCriar = true;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.tag == "PodeCriar")
            {
                // Debug.Log("Colidiu com o trigger " + other.transform.name);
                podeCriar = false;
            }
        }
        */


        void OnTriggerExit(Collider col)
        {
            if (col.gameObject.tag == "PodeCriar")
            {
                //Debug.Log("Colidiu com o trigger " + col.transform.name);
                podeCriar = true;
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag == "PodeCriar")
            {
                //Debug.Log("Colidiu com o trigger " + col.transform.name);
                podeCriar = false;
            }
        }
    }
}
