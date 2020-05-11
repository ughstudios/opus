using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using BeardedManStudios.Forge.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Steamworks;
using System;
using UnityEngine.Rendering.PostProcessing;
using AmbientSounds;
using System.Linq;
using System.Reflection;
using UnityEngine.Profiling;
using UnityEditor;
using Unity.Profiling;

public class NewCharacterController : DamageableEntity
{
    //character controller replacing need for rigidbody
    [SerializeField] CharacterController _charController = null;

    //input to controll movement and velocity
    [SerializeField] Vector3 _moveInput = Vector3.zero;
    [SerializeField] Vector3 _velocity = Vector3.zero;

    [SerializeField] GameObject _camera = null;
    [SerializeField] GameObject _hudCanvas = null;
    [SerializeField] float _newCamPosZ = 0.0f;

    [SerializeField] GameObject _crossHair = null;
    [SerializeField] bool _isSniping = false;
    Vector3 _initCamAttachedPos = new Vector3(0, 4, -10);

    [SerializeField]
    float _movementSpeed = 12.0f,
                            _initMovementSpeed = 0.0f,
                            _runSpeed = 24f;

    [SerializeField] float _gravity = -9.81f;

    [SerializeField] Transform _groundCheck = null;
    [SerializeField] float _groundDistance = 0.4f;//Radius of distance
    [SerializeField] LayerMask _groundMask;
    [SerializeField]
    bool _isGrounded = false,
                            _comingDown = false;

    [SerializeField] float _jumpHeight = 20f;

    //for attacks
    [SerializeField] bool _isAiming = false;
    [SerializeField] bool _startBaseCoolDown = false;
    [SerializeField]
    float _poisonCoolDownTime = 1.5f,
                            _initPoisonCoolDownTime = 0.0f,
                            _fireCoolDownTime = 3.0f,
                            _initFireCoolTime = 0.0f,
                            _coolDownRate = 1.0f,
                            _flameRate = 3.0f,
                            _fireRefillRate = 4.0f,
                            _fireCanvasVal = 0.0f;

    [SerializeField]
    bool _startFlameCoolDown = false,
                            _startFireAttack = false,
                            _flameInstantiated = false;

    [SerializeField] Transform _fireAttackPos = null;
    [SerializeField]
    int _fireInt = 0,
                            _aimInt = 0,
                            _hasSnipped = 0;

    [SerializeField] bool _canAim = false;

    //UI refill indicators
    public RectTransform _poisonReflillTransform = null,
                            _fireReflillTransform = null,
                            _healthTransform = null,
                            _staminaTransform = null;
    [SerializeField] GameObject _staminaObj = null;
    [SerializeField]
    bool _isRunning = false,
                            _staminaIsFull = false;
    [SerializeField]
    float _staminaLevel = 4.0f,
                            _initStaminaLevel = 0.0f;
    [SerializeField] Image _staminaImageToChange = null;
    [SerializeField] Sprite[] _staminaImages = new Sprite[2];

    [SerializeField] float _healthCanvasValue;

    [SerializeField] bool _isDead = false;

    [SerializeField] GameObject _chat;
    [SerializeField] TMP_InputField _chatInput;
    [SerializeField] GameObject _pauseMenu;
    public uint _idNum;

    public GameObject _playerSkin = null;//this is for the 'enemies' to show enemies different from us
    public GameObject _enemySkin = null;//this is for the 'enemies' to show enemies different from us

    [SerializeField] PostProcessVolume postProcessVolume;
    [SerializeField] TerrainManager tm;
    [SerializeField] AmbienceManager ambienceManager;
    private Dictionary<Biome, PostProcessVolume> ppVolumes =
            new Dictionary<Biome, PostProcessVolume>();

    private Biome previousBiome = null;
    private bool isPaused = false;
    private bool isChatting = false;
    private PlayerCamera pCam;
    public GameObject LoadingScreenUI;
    private bool generationStarted = false;


