using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AnimVFXSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string AnimationFolder = "Assets/Animations";
    private const string MaterialFolder = "Assets/Materials";
    private const string PrefabFolder = "Assets/Prefabs";

    [MenuItem("Tools/Build Anim VFX Practice Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/Animations");
        EnsureFolder("Assets/Scripts");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Scenes");

        ClearScene();

        var groundMat = CreateMaterial("MAT_BasaltGround", new Color(0.12f, 0.13f, 0.15f), 0.2f);
        var crystalMat = CreateMaterial("MAT_AquaCrystal", new Color(0.15f, 0.85f, 1f), 0.85f, new Color(0.05f, 0.55f, 1f) * 1.8f);
        var liftMat = CreateMaterial("MAT_GraphiteLift", new Color(0.28f, 0.30f, 0.34f), 0.35f);
        var railMat = CreateMaterial("MAT_CopperRails", new Color(0.9f, 0.46f, 0.18f), 0.45f, new Color(0.55f, 0.2f, 0.05f) * 0.7f);
        var particleMat = CreateParticleMaterial("MAT_EmberParticle", new Color(1f, 0.52f, 0.18f, 0.72f), new Color(1f, 0.36f, 0.08f) * 2f);

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Basalt Ground";
        ground.transform.position = new Vector3(0f, -0.05f, 0f);
        ground.transform.localScale = new Vector3(16f, 0.1f, 12f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;

        var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crystal.name = "Animated Crystal Beacon";
        crystal.transform.position = new Vector3(-3.8f, 1.15f, 0f);
        crystal.transform.localScale = new Vector3(0.9f, 1.35f, 0.9f);
        crystal.GetComponent<Renderer>().sharedMaterial = crystalMat;
        var crystalAnimator = crystal.AddComponent<Animator>();
        crystalAnimator.runtimeAnimatorController = CreateCrystalController();

        var lift = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lift.name = "Trigger Driven Crystal Lift";
        lift.transform.position = new Vector3(2.8f, 0.25f, 0f);
        lift.transform.localScale = new Vector3(1.8f, 0.18f, 1.8f);
        lift.GetComponent<Renderer>().sharedMaterial = liftMat;
        var liftAnimator = lift.AddComponent<Animator>();
        liftAnimator.runtimeAnimatorController = CreateLiftController();
        var liftController = lift.AddComponent<CrystalLiftController>();
        SetSerializedObjectReference(liftController, "liftAnimator", liftAnimator);

        CreateRail("Copper Rail Left", new Vector3(1.45f, 2.1f, -1.25f), railMat);
        CreateRail("Copper Rail Right", new Vector3(4.15f, 2.1f, -1.25f), railMat);
        CreateRail("Rear Copper Rail Left", new Vector3(1.45f, 2.1f, 1.25f), railMat);
        CreateRail("Rear Copper Rail Right", new Vector3(4.15f, 2.1f, 1.25f), railMat);

        var ambient = CreateAmbientParticles(particleMat);
        var burst = CreateBurstParticles(particleMat);
        var burstTrigger = crystal.AddComponent<SparkBurstTrigger>();
        SetSerializedObjectReference(burstTrigger, "burstEffect", burst);

        CreateCameraAndLight();
        CreateChecklistMarker();

        PrefabUtility.SaveAsPrefabAsset(crystal, $"{PrefabFolder}/PF_AnimatedCrystalBeacon.prefab");
        PrefabUtility.SaveAsPrefabAsset(lift, $"{PrefabFolder}/PF_TriggerDrivenCrystalLift.prefab");
        PrefabUtility.SaveAsPrefabAsset(ambient.gameObject, $"{PrefabFolder}/PF_FX_AmberDrift.prefab");
        PrefabUtility.SaveAsPrefabAsset(burst.gameObject, $"{PrefabFolder}/PF_FX_CrystalSparkBurst.prefab");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ClearScene()
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }
    }

    private static RuntimeAnimatorController CreateCrystalController()
    {
        var clip = new AnimationClip
        {
            name = "CrystalLoop_PulseSpin",
            frameRate = 60f
        };

        SetCurve(clip, "m_LocalScale.x", new Keyframe(0f, 0.9f), new Keyframe(0.45f, 1.15f), new Keyframe(0.9f, 0.82f), new Keyframe(1.35f, 0.9f));
        SetCurve(clip, "m_LocalScale.y", new Keyframe(0f, 1.35f), new Keyframe(0.45f, 1.65f), new Keyframe(0.9f, 1.18f), new Keyframe(1.35f, 1.35f));
        SetCurve(clip, "m_LocalScale.z", new Keyframe(0f, 0.9f), new Keyframe(0.45f, 1.15f), new Keyframe(0.9f, 0.82f), new Keyframe(1.35f, 0.9f));
        SetCurve(clip, "localEulerAnglesRaw.y", new Keyframe(0f, 0f), new Keyframe(0.45f, 130f), new Keyframe(0.9f, 255f), new Keyframe(1.35f, 360f));
        SetLoopTime(clip, true);

        var clipPath = $"{AnimationFolder}/CrystalLoop_PulseSpin.anim";
        SaveAsset(clip, clipPath);

        var controllerPath = $"{AnimationFolder}/CrystalBeacon.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var state = controller.layers[0].stateMachine.AddState("Looping Pulse Spin", new Vector3(260f, 100f));
        state.motion = clip;
        controller.layers[0].stateMachine.defaultState = state;
        return controller;
    }

    private static RuntimeAnimatorController CreateLiftController()
    {
        var lower = CreatePositionClip("Lift_DockLower", new Vector3(2.8f, 0.25f, 0f), new Vector3(2.8f, 0.25f, 0f), 0.3f, true);
        var rise = CreatePositionClip("Lift_RiseEase", new Vector3(2.8f, 0.25f, 0f), new Vector3(2.8f, 3.25f, 0f), 1.5f, false);
        var upper = CreatePositionClip("Lift_DockUpper", new Vector3(2.8f, 3.25f, 0f), new Vector3(2.8f, 3.25f, 0f), 0.3f, true);
        var drop = CreatePositionClip("Lift_DropEase", new Vector3(2.8f, 3.25f, 0f), new Vector3(2.8f, 0.25f, 0f), 1.5f, false);

        var controllerPath = $"{AnimationFolder}/CrystalLift.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("MoveLift", AnimatorControllerParameterType.Trigger);

        var machine = controller.layers[0].stateMachine;
        var dockLower = machine.AddState("Dock Lower", new Vector3(260f, 70f));
        var liftRise = machine.AddState("Rise One-Shot", new Vector3(540f, 70f));
        var dockUpper = machine.AddState("Dock Upper", new Vector3(540f, 210f));
        var liftDrop = machine.AddState("Drop One-Shot", new Vector3(260f, 210f));

        dockLower.motion = lower;
        liftRise.motion = rise;
        dockUpper.motion = upper;
        liftDrop.motion = drop;
        machine.defaultState = dockLower;

        AddTriggerTransition(dockLower, liftRise, false);
        AddExitTransition(liftRise, dockUpper);
        AddTriggerTransition(dockUpper, liftDrop, false);
        AddExitTransition(liftDrop, dockLower);

        return controller;
    }

    private static AnimationClip CreatePositionClip(string name, Vector3 from, Vector3 to, float duration, bool loop)
    {
        var clip = new AnimationClip { name = name, frameRate = 60f };
        SetCurve(clip, "m_LocalPosition.x", Ease(new Keyframe(0f, from.x), new Keyframe(duration, to.x)));
        SetCurve(clip, "m_LocalPosition.y", Ease(new Keyframe(0f, from.y), new Keyframe(duration * 0.5f, Mathf.Lerp(from.y, to.y, 0.45f)), new Keyframe(duration, to.y)));
        SetCurve(clip, "m_LocalPosition.z", Ease(new Keyframe(0f, from.z), new Keyframe(duration, to.z)));
        SetLoopTime(clip, loop);
        SaveAsset(clip, $"{AnimationFolder}/{name}.anim");
        return clip;
    }

    private static ParticleSystem CreateAmbientParticles(Material particleMat)
    {
        var go = new GameObject("FX_AmberDrift_Continuous");
        go.transform.position = new Vector3(0f, 2.2f, 0f);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 7f;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
        main.maxParticles = 180;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 32f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(13f, 3.2f, 9f);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(MakeGradient(
            new Color(1f, 0.58f, 0.16f, 0f),
            new Color(1f, 0.78f, 0.28f, 0.75f),
            new Color(0.65f, 0.18f, 0.08f, 0f)));

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 1.15f));

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.28f;
        noise.frequency = 0.55f;
        noise.scrollSpeed = 0.35f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = particleMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        return ps;
    }

    private static ParticleSystem CreateBurstParticles(Material particleMat)
    {
        var go = new GameObject("FX_CrystalSparkBurst_OneShot");
        go.transform.position = new Vector3(-3.8f, 1.2f, 0f);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.3f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.05f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 3.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 38) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(MakeGradient(
            new Color(0.35f, 0.95f, 1f, 1f),
            new Color(1f, 0.82f, 0.32f, 0.8f),
            new Color(1f, 0.45f, 0.08f, 0f)));

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(0.2f, 1.2f), new Keyframe(1f, 0f)));

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = particleMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }

    private static void CreateRail(string name, Vector3 position, Material material)
    {
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.position = position;
        rail.transform.localScale = new Vector3(0.16f, 4.2f, 0.16f);
        rail.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateCameraAndLight()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 4.7f, -8.8f);
        cameraObject.transform.rotation = Quaternion.Euler(27f, 0f, 0f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 48f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 1000f;

        var lightObject = new GameObject("Warm Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(46f, -32f, 18f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.88f, 0.72f);
        light.intensity = 1.25f;
    }

    private static void CreateChecklistMarker()
    {
        var marker = new GameObject("Worksheet Controls Marker");
        marker.transform.position = new Vector3(0f, 0f, 0f);
    }

    private static Material CreateMaterial(string name, Color baseColor, float smoothness, Color? emission = null)
    {
        var path = $"{MaterialFolder}/{name}.mat";
        AssetDatabase.DeleteAsset(path);

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        var material = new Material(shader) { name = name };
        SetMaterialColor(material, "_BaseColor", baseColor);
        SetMaterialColor(material, "_Color", baseColor);
        material.SetFloat("_Smoothness", smoothness);

        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            SetMaterialColor(material, "_EmissionColor", emission.Value);
        }

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Material CreateParticleMaterial(string name, Color baseColor, Color emission)
    {
        var path = $"{MaterialFolder}/{name}.mat";
        AssetDatabase.DeleteAsset(path);

        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Additive");
        }

        var material = new Material(shader) { name = name };
        SetMaterialColor(material, "_BaseColor", baseColor);
        SetMaterialColor(material, "_Color", baseColor);
        SetMaterialColor(material, "_EmissionColor", emission);
        SetMaterialFloat(material, "_Surface", 1f);
        SetMaterialFloat(material, "_Blend", 0f);
        SetMaterialFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SetMaterialFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        SetMaterialFloat(material, "_ZWrite", 0f);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_EMISSION");

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SetMaterialColor(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
        }
    }

    private static void SetMaterialFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }

    private static void SetCurve(AnimationClip clip, string propertyName, params Keyframe[] keys)
    {
        SetCurve(clip, propertyName, new AnimationCurve(keys));
    }

    private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve)
    {
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), propertyName), curve);
    }

    private static Keyframe[] Ease(params Keyframe[] keys)
    {
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i].inTangent = 0f;
            keys[i].outTangent = 0f;
        }

        return keys;
    }

    private static void SetLoopTime(AnimationClip clip, bool loop)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void AddTriggerTransition(AnimatorState from, AnimatorState to, bool hasExitTime)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = hasExitTime;
        transition.duration = 0.08f;
        transition.exitTime = 0f;
        transition.AddCondition(AnimatorConditionMode.If, 0f, "MoveLift");
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.95f;
        transition.duration = 0.15f;
    }

    private static Gradient MakeGradient(Color start, Color middle, Color end)
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(middle, 0.45f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(middle.a, 0.45f),
                new GradientAlphaKey(end.a, 1f)
            });
        return gradient;
    }

    private static void SetSerializedObjectReference(Object target, string fieldName, Object value)
    {
        var serializedObject = new SerializedObject(target);
        serializedObject.FindProperty(fieldName).objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SaveAsset(Object asset, string path)
    {
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }
}
