using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCharHelpers : MonoBehaviour
{
    [SerializeField] MainMenuPlayer _menuCharController = null;

    public void Throw()
    {
        _menuCharController.InstantiateSpell();
    }

    public void Launch()
    {
        _menuCharController.InstantiateLightening();
    }

    public void Flame()
    {
        _menuCharController.InstantiateFlame();
    }

    public void StopFlame()
    {
        _menuCharController.StopFlame();
    }
}
