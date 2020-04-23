using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\"][\"int\"][\"int\"][][][\"float\", \"float\", \"float\", \"bool\", \"bool\", \"int\", \"int\", \"int\"][\"string\", \"string\"]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"damage\"][\"amount\"][\"value\"][][][\"x\", \"z\", \"_movementSpeed\", \"_isGrounded\", \"_isAiming\", \"_aimInt\", \"_fireInt\", \"_hasSnipped\"][\"dyingPlayer\", \"killingPlayer\"]]")]
	public abstract partial class PlayerBehavior : NetworkBehavior
	{
		public const byte RPC_SERVER__TAKE_DAMAGE = 0 + 5;
		public const byte RPC_SERVER__ADD_HEALTH = 1 + 5;
		public const byte RPC_SERVER__SET_HEALTH = 2 + 5;
		public const byte RPC_DIE = 3 + 5;
		public const byte RPC_FIRE_ANIM = 4 + 5;
		public const byte RPC_TRIGGER_WALK_ANIM = 5 + 5;
		public const byte RPC_SERVER__ANNOUNCE_DEATH = 6 + 5;
		
		public PlayerNetworkObject networkObject = null;

		public override void Initialize(NetworkObject obj)
		{
			// We have already initialized this object
			if (networkObject != null && networkObject.AttachedBehavior != null)
				return;
			
			networkObject = (PlayerNetworkObject)obj;
			networkObject.AttachedBehavior = this;

			base.SetupHelperRpcs(networkObject);
			networkObject.RegisterRpc("Server_TakeDamage", Server_TakeDamage, typeof(int));
			networkObject.RegisterRpc("Server_AddHealth", Server_AddHealth, typeof(int));
			networkObject.RegisterRpc("Server_SetHealth", Server_SetHealth, typeof(int));
			networkObject.RegisterRpc("Die", Die);
			networkObject.RegisterRpc("FireAnim", FireAnim);
			networkObject.RegisterRpc("TriggerWalkAnim", TriggerWalkAnim, typeof(float), typeof(float), typeof(float), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int));
			networkObject.RegisterRpc("Server_AnnounceDeath", Server_AnnounceDeath, typeof(string), typeof(string));

			networkObject.onDestroy += DestroyGameObject;

			if (!obj.IsOwner)
			{
				if (!skipAttachIds.ContainsKey(obj.NetworkId)){
					uint newId = obj.NetworkId + 1;
					ProcessOthers(gameObject.transform, ref newId);
				}
				else
					skipAttachIds.Remove(obj.NetworkId);
			}

			if (obj.Metadata != null)
			{
				byte transformFlags = obj.Metadata[0];

				if (transformFlags != 0)
				{
					BMSByte metadataTransform = new BMSByte();
					metadataTransform.Clone(obj.Metadata);
					metadataTransform.MoveStartIndex(1);

					if ((transformFlags & 0x01) != 0 && (transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() =>
						{
							transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform);
							transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform);
						});
					}
					else if ((transformFlags & 0x01) != 0)
					{
						MainThreadManager.Run(() => { transform.position = ObjectMapper.Instance.Map<Vector3>(metadataTransform); });
					}
					else if ((transformFlags & 0x02) != 0)
					{
						MainThreadManager.Run(() => { transform.rotation = ObjectMapper.Instance.Map<Quaternion>(metadataTransform); });
					}
				}
			}

			MainThreadManager.Run(() =>
			{
				NetworkStart();
				networkObject.Networker.FlushCreateActions(networkObject);
			});
		}

		protected override void CompleteRegistration()
		{
			base.CompleteRegistration();
			networkObject.ReleaseCreateBuffer();
		}

		public override void Initialize(NetWorker networker, byte[] metadata = null)
		{
			Initialize(new PlayerNetworkObject(networker, createCode: TempAttachCode, metadata: metadata));
		}

		private void DestroyGameObject(NetWorker sender)
		{
			MainThreadManager.Run(() => { try { Destroy(gameObject); } catch { } });
			networkObject.onDestroy -= DestroyGameObject;
		}

		public override NetworkObject CreateNetworkObject(NetWorker networker, int createCode, byte[] metadata = null)
		{
			return new PlayerNetworkObject(networker, this, createCode, metadata);
		}

		protected override void InitializedTransform()
		{
			networkObject.SnapInterpolations();
		}

		/// <summary>
		/// Arguments:
		/// int damage
		/// </summary>
		public abstract void Server_TakeDamage(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int amount
		/// </summary>
		public abstract void Server_AddHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int value
		/// </summary>
		public abstract void Server_SetHealth(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Die(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void FireAnim(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void TriggerWalkAnim(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Server_AnnounceDeath(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}