    void Awake()
    {
        _charController = GetComponent<CharacterController>();
        _initMovementSpeed = _movementSpeed;
        _initPoisonCoolDownTime = _poisonCoolDownTime;
        _initFireCoolTime = _fireCoolDownTime;
        _initStaminaLevel = _staminaLevel;

        tm = FindObjectOfType<TerrainManager>();


#if UNITY_SERVER
            return;
#endif

        ambienceManager = FindObjectOfType<AmbienceManager>();
        if (ambienceManager == null)
            ambienceManager = gameObject.AddComponent<AmbienceManager>();
        ambienceManager.m_playerObject = transform;

        pCam = GetComponentInChildren<PlayerCamera>();
    }

    protected override void Start()
    {
        base.Start();
        if (networkObject != null)
            networkObject.ownershipChanged += NetworkObject_ownershipChanged;
    }

    private void NetworkObject_ownershipChanged(NetWorker sender)
    {
        if (!networkObject.IsOwner)
        {
            
        }
        else
        {
            networkObject.SendRpc(RPC_SERVER__ANNOUNCE_PLAYER_NAME, Receivers.AllBuffered, SteamClient.Name);
        }
    }


    protected override void NetworkStart()
    {
        base.NetworkStart();

        networkObject.position = transform.position;
        networkObject.rotation = transform.GetChild(0).transform.rotation;
        networkObject.positionInterpolation.target = transform.position;
        networkObject.rotationInterpolation.target = transform.GetChild(0).transform.rotation;

        networkObject.SnapInterpolations();

        networkObject.health = health;
        networkObject.isDead = _isDead;

    }


    void FocusChat()
    {
        _chatInput.Select();
        _chatInput.ActivateInputField();
    }

    void UpdatePlayerPostProcessing()
    {
        if (tm == null)
            tm = FindObjectOfType<TerrainManager>();
        if (tm == null)
            return;
        Biome biome = tm.GetBiome(transform.position);
        List<TerrainManager.BiomeStrength> biomes =
                tm.GetBiomes(transform.position);
        PostProcessVolume ppv;
        float remainder = 0.0f;

        for (int i = 0; i < biomes.Count; i++)
        {
            if (!ppVolumes.ContainsKey(biomes[i].biome))
            {
                if (biomes[i].biome.postProcessing != null)
                {
                    ppv = postProcessVolume.gameObject.
                            AddComponent<PostProcessVolume>();
                    ppv.isGlobal = postProcessVolume.isGlobal;
                    ppv.priority = postProcessVolume.priority;
                    ppv.weight = biomes[i].strength;
                    ppv.profile = biomes[i].biome.postProcessing;
                }
                else
                    ppv = null;
                ppVolumes.Add(biomes[i].biome, ppv);
            }
        }

        List<KeyValuePair<Biome, PostProcessVolume>> ppvs = ppVolumes.ToList();
        for (int i = 0; i < ppvs.Count; i++)
        {
            ppvs[i].Value.weight = 0.0f;
            for (int j = 0; j < biomes.Count; j++)
            {
                if (ppvs[i].Key == biomes[j].biome)
                {
                    if (ppvs[i].Value != null)
                        ppvs[i].Value.weight = biomes[j].strength;
                    else
                        remainder += biomes[j].strength;
                }
            }

        }
        postProcessVolume.weight = remainder;

        if (previousBiome != biome)
        {
            if (previousBiome != null)
            {

                for (int i = 0; i < previousBiome.globalSounds.Length; i++)
                {
                    AmbienceManager.RemoveSequence(previousBiome.globalSounds[i]);
                }

            }
            if (biome != null)
            {
                for (int i = 0; i < biome.globalSounds.Length; i++)
                {
                    Debug.Log(" BSequence Name: " + biome.globalSounds[i].name);
                    AmbienceManager.AddSequence(biome.globalSounds[i]);
                }
            }
            /*
            int idx = ambienceManager.m_globalSequences.Length;
            Array.Resize(ref ambienceManager.m_globalSequences, idx + biome.globalSounds.Length);
            foreach (var sequence in biome.globalSounds)
            {
                //ambienceManager.m_globalSequences[idx++] = sequence;
            }
            */
            previousBiome = biome;
        }

    }





