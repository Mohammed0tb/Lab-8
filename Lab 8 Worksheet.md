# Lab 8 Animation + VFX Implementation

This version keeps the worksheet requirements but changes the submission into a distinct crystal-lift scene.

- Looping keyframe animation: `Animated Crystal Beacon` uses `CrystalLoop_PulseSpin.anim` with scale and rotation curves, 4 keyframes, and loop time enabled.
- One-shot animation: `Trigger Driven Crystal Lift` uses `Lift_RiseEase.anim` and `Lift_DropEase.anim` with loop time disabled.
- Animator state machine: `CrystalLift.controller` has four states, a `MoveLift` trigger parameter, immediate trigger transitions, and exit-time transitions.
- Script-driven transition: `CrystalLiftController` calls `Animator.SetTrigger("MoveLift")` when Space is pressed.
- Continuous VFX: `FX_AmberDrift_Continuous` is a prewarmed looping particle system using a box shape, custom emission rate, material, color over lifetime, size over lifetime, and noise.
- One-shot VFX: `FX_CrystalSparkBurst_OneShot` is non-looping, does not play on awake, uses a burst count of 38, fades over lifetime, changes size over lifetime, and is triggered by `SparkBurstTrigger` when F is pressed.

Controls:

- Space: move the crystal lift between lower and upper docks.
- F: play the crystal spark burst.
