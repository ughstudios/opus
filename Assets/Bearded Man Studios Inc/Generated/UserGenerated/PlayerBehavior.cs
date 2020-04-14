using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedRPC("{\"types\":[[\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][\"int\"][][]]")]
	[GeneratedRPCVariableNames("{\"types\":[[\"damage\"][\"amount\"][\"amount\"][\"amount\"][\"amount\"][\"amount\"][\"value\"][\"value\"][\"value\"][][]]")]
	public abstract partial class PlayerBehavior : NetworkBehavior
	{
		public const byte RPC_SERVER__TAKE_DAMAGE = 0 + 5;
		public const byte RPC_SERVER__DEDUCT_FOOD = 1 + 5;
		public const byte RPC_SERVER__ADD_FOOD = 2 + 5;
		public const byte RPC_SERVER__DEDUCT_WATER = 3 + 5;
		public const byte RPC_SERVER__ADD_WATER = 4 + 5;
		public const byte RPC_SERVER__ADD_HEALTH = 5 + 5;
		public const byte RPC_SERVER__SET_HEALTH = 6 + 5;
		public const byte RPC_SERVER__SET_FOOD = 7 + 5;
		public const byte RPC_SERVER__SET_WATER = 8 + 5;
		public const byte RPC_DIE = 9 + 5;
		public const byte RPC_FIRE_ANIM = 10 + 5;
		
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
			networkObject.RegisterRpc("Server_DeductFood", Server_DeductFood, typeof(int));
			networkObject.RegisterRpc("Server_AddFood", Server_AddFood, typeof(int));
			networkObject.RegisterRpc("Server_DeductWater", Server_DeductWater, typeof(int));
			networkObject.RegisterRpc("Server_AddWater", Server_AddWater, typeof(int));
			networkObject.RegisterRpc("Server_AddHealth", Server_AddHealth, typeof(int));
			networkObject.RegisterRpc("Server_SetHealth", Server_SetHealth, typeof(int));
			networkObject.RegisterRpc("Server_SetFood", Server_SetFood, typeof(int));
			networkObject.RegisterRpc("Server_SetWater", Server_SetWater, typeof(int));
			networkObject.RegisterRpc("Die", Die);
			networkObject.RegisterRpc("FireAnim", FireAnim);

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
		public abstract void Server_DeductFood(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int amount
		/// </summary>
		public abstract void Server_AddFood(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int amount
		/// </summary>
		public abstract void Server_DeductWater(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int amount
		/// </summary>
		public abstract void Server_AddWater(RpcArgs args);
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
		/// int value
		/// </summary>
		public abstract void Server_SetFood(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// int value
		/// </summary>
		public abstract void Server_SetWater(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void Die(RpcArgs args);
		/// <summary>
		/// Arguments:
		/// </summary>
		public abstract void FireAnim(RpcArgs args);

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}