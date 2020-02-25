using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
	[SerializeField] GameObject _player;
	[SerializeField] GameObject _cam;
	[SerializeField] float _zPosMargin = 10.0f,
							_yPosMargin = 5.0f;
	[Range(0,100)]
	[SerializeField] float _rotateSpeed = 10.0f;
	[SerializeField] bool _rotTooFar = false;

	// Update is called once per frame
	void Update()
    {
		transform.position = new Vector3(
				_player.transform.position.x,
				_player.transform.position.y + _yPosMargin,
				_player.transform.position.z - _zPosMargin);

		RotateCam();
	}

	//void LateUpdate()
	//{
	//	if (_rotTooFar)
	//	{
	//		transform.Rotate(0, (-1 * Input.GetAxis(CharacterButtonsConstants.ROTATE_H)) * _rotateSpeed * Time.deltaTime, 0);
	//	}
	//}

	void RotateCam()
	{
		if (_cam.transform.position.z < _player.transform.position.z)
		{
			_rotTooFar = false;
			transform.Rotate(0, Input.GetAxis(CharacterButtonsConstants.ROTATE_H) * _rotateSpeed * Time.deltaTime, 0);
		}
		if (_cam.transform.position.z > _player.transform.position.z) { _rotTooFar = true; }
			
		/*
		if (_cam.transform.position.z > _player.transform.position.z)
			transform.Rotate(0, -Input.GetAxis(CharacterButtonsConstants.ROTATE_H) * _rotateSpeed * Time.deltaTime, 0);
		*/
	}
}
