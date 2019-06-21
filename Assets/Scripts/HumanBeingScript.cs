using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HumanBeingScript : MonoBehaviour
{

	public delegate void BackHome();
	public event BackHome FinallyBackHome;

	public delegate void Reproduced();
	public event Reproduced ReproducedEvent;


	[Range(0, 1000)]
	public float HpMax;
	[Range(0, 1000)]
	public float HpMin;

	public float Hp;
	public float BaseHp;

	[Range(0, 1000)]
	public float AttackMax;
	[Range(0, 1000)]
	public float AttackMin;

	public float Attack;

	[Range(0, 1000)]
    public float SpeedMax;
	[Range(0, 1000)]
    public float SpeedMin;

	public float Speed;

	[Range(0, 1000)]
    public float Charity;

	[Range(0, 1000)]
    public float Gratitude;

	[Range(0, 1000)]
    public float Hate;
	[Range(1,100)]
	public float Hunger = 1;

	[Range(1, 100)]
	public float ReproductionPerc = 5;
	[Range(1, 100)]
    public float GivingPerc = 20;

	public float Food;

	public float GratitiudeFoodPerc = 10;
	public float CharityFoodPerc = 10;
	public float HateGratitutePerc = 10;
    public float HateHatePerc = 15;


	[Header("Charity")]
	public float CModifier = 1;
	public float CGModifier = 1;
	public float CHModifier = 1;
	public float CModifierHealth = 1;
	public float CModifierSpeed = 0;
	public float CModifierAttack = -0.5f;

	[Header("Gratitude")]
	public float GModifier = 1;
	public float GCModifier = 1;
	public float GHModifier = 1;
    public float GModifierHealth = -0.5f;
    public float GModifierSpeed = 1;
    public float GModifierAttack = 0;

	[Header("Hate")]
	public float HModifier = 1;
	public float HCModifier = 1;
	public float HGModifier = 1;
    public float HModifierHealth = 0;
    public float HModifierSpeed = -0.5f;
    public float HModifierAttack = 1;


	public HumanType HType = HumanType.None;
	public HousesTypes HouseType = HousesTypes.Center;
	public StateType CurrentState = StateType.Home;
	public ActionState CurrentAction = ActionState.None;

	public Material CharityM;
	public Material AttackM;
	public Material BegM;
	public Material BaseM;
	//Not visible in Inspector
	[HideInInspector]
	public NavMeshAgent Agent;
	[HideInInspector]
	public Transform TargetHouse;
	[HideInInspector]
	public HouseScript OwnHouse;
	[HideInInspector]
	public Vector3 TargetDest;
	[HideInInspector]
    public Vector3 TargetFoodDest;
	[HideInInspector]
	public bool IsStarted = false;
	[HideInInspector]
	public FoodScript TargetFood;
	[HideInInspector]
	public bool DidIFindFood = false;
	[HideInInspector]
	public bool CanIgetFood = true;
	[HideInInspector]
    public bool CanIAttack = true;
	[HideInInspector]
	public bool AmIActing = false;
	private IEnumerator FollowCo;
	private MeshRenderer MR;

	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
		MR = GetComponent<MeshRenderer>();
	}


	// Use this for initialization
	void Start () {
		StartCoroutine(Live());
		Speed = Random.Range(SpeedMin, SpeedMax) / 10;
		Hp = Random.Range(HpMin, HpMax);
		BaseHp = Hp;
		Attack = Random.Range(AttackMin, AttackMax);
		Agent.speed = Speed;
		GameManagerScript.Instance.DayStarted += Instance_DayStarted;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Vector3.Distance(transform.position, TargetHouse.position) < 1f && CurrentState == StateType.ComingBackHome)
		{
			CurrentState = StateType.Home;
            HomeSweetHome();
		}

		if(Vector3.Distance(transform.position, TargetDest) < 1.2f)
		{
			GoToPosition(TargetHouse.position);
			CurrentState = StateType.ComingBackHome;
		}

		if (Vector3.Distance(transform.position, TargetFoodDest) < 1.2f && CanIgetFood && CurrentState == StateType.FoodFound)
        {
            if (TargetFood.gameObject.activeInHierarchy)
            {
				Food += TargetFood.Food;
                TargetFood.gameObject.SetActive(false);
				DidIFindFood = true;
				GoToPosition(TargetHouse.position);
                CurrentState = StateType.ComingBackHome;
            }
			else
			{
				GoToPosition(TargetDest);
				CurrentState = StateType.LookingForFood;
			}
        }

		if(Hp < 0)
		{
			if(Food == 0)
			{
				GameManagerScript.Instance.HumanBeingDied();
                gameObject.SetActive(false);
			}
			else
			{
				Hp += Food;
				Food = 0;
				if (Hp > BaseHp)
                {
					Food = Hp - BaseHp;
                    Hp = BaseHp;
                }
			}
		}


		HType = Charity > Hate && Charity > Gratitude ? HumanType.Charity :
				 Hate > Charity && Hate > Gratitude ? HumanType.Hate :
				 Gratitude > Hate && Charity < Gratitude ? HumanType.Gratitude : HumanType.None;
		//Debug.Log(Vector3.Distance(transform.position, TargetDest) + "   " + name);
	}


	public void GoToRandomPos()
	{
		if(gameObject.activeInHierarchy)
		{
			IsStarted = true;
			Agent.destination = new Vector3(Random.Range(-GameManagerScript.Instance.GroundSizeWidth, GameManagerScript.Instance.GroundSizeWidth), 0,
			                                Random.Range(-GameManagerScript.Instance.GroundSizeHeight, GameManagerScript.Instance.GroundSizeHeight));
            TargetDest = Agent.destination;
		}
	}

	public void GoToPosition(Vector3 nextPos)
	{
		IsStarted = true;
		Agent.destination = nextPos;
	}

    public void HomeSweetHome()
	{
		if(Hp < BaseHp)
		{
			if(Food > 0)
			{
				Hp += Food;
				if(Hp > BaseHp)
				{
					OwnHouse.FoodStore += Hp - BaseHp;
					Hp = BaseHp;
				}
				else
				{
					Hp += OwnHouse.FoodStore;
					if (Hp > BaseHp)
                    {
                        OwnHouse.FoodStore = Hp - BaseHp;
                        Hp = BaseHp;
                    }
				}
			}
			else
			{
				Hp += OwnHouse.FoodStore;
                if (Hp > BaseHp)
                {
                    OwnHouse.FoodStore = Hp - BaseHp;
                    Hp = BaseHp;
                }
			}
		}
		Food = 0;
		FinallyBackHome();
		Reproduce();
		CanIgetFood = true;
		ResetAction();
	}

    public void Reproduce()
	{
		if(Random.Range(0,100) < ReproductionPerc)
		{
			
			GameManagerScript.Instance.Reproduction(TargetHouse);
		}
	}


	private void OnTriggerEnter(Collider other)
	{
		if(GameManagerScript.Instance.GameStatus == GameStateType.DayStarted)
		{
			if (other.tag == "Food" && CurrentState != StateType.FoodFound && CanIgetFood)
            {
                //Debug.Log("Food");
                TargetFoodDest = other.transform.position;
                GoToPosition(other.transform.position);
                TargetFood = other.GetComponent<FoodScript>();
                CurrentState = StateType.FoodFound;
            }
			else if (other.tag == "Human" && CurrentState != StateType.FoodFound && CurrentState != StateType.Home)
            {
				MeetOthers(other);
            }
            else if (other.tag == "House")
            {
                //Debug.Log("House");
            }
		}

	}

	private void OnTriggerStay(Collider other)
	{
		/*if (GameManagerScript.Instance.GameStatus == GameStateType.DayStarted)
		{
			if (other.tag == "Food" && CurrentState != StateType.FoodFound && CanIgetFood)
			{
				//Debug.Log("Food");
				TargetFoodDest = other.transform.position;
				GoToPosition(other.transform.position);
				TargetFood = other.GetComponent<FoodScript>();
				CurrentState = StateType.FoodFound;
			}
			else if (other.tag == "Human" && CurrentState != StateType.FoodFound && CurrentState != StateType.Home)
            {
                MeetOthers(other);
            }
		}*/
	}

	private IEnumerator Live()
	{
		int i = 0;
		while(true)
		{
			if(i == 13 && (CurrentState != StateType.Home && CurrentAction != ActionState.Fight && CurrentState != StateType.ComingBackHome))
			{
				GoToPosition(TargetHouse.position);
                CurrentState = StateType.ComingBackHome;
				CanIgetFood = false;
			}

			while(CurrentState == StateType.Home)
			{
				yield return new WaitForEndOfFrame();
			}

			Hp-=Hunger;

			yield return new WaitForSecondsRealtime(1);

			i++;
		}
	}

    void Instance_DayStarted()
	{
		GoToRandomPos();
        CurrentState = StateType.LookingForFood;
	}


	private void MeetOthers(Collider other)
	{
		if(CurrentAction == ActionState.None && CurrentState != StateType.Home)
		{
			
			HumanBeingScript human = other.GetComponent<HumanBeingScript>();
			if(HouseType != human.HouseType)
			{
				ActionState CurrentEnemyAction = human.GetCurrentAction(this);
                GetCurrentAction(human);

                switch (CurrentAction)
                {
                    case ActionState.None:
                        break;
                    case ActionState.Charity:
                        switch (CurrentEnemyAction)
                        {
                            case ActionState.Begging:
                                human.Food += (Food / 100) * GivingPerc + (Food / 100) * GratitiudeFoodPerc;
                                Food -= (Food / 100) * GivingPerc - (Food / 100) * CharityFoodPerc;
                                break;
                        }
                        Invoke("ResetAction", 5);
                        break;
                    case ActionState.Begging:
                        Invoke("ResetAction", 5);
                        break;
                    case ActionState.Fight:
                        switch (CurrentEnemyAction)
                        {
                            case ActionState.Charity:
                                AttackEnemy(human.transform);
                                break;
                            case ActionState.Begging:
                                Attack += (Attack / 100) * HateGratitutePerc;
                                AttackEnemy(human.transform);
                                break;
                            case ActionState.Fight:
                                Attack += (Attack / 100) * HateHatePerc;
                                AttackEnemy(human.transform);
                                break;
                        }
                        break;
                }
			}
		}
	}

    public void ResetAction()
	{
		CurrentAction = ActionState.None;
		MR.material = BaseM;
	}


	public ActionState GetCurrentAction(HumanBeingScript enemy)
	{
		if(CurrentAction == ActionState.None && CurrentState != StateType.Home)
		{
			
			if (DidIFindFood)
            {
                if (enemy.DidIFindFood)
                {
                    float AttackPerc = (Hate * 100) / (Charity + Hate);
                    CurrentAction = Random.Range(0, 99) < AttackPerc ? ActionState.Fight : ActionState.Charity;
                }
                else
                {
                    float CharityPerc = (Hate * 100) / (Charity + Hate);
                    CurrentAction = Random.Range(0, 99) < CharityPerc ? ActionState.Charity : ActionState.Fight;
                }
            }
            else
            {
                if (enemy.DidIFindFood)
                {
                    float AttackPerc = (Hate * 100) / (Gratitude + Hate);
                    CurrentAction = Random.Range(0, 99) < AttackPerc ? ActionState.Fight : ActionState.Begging;
                }
                else
                {
                    CurrentAction = ActionState.None;
                }
            }

            switch (CurrentAction)
            {
                case ActionState.Charity:
					Charity += CModifier;
					Gratitude += CGModifier;
					Hate += CHModifier;
					BaseHp += CModifierHealth;
					Speed += CModifierSpeed;
					Attack += CModifierAttack;
                    MR.material = CharityM;
                    break;
                case ActionState.Begging:
					Gratitude += GModifier;
					Charity += GCModifier;
					Hate += GHModifier;
					BaseHp += GModifierHealth;
					Speed += GModifierSpeed;
					Attack += GModifierAttack;
                    MR.material = BegM;
                    break;
                case ActionState.Fight:
					Hate += HModifier;
					Gratitude += HGModifier;
                    Charity += HCModifier;
					BaseHp += HModifierHealth;
					Speed += HModifierSpeed;
					Attack += HModifierAttack;
                    MR.material = AttackM;
                    break;
            }
		}

		return CurrentAction;

	}


	public void AttackEnemy(Transform humanT)
	{
		if(FollowCo == null)
		{
			FollowCo = FollowEnemy(humanT);
			StartCoroutine(FollowCo);
		}
	}

	public void UnderAttack(float damage)
	{
		Hp -= damage;
	}

	private IEnumerator FollowEnemy(Transform humanT)
	{
		bool EnemyAlive = true;
		HumanBeingScript Enemy = humanT.GetComponent<HumanBeingScript>();
		while(EnemyAlive)
		{
			GoToPosition(humanT.position);
			float Dist = Vector3.Distance(transform.position, humanT.position);
			if(Dist < 1f && CanIAttack)
			{
				CanIAttack = false;
				Invoke("AttackAction", 1);
				Enemy.UnderAttack(Attack);
			}

			if(!Enemy.gameObject.activeInHierarchy || Enemy.CurrentState == StateType.Home)
			{
				EnemyAlive = false;
			}
			yield return new WaitForEndOfFrame();
		}

		CurrentState = StateType.ComingBackHome;
		GoToPosition(TargetHouse.position);
	}


    private void AttackAction()
	{
		CanIAttack = true;
	}
}



public enum StateType
{
	Home = 0,
    LookingForFood = 1,
    FoodFound = 2,
    ComingBackHome = 3
}


public enum ActionState
{
	None,
    Fight,
    Charity,
    Begging
}

public enum HumanType
{
	None,
	Charity,
    Gratitude,
    Hate
}