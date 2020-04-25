using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15,0,0,0,0,0,0,0,0,0,0]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 9;

		private byte[] _dirtyFields = new byte[2];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _position;
		public event FieldEvent<Vector3> positionChanged;
		public InterpolateVector3 positionInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 position
		{
			get { return _position; }
			set
			{
				// Don't do anything if the value is the same
				if (_position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_position = value;
				hasDirtyFields = true;
			}
		}

		public void SetpositionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_position(ulong timestep)
		{
			if (positionChanged != null) positionChanged(_position, timestep);
			if (fieldAltered != null) fieldAltered("position", _position, timestep);
		}
		[ForgeGeneratedField]
		private Quaternion _rotation;
		public event FieldEvent<Quaternion> rotationChanged;
		public InterpolateQuaternion rotationInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion rotation
		{
			get { return _rotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_rotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_rotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetrotationDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_rotation(ulong timestep)
		{
			if (rotationChanged != null) rotationChanged(_rotation, timestep);
			if (fieldAltered != null) fieldAltered("rotation", _rotation, timestep);
		}
		[ForgeGeneratedField]
		private int _health;
		public event FieldEvent<int> healthChanged;
		public Interpolated<int> healthInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int health
		{
			get { return _health; }
			set
			{
				// Don't do anything if the value is the same
				if (_health == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_health = value;
				hasDirtyFields = true;
			}
		}

		public void SethealthDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_health(ulong timestep)
		{
			if (healthChanged != null) healthChanged(_health, timestep);
			if (fieldAltered != null) fieldAltered("health", _health, timestep);
		}
		[ForgeGeneratedField]
		private bool _onGround;
		public event FieldEvent<bool> onGroundChanged;
		public Interpolated<bool> onGroundInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool onGround
		{
			get { return _onGround; }
			set
			{
				// Don't do anything if the value is the same
				if (_onGround == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_onGround = value;
				hasDirtyFields = true;
			}
		}

		public void SetonGroundDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_onGround(ulong timestep)
		{
			if (onGroundChanged != null) onGroundChanged(_onGround, timestep);
			if (fieldAltered != null) fieldAltered("onGround", _onGround, timestep);
		}
		[ForgeGeneratedField]
		private bool _isDead;
		public event FieldEvent<bool> isDeadChanged;
		public Interpolated<bool> isDeadInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool isDead
		{
			get { return _isDead; }
			set
			{
				// Don't do anything if the value is the same
				if (_isDead == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x10;
				_isDead = value;
				hasDirtyFields = true;
			}
		}

		public void SetisDeadDirty()
		{
			_dirtyFields[0] |= 0x10;
			hasDirtyFields = true;
		}

		private void RunChange_isDead(ulong timestep)
		{
			if (isDeadChanged != null) isDeadChanged(_isDead, timestep);
			if (fieldAltered != null) fieldAltered("isDead", _isDead, timestep);
		}
		[ForgeGeneratedField]
		private int _runninVal;
		public event FieldEvent<int> runninValChanged;
		public Interpolated<int> runninValInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int runninVal
		{
			get { return _runninVal; }
			set
			{
				// Don't do anything if the value is the same
				if (_runninVal == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x20;
				_runninVal = value;
				hasDirtyFields = true;
			}
		}

		public void SetrunninValDirty()
		{
			_dirtyFields[0] |= 0x20;
			hasDirtyFields = true;
		}

		private void RunChange_runninVal(ulong timestep)
		{
			if (runninValChanged != null) runninValChanged(_runninVal, timestep);
			if (fieldAltered != null) fieldAltered("runninVal", _runninVal, timestep);
		}
		[ForgeGeneratedField]
		private int _horizontalVal;
		public event FieldEvent<int> horizontalValChanged;
		public Interpolated<int> horizontalValInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int horizontalVal
		{
			get { return _horizontalVal; }
			set
			{
				// Don't do anything if the value is the same
				if (_horizontalVal == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x40;
				_horizontalVal = value;
				hasDirtyFields = true;
			}
		}

		public void SethorizontalValDirty()
		{
			_dirtyFields[0] |= 0x40;
			hasDirtyFields = true;
		}

		private void RunChange_horizontalVal(ulong timestep)
		{
			if (horizontalValChanged != null) horizontalValChanged(_horizontalVal, timestep);
			if (fieldAltered != null) fieldAltered("horizontalVal", _horizontalVal, timestep);
		}
		[ForgeGeneratedField]
		private bool _isJumping;
		public event FieldEvent<bool> isJumpingChanged;
		public Interpolated<bool> isJumpingInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool isJumping
		{
			get { return _isJumping; }
			set
			{
				// Don't do anything if the value is the same
				if (_isJumping == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x80;
				_isJumping = value;
				hasDirtyFields = true;
			}
		}

		public void SetisJumpingDirty()
		{
			_dirtyFields[0] |= 0x80;
			hasDirtyFields = true;
		}

		private void RunChange_isJumping(ulong timestep)
		{
			if (isJumpingChanged != null) isJumpingChanged(_isJumping, timestep);
			if (fieldAltered != null) fieldAltered("isJumping", _isJumping, timestep);
		}
		[ForgeGeneratedField]
		private bool _isThrowing;
		public event FieldEvent<bool> isThrowingChanged;
		public Interpolated<bool> isThrowingInterpolation = new Interpolated<bool>() { LerpT = 0f, Enabled = false };
		public bool isThrowing
		{
			get { return _isThrowing; }
			set
			{
				// Don't do anything if the value is the same
				if (_isThrowing == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[1] |= 0x1;
				_isThrowing = value;
				hasDirtyFields = true;
			}
		}

		public void SetisThrowingDirty()
		{
			_dirtyFields[1] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_isThrowing(ulong timestep)
		{
			if (isThrowingChanged != null) isThrowingChanged(_isThrowing, timestep);
			if (fieldAltered != null) fieldAltered("isThrowing", _isThrowing, timestep);
		}
		[ForgeGeneratedField]
		private int _fireInt;
		public event FieldEvent<int> fireIntChanged;
		public Interpolated<int> fireIntInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int fireInt
		{
			get { return _fireInt; }
			set
			{
				// Don't do anything if the value is the same
				if (_fireInt == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[1] |= 0x2;
				_fireInt = value;
				hasDirtyFields = true;
			}
		}

		public void SetfireIntDirty()
		{
			_dirtyFields[1] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_fireInt(ulong timestep)
		{
			if (fireIntChanged != null) fireIntChanged(_fireInt, timestep);
			if (fieldAltered != null) fieldAltered("fireInt", _fireInt, timestep);
		}
		[ForgeGeneratedField]
		private int _aimInt;
		public event FieldEvent<int> aimIntChanged;
		public Interpolated<int> aimIntInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int aimInt
		{
			get { return _aimInt; }
			set
			{
				// Don't do anything if the value is the same
				if (_aimInt == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[1] |= 0x4;
				_aimInt = value;
				hasDirtyFields = true;
			}
		}

		public void SetaimIntDirty()
		{
			_dirtyFields[1] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_aimInt(ulong timestep)
		{
			if (aimIntChanged != null) aimIntChanged(_aimInt, timestep);
			if (fieldAltered != null) fieldAltered("aimInt", _aimInt, timestep);
		}
		[ForgeGeneratedField]
		private int _hasSnipped;
		public event FieldEvent<int> hasSnippedChanged;
		public Interpolated<int> hasSnippedInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int hasSnipped
		{
			get { return _hasSnipped; }
			set
			{
				// Don't do anything if the value is the same
				if (_hasSnipped == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[1] |= 0x8;
				_hasSnipped = value;
				hasDirtyFields = true;
			}
		}

		public void SethasSnippedDirty()
		{
			_dirtyFields[1] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_hasSnipped(ulong timestep)
		{
			if (hasSnippedChanged != null) hasSnippedChanged(_hasSnipped, timestep);
			if (fieldAltered != null) fieldAltered("hasSnipped", _hasSnipped, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			positionInterpolation.current = positionInterpolation.target;
			rotationInterpolation.current = rotationInterpolation.target;
			healthInterpolation.current = healthInterpolation.target;
			onGroundInterpolation.current = onGroundInterpolation.target;
			isDeadInterpolation.current = isDeadInterpolation.target;
			runninValInterpolation.current = runninValInterpolation.target;
			horizontalValInterpolation.current = horizontalValInterpolation.target;
			isJumpingInterpolation.current = isJumpingInterpolation.target;
			isThrowingInterpolation.current = isThrowingInterpolation.target;
			fireIntInterpolation.current = fireIntInterpolation.target;
			aimIntInterpolation.current = aimIntInterpolation.target;
			hasSnippedInterpolation.current = hasSnippedInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _health);
			UnityObjectMapper.Instance.MapBytes(data, _onGround);
			UnityObjectMapper.Instance.MapBytes(data, _isDead);
			UnityObjectMapper.Instance.MapBytes(data, _runninVal);
			UnityObjectMapper.Instance.MapBytes(data, _horizontalVal);
			UnityObjectMapper.Instance.MapBytes(data, _isJumping);
			UnityObjectMapper.Instance.MapBytes(data, _isThrowing);
			UnityObjectMapper.Instance.MapBytes(data, _fireInt);
			UnityObjectMapper.Instance.MapBytes(data, _aimInt);
			UnityObjectMapper.Instance.MapBytes(data, _hasSnipped);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			positionInterpolation.current = _position;
			positionInterpolation.target = _position;
			RunChange_position(timestep);
			_rotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			rotationInterpolation.current = _rotation;
			rotationInterpolation.target = _rotation;
			RunChange_rotation(timestep);
			_health = UnityObjectMapper.Instance.Map<int>(payload);
			healthInterpolation.current = _health;
			healthInterpolation.target = _health;
			RunChange_health(timestep);
			_onGround = UnityObjectMapper.Instance.Map<bool>(payload);
			onGroundInterpolation.current = _onGround;
			onGroundInterpolation.target = _onGround;
			RunChange_onGround(timestep);
			_isDead = UnityObjectMapper.Instance.Map<bool>(payload);
			isDeadInterpolation.current = _isDead;
			isDeadInterpolation.target = _isDead;
			RunChange_isDead(timestep);
			_runninVal = UnityObjectMapper.Instance.Map<int>(payload);
			runninValInterpolation.current = _runninVal;
			runninValInterpolation.target = _runninVal;
			RunChange_runninVal(timestep);
			_horizontalVal = UnityObjectMapper.Instance.Map<int>(payload);
			horizontalValInterpolation.current = _horizontalVal;
			horizontalValInterpolation.target = _horizontalVal;
			RunChange_horizontalVal(timestep);
			_isJumping = UnityObjectMapper.Instance.Map<bool>(payload);
			isJumpingInterpolation.current = _isJumping;
			isJumpingInterpolation.target = _isJumping;
			RunChange_isJumping(timestep);
			_isThrowing = UnityObjectMapper.Instance.Map<bool>(payload);
			isThrowingInterpolation.current = _isThrowing;
			isThrowingInterpolation.target = _isThrowing;
			RunChange_isThrowing(timestep);
			_fireInt = UnityObjectMapper.Instance.Map<int>(payload);
			fireIntInterpolation.current = _fireInt;
			fireIntInterpolation.target = _fireInt;
			RunChange_fireInt(timestep);
			_aimInt = UnityObjectMapper.Instance.Map<int>(payload);
			aimIntInterpolation.current = _aimInt;
			aimIntInterpolation.target = _aimInt;
			RunChange_aimInt(timestep);
			_hasSnipped = UnityObjectMapper.Instance.Map<int>(payload);
			hasSnippedInterpolation.current = _hasSnipped;
			hasSnippedInterpolation.target = _hasSnipped;
			RunChange_hasSnipped(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _rotation);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _health);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _onGround);
			if ((0x10 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _isDead);
			if ((0x20 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _runninVal);
			if ((0x40 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _horizontalVal);
			if ((0x80 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _isJumping);
			if ((0x1 & _dirtyFields[1]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _isThrowing);
			if ((0x2 & _dirtyFields[1]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _fireInt);
			if ((0x4 & _dirtyFields[1]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _aimInt);
			if ((0x8 & _dirtyFields[1]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _hasSnipped);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (positionInterpolation.Enabled)
				{
					positionInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					positionInterpolation.Timestep = timestep;
				}
				else
				{
					_position = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_position(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (rotationInterpolation.Enabled)
				{
					rotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					rotationInterpolation.Timestep = timestep;
				}
				else
				{
					_rotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_rotation(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (healthInterpolation.Enabled)
				{
					healthInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					healthInterpolation.Timestep = timestep;
				}
				else
				{
					_health = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_health(timestep);
				}
			}
			if ((0x8 & readDirtyFlags[0]) != 0)
			{
				if (onGroundInterpolation.Enabled)
				{
					onGroundInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					onGroundInterpolation.Timestep = timestep;
				}
				else
				{
					_onGround = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_onGround(timestep);
				}
			}
			if ((0x10 & readDirtyFlags[0]) != 0)
			{
				if (isDeadInterpolation.Enabled)
				{
					isDeadInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					isDeadInterpolation.Timestep = timestep;
				}
				else
				{
					_isDead = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_isDead(timestep);
				}
			}
			if ((0x20 & readDirtyFlags[0]) != 0)
			{
				if (runninValInterpolation.Enabled)
				{
					runninValInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					runninValInterpolation.Timestep = timestep;
				}
				else
				{
					_runninVal = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_runninVal(timestep);
				}
			}
			if ((0x40 & readDirtyFlags[0]) != 0)
			{
				if (horizontalValInterpolation.Enabled)
				{
					horizontalValInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					horizontalValInterpolation.Timestep = timestep;
				}
				else
				{
					_horizontalVal = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_horizontalVal(timestep);
				}
			}
			if ((0x80 & readDirtyFlags[0]) != 0)
			{
				if (isJumpingInterpolation.Enabled)
				{
					isJumpingInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					isJumpingInterpolation.Timestep = timestep;
				}
				else
				{
					_isJumping = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_isJumping(timestep);
				}
			}
			if ((0x1 & readDirtyFlags[1]) != 0)
			{
				if (isThrowingInterpolation.Enabled)
				{
					isThrowingInterpolation.target = UnityObjectMapper.Instance.Map<bool>(data);
					isThrowingInterpolation.Timestep = timestep;
				}
				else
				{
					_isThrowing = UnityObjectMapper.Instance.Map<bool>(data);
					RunChange_isThrowing(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[1]) != 0)
			{
				if (fireIntInterpolation.Enabled)
				{
					fireIntInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					fireIntInterpolation.Timestep = timestep;
				}
				else
				{
					_fireInt = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_fireInt(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[1]) != 0)
			{
				if (aimIntInterpolation.Enabled)
				{
					aimIntInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					aimIntInterpolation.Timestep = timestep;
				}
				else
				{
					_aimInt = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_aimInt(timestep);
				}
			}
			if ((0x8 & readDirtyFlags[1]) != 0)
			{
				if (hasSnippedInterpolation.Enabled)
				{
					hasSnippedInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					hasSnippedInterpolation.Timestep = timestep;
				}
				else
				{
					_hasSnipped = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_hasSnipped(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (positionInterpolation.Enabled && !positionInterpolation.current.UnityNear(positionInterpolation.target, 0.0015f))
			{
				_position = (Vector3)positionInterpolation.Interpolate();
				//RunChange_position(positionInterpolation.Timestep);
			}
			if (rotationInterpolation.Enabled && !rotationInterpolation.current.UnityNear(rotationInterpolation.target, 0.0015f))
			{
				_rotation = (Quaternion)rotationInterpolation.Interpolate();
				//RunChange_rotation(rotationInterpolation.Timestep);
			}
			if (healthInterpolation.Enabled && !healthInterpolation.current.UnityNear(healthInterpolation.target, 0.0015f))
			{
				_health = (int)healthInterpolation.Interpolate();
				//RunChange_health(healthInterpolation.Timestep);
			}
			if (onGroundInterpolation.Enabled && !onGroundInterpolation.current.UnityNear(onGroundInterpolation.target, 0.0015f))
			{
				_onGround = (bool)onGroundInterpolation.Interpolate();
				//RunChange_onGround(onGroundInterpolation.Timestep);
			}
			if (isDeadInterpolation.Enabled && !isDeadInterpolation.current.UnityNear(isDeadInterpolation.target, 0.0015f))
			{
				_isDead = (bool)isDeadInterpolation.Interpolate();
				//RunChange_isDead(isDeadInterpolation.Timestep);
			}
			if (runninValInterpolation.Enabled && !runninValInterpolation.current.UnityNear(runninValInterpolation.target, 0.0015f))
			{
				_runninVal = (int)runninValInterpolation.Interpolate();
				//RunChange_runninVal(runninValInterpolation.Timestep);
			}
			if (horizontalValInterpolation.Enabled && !horizontalValInterpolation.current.UnityNear(horizontalValInterpolation.target, 0.0015f))
			{
				_horizontalVal = (int)horizontalValInterpolation.Interpolate();
				//RunChange_horizontalVal(horizontalValInterpolation.Timestep);
			}
			if (isJumpingInterpolation.Enabled && !isJumpingInterpolation.current.UnityNear(isJumpingInterpolation.target, 0.0015f))
			{
				_isJumping = (bool)isJumpingInterpolation.Interpolate();
				//RunChange_isJumping(isJumpingInterpolation.Timestep);
			}
			if (isThrowingInterpolation.Enabled && !isThrowingInterpolation.current.UnityNear(isThrowingInterpolation.target, 0.0015f))
			{
				_isThrowing = (bool)isThrowingInterpolation.Interpolate();
				//RunChange_isThrowing(isThrowingInterpolation.Timestep);
			}
			if (fireIntInterpolation.Enabled && !fireIntInterpolation.current.UnityNear(fireIntInterpolation.target, 0.0015f))
			{
				_fireInt = (int)fireIntInterpolation.Interpolate();
				//RunChange_fireInt(fireIntInterpolation.Timestep);
			}
			if (aimIntInterpolation.Enabled && !aimIntInterpolation.current.UnityNear(aimIntInterpolation.target, 0.0015f))
			{
				_aimInt = (int)aimIntInterpolation.Interpolate();
				//RunChange_aimInt(aimIntInterpolation.Timestep);
			}
			if (hasSnippedInterpolation.Enabled && !hasSnippedInterpolation.current.UnityNear(hasSnippedInterpolation.target, 0.0015f))
			{
				_hasSnipped = (int)hasSnippedInterpolation.Interpolate();
				//RunChange_hasSnipped(hasSnippedInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[2];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
