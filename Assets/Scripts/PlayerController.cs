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

		//Cursor.visible = false;

		if (GameObject.FindGameObjectWithTag("ServerCamera") != null)
			serverCam = GameObject.FindGameObjectWithTag("ServerCamera").GetComponent<Camera>();

		if (networkObject != null && networkObject.IsOwner && camera != null)
		{
			GameObject playerCamera = Instantiate(camera, gameObject.transform);
			serverCam.enabled = false;
		}
	}
	
    void OnTriggerStay(Collider collider)
	{
		if (collider.gameObject.tag == "Water Hex")
		{
			if (Input.GetButtonDown("Gather"))
			{
				networkObject.SendRpc(RPC_SERVER__SET_WATER, Receivers.All, MaxEverything);
				SurvivalTimer survivalTimer = GetComponent<SurvivalTimer>();
				survivalTimer.reset_water_bar_to_full();
			}
		}
		canPlant = false;
	}

	private void Update()
	{

		if (health <= 0)
		{
			_isDead = true;
		}

		if (networkObject != null)
		{
			if (networkObject.IsOwner)
			{
				if(serverCam != null)
					serverCam.enabled = false;

				//if(camera != null)
					//camera.SetActive(true);

				_hudCanvas.SetActive(true);

				if (UseChild)
				{
					networkObject.isJumping = animator.GetBool("isJumping");
					networkObject.onGround = animator.GetBool("onGround");
					networkObject.runningVal = animator.GetInteger("runningVal");
					networkObject.isThrowing = animator.GetBool("isThrowing");
				}

				networkObject.health = health;
				networkObject.isDead = _isDead;
			}
		}


		if (Input.GetButtonDown(CharacterButtonsConstants.THROW) && OnGround || Input.GetMouseButtonDown(0) && OnGround)
		{
			movementSpeed = 0.0f;
			rb.velocity = Vector3.zero;
			animator.SetBool("isThrowing", true);
		}

		if(networkObject != null && networkObject.IsOwner)
			HealthCanvasMeter();

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
					if(animator.GetBool("isJumping") != networkObject.isJumping) animator.SetBool("isJumping", networkObject.isJumping);
					if (animator.GetBool("onGround") != networkObject.onGround) animator.SetBool("onGround", networkObject.onGround);
					if (animator.GetInteger("runningVal") != networkObject.runningVal) animator.SetInteger("runningVal", networkObject.runningVal);
					if (animator.GetBool("isThrowing") != networkObject.isThrowing) animator.SetBool("isThrowing", networkObject.isThrowing);
				}

				health = networkObject.health;
				_isDead = networkObject.isDead;

				if (serverCam != null)
					serverCam.enabled = true;

				//if(camera != null)
					//camera.SetActive(false);

				_hudCanvas.SetActive(false);

				return;
			}
		}

		
		
		moveInput.Set(Input.GetAxisRaw("Horizontal"),
				Input.GetButton("Jump") ? 1f : 0f,
				Input.GetAxisRaw("Vertical"));

		base.FixedUpdate();

		if (canPlant && isGrounded && groundCollider != null &&
				groundCollider.GetComponent<DamageableEntity>() == null
				&& Input.GetButton("Gather"))
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

	public override void TakeDamage(DamageableEntity source, int damage)
	{
		//return 
		base.TakeDamage(source, damage);
	}

	/*
	protected override void OnDeath()
	{
		base.OnDeath();


		//if (networkObject.isDead)
		//if (!_loadedFromServer) SceneManager.LoadScene("GameOver");
	}
	*/

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

	public float MouseAngle
	{
		get => m_LookAngle;
	}
}
