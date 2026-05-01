using UnityEngine;
using UnityEngine.InputSystem;

public class SparkBurstTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem burstEffect;
    [SerializeField] private Key triggerKey = Key.F;

    private void Update()
    {
        if (Keyboard.current == null || burstEffect == null)
        {
            return;
        }

        if (Keyboard.current[triggerKey].wasPressedThisFrame)
        {
            burstEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            burstEffect.Play();
        }
    }
}
