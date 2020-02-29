using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelingSpell : MonoBehaviour
{
	[SerializeField] float _speed = 80f;
	Transform _firePos = null;

	void Start()
	{
		_firePos = GameObject.FindGameObjectWithTag("ThrowPos").gameObject.transform;

		GetComponent<Rigidbody>().velocity = _firePos.transform.forward * _speed;
		//GetComponent<Rigidbody>().AddForce(_firePos.transform.forward * 100);
	}

	void OnCollisionEnter()
	{
		GetComponentInChildren<MeshRenderer>().enabled = false;

		gameObject.transform.GetChild(1).transform.gameObject.SetActive(true);

		float time = gameObject.transform.GetChild(1).transform.gameObject.GetComponent<ParticleSystem>().time;
		float duration = gameObject.transform.GetChild(1).transform.gameObject.GetComponent<ParticleSystem>().main.duration;

		if (time == duration)
			Destroy(gameObject);
	}

	public void SetSpeed(float speed)
	{
		if (speed > 0) _speed = speed;
	}
}
