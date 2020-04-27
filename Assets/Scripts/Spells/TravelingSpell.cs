using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;


public class TravelingSpell : ThrowObjBehavior
{
	[SerializeField] float _speed = 80f;
	[SerializeField] float	_duration = 0.0f,
							_initDuration = 0.0f,
							_destroyTimeMargin = 2.0f,
							_maxLifeTime = 6.0f;

	[SerializeField] int _damageAmount = 5;
	Transform _firePos = null;
	[SerializeField] bool	_didHit = false,
							_useGravity = false,
							_reverse = false;

	public string playerWhoSpawnedUs;
	public uint _netId;

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
				if (networkObject != null)
					networkObject.Destroy();
				else
					Destroy(gameObject);
			}
		}

		//in case someone fires into the sky to prevent the spell from traveling forever
		_maxLifeTime -= Time.deltaTime;

		if (_maxLifeTime <= 0.0f)
		{
			if (networkObject != null)
				networkObject.Destroy();
			else
				Destroy(gameObject);

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
			if(col.gameObject.GetComponent<NewCharacterController>().playerName != playerWhoSpawnedUs)//so we can't hurt ourselves with our own attacks 
			{
				DamageableEntity de = col.gameObject.GetComponent<DamageableEntity>();
				Debug.Log("TravellingSpell::playerWhoSpawnedUs: " + playerWhoSpawnedUs);
				de.TakeDamage(playerWhoSpawnedUs, _damageAmount);
				GetComponent<Collider>().enabled = false;
			}
		}
			
	}

	public void SetSpeed(float speed)
	{
		if (speed > 0) _speed = speed;
	}

}
