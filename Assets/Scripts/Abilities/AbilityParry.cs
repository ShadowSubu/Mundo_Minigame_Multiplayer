using UnityEngine;

public class AbilityParry : AbilityBase
{
    internal override void OnAbilityUse(Ray ray)
    {
        Debug.Log("Parry Ability Used");
    }

    private void Parry()
    {

    }
}
