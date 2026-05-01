using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalLiftController : MonoBehaviour
{
    [SerializeField] private Animator liftAnimator;
    [SerializeField] private string moveTrigger = "MoveLift";
    [SerializeField] private Key activationKey = Key.Space;

    private void Awake()
    {
        if (liftAnimator == null)
        {
            liftAnimator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || liftAnimator == null)
        {
            return;
        }

        if (Keyboard.current[activationKey].wasPressedThisFrame)
        {
            liftAnimator.SetTrigger(moveTrigger);
        }
    }
}
