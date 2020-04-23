using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0,0]")]
	public partial class GameModeNetworkObject : NetworkObject
	{
		public const int IDENTITY = 5;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		[ForgeGeneratedField]
		private float _matchTimer;
		public event FieldEvent<float> matchTimerChanged;
		public InterpolateFloat matchTimerInterpolation = new InterpolateFloat() { LerpT = 0f, Enabled = false };
		public float matchTimer
		{
			get { return _matchTimer; }
			set
			{
				// Don't do anything if the value is the same
				if (_matchTimer == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_matchTimer = value;
				hasDirtyFields = true;
			}
		}

		public void SetmatchTimerDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_matchTimer(ulong timestep)
		{
			if (matchTimerChanged != null) matchTimerChanged(_matchTimer, timestep);
			if (fieldAltered != null) fieldAltered("matchTimer", _matchTimer, timestep);
		}
		[ForgeGeneratedField]
		private int _playerCount;
		public event FieldEvent<int> playerCountChanged;
		public Interpolated<int> playerCountInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int playerCount
		{
			get { return _playerCount; }
			set
			{
				// Don't do anything if the value is the same
				if (_playerCount == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_playerCount = value;
				hasDirtyFields = true;
			}
		}

		public void SetplayerCountDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_playerCount(ulong timestep)
		{
			if (playerCountChanged != null) playerCountChanged(_playerCount, timestep);
			if (fieldAltered != null) fieldAltered("playerCount", _playerCount, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			matchTimerInterpolation.current = matchTimerInterpolation.target;
			playerCountInterpolation.current = playerCountInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _matchTimer);
			UnityObjectMapper.Instance.MapBytes(data, _playerCount);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_matchTimer = UnityObjectMapper.Instance.Map<float>(payload);
			matchTimerInterpolation.current = _matchTimer;
			matchTimerInterpolation.target = _matchTimer;
			RunChange_matchTimer(timestep);
			_playerCount = UnityObjectMapper.Instance.Map<int>(payload);
			playerCountInterpolation.current = _playerCount;
			playerCountInterpolation.target = _playerCount;
			RunChange_playerCount(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _matchTimer);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _playerCount);

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
				if (matchTimerInterpolation.Enabled)
				{
					matchTimerInterpolation.target = UnityObjectMapper.Instance.Map<float>(data);
					matchTimerInterpolation.Timestep = timestep;
				}
				else
				{
					_matchTimer = UnityObjectMapper.Instance.Map<float>(data);
					RunChange_matchTimer(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (playerCountInterpolation.Enabled)
				{
					playerCountInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					playerCountInterpolation.Timestep = timestep;
				}
				else
				{
					_playerCount = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_playerCount(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (matchTimerInterpolation.Enabled && !matchTimerInterpolation.current.UnityNear(matchTimerInterpolation.target, 0.0015f))
			{
				_matchTimer = (float)matchTimerInterpolation.Interpolate();
				//RunChange_matchTimer(matchTimerInterpolation.Timestep);
			}
			if (playerCountInterpolation.Enabled && !playerCountInterpolation.current.UnityNear(playerCountInterpolation.target, 0.0015f))
			{
				_playerCount = (int)playerCountInterpolation.Interpolate();
				//RunChange_playerCount(playerCountInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public GameModeNetworkObject() : base() { Initialize(); }
		public GameModeNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public GameModeNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
