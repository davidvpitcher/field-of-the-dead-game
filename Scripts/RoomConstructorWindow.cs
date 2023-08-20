using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
public class RoomConstructorWindow : EditorWindow
{
    public GameObject _floorPrefab;
    public GameObject _wallPrefab;
    public GameObject _ceilingPrefab;

    public int _roomLength;
    public int _roomWidth;
    public int _roomHeight;
    private Texture2D _myTexture;



    private List<Material> _floorMaterials;
    private List<Material> _wallMaterials;
    private List<Material> _ceilingMaterials; //

    public Vector3 _roomPosition;

    private List<GameObject> _instantiatedObjects = new List<GameObject>(); 

    [MenuItem("Window/Room Constructor")]
    public static void ShowWindow()
    {
        GetWindow<RoomConstructorWindow>("Room Constructor");
    }

    private void OnEnable()
    {
        _myTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/EditorResources/logo.png");

        _floorMaterials = new List<Material>();
        _wallMaterials = new List<Material>();
        _ceilingMaterials = new List<Material>();
    }

    private void DrawMaterialListField(string label, List<Material> materials)
    {
        GUILayout.Label(label);
        for (int i = 0; i < materials.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            materials[i] = EditorGUILayout.ObjectField(materials[i], typeof(Material), true) as Material;
            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                materials.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Material", GUILayout.Width(120)))
        {
            materials.Add(null);
        }
        GUILayout.EndHorizontal();
    }


    private void LoadRoomPositionFromSelected()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            _roomPosition = selectedObject.transform.position;
        }
        else
        {
            Debug.LogError("No object selected to load position from.");
        }
    }


    private void ApplyMaterials(GameObject newObject, List<Material> materials)
    {
        if (materials != null && materials.Count > 0)
        {
            Renderer renderer = newObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterials = materials.ToArray();
            }
        }
    }
    protected virtual void OnGUI()
    {
        if (_myTexture != null)
        {
            GUILayout.Label(_myTexture);
        }

        _floorPrefab = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", _floorPrefab, typeof(GameObject), false);
        _wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", _wallPrefab, typeof(GameObject), false);
        _ceilingPrefab = (GameObject)EditorGUILayout.ObjectField("Ceiling Prefab", _ceilingPrefab, typeof(GameObject), false);

        _roomLength = EditorGUILayout.IntField("Room Length", _roomLength);
        _roomWidth = EditorGUILayout.IntField("Room Width", _roomWidth);
        _roomHeight = EditorGUILayout.IntField("Room Height", _roomHeight);

        _roomPosition = EditorGUILayout.Vector3Field("Room Position", _roomPosition);

        if (GUILayout.Button("Load Room Position From Selected"))
        {
            LoadRoomPositionFromSelected();
        }

        DrawMaterialListField("Floor Materials", _floorMaterials);
        DrawMaterialListField("Wall Materials", _wallMaterials);
        DrawMaterialListField("Ceiling Materials", _ceilingMaterials);

        if (GUILayout.Button("Load Floor Prefab From Selected"))
        {
            (_floorPrefab, _floorMaterials) = LoadPrefabAndMaterialsFromSelected();
        }

        if (GUILayout.Button("Load Wall Prefab From Selected"))
        {
            (_wallPrefab, _wallMaterials) = LoadPrefabAndMaterialsFromSelected();
        }

        if (GUILayout.Button("Load Ceiling Prefab From Selected"))
        {
            (_ceilingPrefab, _ceilingMaterials) = LoadPrefabAndMaterialsFromSelected();
        }
        if (GUILayout.Button("Construct Room"))
        {
            ConstructRoom();

        }
        if (GUILayout.Button("Destroy Last Room"))
        {
            DestroyLastRoom();

        }

       
        if (GUILayout.Button("Display Prefab Info"))
        {
            DisplayPrefabInfo();
        }
    }

    private void DisplayPrefabInfo()
    {
        DisplayPrefabInfo(_floorPrefab, "Floor");
        DisplayPrefabInfo(_wallPrefab, "Wall");
        DisplayPrefabInfo(_ceilingPrefab, "Ceiling");
    }


    private void DisplayPrefabInfo(GameObject prefab, string name)
    {
        if (prefab != null)
        {
            Debug.Log($"{name} Prefab Info:");

           
            Debug.Log($"Pivot Point: {prefab.transform.position}");

      
            Debug.Log($"Rotation: {prefab.transform.rotation}");

           
            Debug.Log($"Scale: {prefab.transform.localScale}");

    
            Renderer renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
               
                Debug.Log($"Bounds Center: {renderer.bounds.center}");
                Debug.Log($"Bounds Extents: {renderer.bounds.extents}");
            }
            else
            {
                Debug.LogError($"{name} Prefab does not have a Renderer component.");
            }

          
            Collider collider = prefab.GetComponent<Collider>();
            if (collider != null)
            {
                
                Debug.Log($"Collider Bounds: {collider.bounds}");
                Debug.Log($"Collider Center: {collider.bounds.center}");
                Debug.Log($"Collider Size: {collider.bounds.size}");
            }
            else
            {
                Debug.LogError($"{name} Prefab does not have a Collider component.");
            }

           
            MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
         
                Debug.Log($"Mesh Bounds: {meshFilter.sharedMesh.bounds}");
            }
            else
            {
                Debug.LogError($"{name} Prefab does not have a Mesh component.");
            }
        }
        else
        {
            Debug.LogError($"No {name} Prefab Loaded.");
        }
    }


    private (GameObject, List<Material>) LoadPrefabAndMaterialsFromSelected()
    {
        GameObject selectedObject = Selection.activeGameObject;
        List<Material> materials = new List<Material>();
        GameObject prefab = null;

        if (selectedObject != null)
        {
          
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
            {
                materials.AddRange(renderer.sharedMaterials);
            }
            else
            {
                Debug.LogError("No materials found on the selected object or its Renderer component.");
            }

         
            string selectedObjectName = selectedObject.name;
            string prefabName = selectedObjectName.Split(' ')[0];

            string[] prefabGUIDs = AssetDatabase.FindAssets($"t:prefab {prefabName}");
            if (prefabGUIDs.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[0]);
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            else
            {
                Debug.LogError($"No prefab found with the name: {prefabName}");
            }
        }
        else
        {
            Debug.LogError("No object selected to load materials and prefab from.");
        }

        return (prefab, materials);
    }





