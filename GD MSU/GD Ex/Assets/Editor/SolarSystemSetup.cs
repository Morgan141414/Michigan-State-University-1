using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SolarSystemSetup
{
    private const string ScenePath = "Assets/Scenes/SolarSystem.unity";
    private const int TextureWidth = 256;
    private const int TextureHeight = 128;

    private static readonly string[] PlanetNames = { "Mercury", "Venus", "Earth", "Mars", "Jupiter" };
    private static readonly Color[] PlanetColors =
    {
        new Color(0.55f, 0.47f, 0.39f),
        new Color(0.88f, 0.62f, 0.24f),
        new Color(0.12f, 0.38f, 0.72f),
        new Color(0.72f, 0.22f, 0.12f),
        new Color(0.77f, 0.55f, 0.32f)
    };
    private static readonly float[] OrbitRadii = { 6f, 9f, 13f, 17f, 23f };
    private static readonly float[] PlanetScales = { 0.7f, 1.15f, 1.25f, 0.95f, 2.8f };

    [MenuItem("Tools/Solar System/Build Demo Scene")]
    public static void BuildDemoScene()
    {
        EnsureFolders();
        AssetDatabase.Refresh();

        Material[] planetMaterials = new Material[PlanetNames.Length];
        for (int index = 0; index < PlanetNames.Length; index++)
        {
            planetMaterials[index] = CreatePlanetMaterial(index);
        }
        Material sunMaterial = CreateMaterial("Sun", new Color(1f, 0.48f, 0.08f), 0.25f);
        Material cometMaterial = CreateMaterial("Comet", new Color(0.42f, 0.9f, 1f), 0.4f);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject world = new GameObject("Solar System");

        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.SetParent(world.transform);
        sun.transform.localScale = Vector3.one * 4.2f;
        sun.GetComponent<Renderer>().sharedMaterial = sunMaterial;
        Light sunlight = sun.AddComponent<Light>();
        sunlight.type = LightType.Point;
        sunlight.color = new Color(1f, 0.78f, 0.48f);
        sunlight.intensity = 2.2f;
        sunlight.range = 90f;
        sun.AddComponent<SolarSystemAudio>();

        GameObject planetsGroup = new GameObject("Planets");
        planetsGroup.transform.SetParent(world.transform);
        Transform[] planets = new Transform[PlanetNames.Length];
        for (int index = 0; index < PlanetNames.Length; index++)
        {
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = PlanetNames[index];
            planet.transform.SetParent(planetsGroup.transform);
            planet.transform.localScale = Vector3.one * PlanetScales[index];
            planet.GetComponent<Renderer>().sharedMaterial = planetMaterials[index];

            OrbitMotion orbit = planet.AddComponent<OrbitMotion>();
            orbit.radius = OrbitRadii[index];
            orbit.degreesPerSecond = 5.5f - index * 0.55f;
            orbit.phaseDegrees = index * 61f;

            PlanetTarget target = planet.AddComponent<PlanetTarget>();
            target.displayName = PlanetNames[index];
            planets[index] = planet.transform;
        }

        GameObject lighting = new GameObject("Lighting");
        lighting.transform.SetParent(world.transform);
        GameObject fillObject = new GameObject("Directional Fill");
        fillObject.transform.SetParent(lighting.transform);
        Light fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.35f;
        fill.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.2f);

        GameObject cameras = new GameObject("Cameras");
        cameras.transform.SetParent(world.transform);
        GameObject mainCameraObject = new GameObject("Main Camera");
        mainCameraObject.transform.SetParent(cameras.transform);
        Camera mainCamera = mainCameraObject.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";
        mainCamera.fieldOfView = 55f;
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 250f;
        mainCameraObject.AddComponent<AudioListener>();

        GameObject comets = new GameObject("Comets");
        comets.transform.SetParent(world.transform);
        CometSpawner spawner = comets.AddComponent<CometSpawner>();
        spawner.cometMaterial = cometMaterial;

        RenderTexture minimapTexture = CreateMinimapTexture();
        GameObject mapCameraObject = new GameObject("Minimap Camera");
        mapCameraObject.transform.SetParent(cameras.transform);
        mapCameraObject.transform.position = new Vector3(0f, 58f, 0f);
        mapCameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Camera mapCamera = mapCameraObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 31f;
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(0.015f, 0.025f, 0.06f);
        mapCamera.targetTexture = minimapTexture;
        mapCamera.depth = -1f;

        Canvas canvas = CreateInterface(minimapTexture, out Text selectionLabel);
        SolarSystemCameraController cameraController = mainCameraObject.AddComponent<SolarSystemCameraController>();
        cameraController.targets = planets;
        cameraController.selectionLabel = selectionLabel;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Solar System Simulation";
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
        Debug.Log("Solar System scene created. Open Assets/Scenes/SolarSystem.unity and press Play.", canvas);
    }

    private static Canvas CreateInterface(RenderTexture minimapTexture, out Text selectionLabel)
    {
        GameObject canvasObject = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject labelObject = new GameObject("Selection Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(28f, -24f);
        labelRect.sizeDelta = new Vector2(440f, 56f);
        selectionLabel = labelObject.GetComponent<Text>();
        selectionLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        selectionLabel.fontSize = 28;
        selectionLabel.fontStyle = FontStyle.Bold;
        selectionLabel.color = new Color(0.87f, 0.93f, 1f);
        selectionLabel.text = "SOLAR SYSTEM";
        selectionLabel.raycastTarget = false;

        GameObject mapObject = new GameObject("System Map", typeof(RectTransform), typeof(RawImage));
        mapObject.transform.SetParent(canvasObject.transform, false);
        RectTransform mapRect = mapObject.GetComponent<RectTransform>();
        mapRect.anchorMin = new Vector2(1f, 1f);
        mapRect.anchorMax = new Vector2(1f, 1f);
        mapRect.pivot = new Vector2(1f, 1f);
        mapRect.anchoredPosition = new Vector2(-24f, -24f);
        mapRect.sizeDelta = new Vector2(280f, 190f);
        RawImage mapImage = mapObject.GetComponent<RawImage>();
        mapImage.texture = minimapTexture;
        mapImage.color = Color.white;
        mapImage.raycastTarget = false;
        return canvas;
    }

    private static Material CreatePlanetMaterial(int index)
    {
        string name = PlanetNames[index];
        string texturePath = "Assets/Textures/" + name + "_Surface.asset";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = CreatePlanetTexture(name, PlanetColors[index], index + 7);
            AssetDatabase.CreateAsset(texture, texturePath);
        }

        Material material = CreateMaterial(name, PlanetColors[index], 0.12f);
        material.mainTexture = texture;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D CreatePlanetTexture(string name, Color baseColor, int seed)
    {
        Texture2D texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        texture.name = name + " Surface";
        texture.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < TextureHeight; y++)
        {
            for (int x = 0; x < TextureWidth; x++)
            {
                float latitude = Mathf.Sin((float)y / TextureHeight * Mathf.PI * 5f + seed);
                float noise = Mathf.PerlinNoise((float)x / 36f + seed, (float)y / 24f + seed * 0.37f);
                float band = Mathf.Clamp01(0.48f + latitude * 0.19f + (noise - 0.5f) * 0.5f);
                Color pixel = Color.Lerp(baseColor * 0.48f, baseColor * 1.38f, band);
                pixel.a = 1f;
                texture.SetPixel(x, y, pixel);
            }
        }
        texture.Apply();
        return texture;
    }

    private static Material CreateMaterial(string name, Color color, float smoothness)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            material.name = name;
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        material.SetFloat("_Glossiness", smoothness);
        if (name == "Sun")
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.7f);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static RenderTexture CreateMinimapTexture()
    {
        const string path = "Assets/Textures/SystemMap.renderTexture";
        RenderTexture texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
        if (texture == null)
        {
            texture = new RenderTexture(512, 320, 16, RenderTextureFormat.ARGB32)
            {
                name = "System Map"
            };
            AssetDatabase.CreateAsset(texture, path);
        }
        return texture;
    }

    private static void EnsureFolders()
    {
        string[] folders =
        {
            "Assets/Scenes", "Assets/Materials", "Assets/Textures", "Assets/Scripts",
            "Assets/Sound", "Assets/Editor"
        };
        foreach (string folder in folders)
        {
            Directory.CreateDirectory(folder);
        }
    }
}