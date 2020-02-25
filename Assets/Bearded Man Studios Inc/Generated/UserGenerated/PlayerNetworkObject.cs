using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15,0,0,0,0,0,0]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 8;

		private byte[] _dirtyFields = new byte[1];

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
		private int _food;
		public event FieldEvent<int> foodChanged;
		public Interpolated<int> foodInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int food
		{
			get { return _food; }
			set
			{
				// Don't do anything if the value is the same
				if (_food == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_food = value;
				hasDirtyFields = true;
			}
		}

		public void SetfoodDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_food(ulong timestep)
		{
			if (foodChanged != null) foodChanged(_food, timestep);
			if (fieldAltered != null) fieldAltered("food", _food, timestep);
		}
		[ForgeGeneratedField]
		private int _water;
		public event FieldEvent<int> waterChanged;
		public Interpolated<int> waterInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int water
		{
			get { return _water; }
			set
			{
				// Don't do anything if the value is the same
				if (_water == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x10;
				_water = value;
				hasDirtyFields = true;
			}
		}

		public void SetwaterDirty()
		{
			_dirtyFields[0] |= 0x10;
			hasDirtyFields = true;
		}

		private void RunChange_water(ulong timestep)
		{
			if (waterChanged != null) waterChanged(_water, timestep);
			if (fieldAltered != null) fieldAltered("water", _water, timestep);
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
				_dirtyFields[0] |= 0x20;
				_isJumping = value;
				hasDirtyFields = true;
			}
		}

		public void SetisJumpingDirty()
		{
			_dirtyFields[0] |= 0x20;
			hasDirtyFields = true;
		}

		private void RunChange_isJumping(ulong timestep)
		{
			if (isJumpingChanged != null) isJumpingChanged(_isJumping, timestep);
			if (fieldAltered != null) fieldAltered("isJumping", _isJumping, timestep);
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
				_dirtyFields[0] |= 0x40;
				_onGround = value;
				hasDirtyFields = true;
			}
		}

		public void SetonGroundDirty()
		{
			_dirtyFields[0] |= 0x40;
			hasDirtyFields = true;
		}

		private void RunChange_onGround(ulong timestep)
		{
			if (onGroundChanged != null) onGroundChanged(_onGround, timestep);
			if (fieldAltered != null) fieldAltered("onGround", _onGround, timestep);
		}
		[ForgeGeneratedField]
		private int _runningVal;
		public event FieldEvent<int> runningValChanged;
		public Interpolated<int> runningValInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int runningVal
		{
			get { return _runningVal; }
			set
			{
				// Don't do anything if the value is the same
				if (_runningVal == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x80;
				_runningVal = value;
				hasDirtyFields = true;
			}
		}

		public void SetrunningValDirty()
		{
			_dirtyFields[0] |= 0x80;
			hasDirtyFields = true;
		}

		private void RunChange_runningVal(ulong timestep)
		{
			if (runningValChanged != null) runningValChanged(_runningVal, timestep);
			if (fieldAltered != null) fieldAltered("runningVal", _runningVal, timestep);
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
			foodInterpolation.current = foodInterpolation.target;
			waterInterpolation.current = waterInterpolation.target;
			isJumpingInterpolation.current = isJumpingInterpolation.target;
			onGroundInterpolation.current = onGroundInterpolation.target;
			runningValInterpolation.current = runningValInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _health);
			UnityObjectMapper.Instance.MapBytes(data, _food);
			UnityObjectMapper.Instance.MapBytes(data, _water);
			UnityObjectMapper.Instance.MapBytes(data, _isJumping);
			UnityObjectMapper.Instance.MapBytes(data, _onGround);
			UnityObjectMapper.Instance.MapBytes(data, _runningVal);

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
			_food = UnityObjectMapper.Instance.Map<int>(payload);
			foodInterpolation.current = _food;
			foodInterpolation.target = _food;
			RunChange_food(timestep);
			_water = UnityObjectMapper.Instance.Map<int>(payload);
			waterInterpolation.current = _water;
			waterInterpolation.target = _water;
			RunChange_water(timestep);
			_isJumping = UnityObjectMapper.Instance.Map<bool>(payload);
			isJumpingInterpolation.current = _isJumping;
			isJumpingInterpolation.target = _isJumping;
			RunChange_isJumping(timestep);
			_onGround = UnityObjectMapper.Instance.Map<bool>(payload);
			onGroundInterpolation.current = _onGround;
			onGroundInterpolation.target = _onGround;
			RunChange_onGround(timestep);
			_runningVal = UnityObjectMapper.Instance.Map<int>(payload);
			runningValInterpolation.current = _runningVal;
			runningValInterpolation.target = _runningVal;
			RunChange_runningVal(timestep);
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
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _food);
			if ((0x10 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _water);
			if ((0x20 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _isJumping);
			if ((0x40 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _onGround);
			if ((0x80 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _runningVal);

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
				if (foodInterpolation.Enabled)
				{
					foodInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					foodInterpolation.Timestep = timestep;
				}
				else
				{
					_food = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_food(timestep);
				}
			}
			if ((0x10 & readDirtyFlags[0]) != 0)
			{
				if (waterInterpolation.Enabled)
				{
					waterInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					waterInterpolation.Timestep = timestep;
				}
				else
				{
					_water = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_water(timestep);
				}
			}
			if ((0x20 & readDirtyFlags[0]) != 0)
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
			if ((0x40 & readDirtyFlags[0]) != 0)
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
			if ((0x80 & readDirtyFlags[0]) != 0)
			{
				if (runningValInterpolation.Enabled)
				{
					runningValInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					runningValInterpolation.Timestep = timestep;
				}
				else
				{
					_runningVal = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_runningVal(timestep);
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
			if (foodInterpolation.Enabled && !foodInterpolation.current.UnityNear(foodInterpolation.target, 0.0015f))
			{
				_food = (int)foodInterpolation.Interpolate();
				//RunChange_food(foodInterpolation.Timestep);
			}
			if (waterInterpolation.Enabled && !waterInterpolation.current.UnityNear(waterInterpolation.target, 0.0015f))
			{
				_water = (int)waterInterpolation.Interpolate();
				//RunChange_water(waterInterpolation.Timestep);
			}
			if (isJumpingInterpolation.Enabled && !isJumpingInterpolation.current.UnityNear(isJumpingInterpolation.target, 0.0015f))
			{
				_isJumping = (bool)isJumpingInterpolation.Interpolate();
				//RunChange_isJumping(isJumpingInterpolation.Timestep);
			}
			if (onGroundInterpolation.Enabled && !onGroundInterpolation.current.UnityNear(onGroundInterpolation.target, 0.0015f))
			{
				_onGround = (bool)onGroundInterpolation.Interpolate();
				//RunChange_onGround(onGroundInterpolation.Timestep);
			}
			if (runningValInterpolation.Enabled && !runningValInterpolation.current.UnityNear(runningValInterpolation.target, 0.0015f))
			{
				_runningVal = (int)runningValInterpolation.Interpolate();
				//RunChange_runningVal(runningValInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
