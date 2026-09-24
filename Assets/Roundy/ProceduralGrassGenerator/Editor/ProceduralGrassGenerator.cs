using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ProceduralGrassGenerator : EditorWindow
{
    // Base Parameters
    private int bladeCount = 5;
    private float clumpRadius = 0.2f;
    private float baseHeight = 1f;

    // Blade Shape
    private int segmentsPerBlade = 2;
    private float bladeWidth = 0.1f;
    private float tipWidth = 0.0f;
    private AnimationCurve bladeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    // Randomization
    private float heightVariation = 0.2f;
    private float widthVariation = 0.2f;
    private float curveVariation = 0.3f;
    private float angleVariation = 30f;

    // Color
    private Color tipColor = new Color(0.5f, 1f, 0.5f);
    private Color baseColor = new Color(0.05f, 0.2f, 0.05f);

    // Mesh Options
    private bool doubleSided = false;
    private bool weldVertices = true;
    private float weldThreshold = 0.0001f;

    // Ambient Occlusion
    private bool useAO = true;
    private float aoRadius = 0.1f;
    private float aoStrength = 0.25f;
    private int aoSamples = 8;

    // Material Settings
    private Shader selectedShader;
    private bool createNewMaterial = true;
    private Material existingMaterial;

    // Bulk Generation
    private int bulkGenerationCount = 5;
    private bool randomizeEachMesh = true;

    // Preview
    private GameObject previewObject;
    private Material previewMaterial;
    private Mesh previewMesh;
    private bool autoUpdate = true;
    private Vector2 scrollPosition;
    private int currentSeed;

    // Asset Management
    private const string BASE_PATH = "Assets/GeneratedGrass/";
    private const string MESH_PATH = BASE_PATH + "Meshes/";
    private const string MATERIAL_PATH = BASE_PATH + "Materials/";
    private const string PREFAB_PATH = BASE_PATH + "Prefabs/";

    private Material currentMaterial;

    [MenuItem("Tools/Roundy/Procedural Grass Generator")]
    public static void ShowWindow()
    {
        GetWindow<ProceduralGrassGenerator>("Procedural Grass Generator");
    }

    private void OnEnable()
    {
        if (selectedShader == null)
        {
            // Try to find the custom grass shader first
            selectedShader = Shader.Find("Roundy/GrassBladeWind");

            // Fall back to particles if custom shader not found
            if (selectedShader == null)
            {
                selectedShader = Shader.Find("Particles/Standard Unlit");
                Debug.LogWarning("Grass blade shader not found, falling back to Particles/Standard Unlit. For better results, ensure Roundy/GrassBladeWind shader is in your project.");
            }
        }

        if (bladeCurve == null || bladeCurve.keys.Length == 0)
        {
            bladeCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 1),
                new Keyframe(1, 1, 1, 0)
            );
        }

        EnsureDirectoriesExist();
        currentSeed = Random.Range(0, 100000);
        lastState = GrassState.Capture(this);
    }

    private void OnDestroy()
    {
        if (previewObject != null)
            DestroyImmediate(previewObject);
        if (previewMaterial != null && !AssetDatabase.Contains(previewMaterial))
            DestroyImmediate(previewMaterial);
    }
    private struct GrassState
    {
        public float[] baseParameters;
        public float[] shapeParameters;
        public float[] randomParameters;
        public Color[] colorParameters;
        public bool[] meshOptions;
        public float[] aoParameters;
        public int currentSeed;
        public bool doubleSided;  // Add explicit tracking for double-sided state

        public static GrassState Capture(ProceduralGrassGenerator generator)
        {
            return new GrassState
            {
                baseParameters = new float[] { generator.bladeCount, generator.clumpRadius, generator.baseHeight },
                shapeParameters = new float[] { generator.segmentsPerBlade, generator.bladeWidth, generator.tipWidth },
                randomParameters = new float[] { generator.heightVariation, generator.widthVariation, generator.curveVariation, generator.angleVariation },
                colorParameters = new Color[] { generator.tipColor, generator.baseColor },
                meshOptions = new bool[] { generator.doubleSided, generator.weldVertices },
                aoParameters = new float[] { generator.aoRadius, generator.aoStrength, generator.aoSamples },
                currentSeed = generator.currentSeed,
                doubleSided = generator.doubleSided  // Store double-sided state
            };
        }
    }

    private GrassState lastState;
    private Mesh baseMesh;
    private List<Vector3> baseVertices;
    private List<Vector2> baseUVs;
    private List<Color> baseColors;
    private List<int> baseTriangles;

    private void EnsureDirectoriesExist()
    {
        string[] paths = { BASE_PATH, MESH_PATH, MATERIAL_PATH, PREFAB_PATH };
        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawTitle();

        DrawStatisticsSection();
        DrawBaseSettingsSection();
        DrawBladeShapeSection();
        DrawRandomizationSection();
        DrawColorSection();
        DrawMeshOptionsSection();
        DrawAmbientOcclusionSection();
        DrawMaterialSection();
        DrawBulkGenerationSection();
        DrawGenerationButtons();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawTitle()
    {
        EditorGUILayout.Space(10);  // Add some padding at the top

        // Create a style for the title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        // Draw the title with the custom style
        EditorGUILayout.LabelField("Procedural Grass Generator v0.1", titleStyle, GUILayout.Height(30));

        // Add a line separator
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);  // Add some padding after the separator
    }

    private void DrawStatisticsSection()
    {
        EditorGUILayout.Space();
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            if (previewMesh != null)
            {
                EditorGUILayout.LabelField($"Vertices: {previewMesh.vertexCount}");
                EditorGUILayout.LabelField($"Triangles: {previewMesh.triangles.Length / 3}");
            }
        }
    }

    private void DrawBaseSettingsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            bladeCount = EditorGUILayout.IntSlider("Blade Count", bladeCount, 1, 20);
            clumpRadius = EditorGUILayout.Slider("Clump Radius", clumpRadius, 0.01f, 1.0f);
            baseHeight = EditorGUILayout.Slider("Base Height", baseHeight, 0.1f, 2f);
        }
    }

    private void DrawBladeShapeSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Blade Shape", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            segmentsPerBlade = EditorGUILayout.IntSlider("Segments Per Blade", segmentsPerBlade, 1, 8);
            bladeWidth = EditorGUILayout.Slider("Base Width", bladeWidth, 0.01f, 0.3f);
            tipWidth = EditorGUILayout.Slider("Tip Width", tipWidth, 0f, 0.3f);
            bladeCurve = EditorGUILayout.CurveField("Blade Curve", bladeCurve);
        }
    }

    private void DrawRandomizationSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Randomization", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            heightVariation = EditorGUILayout.Slider("Height Variation", heightVariation, 0f, 1f);
            widthVariation = EditorGUILayout.Slider("Width Variation", widthVariation, 0f, 1f);
            curveVariation = EditorGUILayout.Slider("Curve Variation", curveVariation, 0f, 1f);
            angleVariation = EditorGUILayout.Slider("Angle Variation", angleVariation, 0f, 360f);
        }
    }

    private void DrawColorSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            tipColor = EditorGUILayout.ColorField("Tip Color", tipColor);
            baseColor = EditorGUILayout.ColorField("Base Color", baseColor);
        }
    }
    private bool cullOff = false;

    private void DrawMeshOptionsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mesh Options", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            doubleSided = EditorGUILayout.Toggle("Double Sided", doubleSided);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "Note: Double-sided rendering can also be achieved by setting 'Cull Off' in the shader, " +
                    "which is more efficient than duplicating geometry.",
                    EditorStyles.wordWrappedMiniLabel
                );
            }

            weldVertices = EditorGUILayout.Toggle("Weld Vertices", weldVertices);
            if (weldVertices)
            {
                EditorGUI.indentLevel++;
                weldThreshold = EditorGUILayout.FloatField("Weld Threshold", weldThreshold);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void DrawAmbientOcclusionSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ambient Occlusion", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            useAO = EditorGUILayout.Toggle("Enable AO", useAO);
            if (useAO)
            {
                EditorGUI.indentLevel++;
                aoRadius = EditorGUILayout.Slider("AO Radius", aoRadius, 0.01f, 0.5f);
                aoStrength = EditorGUILayout.Slider("AO Strength", aoStrength, 0f, 1f);
                aoSamples = EditorGUILayout.IntSlider("AO Samples", aoSamples, 4, 16);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void DrawMaterialSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Material Settings", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            createNewMaterial = EditorGUILayout.Toggle("Create New Material", createNewMaterial);
            if (createNewMaterial)
            {
                selectedShader = EditorGUILayout.ObjectField("Shader", selectedShader, typeof(Shader), false) as Shader;
                cullOff = EditorGUILayout.Toggle("Disable Backface Culling", cullOff);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(
                        "Disabling backface culling in the shader is more efficient than using double-sided geometry.",
                        EditorStyles.wordWrappedMiniLabel
                    );
                }
            }
            else
            {
                existingMaterial = EditorGUILayout.ObjectField("Material", existingMaterial, typeof(Material), false) as Material;
            }
        }
    }
    private void GenerateBaseGeometry()
    {
        baseVertices = new List<Vector3>();
        baseUVs = new List<Vector2>();
        baseColors = new List<Color>();
        baseTriangles = new List<int>();

        System.Random rand = new System.Random(currentSeed);
        for (int blade = 0; blade < bladeCount; blade++)
        {
            GenerateBlade(blade, rand, baseVertices, baseUVs, baseColors, baseTriangles);
        }
    }
    private void UpdatePreview()
    {
        var currentState = GrassState.Capture(this);
        bool needsFullRegeneration = NeedsFullRegeneration(currentState);
        bool needsTopologyChange = NeedsTopologyChange(currentState);

        // Force full regeneration for topology changes
        if (needsTopologyChange)
        {
            needsFullRegeneration = true;
        }

        // Initialize or regenerate base geometry if needed
        if (baseVertices == null || baseUVs == null || baseColors == null || baseTriangles == null || needsFullRegeneration)
        {
            GenerateBaseGeometry();
        }

        // Ensure base geometry exists
        if (baseVertices == null || baseUVs == null || baseColors == null || baseTriangles == null)
        {
            Debug.LogError("Failed to generate base geometry");
            return;
        }

        Mesh previewMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>(baseVertices);
        List<Vector2> uvs = new List<Vector2>(baseUVs);
        List<Color> colors = new List<Color>(baseColors);
        List<int> triangles = new List<int>(baseTriangles);

        if (!needsFullRegeneration && NeedsGeometryUpdate(currentState))
        {
            UpdateGeometry(ref vertices, ref uvs, ref colors, ref triangles);
        }

        // Always apply double-sided if enabled
        if (doubleSided)
        {
            CreateDoubleSidedMesh(ref vertices, ref uvs, ref colors, ref triangles);
        }

        if (weldVertices)
        {
            WeldVertices(ref vertices, ref uvs, ref colors, ref triangles);
        }

        if (useAO)
        {
            CalculateAO(vertices, ref colors);
        }

        previewMesh.vertices = vertices.ToArray();
        previewMesh.triangles = triangles.ToArray();
        previewMesh.uv = uvs.ToArray();
        previewMesh.colors = colors.ToArray();
        previewMesh.RecalculateNormals();
        previewMesh.RecalculateBounds();

        if (this.previewMesh != null)
        {
            DestroyImmediate(this.previewMesh);
        }
        this.previewMesh = previewMesh;
        SetupPreviewObject();

        lastState = currentState;
    }
    private bool NeedsTopologyChange(GrassState currentState)
    {
        bool tipWidthChanged = Mathf.Approximately(tipWidth, 0) != Mathf.Approximately(lastState.shapeParameters[2], 0);
        bool segmentsChanged = !Mathf.Approximately(currentState.shapeParameters[0], lastState.shapeParameters[0]);

        return tipWidthChanged || segmentsChanged;
    }
    private bool NeedsGeometryUpdate(GrassState currentState)
    {
        return !ArraysEqual(currentState.shapeParameters, lastState.shapeParameters) ||
               !ArraysEqual(currentState.colorParameters, lastState.colorParameters) ||
               currentState.doubleSided != lastState.doubleSided;  // Check double-sided state change
    }

    private bool NeedsFullRegeneration(GrassState currentState)
    {
        return currentState.currentSeed != lastState.currentSeed ||
               !ArraysEqual(currentState.baseParameters, lastState.baseParameters) ||
               !ArraysEqual(currentState.randomParameters, lastState.randomParameters) ||
               !ArraysEqual(currentState.colorParameters, lastState.colorParameters);  // Add color check here
    }

    private bool ArraysEqual<T>(T[] a1, T[] a2)
    {
        if (a1 == null || a2 == null || a1.Length != a2.Length)
            return false;
        return !a1.Where((t, i) => !t.Equals(a2[i])).Any();
    }

    private void UpdateGeometry(ref List<Vector3> vertices, ref List<Vector2> uvs,
    ref List<Color> colors, ref List<int> triangles)
    {
        // Clear existing data if double-sided state changed
        if (lastState.doubleSided != doubleSided)
        {
            // If we're switching double-sided state, we need to regenerate the base geometry
            GenerateBaseGeometry();
            vertices = new List<Vector3>(baseVertices);
            uvs = new List<Vector2>(baseUVs);
            colors = new List<Color>(baseColors);
            triangles = new List<int>(baseTriangles);
            return;
        }

        bool isZeroTipWidth = Mathf.Approximately(tipWidth, 0f);
        int vertsPerBlade = (segmentsPerBlade * 2) + (isZeroTipWidth ? 1 : 2);

        // Update vertices for each blade
        for (int blade = 0; blade < bladeCount; blade++)
        {
            int startIndex = blade * vertsPerBlade;

            // Update all segments except the last one
            for (int seg = 0; seg < segmentsPerBlade; seg++)
            {
                int vertIndex = startIndex + (seg * 2);
                if (vertIndex + 1 >= vertices.Count) continue;

                float t = uvs[vertIndex].y;
                float segmentWidth = Mathf.Lerp(bladeWidth, isZeroTipWidth ? 0.01f : tipWidth, t);

                Vector3 center = (vertices[vertIndex] + vertices[vertIndex + 1]) * 0.5f;
                Vector3 direction = (vertices[vertIndex + 1] - vertices[vertIndex]).normalized;

                vertices[vertIndex] = center - direction * segmentWidth * 0.5f;
                vertices[vertIndex + 1] = center + direction * segmentWidth * 0.5f;

                Color vertexColor = Color.Lerp(baseColor, tipColor, t);
                colors[vertIndex] = vertexColor;
                colors[vertIndex + 1] = vertexColor;
            }

            // Handle tip
            if (isZeroTipWidth)
            {
                int tipIndex = startIndex + (segmentsPerBlade * 2);
                if (tipIndex < vertices.Count)
                {
                    colors[tipIndex] = tipColor;
                }
            }
            else
            {
                int lastSegIndex = startIndex + (segmentsPerBlade * 2);
                if (lastSegIndex + 1 < vertices.Count)
                {
                    Vector3 center = (vertices[lastSegIndex] + vertices[lastSegIndex + 1]) * 0.5f;
                    Vector3 direction = (vertices[lastSegIndex + 1] - vertices[lastSegIndex]).normalized;

                    vertices[lastSegIndex] = center - direction * tipWidth * 0.5f;
                    vertices[lastSegIndex + 1] = center + direction * tipWidth * 0.5f;

                    colors[lastSegIndex] = tipColor;
                    colors[lastSegIndex + 1] = tipColor;
                }
            }
        }
    }
    private void DrawBulkGenerationSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bulk Generation", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            bulkGenerationCount = EditorGUILayout.IntSlider("Generation Count", bulkGenerationCount, 1, 20);
            randomizeEachMesh = EditorGUILayout.Toggle("Randomize Each Mesh", randomizeEachMesh);
        }
    }
    private void DrawGenerationButtons()
    {
        EditorGUILayout.Space();
        autoUpdate = EditorGUILayout.Toggle("Auto Update Preview", autoUpdate);



        if (GUILayout.Button("Randomize Seed"))
        {
            currentSeed = Random.Range(0, 100000);
            GeneratePreview();
        }

        if (GUILayout.Button("Generate Preview") || (GUI.changed && autoUpdate))
        {
            UpdatePreview();
        }

        if (previewMesh != null)
        {
            if (GUILayout.Button("Clear Preview"))
            {
                ClearPreview();
            }

            if (GUILayout.Button("Save Single Grass Asset"))
            {
                SaveGrassAsset();
                ClearPreview();
            }

            if (GUILayout.Button("Generate Bulk Assets"))
            {
                GenerateBulkAssets();
                ClearPreview();
            }
        }
    }

    private void ClearPreview()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }
        if (previewMaterial != null && !AssetDatabase.Contains(previewMaterial))
        {
            DestroyImmediate(previewMaterial);
            previewMaterial = null;
        }
        if (previewMesh != null)
        {
            DestroyImmediate(previewMesh);
            previewMesh = null;
        }
    }

    private void GeneratePreview()
    {
        currentSeed = Random.Range(0, 100000);
        previewMesh = GenerateGrassClumpMesh(currentSeed);
        SetupPreviewObject();
    }

    private void SetupPreviewObject()
    {
        if (previewObject == null)
        {
            previewObject = new GameObject("GrassPreview");
            previewObject.AddComponent<MeshFilter>();
            previewObject.AddComponent<MeshRenderer>();
        }

        // Material handling
        if (!createNewMaterial && existingMaterial != null)
        {
            currentMaterial = existingMaterial;
        }
        else if (currentMaterial == null || createNewMaterial)
        {
            currentMaterial = new Material(selectedShader ?? Shader.Find("Particles/Standard Unlit"));
            if (cullOff)
            {
                currentMaterial.SetFloat("_Cull", 0);
            }
        }

        previewMaterial = currentMaterial;
        previewObject.GetComponent<MeshFilter>().sharedMesh = previewMesh;
        previewObject.GetComponent<MeshRenderer>().sharedMaterial = previewMaterial;
    }

    private Material CreateMaterial(string guid)
    {
        Material newMaterial;
        if (createNewMaterial)
        {
            // Create a new material instance using the selected shader
            newMaterial = new Material(selectedShader != null ? selectedShader : Shader.Find("Particles/Standard Unlit"));
            if (cullOff)
            {
                newMaterial.SetFloat("_Cull", 0); // 0 = Off, 1 = Front, 2 = Back
            }
            string materialPath = $"{MATERIAL_PATH}GrassMaterial_{guid}.mat";
            AssetDatabase.CreateAsset(newMaterial, materialPath);
            return newMaterial;
        }
        else
        {
            // Use existing material
            return existingMaterial;
        }
    }


    private void SaveGrassAsset()
    {
        string guid = System.Guid.NewGuid().ToString("N").Substring(0, 8);
        SaveGrassAssetWithGuid(guid);
    }

    private void SaveGrassAssetWithGuid(string guid)
    {
        // Save mesh
        string meshPath = $"{MESH_PATH}GrassClump_{guid}.asset";
        Mesh savedMesh = Instantiate(previewMesh);
        AssetDatabase.CreateAsset(savedMesh, meshPath);

        // Create or get material
        Material material = CreateMaterial(guid);
        if (material == null)
        {
            Debug.LogError("Failed to create or get material for grass asset");
            return;
        }

        // Create prefab
        GameObject prefabRoot = new GameObject($"GrassClump_{guid}");
        MeshFilter meshFilter = prefabRoot.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = prefabRoot.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = savedMesh;
        meshRenderer.sharedMaterial = material;

        // Save prefab
        string prefabPath = $"{PREFAB_PATH}GrassClump_{guid}.prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        DestroyImmediate(prefabRoot);

        if (prefabAsset != null)
        {
            // Create instance in scene
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance != null)
            {
                Undo.RegisterCreatedObjectUndo(instance, "Spawn Grass Prefab");
                Selection.activeObject = instance;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void GenerateBulkAssets()
    {
        string materialGuid = System.Guid.NewGuid().ToString("N").Substring(0, 8);
        Material sharedMaterial = null;

        // If creating new material, create one to share across all instances
        if (createNewMaterial)
        {
            sharedMaterial = new Material(selectedShader != null ? selectedShader : Shader.Find("Particles/Standard Unlit"));
            string materialPath = $"{MATERIAL_PATH}GrassMaterial_{materialGuid}.mat";
            AssetDatabase.CreateAsset(sharedMaterial, materialPath);
        }
        else
        {
            sharedMaterial = existingMaterial;
        }

        if (sharedMaterial == null)
        {
            Debug.LogError("Failed to create or get material for bulk generation");
            return;
        }

        // Keep track of created instances for undo operation
        List<GameObject> createdInstances = new List<GameObject>();

        for (int i = 0; i < bulkGenerationCount; i++)
        {
            if (randomizeEachMesh)
            {
                currentSeed = Random.Range(0, 100000);
                previewMesh = GenerateGrassClumpMesh(currentSeed);
            }

            string guid = System.Guid.NewGuid().ToString("N").Substring(0, 8);

            // Save mesh
            string meshPath = $"{MESH_PATH}GrassClump_{guid}.asset";
            Mesh savedMesh = Instantiate(previewMesh);
            AssetDatabase.CreateAsset(savedMesh, meshPath);

            // Create prefab
            GameObject prefabRoot = new GameObject($"GrassClump_{guid}");
            MeshFilter meshFilter = prefabRoot.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = prefabRoot.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = savedMesh;
            meshRenderer.sharedMaterial = sharedMaterial;

            // Save prefab
            string prefabPath = $"{PREFAB_PATH}GrassClump_{guid}.prefab";
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            DestroyImmediate(prefabRoot);

            // Instantiate in scene
            if (prefabAsset != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
                if (instance != null)
                {
                    // Arrange instances in a grid
                    float spacing = 2f; // Space between instances
                    int rowSize = Mathf.CeilToInt(Mathf.Sqrt(bulkGenerationCount)); // Calculate grid size
                    int row = i / rowSize;
                    int col = i % rowSize;

                    instance.transform.position = new Vector3(col * spacing, 0, row * spacing);
                    createdInstances.Add(instance);
                }
            }
        }

        // Group all undo operations together
        if (createdInstances.Count > 0)
        {
            Undo.RecordObjects(createdInstances.ToArray(), "Spawn Grass Prefabs");
            // Select all created instances
            Selection.objects = createdInstances.ToArray();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Generated {bulkGenerationCount} grass assets with shared material", "OK");
    }



    private Mesh GenerateGrassClumpMesh(int seed)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        System.Random rand = new System.Random(seed);

        for (int blade = 0; blade < bladeCount; blade++)
        {
            GenerateBlade(blade, rand, vertices, uvs, colors, triangles);
        }

        if (doubleSided)
        {
            CreateDoubleSidedMesh(ref vertices, ref uvs, ref colors, ref triangles);
        }

        if (weldVertices)
        {
            WeldVertices(ref vertices, ref uvs, ref colors, ref triangles);
        }

        if (useAO)
        {
            CalculateAO(vertices, ref colors);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void GenerateBlade(int bladeIndex, System.Random rand,
    List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, List<int> triangles)
    {
        float heightMultiplier = 1f + Mathf.Clamp(RandomRange(rand, -heightVariation, heightVariation), -0.9f, 1f);
        float finalHeight = Mathf.Max(baseHeight * heightMultiplier, 0.01f);
        float width = bladeWidth * (1f + RandomRange(rand, -widthVariation, widthVariation));
        float curveStrength = RandomRange(rand, -curveVariation, curveVariation);

        float angle = (bladeIndex / (float)bladeCount) * Mathf.PI * 2f +
                      RandomRange(rand, -angleVariation, angleVariation) * Mathf.Deg2Rad;

        Vector3 baseOffset = new Vector3(
            Mathf.Cos(angle) * clumpRadius * RandomRange(rand, 0f, 1f),
            0f,
            Mathf.Sin(angle) * clumpRadius * RandomRange(rand, 0f, 1f)
        );

        int baseVertex = vertices.Count;
        bool isZeroTipWidth = Mathf.Approximately(tipWidth, 0f);

        // Generate all segments normally except the last one
        for (int seg = 0; seg <= segmentsPerBlade; seg++)
        {
            float t = seg / (float)segmentsPerBlade;
            float curveT = bladeCurve.Evaluate(t);
            float segmentHeight = finalHeight * curveT;
            float xOffset = t * t * curveStrength;

            // Special handling for the last segment if tip width is zero
            float segmentWidth;
            if (isZeroTipWidth && seg == segmentsPerBlade)
            {
                // Don't add the final two vertices if we're going to make a triangle tip
                continue;
            }
            else
            {
                segmentWidth = Mathf.Lerp(width, isZeroTipWidth ? 0.01f : tipWidth, t);
            }

            Vector3 center = baseOffset + new Vector3(xOffset, segmentHeight, 0f);
            Vector3 right = new Vector3(segmentWidth / 2f, 0f, 0f);
            right = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) * right;

            vertices.Add(center - right);
            vertices.Add(center + right);

            float uvY = segmentHeight / finalHeight;
            float uvWidth = segmentWidth / width;
            uvs.Add(new Vector2(0.5f - uvWidth * 0.5f, uvY));
            uvs.Add(new Vector2(0.5f + uvWidth * 0.5f, uvY));

            Color vertexColor = Color.Lerp(baseColor, tipColor, t);
            colors.Add(vertexColor);
            colors.Add(vertexColor);

            // Add triangles for all but the last segment
            if (seg < segmentsPerBlade - 1)
            {
                int segBase = baseVertex + seg * 2;
                triangles.Add(segBase);
                triangles.Add(segBase + 1);
                triangles.Add(segBase + 2);

                triangles.Add(segBase + 1);
                triangles.Add(segBase + 3);
                triangles.Add(segBase + 2);
            }
        }

        // Handle the final segment
        if (isZeroTipWidth)
        {
            // Add single tip vertex
            float t = 1f;
            float finalCurveT = bladeCurve.Evaluate(t);
            Vector3 tipPos = baseOffset + new Vector3(curveStrength, finalHeight * finalCurveT, 0f);

            vertices.Add(tipPos);
            uvs.Add(new Vector2(0.5f, 1f));
            colors.Add(tipColor);

            // Create single triangle for tip
            int lastSegStart = baseVertex + (segmentsPerBlade - 1) * 2;
            triangles.Add(lastSegStart);
            triangles.Add(lastSegStart + 1);
            triangles.Add(vertices.Count - 1);
        }
        else
        {
            // Add final quad triangles normally
            int lastSegStart = baseVertex + (segmentsPerBlade - 1) * 2;
            triangles.Add(lastSegStart);
            triangles.Add(lastSegStart + 1);
            triangles.Add(lastSegStart + 2);

            triangles.Add(lastSegStart + 1);
            triangles.Add(lastSegStart + 3);
            triangles.Add(lastSegStart + 2);
        }
    }


    private void CreateDoubleSidedMesh(ref List<Vector3> vertices, ref List<Vector2> uvs,
        ref List<Color> colors, ref List<int> triangles)
    {
        int originalVertCount = vertices.Count;

        // Duplicate vertices for back faces
        for (int i = 0; i < originalVertCount; i++)
        {
            vertices.Add(vertices[i]);
            uvs.Add(uvs[i]);
            colors.Add(colors[i]);
        }

        // Add back face triangles (reversed winding)
        int triCount = triangles.Count;
        for (int i = 0; i < triCount; i += 3)
        {
            triangles.Add(triangles[i] + originalVertCount);
            triangles.Add(triangles[i + 2] + originalVertCount);
            triangles.Add(triangles[i + 1] + originalVertCount);
        }
    }

    private void WeldVertices(ref List<Vector3> vertices, ref List<Vector2> uvs,
        ref List<Color> colors, ref List<int> triangles)
    {
        Dictionary<Vector3, int> welded = new Dictionary<Vector3, int>();
        List<Vector3> newVerts = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<Color> newColors = new List<Color>();
        List<int> vertexRemap = new List<int>();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 roundedVertex = RoundVector3(vertices[i], weldThreshold);
            if (!welded.ContainsKey(roundedVertex))
            {
                welded.Add(roundedVertex, newVerts.Count);
                newVerts.Add(vertices[i]);
                newUVs.Add(uvs[i]);
                newColors.Add(colors[i]);
                vertexRemap.Add(welded[roundedVertex]);
            }
            else
            {
                vertexRemap.Add(welded[roundedVertex]);
            }
        }

        // Remap triangles
        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i] = vertexRemap[triangles[i]];
        }

        vertices = newVerts;
        uvs = newUVs;
        colors = newColors;
    }

    private Vector3 RoundVector3(Vector3 v, float precision)
    {
        return new Vector3(
            Mathf.Round(v.x / precision) * precision,
            Mathf.Round(v.y / precision) * precision,
            Mathf.Round(v.z / precision) * precision
        );
    }

    private void CalculateAO(List<Vector3> vertices, ref List<Color> colors)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float ao = 1.0f;
            Vector3 pos = vertices[i];

            // Sample points in hemisphere
            for (int s = 0; s < aoSamples; s++)
            {
                float angle = (float)s / aoSamples * Mathf.PI * 2f;
                Vector3 sampleOffset = new Vector3(
                    Mathf.Cos(angle) * aoRadius,
                    0f,
                    Mathf.Sin(angle) * aoRadius
                );

                // Check for nearby geometry
                foreach (Vector3 otherPos in vertices)
                {
                    if ((otherPos - (pos + sampleOffset)).magnitude < aoRadius)
                    {
                        ao -= aoStrength / aoSamples;
                    }
                }
            }

            ao = Mathf.Clamp01(ao);
            colors[i] *= ao;
        }
    }

    private float RandomRange(System.Random rand, float min, float max)
    {
        return (float)rand.NextDouble() * (max - min) + min;
    }
}