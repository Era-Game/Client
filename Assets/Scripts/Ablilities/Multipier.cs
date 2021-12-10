using Managers;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu]
    public class Multipier : Ability
    {
        public float multiply_factor;

        public override void Activate()
        {
            Debug.Log("Setting Multiplier Factor: " + multiply_factor);
            //API.instance.Ability_Multiplier();
        }

        public override void BeginCooldown()
        {
            Debug.Log("Setting Multiplier Factor Back To 1");
            RelayGameManager.instance.setMultiplierFactor(1);
        }

        public float getMultiplyFactor()
        {
            return multiply_factor;
        }
    }
}

