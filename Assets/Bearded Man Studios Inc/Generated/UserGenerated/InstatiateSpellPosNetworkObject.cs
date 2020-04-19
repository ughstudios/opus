using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.1,0.1,0.1,0]")]
	public partial class InstatiateSpellPosNetworkObject : NetworkObject
	{
		public const int IDENTITY = 13;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private Vector3 _position;
		public event FieldEvent<Vector3> positionChanged;
		public InterpolateVector3 positionInterpolation = new InterpolateVector3() { LerpT = 0.1f, Enabled = true };
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
		public InterpolateQuaternion rotationInterpolation = new InterpolateQuaternion() { LerpT = 0.1f, Enabled = true };
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
		private Quaternion _localRotation;
		public event FieldEvent<Quaternion> localRotationChanged;
		public InterpolateQuaternion localRotationInterpolation = new InterpolateQuaternion() { LerpT = 0.1f, Enabled = true };
		public Quaternion localRotation
		{
			get { return _localRotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_localRotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_localRotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetlocalRotationDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_localRotation(ulong timestep)
		{
			if (localRotationChanged != null) localRotationChanged(_localRotation, timestep);
			if (fieldAltered != null) fieldAltered("localRotation", _localRotation, timestep);
		}
		[ForgeGeneratedField]
		private Quaternion _m_TransformTargetRot;
		public event FieldEvent<Quaternion> m_TransformTargetRotChanged;
		public InterpolateQuaternion m_TransformTargetRotInterpolation = new InterpolateQuaternion() { LerpT = 0f, Enabled = false };
		public Quaternion m_TransformTargetRot
		{
			get { return _m_TransformTargetRot; }
			set
			{
				// Don't do anything if the value is the same
				if (_m_TransformTargetRot == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_m_TransformTargetRot = value;
				hasDirtyFields = true;
			}
		}

		public void Setm_TransformTargetRotDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_m_TransformTargetRot(ulong timestep)
		{
			if (m_TransformTargetRotChanged != null) m_TransformTargetRotChanged(_m_TransformTargetRot, timestep);
			if (fieldAltered != null) fieldAltered("m_TransformTargetRot", _m_TransformTargetRot, timestep);
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
			localRotationInterpolation.current = localRotationInterpolation.target;
			m_TransformTargetRotInterpolation.current = m_TransformTargetRotInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _localRotation);
			UnityObjectMapper.Instance.MapBytes(data, _m_TransformTargetRot);

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
			_localRotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			localRotationInterpolation.current = _localRotation;
			localRotationInterpolation.target = _localRotation;
			RunChange_localRotation(timestep);
			_m_TransformTargetRot = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			m_TransformTargetRotInterpolation.current = _m_TransformTargetRot;
			m_TransformTargetRotInterpolation.target = _m_TransformTargetRot;
			RunChange_m_TransformTargetRot(timestep);
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
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _localRotation);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _m_TransformTargetRot);

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
				if (localRotationInterpolation.Enabled)
				{
					localRotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					localRotationInterpolation.Timestep = timestep;
				}
				else
				{
					_localRotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_localRotation(timestep);
				}
			}
			if ((0x8 & readDirtyFlags[0]) != 0)
			{
				if (m_TransformTargetRotInterpolation.Enabled)
				{
					m_TransformTargetRotInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					m_TransformTargetRotInterpolation.Timestep = timestep;
				}
				else
				{
					_m_TransformTargetRot = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_m_TransformTargetRot(timestep);
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
			if (localRotationInterpolation.Enabled && !localRotationInterpolation.current.UnityNear(localRotationInterpolation.target, 0.0015f))
			{
				_localRotation = (Quaternion)localRotationInterpolation.Interpolate();
				//RunChange_localRotation(localRotationInterpolation.Timestep);
			}
			if (m_TransformTargetRotInterpolation.Enabled && !m_TransformTargetRotInterpolation.current.UnityNear(m_TransformTargetRotInterpolation.target, 0.0015f))
			{
				_m_TransformTargetRot = (Quaternion)m_TransformTargetRotInterpolation.Interpolate();
				//RunChange_m_TransformTargetRot(m_TransformTargetRotInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public InstatiateSpellPosNetworkObject() : base() { Initialize(); }
		public InstatiateSpellPosNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public InstatiateSpellPosNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