    void Update()
    {

        float x = 0.0f, z = 0.0f;
        if (!isPaused && !isChatting)
        {
            x = CrossPlatformInputManager.GetAxisRaw("Horizontal");
            z = CrossPlatformInputManager.GetAxisRaw("Vertical");
        }
        Animations(x, z);

        if (SceneManager.GetActiveScene().name == "TerrainGenTest")
        {
            if (!generationStarted)
            {
                tm.follow.Add(gameObject);
                tm.StartGeneration();
                generationStarted = true;
            }

            UpdatePlayerPostProcessing();
        }


        if (networkObject != null)
        {

            if (networkObject.IsOwner)
            {
                
                if (!generationStarted)
                {
                    tm.follow.Add(gameObject);
                    tm.StartGeneration();
                    generationStarted = true;
                }

                if (LoadingScreenUI != null && tm.TerrainExistsAt(transform.position))
                {
                    GetComponent<CapsuleCollider>().enabled = true;
                    GetComponent<CharacterController>().enabled = true;

                    Destroy(LoadingScreenUI);
                }


                UpdatePlayerPostProcessing();

                //networkObject.SendRpc(RPC_TRIGGER_WALK_ANIM,Receivers.All,x,z,_movementSpeed,_isGrounded,_isAiming,_aimInt,_fireInt,_hasSnipped);

                if (!_chat.activeSelf)
                {
                    _chat.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (!_chatInput.isFocused)
                    {
                        FocusChat();
                        isChatting = true;
                        pCam.isChatting = true;
                    }
                }

                if (_chatInput.isFocused)
                {
                    return;
                }
                else
                {
                    isChatting = false;
                    pCam.isChatting = false;
                }


                networkObject.health = health;
                HealthCanvasMeter();

                networkObject.onGround = _anim.GetBool("onGround");
                networkObject.runninVal = _anim.GetInteger("runningVal");
                networkObject.horizontalVal = _anim.GetInteger("horizontalVal");
                networkObject.isJumping = _anim.GetBool("isJumping");
                networkObject.isThrowing = _anim.GetBool("isThrowing");
                networkObject.fireInt = _anim.GetInteger("fireInt");
                networkObject.aimInt = _anim.GetInteger("aimInt");
                networkObject.hasSnipped = _anim.GetInteger("hasSnipped");

              
            }
            else
            {
   

                health = networkObject.health;

                _anim.SetBool("onGround", networkObject.onGround);
                _anim.SetInteger("runningVal", networkObject.runninVal);
                _anim.SetInteger("horizontalVal", networkObject.horizontalVal);
                _anim.SetBool("isJumping", networkObject.isJumping);
                _anim.SetBool("isThrowing", networkObject.isThrowing);
                _anim.SetInteger("fireInt", networkObject.fireInt);
                _anim.SetInteger("aimInt", networkObject.aimInt);
                _anim.SetInteger("hasSnipped", networkObject.hasSnipped);

                _idNum = networkObject.NetworkId;
                Destroy(_camera.GetComponentInChildren<AudioListener>());//Turn this off from other cams, will give log a few times until destroyed
                

            }
        }

        PhysicsCheck();
        MovePlayer(x, z);
        RotateChild(x, z);
        if (!isPaused && !isChatting)
        {
            StartJump();
            StartThrowAttack();
        }
        PoisonCoolDown();
        if (!isPaused && !isChatting)
        {
            FireIntAttack();
            if (_canAim) Aim();
            SnipAttack();
        }
        HudAttackMeter(_poisonReflillTransform, _poisonCoolDownTime);//For base attack, poison
        HudAttackMeter(_fireReflillTransform, _fireCanvasVal);//For fire attacke
        HudHorizontalMeter(_staminaTransform, _staminaLevel, _initStaminaLevel);//For sprinting
        Die();
        TogglePauseMenu();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        SyncWithNetworkViaFixedUpate();
    }


