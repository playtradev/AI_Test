using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManagerScript : MonoBehaviour {

	public static UIManagerScript Instance;

	public TextMeshProUGUI Timer;


	private void Awake()
	{
		Instance = this;
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TimerUpdate(int timer)
	{
		Timer.text = "Day Timer:" + timer;
	}

}
