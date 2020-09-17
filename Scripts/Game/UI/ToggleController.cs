using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour 
{
	private  bool isOn;

    [SerializeField]
    private Color onColorBg = Color.white;
    [SerializeField]
    private Color offColorBg = Color.white;

    [SerializeField]
    private Image toggleBgImage = null;
    [SerializeField]
    private RectTransform toggle = null;

    [SerializeField]
    private GameObject handle = null;
	private RectTransform handleTransform;

	private float handleSize;
	private float onPosX;
	private float offPosX;

    [SerializeField]
    private float handleOffset = 0.0f;

    [SerializeField]
    private GameObject onIcon = null;
    [SerializeField]
    private GameObject offIcon = null;

    [SerializeField]
	private float speed = 0.0f;
	static float t = 0.0f;

	private bool switching = false;


	void Awake()
	{
		handleTransform = handle.GetComponent<RectTransform>();
		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleSize = handleRect.sizeDelta.x;
		float toggleSizeX = toggle.sizeDelta.x;
		onPosX = (toggleSizeX / 2) - (handleSize/2) - handleOffset;
		offPosX = onPosX * -1;

	}


	void Start()
	{
		if(isOn)
		{
			toggleBgImage.color = onColorBg;
			handleTransform.localPosition = new Vector3(onPosX, 0f, 0f);
			onIcon.gameObject.SetActive(true);
			offIcon.gameObject.SetActive(false);
		}
		else
		{
			toggleBgImage.color = offColorBg;
			handleTransform.localPosition = new Vector3(offPosX, 0f, 0f);
			onIcon.gameObject.SetActive(false);
			offIcon.gameObject.SetActive(true);
		}
	}
		
	void Update()
	{

		if(switching)
		{
			Toggle(isOn);
		}
	}

    private void DoYourStaff()
	{
		Debug.Log(isOn);
	}

    private void Switching()
	{
		switching = true;
	}



    private void Toggle(bool toggleStatus)
	{
        if (!onIcon.activeSelf || !offIcon.activeSelf)
        {
			onIcon.SetActive(true);
			offIcon.SetActive(true);
		}
		
		if(toggleStatus)
		{
			toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
			Transparency (onIcon, 1f, 0f);
			Transparency (offIcon, 0f, 1f);
			handleTransform.localPosition = SmoothMove(handle, onPosX, offPosX);
		}
		else 
		{
			toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
			Transparency (onIcon, 0f, 1f);
			Transparency (offIcon, 1f, 0f);
			handleTransform.localPosition = SmoothMove(handle, offPosX, onPosX);
		}
			
	}


	Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
	{
		
		Vector3 position = new Vector3 (Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
		StopSwitching();
		return position;
	}

	Color SmoothColor(Color startCol, Color endCol)
	{
		Color resultCol;
		resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
		return resultCol;
	}

	CanvasGroup Transparency (GameObject alphaObj, float startAlpha, float endAlpha)
	{
		CanvasGroup alphaVal;
		alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
		alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
		return alphaVal;
	}

	void StopSwitching()
	{
		if(t > 1.0f)
		{
			switching = false;

			t = 0.0f;
			switch(isOn)
			{
			case true:
				isOn = false;
				DoYourStaff();
				break;

			case false:
				isOn = true;
				DoYourStaff();
				break;
			}

		}
	}

}
