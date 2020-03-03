using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class TravelingSpell : ThrowObjBehavior
{
	[SerializeField] float _speed = 80f;
	Transform _firePos = null;

	protected override void NetworkStart()
	{
		base.NetworkStart();
		networkObject.position = transform.position;
		networkObject.rotation = transform.rotation;

		networkObject.positionInterpolation.target = transform.position;
		networkObject.rotationInterpolation.target = transform.rotation;


		networkObject.SnapInterpolations();
	}

	void Start()
	{
		//_firePos = GameObject.FindGameObjectWithTag("ThrowPos").gameObject.transform;

		GetComponent<Rigidbody>().velocity = transform.forward * _speed;

		//GetComponent<Rigidbody>().AddForce(_firePos.transform.forward * 100);
	}

	void Update()
	{

		if (networkObject != null)
		{
			if (!networkObject.IsOwner)
			{
				transform.position = networkObject.position;
				transform.rotation = networkObject.rotation;
			}

			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
		}
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
