using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] bool   _rotateClickwise = true,
                            _setThrow = false,
                            _setLaunch = false,
                            _setFire = false;

    [SerializeField] float _rotateSpeed = 10.0f;
    [SerializeField] Animator _anim = null;

    [SerializeField]
    GameObject  _spell,
                _lightening,
                _fire;

    [SerializeField]
    float   _timer = 0.0f,
            _maxTime = 10.0f,
            _resetMargin = 1.0f;

    [SerializeField] int _animCounter = 0;

    [SerializeField] Transform _throwTransform;
    // Update is called once per frame
    void Update()
    {
        if(_rotateClickwise)
            transform.Rotate(new Vector3(0, _rotateSpeed * Time.deltaTime, 0));
        else
            transform.Rotate(new Vector3(0, -_rotateSpeed * Time.deltaTime,0));

        Timer();
        SetAnimations();
    }

    void Timer()
    {
        _timer += Time.deltaTime;

        if (_timer >= _maxTime + _resetMargin)
        {
            _timer = 0.0f;
            _animCounter = 0;
            _setThrow = false;
            _setLaunch = false;
            _setFire = false;
        }
    }

    void SetAnimations()
    {
        float firstAnim = _maxTime / 3;
        float secondAnim = _maxTime / 2;
        float thirdAnim = _maxTime;

        if (_timer > firstAnim && _animCounter == 0)
            _animCounter++;
        if (_timer > secondAnim && _animCounter == 1)
            _animCounter++;
        if (_timer > thirdAnim && _animCounter == 2)
            _animCounter++;


        switch (_animCounter)
        {
            case 1:
                TriggerAnimations(_setThrow, "throw");
                _setThrow = true;
                break;

            case 2:
                TriggerAnimations(_setLaunch, "launch");
                _setLaunch = true;
                break;
            case 3:
                TriggerAnimations(_setFire, "fire");
                _setFire = true;
                break;
            default:
                
                break;
        }
    }

    void TriggerAnimations(bool setBool, string animName)
    {
        if (!setBool)
        {
            _anim.SetBool(animName, true);
        }
    }

    public void InstantiateSpell()
    {
        Instantiate(_spell, _throwTransform.transform.position, _throwTransform.transform.rotation);
        _anim.SetBool("throw", false);
    }

    public void InstantiateLightening()
    {
        Instantiate(_lightening, _throwTransform.transform.position, _throwTransform.transform.rotation);
        _anim.SetBool("launch", false);
    }

    public void InstantiateFlame()
    {
        _fire.SetActive(true);
        _anim.SetBool("fire", false);
    }

    public void StopFlame()
    {
        _fire.SetActive(false);
    }
}
