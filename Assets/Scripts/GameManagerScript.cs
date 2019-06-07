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

	[Range(0, 10)]
	public int MaxNumChildren = 3;

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

    [Range(0,100)]
	public float GroundSizeWidth = 40;
	[Range(0, 100)]
    public float GroundSizeHeight = 40;

	[HideInInspector]
	private int ReproducedLastDay;
	[HideInInspector]
    private int DiedLastDay;



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

		UIManagerScript.Instance.InfoUpdate(HumansList.Where(r=>r.gameObject.activeInHierarchy && r.HType == HumanType.Charity).ToList().Count.ToString(),
		                                    HumansList.Where(r =>r.gameObject.activeInHierarchy &&  r.HType == HumanType.Gratitude).ToList().Count.ToString(),
		                                    HumansList.Where(r =>r.gameObject.activeInHierarchy &&  r.HType == HumanType.Hate).ToList().Count.ToString());

		UIManagerScript.Instance.NumberOfEntity.text = HumansList.Where(r => r.gameObject.activeInHierarchy).ToList().Count.ToString();

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
			hbs.OwnHouse = house.GetComponent<HouseScript>();
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
		Vector3 res = new Vector3(Random.Range(-GroundSizeWidth,GroundSizeWidth),y, Random.Range(-GroundSizeWidth, GroundSizeWidth));

		return res;
	}

	public void Reproduction(Transform home)
	{
		for (int i = 0; i < MaxNumChildren; i++)
		{
			GameObject human = Instantiate(Human, home.position, Quaternion.identity, HumansContainer);
            HumanBeingScript hbs = human.GetComponent<HumanBeingScript>();
            HumansList.Add(hbs);
            hbs.TargetHouse = home;
            hbs.FinallyBackHome += Hbs_FinallyBackHome;
			hbs.OwnHouse = home.GetComponent<HouseScript>();
			ReproducedLastDay++;
		}
	}

    public void HumanBeingDied()
	{
		DiedLastDay++;
	}


    public void DayStarting()
	{
		UIManagerScript.Instance.InfoDailyUpdate(ReproducedLastDay.ToString(), DiedLastDay.ToString());
		ReproducedLastDay = 0;
		DiedLastDay = 0;
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
		UIManagerScript.Instance.AddDay();
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