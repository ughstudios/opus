using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateFire : MonoBehaviour
{
	[SerializeField] GameObject _fireAttackPS = null;
	[SerializeField] GameObject _fireAttackHolder = null;
	

	public void FireAttack()
	{
		_fireAttackHolder.SetActive(true);
	}

	public void StopFireAttack()
	{
		_fireAttackHolder.SetActive(false);
	}

}
