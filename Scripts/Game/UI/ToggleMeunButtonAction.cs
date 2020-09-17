using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ToggleMeunButtonAction : MonoBehaviour
{
    [SerializeField] private GameObject popBtn = null;
    [SerializeField] private bool isUp;

    public void OnClickTurn()
    {
        if (isUp)
        {
            transform.rotation = Quaternion.Euler(0,0,180);
            popBtn.gameObject.SetActive(true);
            isUp = false;

            Debug.Log("Popup on");

        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            popBtn.gameObject.SetActive(false);
            isUp = true;

            Debug.Log("Popup off");
        }
    }
}
