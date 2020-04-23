using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{
	private bool canPlant = true;
	public Camera serverCam;
	public GameObject camera = null;
	public GameObject _testSpell = null;
	public Transform _throwPos = null;
	public bool _isDead = false; //Set _loadFromServer to true if prefab is to be loaded to server, don't apply to prefab
	public RectTransform _healthTransform = null;
	public float _healthCanvasValue = 0.0f;
	public GameObject _hudCanvas = null;

	#region Attacks
	[SerializeField] float	_poisonCoolDownTime = 1.5f,
							_fireCoolDownTime = 3.0f,
							_initFireCoolTime = 0.0f,
							_fireCanvasVal = 0.0f,
							_flameRate = 3,
							_fireRefillRate = 3.0f,
							_initCoolDownTime = 0.0f,
							_coolDownRate = 1.0f;

	[SerializeField] bool	_startBaseCoolDown = false,
							_startFlameCoolDown = false,
							_startFireAttack = false,
							_flameInstantiated = false;

	public RectTransform	_poisonReflillTransform = null,
							_fireReflillTransform = null;

	[SerializeField] GameObject	_fireAttackPS = null;
	[SerializeField] Transform	_fireAttackPos = null;

	//these ints are for the animations, networking anims not working as bools
	[SerializeField] int	fireInt = 0,
							aimInt = 0,
							hasSnipped = 0;

	[SerializeField] bool	_flamesStarted = false,
							_isAiming = false,
							_isSniping = false;

	Vector3 _initCamAttachedPos = new Vector3(0,4,-10);

	float	newCamPosX = 4f,
			newCamPosZ = -10f;

	[SerializeField] GameObject _crossHair = null;
	#endregion

	protected override void NetworkStart()
	{
		base.NetworkStart();

		networkObject.position = transform.position;

		if (!UseChild) networkObject.rotation = transform.rotation;
		if (UseChild) networkObject.rotation = transform.GetChild(0).transform.rotation;

		networkObject.positionInterpolation.target = transform.position;

		if(!UseChild) networkObject.rotationInterpolation.target = transform.rotation;
		if (UseChild) networkObject.rotationInterpolation.target = transform.GetChild(0).transform.rotation;

		networkObject.SnapInterpolations();

		networkObject.health = health;
		networkObject.isDead = _isDead;
	}

	protected override void Start()
	{
		base.Start();

		Cursor.visible = false;
		//Physics.gravity = new Vector3(0,-50,0);//This is for the entire system
		_initCoolDownTime = _poisonCoolDownTime;
		_initFireCoolTime = _fireCoolDownTime;

		if (GameObject.FindGameObjectWithTag("ServerCamera") != null)
			serverCam = GameObject.FindGameObjectWithTag("ServerCamera").GetComponent<Camera>();
	}
	
    void OnTriggerStay(Collider collider)
	{
		if (collider.gameObject.tag == "Water Hex")
		{
			if (Input.GetButtonDown("Gather"))
			{
				
			}
		}
		canPlant = false;
	}

	protected override void Update()
	{
		base.Update();
		
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (health <= 0)
		{
			_isDead = true;
		}

		
		if (_isAiming) aimInt = 1;
		if (!_isAiming) aimInt = 0;

		animator.SetInteger("fireInt", fireInt);
		animator.SetInteger("aimInt", aimInt);
		animator.SetInteger("hasSnipped", hasSnipped);

		//when networking animations that require a button to be held use an int not a bool to control the animation!!!
		/*TEMP DISABLED TO FIX POSSIBLE CACHE
		if (networkObject != null)
		{
			if (networkObject.IsOwner)
			{
				networkObject.fireInt = animator.GetInteger("fireInt");
				networkObject.aimInt = animator.GetInteger("aimInt");
				networkObject.hasSnipped = animator.GetInteger("hasSnipped");
			}

			if (!networkObject.IsOwner)
			{
				if (animator.GetInteger("fireInt") != networkObject.fireInt)
					animator.SetInteger("fireInt", networkObject.fireInt);

				if (animator.GetInteger("aimInt") != networkObject.aimInt)
					animator.SetInteger("aimInt", networkObject.aimInt);

				if (animator.GetInteger("hasSnipped") != networkObject.hasSnipped)
					animator.SetInteger("hasSnipped", networkObject.hasSnipped);
			}
		}

		if (networkObject != null)
		{
			if (networkObject.IsOwner)
			{
				if(serverCam != null)
					serverCam.enabled = false;

				_hudCanvas.SetActive(true);
				camera.SetActive(true);

				if (UseChild)
				{
					networkObject.runningVal = animator.GetInteger("runningVal");
					networkObject.isJumping = animator.GetBool("isJumping");
					networkObject.onGround = animator.GetBool("onGround");
					networkObject.isThrowing = animator.GetBool("isThrowing");
					networkObject.horizontalVal = animator.GetInteger("horizontalVal");
					networkObject.hasSnipped = animator.GetInteger("hasSnipped");
				}

				networkObject.health = health;
				networkObject.isDead = _isDead;
			}
		}

		if (networkObject != null && networkObject.IsOwner)
			HealthCanvasMeter();

		ThrowAttack();
		FireIntAttack();
		Aim();
		SnipAttack();

		if (_startBaseCoolDown) CoolDown();

		HudAttackMeter(_poisonReflillTransform, _poisonCoolDownTime);//For base attack, poison
		HudAttackMeter(_fireReflillTransform, _fireCanvasVal);//For fire attacke

		
	}

	protected override void FixedUpdate()
	{
		if (networkObject != null)
		{
			if (!networkObject.IsOwner)
			{
				transform.position = networkObject.position;

				if (!UseChild)
					transform.rotation = networkObject.rotation;

				if(UseChild)
					transform.GetChild(0).transform.rotation = networkObject.rotation;


				if (UseChild)
				{
					if (animator.GetInteger("runningVal") != networkObject.runningVal) animator.SetInteger("runningVal", networkObject.runningVal);
					if (animator.GetBool("isJumping") != networkObject.isJumping) animator.SetBool("isJumping", networkObject.isJumping);
					if (animator.GetBool("onGround") != networkObject.onGround) animator.SetBool("onGround", networkObject.onGround);
					if (animator.GetBool("isThrowing") != networkObject.isThrowing) animator.SetBool("isThrowing", networkObject.isThrowing);
					if (animator.GetInteger("horizontalVal") != networkObject.horizontalVal) animator.SetInteger("horizontalVal", networkObject.horizontalVal);
				}

				health = networkObject.health;
				_isDead = networkObject.isDead;

				if (serverCam != null)
					serverCam.enabled = true;

				_hudCanvas.SetActive(false);
				camera.SetActive(false);

				return;
			}
		}	

		/*
		moveInput.Set(Input.GetAxisRaw("Horizontal"),
				Input.GetButton("Jump") ? 1f : 0f,
				Input.GetAxisRaw("Vertical"));
		*/

		base.FixedUpdate();

		if (canPlant && isGrounded && groundCollider != null &&
				groundCollider.GetComponent<DamageableEntity>() == null
				&& Input.GetButton("Gather"))
		{
			if (GetComponent<Inventory>() != null)
			{
				Inventory inv = GetComponent<Inventory>();
				RaycastHit hit;
				Item toPlant = inv.GetRandomPlantable();
				if (toPlant != null && Physics.Raycast(transform.position,
						Vector3.down, out hit, 5f))
				{
					Harvestable plant = Instantiate<Harvestable>(toPlant.plantPrefab,
							hit.point, Quaternion.Euler(0f, Random.value * 360f, 0f));
					plant.canHarvest = false;
					plant.ForceUpdate();
					inv.RemoveCountOfItemFromInventory(toPlant, 1);
				}
			}
		}

		if (networkObject != null)
		{
			networkObject.position = transform.position;

			if(!UseChild)
				networkObject.rotation = transform.rotation;

			if(UseChild)
				networkObject.rotation = transform.GetChild(0).transform.rotation;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		canPlant = true;
	}

	public override void TakeDamage(string killingPlayer, int damage)
	{
		base.TakeDamage(killingPlayer, damage);
	}

	

	private bool IsUnder(Collider col)
	{
		RaycastHit[] hits = Physics.SphereCastAll(transform.position + groundCheckOffset, groundCheckRadius,
				Vector3.down, groundCheckDist);
		foreach (RaycastHit hit in hits)
		{
			if (hit.collider == col)
				return true;
		}
		return false;
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		GroundCheck();
		if (IsUnder(collision.collider))
			base.OnCollisionEnter(collision);
	}

	void HealthCanvasMeter()
	{
		if (health < 0)
			_healthCanvasValue = 0.0f;
		if (health > 100)
			_healthCanvasValue = 1.0f;

		if (health <= 100 && health >= 0)
			_healthCanvasValue = (float)health / 100;

		_healthTransform.transform.localScale = new Vector3(_healthCanvasValue,1,1);
	}

	void HudAttackMeter(Transform hudAttackTransform, float attackCoolDownTime)
	{
		float poisonRefillVal = attackCoolDownTime;

		hudAttackTransform.transform.localScale = new Vector3(1,poisonRefillVal,1);
	}

	void CoolDown()
	{
		_poisonCoolDownTime += Time.deltaTime * _coolDownRate;

		if (_poisonCoolDownTime >= _initCoolDownTime)
		{
			_poisonCoolDownTime = _initCoolDownTime;
			_startBaseCoolDown = false;
		}
	}

	void ThrowAttack()
	{
		if (!_isAiming)
		{
			if (!_startBaseCoolDown && Input.GetButtonDown(CharacterButtonsConstants.THROW) && OnGround ||
			!_startBaseCoolDown && Input.GetMouseButtonDown(0) && OnGround)
			{
				_startBaseCoolDown = true;
				_poisonCoolDownTime = 0.0f;

				if (animator.GetBool("isThrowing") == false) //only if we are not throwing can we throw again to avoid getting stuck
				{
					animator.SetBool("isThrowing", true);
					movementSpeed = 0.0f;
					rb.velocity = Vector3.zero;
				}
			}
		}
	}

	void FireIntAttack()
	{

		if (!_startFlameCoolDown)
		{
			if (Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK))
			{
				fireInt = 1;
			}
			else
			{
				fireInt = 0;
			}
		}
		else {
			fireInt = 0;
		}

		if (_fireCoolDownTime > 0)
		{
			if(animator.GetInteger("fireInt") > 0 && Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK))
			{
				movementSpeed = 0.0f;
				_fireCoolDownTime -= Time.deltaTime/_flameRate;
			}
		}

		if (_startFireAttack)
		{
			if (!_flameInstantiated)
			{
				//_fireAttackPos.GetComponent<InstantiateFire>().FireAttack();
				_flameInstantiated = true;
			}
		}
		else {
			if (_fireAttackPos.childCount > 0)
			{
				
				_flameInstantiated = false;
			}
		}

		if (!Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK) && _fireCoolDownTime <= _initFireCoolTime ||
			_fireCoolDownTime <= 0)
		{
			_startFlameCoolDown = true;
			_startFireAttack = false;
		}
				

		if (movementSpeed == 0 && animator.GetInteger("fireInt") == 0 && !animator.GetBool("isThrowing"))
			ResetMovementSpeed();

		if (_startFlameCoolDown)
		{
			_fireCoolDownTime += Time.deltaTime/_fireRefillRate;

			if (_fireCoolDownTime >= _initFireCoolTime)
				_startFlameCoolDown = false;
		}
		
		_fireCanvasVal = _fireCoolDownTime /_initFireCoolTime;//Convert to %, 100% = 1 on transform
	}

	void Aim()
	{
		if (Input.GetButton(CharacterButtonsConstants.AIM))
		{
			_isAiming = true;
			movementSpeed = 0.0f;
		}
		else {
			_isAiming = false;
		}

		if (_isAiming)
		{
			newCamPosZ += (12*Time.deltaTime);

			if (newCamPosZ >= -4)
			{
				newCamPosZ = -4f;

				_crossHair.SetActive(true);
			}
				
			camera.transform.GetChild(0).transform.localPosition = new Vector3(0, 4, newCamPosZ);
		}
		else {
			newCamPosZ = -10f;
			camera.transform.GetChild(0).transform.localPosition = _initCamAttachedPos;
			_crossHair.SetActive(false);
		}
	}

	void SnipAttack()
	{
		if (_isAiming && Input.GetButtonDown(CharacterButtonsConstants.THROW) || _isAiming && Input.GetMouseButtonDown(0))
		{
			_isSniping = true;
			hasSnipped = 1;
		}
		//Network latency causing too many issues with animations
		/*
		else {
			hasSnipped = 0;
		}
		*/

		if (_isSniping)
			movementSpeed = 0.0f;
	}

	public void SetIsSnippingToFalse()
	{
		_isSniping = false;
		hasSnipped = 0;
	}

	public void StartFireAttack()
	{
		_startFireAttack = true;
	}

	public int GetFireInt()
	{
		return fireInt;
	}

	public float MouseAngle
	{
		get => m_LookAngle;
	}
}
