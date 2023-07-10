using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{

	private FSM monsterFSM;

	// Start is called before the first frame update
	void Start()
	{
		FSMState calmState = new FSMState();
		FSMState annoyedState = new FSMState();
		FSMState replenishState = new FSMState();
		FSMState angryState = new FSMState();
		FSMState berserkState = new FSMState();
	}


	





	// Update is called once per frame
	void Update()
	{
		
	}
}