    void PhysicsCheck()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2;//could set this to 0
        }
    }

    void MovePlayer(float x, float z)
    {

        if (Input.GetKey(KeyCode.LeftControl))
        {
            _staminaObj.SetActive(true);
        }
        else
        {
            _staminaObj.SetActive(false);
        }


        if (Input.GetKey(KeyCode.LeftControl) && _staminaIsFull)
        {
            _staminaImageToChange.sprite = _staminaImages[0];

            if (_staminaLevel > 0)
            {
                _isRunning = true;
                _staminaLevel -= Time.deltaTime;

                _anim.speed = 2.0f;

                if (_staminaLevel <= 0.0f)
                {
                    _staminaIsFull = false;
                    _isRunning = false;
                }
            }
        }
        else
        {

            _anim.speed = 1f;

            _isRunning = false;
            _staminaImageToChange.sprite = _staminaImages[1];

            if (_staminaLevel < _initStaminaLevel)
            {
                _staminaLevel += Time.deltaTime;
            }

            if (_staminaLevel >= _initStaminaLevel) _staminaIsFull = true;
        }


        _moveInput = transform.right * x + transform.forward * z;

        if (_isRunning)
        {
            _charController.Move(_moveInput * _runSpeed * Time.deltaTime);
        }
        else
        {
            _charController.Move(_moveInput * _movementSpeed * Time.deltaTime);
        }

        _velocity.y += _gravity * Time.deltaTime;

        _charController.Move(_velocity * Time.deltaTime); //delta y = 1/2 * gravity * time sq. 
    }

    void RotateChild(float x, float z)
    {

        if (z > 0 && x > 0)//combining x and z movement
        {
            _canAim = false;
            transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, 45, 0));
        }
        else if (z > 0 && x < 0)
        {
            _canAim = false;
            transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, -45, 0));
        }
        else
        {
            _canAim = true;
            transform.GetChild(0).transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
    }

    void StartJump()
    {
        if (Input.GetButtonDown(CharacterButtonsConstants.JUMP) && _isGrounded)
        {
            _anim.SetBool("isJumping", true);
        }
    }

    void StartThrowAttack()
    {
        if (!_isAiming)
        {
            if (!_startBaseCoolDown && Input.GetButtonDown(CharacterButtonsConstants.THROW) && _isGrounded ||
            !_startBaseCoolDown && Input.GetMouseButtonDown(0) && _isGrounded)
            {
                _startBaseCoolDown = true;
                _poisonCoolDownTime = 0.0f;

                if (_anim.GetBool("isThrowing") == false) //only if we are not throwing can we throw again to avoid getting stuck
                {
                    _anim.SetBool("isThrowing", true);
                    _movementSpeed = 0.0f;
                }
            }
        }
    }

    void FireIntAttack()
    {
        if (!_startFlameCoolDown)
        {
            if (Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK))
            {
                _fireInt = 1;
            }
            else
            {
                _fireInt = 0;
            }
        }
        else
        {
            _fireInt = 0;
        }

        if (_fireCoolDownTime > 0)
        {
            if (_anim.GetInteger("fireInt") > 0 && Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK))
            {
                _movementSpeed = 0.0f;
                _fireCoolDownTime -= Time.deltaTime / _flameRate;
            }
        }

        if (_startFireAttack)
        {
            if (!_flameInstantiated)
            {
                //_fireAttackPos.GetComponent<InstantiateFire>().FireAttack();
                _flameInstantiated = true;
            }
        }
        else
        {
            if (_fireAttackPos.childCount > 0)
            {

                _flameInstantiated = false;
            }
        }

        if (!Input.GetButton(CharacterButtonsConstants.BLOW_ATTACK) && _fireCoolDownTime <= _initFireCoolTime ||
            _fireCoolDownTime <= 0)
        {
            _startFlameCoolDown = true;
            _startFireAttack = false;
        }


        if (_movementSpeed == 0 && _anim.GetInteger("fireInt") == 0 && !_anim.GetBool("isThrowing") && !_isAiming)
            ResetMovementSpeed();

        if (_startFlameCoolDown)
        {
            _fireCoolDownTime += Time.deltaTime / _fireRefillRate;

            if (_fireCoolDownTime >= _initFireCoolTime)
                _startFlameCoolDown = false;
        }

        _fireCanvasVal = _fireCoolDownTime / _initFireCoolTime;//Convert to %, 100% = 1 on transform
    }


    void Aim()
    {
        if (Input.GetButton(CharacterButtonsConstants.AIM))
        {
            _isAiming = true;
            _movementSpeed = 0.0f;
        }
        else
        {
            _isAiming = false;
        }

        if (_isAiming)
        {
            _newCamPosZ += (12 * Time.deltaTime);

            if (_newCamPosZ >= -4)
            {
                _newCamPosZ = -4f;

                _crossHair.SetActive(true);
            }

            _camera.transform.GetChild(0).transform.localPosition = new Vector3(0, 4, _newCamPosZ);
        }
        else
        {
            _newCamPosZ = -10f;
            _camera.transform.GetChild(0).transform.localPosition = _initCamAttachedPos;
            _crossHair.SetActive(false);
        }
    }

    void SnipAttack()
    {
        if (_isAiming && Input.GetButtonDown(CharacterButtonsConstants.THROW) || _isAiming && Input.GetMouseButtonDown(0))
        {
            _isSniping = true;
            _hasSnipped = 1;
        }

        if (_isSniping)
            _movementSpeed = 0.0f;
    }

    void PoisonCoolDown()
    {
        _poisonCoolDownTime += Time.deltaTime * _coolDownRate;

        if (_poisonCoolDownTime >= _initPoisonCoolDownTime)
        {
            _poisonCoolDownTime = _initPoisonCoolDownTime;
            _startBaseCoolDown = false;
        }
    }

    #region These are to be called in the animator
    public void Jump()
    {
        //vel = square-root of desired jump height * (-2) * gravity
        _velocity.y = Mathf.Sqrt(_jumpHeight * (-2) * _gravity);
    }

    public void ResetMovementSpeed()
    {
        _movementSpeed = _initMovementSpeed;
    }

    public void SetIsSnippingToFalse()
    {
        _isSniping = false;
        _hasSnipped = 0;
    }

    public void MovementSpeedZero()
    {
        _movementSpeed = 0.0f;
    }

    public void SetDecentToTrue()
    {
        _comingDown = true;
    }
    #endregion

    void Animations(float x, float z)
    {
        _anim.SetBool("onGround", _isGrounded);

        //if (_isGrounded) _anim.SetBool("isJumping", false);//Possibly causing issues over the network

        if (_isGrounded && _comingDown)//fixed network jump issue
        {
            _comingDown = false;

            _anim.SetBool("isJumping", false);
        }

        if (_movementSpeed != 0)
        {
            if (z > 0) _anim.SetInteger("runningVal", 1);
            else if (z < 0) _anim.SetInteger("runningVal", -1);
            else _anim.SetInteger("runningVal", 0);


            if (x > 0 && z == 0) _anim.SetInteger("horizontalVal", 1);
            else if (x < 0 && z == 0) _anim.SetInteger("horizontalVal", -1);
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
    }

    void HudAttackMeter(Transform hudAttackTransform, float attackCoolDownTime)
    {
        float poisonRefillVal = attackCoolDownTime;

        hudAttackTransform.transform.localScale = new Vector3(1, poisonRefillVal, 1);
    }

    void HudHorizontalMeter(Transform boostTransform, float boostCoolDownTime, float divisibleBy)
    {
        float horizontalRefillVal = boostCoolDownTime / divisibleBy;

        boostTransform.transform.localScale = new Vector3(horizontalRefillVal, 1, 1);
    }

    void SyncWithNetworkViaFixedUpate()
    {
        if (networkObject == null)
            return;


        if (!networkObject.IsOwner)
        {
            //position and rotation
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;
            //transform.GetChild(0).transform.rotation = networkObject.rotation;

            //camera and canvas
            _hudCanvas.SetActive(false);
            _camera.SetActive(false);
            _playerSkin.SetActive(false);
            _enemySkin.SetActive(true);
            
        }

        if (networkObject.IsOwner)
        {
            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;

            _hudCanvas.SetActive(true);
            _camera.SetActive(true);
            _playerSkin.SetActive(true);
            _enemySkin.SetActive(false);
        }

    }

    void HealthCanvasMeter()
    {
        if (health < 0)
            _healthCanvasValue = 0.0f;
        if (health > 100)
            _healthCanvasValue = 1.0f;

        if (health <= 100 && health >= 0)
            _healthCanvasValue = (float)health / 100;

        _healthTransform.transform.localScale = new Vector3(1, _healthCanvasValue, 1);
    }

    void Die()
    {
        if (health <= 0)
        {
            _isDead = true;
        }
    }

    void TogglePauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (!_pauseMenu.activeSelf)
            {
                Cursor.visible = true;
                _pauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                isPaused = true;
                if (pCam != null)
                    pCam.isPaused = true;
            }
            else
            {
                Cursor.visible = false;
                _pauseMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                isPaused = false;
                if (pCam != null)
                    pCam.isPaused = false;
            }

        }
    }

    public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
