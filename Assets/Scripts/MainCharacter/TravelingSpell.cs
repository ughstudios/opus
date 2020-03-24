using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class TravelingSpell : ThrowObjBehavior
{
	[SerializeField] float _speed = 80f;
	[SerializeField] float	_duration = 0.0f,
							_initDuration = 0.0f,
							_destroyTimeMargin = 2.0f;
	[SerializeField] int _damageAmount = 5;
	Transform _firePos = null;
	[SerializeField] bool _didHit = false;



	protected override void NetworkStart()
	{
		base.NetworkStart();
		networkObject.position = transform.position;
		networkObject.rotation = transform.rotation;

		networkObject.positionInterpolation.target = transform.position;
		networkObject.rotationInterpolation.target = transform.rotation;

		networkObject.didHit = _didHit;

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
				_didHit = networkObject.didHit;
			}

			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
			networkObject.didHit = _didHit;
		}

		if (_didHit)
		{
			float destroyTime = _initDuration + _destroyTimeMargin;
			_duration -= Time.deltaTime;

			if(_duration <= -destroyTime)
				Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision col)
	{
		GetComponentInChildren<MeshRenderer>().enabled = false;

		gameObject.transform.GetChild(1).transform.gameObject.SetActive(true);

		float time = gameObject.transform.GetChild(1).transform.gameObject.GetComponent<ParticleSystem>().time;
		_duration = gameObject.transform.GetChild(1).transform.gameObject.GetComponent<ParticleSystem>().main.duration;
		_initDuration = gameObject.transform.GetChild(1).transform.gameObject.GetComponent<ParticleSystem>().main.duration;
		_didHit = true;

		if (col.gameObject.tag == "Player")
		{
			DamageableEntity de = col.gameObject.GetComponent<DamageableEntity>();

			de.TakeDamage(de, _damageAmount);
		}
			
	}

	public void SetSpeed(float speed)
	{
		if (speed > 0) _speed = speed;
	}
}
