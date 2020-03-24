using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MobController : DamageableEntity
{
	public float movementSpeed = 5f;
	public float moveForceMult = 1f;
	public float jumpSpeed = 5f;
	protected Animator animator;
	protected Rigidbody rb;
	protected bool isGrounded = false;
	protected Vector3 targetRot;
	public Vector3 rightRot = Vector3.zero;
	public Vector3 leftRot = new Vector3(0f, 180f, 0f);
	public Vector3 frontRot = new Vector3(0f, 270f, 0f);
	public Vector3 backRot = new Vector3(0f, 90f, 0f);
	public float rotSpeed = 10f;
	public float groundCheckDist = 1.0f;
	public float groundCheckRadius = 20.0f;
	public Vector3 groundCheckOffset = Vector3.zero;
	protected Collider groundCollider;

	protected Vector3 moveInput = Vector3.zero;

	[SerializeField] bool useChild = false; //Set to True in inspector for player false for hostile
	[SerializeField] bool _isMoving = false;

	public Animator _animContTest;
	public GameObject _currentHitObj;
	public Vector3 _castOffSet = new Vector3(0, 1, 0);

	#region New Ground Detection

	[SerializeField] bool _onGround;
	[SerializeField] Vector3 _bottomPos = Vector3.zero;
	[SerializeField] float _detectionRadius = 1f;
	[SerializeField] Color _newGizmoColor = Color.red;
	[SerializeField] LayerMask _floorDetectionLayer;
	[SerializeField] float _initMovementSpeed;
	[SerializeField] bool _initiateJump = false;

	#endregion

	#region Rotation
	[SerializeField] float _turnSpeed = 2.5f;
	protected float m_LookAngle = 0.0f;
	#endregion

	protected override void Start()
	{
		base.Start();

		animator = GetComponentInChildren<Animator>();
		rb = GetComponent<Rigidbody>();

		targetRot = rightRot;

		rb.constraints = RigidbodyConstraints.FreezeRotation;

		_animContTest = animator;

		_initMovementSpeed = movementSpeed;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		GroundCheck();

		Vector3 horizMoveInput = new Vector3(moveInput.x, 0, moveInput.z);

		if (horizMoveInput.sqrMagnitude > 1)
			horizMoveInput.Normalize();

		Vector3 movement = horizMoveInput * movementSpeed;

		Vector3 horizVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

		//if (horizVel.sqrMagnitude < movement.sqrMagnitude)

		if (rb.velocity.y == 0)
		{
			if (movement.x > 0 || movement.z > 0 || movement.x < 0 || movement.z < 0)
			{
				rb.velocity = Vector3.ClampMagnitude(rb.velocity, movementSpeed);
				rb.AddRelativeForce(new Vector3(movement.x, 0, movement.z), ForceMode.VelocityChange);
			}
			else
			{
				rb.velocity = new Vector3(0, rb.velocity.y, 0);
			}
		}

		//changed to accommodate the new animotor
		//if (horizMoveInput.sqrMagnitude > Mathf.Epsilon)
		if (horizMoveInput.z > 0 || horizMoveInput.z < 0)//for running forward
		{
			if (animator != null)
				//animator.SetBool("Walking?", true);
				animator.SetInteger("runningVal", (int)horizMoveInput.z);
		}
		else
		{
			if (animator != null)
				animator.SetInteger("runningVal", 0);
		}
		
		if (horizMoveInput.x < 0 || horizMoveInput.x > 0)//for moving side to side
		{
			if (animator != null)
				//animator.SetBool("Walking?", true);
				animator.SetInteger("horizontalVal", (int)horizMoveInput.x);
		}
		else
		{
			if (animator != null)
				animator.SetInteger("horizontalVal", 0);
		}

		if (horizMoveInput.z > 0 && horizMoveInput.x > 0 || horizMoveInput.z > 0 && horizMoveInput.x < 0)//combining x and z movement
		{
			animator.SetInteger("runningVal", 1);
		}

		if (horizMoveInput.z < 0 && horizMoveInput.x > 0 || horizMoveInput.z < 0 && horizMoveInput.x < 0)//combining x and z movement
		{
			animator.SetInteger("horizontalVal", 1);
		}


		//these if statements are responsible for determining which direction the character
		//should rotate in
		//This rotation was updated to follow the Camera's rotation due to the addition of the new camera
		#region Rotation
		/*
		if (horizMoveInput.x > Mathf.Epsilon)
		{
			targetRot = rightRot;
		}

		if (horizMoveInput.x < -Mathf.Epsilon)
		{
			targetRot = leftRot;
		}

		if (horizMoveInput.z > Mathf.Epsilon)
		{
			targetRot = frontRot;
		}

		if (horizMoveInput.z < -Mathf.Epsilon)
		{
			targetRot = backRot;
		}
		*/
		Vector3 normalizedRot = new Vector3(Input.GetAxis(CharacterButtonsConstants.HORIZONTAL), 0, Input.GetAxis(CharacterButtonsConstants.VERTICLE)).normalized;
		
		//Rotating child so camera doesn't rotate with object
		//else is used so hostiles can still use MobController


		if (!useChild)
			transform.LookAt(transform.position + new Vector3(normalizedRot.x, 0, normalizedRot.z));

		if (useChild)
		{
			var x = Input.GetAxis("Mouse X");

			m_LookAngle += x * _turnSpeed;

			//Temp fix until new camera is created
			//transform.localRotation = GetComponent<PlayerController>().camera.transform.localRotation;
			transform.localRotation = Quaternion.Euler(new Vector3(0, m_LookAngle,0));

			if (horizMoveInput.z > 0 && horizMoveInput.x > 0)//combining x and z movement
			{
				transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, 45, 0));
			}
			else if (horizMoveInput.z > 0 && horizMoveInput.x < 0)
			{
				transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, -45, 0));
			}
			else {
				transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			}
			
				
		}



		//if using this way of rotation set the child object with animator to 90 on Y rot axis
		//transform.rotation = Quaternion.Euler(targetRot);


		/*
		
		transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetRot, rotSpeed * Time.fixedDeltaTime);
		
        if (animator != null)
            animator.transform.localEulerAngles = Vector3.Lerp(animator.transform.localEulerAngles,
                    targetRot, rotSpeed * Time.fixedDeltaTime);
		*/

		#endregion


		//jumping

		bool bJumping = moveInput.y > Mathf.Epsilon;

		if (!useChild)
		{
			if (bJumping && isGrounded)
			{

				//rigidbody.AddForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
				rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
			}
		}

		//Updated below for 3d character

		if (useChild)
		{
			NewGroundDetection();


			if (Input.GetButtonDown(CharacterButtonsConstants.JUMP) && _onGround)
			{
				_initiateJump = true;
				movementSpeed = 0.0f;
				rb.velocity = Vector3.zero;
				animator.SetBool("isJumping", true);
			}

			if (_initiateJump)
			{
				animator.SetBool("onGround", false);
			}

			if (!_initiateJump)
			{
				animator.SetBool("onGround", _onGround);
			}
		}

		//stopping the character
		if (horizMoveInput.sqrMagnitude < Mathf.Epsilon && !bJumping && !IsFalling() && isGrounded)
		{
			//rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}

	}

	protected override void OnDeath()
	{
		//rb.constraints -= RigidbodyConstraints.FreezeRotation;

		base.OnDeath();
	}

	protected virtual void GroundCheck()
	{
		RaycastHit hit;
		if (Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius,
				Vector3.down, out hit, groundCheckDist))
		{
			isGrounded = true;
			groundCollider = hit.collider;

			_currentHitObj = hit.transform.gameObject;
		}
		else
		{
			isGrounded = false;
			groundCollider = null;
		}

	}

	protected virtual bool IsFalling()
	{
		return rb.velocity.y < 0;
	}

	void NewGroundDetection()
	{
		//This is for the floor detection
		var bPos = _bottomPos;

		bPos.x += transform.position.x;
		bPos.y += transform.position.y;
		bPos.z += transform.position.z;

		_onGround = Physics.CheckSphere(bPos, _detectionRadius, _floorDetectionLayer);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position + groundCheckOffset, groundCheckRadius);
	}


	void OnDrawGizmos()
	{
		Gizmos.color = _newGizmoColor;

		var bPos = _bottomPos;

		bPos.x += transform.position.x;
		bPos.y += transform.position.y;
		bPos.z += transform.position.z;

		Gizmos.DrawWireSphere(bPos, _detectionRadius);
	}

	public void ResetJump()
	{
		ResetMovementSpeed();
		_initiateJump = false;
	}

	public void ResetMovementSpeed()
	{
		movementSpeed = _initMovementSpeed;
	}

	public bool UseChild
	{
		get => useChild;
	}

	public bool OnGround
	{
		get => _onGround;
	}
}
