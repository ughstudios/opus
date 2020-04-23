using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;


public class TravelingSpell : ThrowObjBehavior
{
	[SerializeField] float _speed = 80f;
	[SerializeField] float	_duration = 0.0f,
							_initDuration = 0.0f,
							_destroyTimeMargin = 2.0f;

	[SerializeField] int _damageAmount = 5;
	Transform _firePos = null;
	[SerializeField] bool	_didHit = false,
							_useGravity = false,
							_reverse = false;


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
		GetComponent<Rigidbody>().useGravity = _useGravity;

		GetComponent<Rigidbody>().velocity = _reverse ? -transform.forward * _speed : transform.forward * _speed;
	}

	void Update()
	{

		//transform.position += (transform.forward * _speed * Time.deltaTime);

		if (networkObject != null)
		{
			if (!networkObject.IsOwner)
			{
				//position and rotation
				transform.position = networkObject.position;
				transform.rotation = networkObject.rotation;
				
				_didHit = networkObject.didHit;
			}

			if (networkObject.IsOwner)
			{
				networkObject.position = transform.position;
				networkObject.rotation = transform.rotation;

				networkObject.didHit = _didHit;
			}	
		}

		if (_didHit)
		{
			float destroyTime = _initDuration + _destroyTimeMargin;
			_duration -= Time.deltaTime;

			if (_duration <= -destroyTime)
			{
				networkObject.Destroy();
			}
		}
	}


	void OnCollisionEnter(Collision col)
	{
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		if (GetComponentInChildren<MeshRenderer>() != null)
			GetComponentInChildren<MeshRenderer>().enabled = false;
		else
		{
			Destroy(GetComponent<ParticleSystem>());
		}

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
