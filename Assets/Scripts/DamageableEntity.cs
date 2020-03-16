using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;

public class DamageableEntity : PlayerBehavior
{
    public int health = 100;
    public int food = 100;
    public int water = 100;


    public int MaxEverything = 100;
    public int damage = 0;
    public float damageForce = 0f;
    public float damageRecoilForce = 0f;
	public bool _loadedFromServer = false;

	[SerializeField] bool _canQuit = false;

	public void ResetStats()
    {
        networkObject.SendRpc(RPC_SERVER__SET_FOOD, Receivers.All, MaxEverything);
        networkObject.SendRpc(RPC_SERVER__SET_WATER, Receivers.All, MaxEverything);
        networkObject.SendRpc(RPC_SERVER__SET_HEALTH, Receivers.All, MaxEverything);
    }

    public void ResetWater()
    {
        networkObject.SendRpc(RPC_SERVER__SET_WATER, Receivers.All, MaxEverything);
    }

    public void ReduceWater(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__DEDUCT_WATER, Receivers.All, amount);
    }

    public void ReduceFood(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__DEDUCT_FOOD, Receivers.All, amount);
    }

    public void AddWater(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__ADD_WATER, Receivers.All, amount);
    }

    public void AddFood(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__ADD_FOOD, Receivers.All, amount);
    }

    public void AddHealth(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__ADD_HEALTH, Receivers.All, amount);
    }

    public void SetWater(int value)
    {
        networkObject.SendRpc(RPC_SERVER__SET_WATER, Receivers.All, value);
    }

    public void SetFood(int value)
    {
        networkObject.SendRpc(RPC_SERVER__SET_FOOD, Receivers.All, value);
    }

    public void SetHealth(int value)
    {
        networkObject.SendRpc(RPC_SERVER__SET_HEALTH, Receivers.All, value);
    }


    public override void Server_AddHealth(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.health += args.GetNext<int>();
        });
    }

    public override void Server_AddFood(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.food += args.GetNext<int>();
        });
    }

    public override void Server_DeductFood(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.food -= args.GetNext<int>();
        });
    }

    public override void Server_AddWater(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.water += args.GetNext<int>();
        });
    }

    public override void Server_DeductWater(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.water -= args.GetNext<int>();
        });
    }

    public override void Server_SetFood(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.food = args.GetNext<int>();
        });
    }

    public override void Server_SetHealth(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.health = args.GetNext<int>();
        });
    }

    public override void Server_SetWater(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.water = args.GetNext<int>();
        });
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
		if(networkObject != null)
			networkObject.health = MaxEverything;
    }

    protected virtual void FixedUpdate()
    {
        if (transform.position.y < -1000)
        {
            OnDeath();
        }

    }
    
    public override void Server_TakeDamage(RpcArgs args)
    {
		//this is the arg setup in the wizard
		int damage = args.GetNext<int>();

		int damageDealt = Mathf.Min(damage, networkObject.health);

		MainThreadManager.Run(() =>
        {
			//TakeDamage(null, args.GetNext<int>());


			/*
			if (damage < 1 && networkObject != null)
				return 0;
			*/

			if (networkObject != null)
			{
				//networkObject.health -= damageDealt;
				health -= damageDealt;
			}
				

			if (networkObject.health <= 0)
			{
				OnDeath();
			}

			/*
			Rigidbody rb = GetComponent<Rigidbody>();
			if (damageDealt > 0 && rb != null && networkObject != null)
			{
				rb.AddForce((transform.position - source.transform.position).normalized *
						source.damageForce * rb.mass, ForceMode.Impulse);
			}


			Rigidbody sourceRb = source.GetComponent<Rigidbody>();
			if (damageDealt > 0 && sourceRb != null)
			{
				sourceRb.AddForce((source.transform.position - transform.position).normalized *
						source.damageRecoilForce * sourceRb.mass, ForceMode.Impulse);
			}
			*/

			//return damageDealt;

		});
    }

	public virtual void TakeDamage(DamageableEntity source, int damage)
	{
		if (!networkObject.IsServer)
			return;

        if (networkObject != null)
        {
            networkObject.SendRpc(RPC_SERVER__TAKE_DAMAGE, Receivers.All, damage);

            return;
        }
    }

    protected virtual void OnDeath(bool transferScene)
    {
		if (networkObject != null && networkObject.IsOwner)
		{
			//networkObject.Networker.Disconnect(true);//disconnect from server
			networkObject.SendRpc(RPC_DIE, Receivers.All); //to make this a buffered call

			if (transferScene)
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);//send to connect screen

			//this is to allow proper respawning if player connects after losing
			if (!NetworkManager.Instance.Networker.IsServer)
			{
				NetworkManager.Instance.Disconnect();
			}
		}
	}

	protected virtual void OnDeath()
	{
		if (networkObject.IsOwner)
		{
			//networkObject.Networker.Disconnect(true);//disconnect from server
			networkObject.SendRpc(RPC_DIE, Receivers.All); //to make this a buffered call
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);//send to connect screen

			//this is to allow proper respawning if player connects after losing
			if (!NetworkManager.Instance.Networker.IsServer)
			{
				NetworkManager.Instance.Disconnect();
			}
		}
	}

	protected virtual void OnCollisionEnter(Collision collision)
    {
        DamageableEntity de = null;

        if (damage > 0 &&
                (de = collision.gameObject.GetComponent<DamageableEntity>()) != null)
        {
            //de.TakeDamage(this, damage);
            //de.networkObject.SendRpc(RPC_SERVER__TAKE_DAMAGE, Receivers.All, damage);
        }
    }

	public override void Die(RpcArgs args)
	{
		MainThreadManager.Run(() =>
		{
			networkObject.Destroy();
		});
	}

	//Incase a player closes the game without dying, 
	//the player will be kicked off the server avoiding errors from unexpected closures
	void OnApplicationQuit()
	{
		Debug.Log("Quitting the Player");

		Application.wantsToQuit += StopQuit;
		OnDeath(false);
		Application.wantsToQuit -= StopQuit;
		Application.Quit();
	}

	//added to closing event to allow the player to disconnect 
	bool StopQuit()
	{
		return false;
	}
}
