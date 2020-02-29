using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateSpell : MonoBehaviour
{
	[SerializeField] GameObject _testSpell;

    // Start is called before the first frame update
    public void ThrowSpell()
    {
		Instantiate(_testSpell, transform.position, transform.localRotation);
	}

	public void SetSpell(GameObject spellObj)
	{
		_testSpell = spellObj;
	}
}