private GameObject LoadPrefabFromSelected()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            string selectedObjectName = selectedObject.name;
            string prefabName = selectedObjectName.Split(' ')[0];

            string[] prefabGUIDs = AssetDatabase.FindAssets($"t:prefab {prefabName}");
            if (prefabGUIDs.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                return prefab;
            }
            else
            {
                Debug.LogError($"No prefab found with the name: {prefabName}");
                return null;
            }
        }
        else
        {
            Debug.LogError("No object selected to load prefab from.");
            return null;
        }
    }
    private void DestroyLastRoom()
    {
      
        foreach (GameObject obj in _instantiatedObjects)
        {
            DestroyImmediate(obj);
        }
        _instantiatedObjects.Clear();

    }
    protected virtual void ConstructRoom()

    {
       
        if (_floorPrefab == null || _wallPrefab == null || _ceilingPrefab == null)
        {
            Debug.LogError("Please assign all prefabs before constructing the room.");
            return;
        }

        if (_roomLength <= 0 || _roomWidth <= 0 || _roomHeight <= 0)
        {
            Debug.LogError("Please enter valid room dimensions before constructing the room.");
            return;
        }

        Vector3 floorSize = _floorPrefab.GetComponent<Renderer>().bounds.size;
        Vector3 wallSize = _wallPrefab.GetComponent<Renderer>().bounds.size;
        Vector3 ceilingSize = _ceilingPrefab.GetComponent<Renderer>().bounds.size;

        for (int i = 0; i < _roomLength; i++)
        {
            for (int j = 0; j < _roomWidth; j++)
            {
      
                GameObject floor = PrefabUtility.InstantiatePrefab(_floorPrefab) as GameObject;
                floor.transform.position = new Vector3(i * floorSize.x, 0, j * floorSize.z) + _roomPosition;
                ApplyMaterials(floor, _floorMaterials);
                _instantiatedObjects.Add(floor);
            
                GameObject ceiling = PrefabUtility.InstantiatePrefab(_ceilingPrefab) as GameObject;
                ceiling.transform.position = new Vector3(i * floorSize.x, (_roomHeight * wallSize.y), j * floorSize.z) + _roomPosition;
                ApplyMaterials(ceiling, _ceilingMaterials);
                _instantiatedObjects.Add(ceiling);
            }
        }


        for (int i = 0; i < _roomLength; i++)
        {
            for (int j = 0; j < _roomHeight; j++)
            {
              
                GameObject eastWall = PrefabUtility.InstantiatePrefab(_wallPrefab) as GameObject;
                eastWall.transform.position = new Vector3(i * wallSize.x, j * wallSize.y, 0) + _roomPosition;
              
                ApplyMaterials(eastWall, _wallMaterials);
                _instantiatedObjects.Add(eastWall);

              
                GameObject westWall = PrefabUtility.InstantiatePrefab(_wallPrefab) as GameObject;
                westWall.transform.position = new Vector3(i * wallSize.x - wallSize.x, j * wallSize.y, (_roomWidth * wallSize.x)) + _roomPosition; 
            
                westWall.transform.rotation = Quaternion.Euler(0, -180, 0); 
              
                ApplyMaterials(westWall, _wallMaterials);
                _instantiatedObjects.Add(westWall);

            }
        }

       
        for (int i = 0; i < _roomWidth; i++)
        {
            for (int j = 0; j < _roomHeight; j++)
            {
         
                GameObject northWall = PrefabUtility.InstantiatePrefab(_wallPrefab) as GameObject;
                northWall.transform.rotation = Quaternion.Euler(0, 90, 0);
                northWall.transform.position = new Vector3(-wallSize.x, j * wallSize.y, i * wallSize.x) + _roomPosition;
     
                ApplyMaterials(northWall, _wallMaterials);
                _instantiatedObjects.Add(northWall);

           
                GameObject southWall = PrefabUtility.InstantiatePrefab(_wallPrefab) as GameObject;
                southWall.transform.rotation = Quaternion.Euler(0, -90, 0); 
                southWall.transform.position = new Vector3((_roomLength - 1) * wallSize.x, j * wallSize.y, i * wallSize.x + wallSize.x) + _roomPosition; // Adjust position here
       
                ApplyMaterials(southWall, _wallMaterials);
                _instantiatedObjects.Add(southWall);
            }
        }

    }

}
