using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using Steamworks;

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

    public Animator _anim = null;
    public GameObject DeathUI_Announcement_prefab;
    public GameObject GlobalGameUI;
    public float DEATH_UI_MESSAGE_TIMER = 2;

    public string playerName;

    public GameObject playersKilledScrollBox;

    

    public void ResetStats()
    {
        networkObject.SendRpc(RPC_SERVER__SET_HEALTH, Receivers.All, MaxEverything);
    }

    public void AddHealth(int amount)
    {
        networkObject.SendRpc(RPC_SERVER__ADD_HEALTH, Receivers.All, amount);
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

    public override void Server_SetHealth(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            networkObject.health = args.GetNext<int>();
        });
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (networkObject != null)
            networkObject.health = MaxEverything;

        if (networkObject != null && networkObject.IsOwner)
        {
            networkObject.SendRpc(RPC_SERVER__ANNOUNCE_PLAYER_NAME, Receivers.AllBuffered, SteamClient.Name);
        }

        GlobalGameUI = GameObject.FindWithTag("Global Game UI");
    }

    public override void Server_AnnouncePlayerName(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            string name = args.GetNext<string>();
            //Debug.Log("Steam name: " + name);
            playerName = name;
        });
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

            if (networkObject != null)
            {
                //networkObject.health -= damageDealt;
                health -= damageDealt;
            }


            if (networkObject.health <= 0)
            {
                OnDeath();
            }
        });
    }

    public virtual void TakeDamage(string killingPlayer, int damage)
    {
        if (networkObject != null)
        {
            networkObject.SendRpc(RPC_SERVER__TAKE_DAMAGE, Receivers.All, damage);

            //Debug.Log("damageableEntity::TakeDamage::KillingPlayer: " + killingPlayer);

            if (networkObject.health <= 0)
            {
                //Debug.Log("DamageableEntity::TakeDamage::killingPlayer: "  + killingPlayer);
                AnnounceWhoKilledUs(killingPlayer);
                foreach (var character in FindObjectsOfType<NewCharacterController>())
                {
                    if (character != null && character.playerName == killingPlayer)
                    {
                        character.GetComponentInChildren<Camera>().enabled = true;
                    }
                }
            }
            else
            {
                //Debug.Log("damageableentitiy::networkObject.health: " + networkObject.health);
            }

            return;
        }
    }

    void AnnounceWhoKilledUs(string killingPlayer)
    {
        //Debug.Log("DamageableEntity::AnnounceWhoKilledUs::killingPlayer " + killingPlayer);
        //Debug.Log("DamageableEntity::AnnounceWhoKilledUs::SteamClient.Name " + SteamClient.Name);

        if (networkObject != null)
        {
            networkObject.SendRpc(RPC_SERVER__ANNOUNCE_DEATH, Receivers.All, playerName, killingPlayer);
        }
    }

    public override void Server_AnnounceDeath(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.IsServer)
            {
                return;
            }

            string dyingPlayer = args.GetNext<string>();
            string killingPlayer = args.GetNext<string>();

            //Debug.Log("dying player: " + dyingPlayer + " killingPlayer: " + killingPlayer);

            GameObject go = Instantiate(DeathUI_Announcement_prefab, GlobalGameUI.GetComponent<GameUI>().playersKilledScrollBoxContent.transform);
            Canvas.ForceUpdateCanvases();
            GlobalGameUI.GetComponent<GameUI>().playersKilledScrollBox.normalizedPosition = new Vector2(0, 0);

            PlayerKilledUI pkilled = go.GetComponent<PlayerKilledUI>();
            pkilled.UpdateUI(dyingPlayer, killingPlayer);
            
            Destroy(go, DEATH_UI_MESSAGE_TIMER);
            

        });
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
        if (networkObject != null && networkObject.IsOwner)
        {
            //networkObject.Networker.Disconnect(true);//disconnect from server
            networkObject.SendRpc(RPC_DIE, Receivers.All); //to make this a buffered call
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);//send to connect screen

            //this is to allow proper respawning if player connects after losing
            if (NetworkManager.Instance != null && !NetworkManager.Instance.Networker.IsServer)
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

    public override void FireAnim(RpcArgs args)
    {
        MainThreadManager.Run(() =>
        {
            GetComponent<Animator>().SetInteger("fireInt", 0);
            Debug.Log("Fire Flame");
        });
    }

    public override void TriggerWalkAnim(RpcArgs args)
    {
        float x = args.GetNext<float>();
        float z = args.GetNext<float>();
        float _movementSpeed = args.GetNext<float>();
        bool _isGrounded = args.GetNext<bool>();
        bool _isAiming = args.GetNext<bool>();
        int _aimInt = args.GetNext<int>();
        int _fireInt = args.GetNext<int>();
        int _hasSnipped = args.GetNext<int>();

        MainThreadManager.Run(() =>
        {
            _anim.SetBool("onGround", _isGrounded);

            if (_isGrounded) _anim.SetBool("isJumping", false);

            if (_movementSpeed != 0)
            {
                if (z > 0) _anim.SetInteger("runningVal", 1);
                else if (z < 0) _anim.SetInteger("runningVal", -1);
                else _anim.SetInteger("runningVal", 0);


                if (x > 0) _anim.SetInteger("horizontalVal", 1);
                else if (x < 0) _anim.SetInteger("horizontalVal", -1);
                else _anim.SetInteger("horizontalVal", 0);
            }
            else
            {
                _anim.SetInteger("horizontalVal", 0);
                _anim.SetInteger("runningVal", 0);
            }

            if (_isAiming) _aimInt = 1;
            if (!_isAiming) _aimInt = 0;

            _anim.SetInteger("fireInt", _fireInt);
            _anim.SetInteger("aimInt", _aimInt);
            _anim.SetInteger("hasSnipped", _hasSnipped);
        });
    }
}
