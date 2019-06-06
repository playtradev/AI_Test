using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour {

	public delegate void StartDay();
	public event StartDay DayStarted;


	public static GameManagerScript Instance;

	public GameStateType GameStatus = GameStateType.Intro;

    [Range(0,100)]
	public int DayTime = 30;

	[Range(1,500)]
	public int Humans = 10;

	[Range(1, 500)]
    public int FoodPerDay = 10;

	[Header("GameElements")]
	public GameObject House;

	public GameObject Human;

    public GameObject Food;

	public Transform HousesContainer;
    
	public Transform HumansContainer;

	public Transform FoodContainer;

	[Header("info")]
	public int HumansAtHome = 0;

	//Not visible in Inspector
	[HideInInspector]
	public List<HumanBeingScript> HumansList = new List<HumanBeingScript>();

	[HideInInspector]
	public List<HouseScript> HousesList = new List<HouseScript>();

	[HideInInspector]
	public List<FoodScript> FoodsList = new List<FoodScript>();

	private IEnumerator DayTimeCoroutine;

	private void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		StartGame();
		Invoke("DayStarting", 3);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.A))
		{
			SceneManager.LoadScene(0);
		}
	}

    public void StartGame()
	{
		for (int i = 0; i < Humans; i++)
        {
			GameObject house = Instantiate(House, GetFreeSpaceOnGround(1.5f), Quaternion.identity, HousesContainer);
			HousesList.Add(house.GetComponent<HouseScript>());
			GameObject human = Instantiate(Human, house.transform.position, Quaternion.identity, HumansContainer);
			HumanBeingScript hbs = human.GetComponent<HumanBeingScript>();
			HumansList.Add(hbs);
			hbs.TargetHouse = house.transform;
			hbs.FinallyBackHome+= Hbs_FinallyBackHome;
        }

		for (int i = 0; i < FoodPerDay; i++)
        {
			GameObject food = Instantiate(Food, FoodContainer);
			food.SetActive(false);
			FoodsList.Add(food.GetComponent<FoodScript>());

        }

		GroundScript.Instance.RunTimeBake();
	}
    
    public void SetFood()
	{
		for (int i = 0; i < FoodsList.Count; i++)
		{
			FoodsList[i].gameObject.SetActive(true);
			FoodsList[i].transform.position = GetFreeSpaceOnGround(1.5f);
		}
	}
    

	public Vector3 GetFreeSpaceOnGround(float y)
	{
		Vector3 res = new Vector3(Random.Range(-40,40),y, Random.Range(-40, 40));

		return res;
	}

	public void Reproduction(Transform home)
	{
		GameObject human = Instantiate(Human, home.position, Quaternion.identity, HumansContainer);
        HumanBeingScript hbs = human.GetComponent<HumanBeingScript>();
        HumansList.Add(hbs);
		hbs.TargetHouse = home;
        hbs.FinallyBackHome += Hbs_FinallyBackHome;
	}


    public void DayStarting()
	{
		HumansAtHome = 0;
		if(DayTimeCoroutine != null)
		{
			StopCoroutine(DayTimeCoroutine);

		}
		DayTimeCoroutine = DayTimerCo();
		StartCoroutine(DayTimeCoroutine);
	}

	public IEnumerator DayTimerCo()
	{
		SetFood();
		DayStarted();
		GameStatus = GameStateType.DayStarted;
		int i = DayTime;
		while(i > 0)
		{
			UIManagerScript.Instance.TimerUpdate(i);
			yield return new WaitForSecondsRealtime(1);
			i--;
		}
		GameStatus = GameStateType.EndOfDay;
		Invoke("DayStarting", 1);
	}

	void Hbs_FinallyBackHome()
	{
		HumansAtHome++;
		if(HumansAtHome == HumansList.Where(r=> r.gameObject.activeInHierarchy).ToList().Count)
		{
			Invoke("DayStarting", 1);
		}
	}

}


public enum GameStateType
{
	Intro,
    DayStarted,
    EndOfDay
}