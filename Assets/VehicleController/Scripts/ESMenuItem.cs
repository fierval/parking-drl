using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class ESMenuItem : MonoBehaviour
{

    [MenuItem("GameObject/Easy Vehicle System/Vehicles/Car", false, 10)]
    static void CreateCar01Prefab()
    {
        GameObject Car01prefab = Resources.Load("Vehicles/CarEngineEmpty") as GameObject;
        EditorUtility.SetDirty(Car01prefab);
        Instantiate(Car01prefab, Vector3.zero, Quaternion.identity);
       
    }


    [MenuItem("Easy Vehicle System/AI/NewPath(Simple)", false, 10)]
    static void CreatePathPrefab()
    {
        GameObject pathprefab = Resources.Load("Path/Path") as GameObject;
        EditorUtility.SetDirty(pathprefab);
        Instantiate(pathprefab, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("GameObject/Easy Vehicle System/Vehicles/Bus", false, 10)]
    static void CreateBusPrefab()
    {
        GameObject BusPrefab = Resources.Load("Vehicles/BusEngineEmpty") as GameObject;
        EditorUtility.SetDirty(BusPrefab);
        Instantiate(BusPrefab, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("GameObject/Easy Vehicle System/Vehicles/HeavyTruck", false, 10)]
    static void CreateheavyTruckPrefab()
    {
        GameObject heavyTruckPrefab = Resources.Load("Vehicles/heavyTruckEngineEmpty") as GameObject;
        EditorUtility.SetDirty(heavyTruckPrefab);
        Instantiate(heavyTruckPrefab, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("GameObject/Easy Vehicle System/Vehicles/TukTukEngine", false, 10)]
    static void CreateTukTukEnginePrefab()
    {
        GameObject TukTukEnginePrefab = Resources.Load("Vehicles/TukTukEngineEmpty") as GameObject;
        EditorUtility.SetDirty(TukTukEnginePrefab);
        Instantiate(TukTukEnginePrefab, Vector3.zero, Quaternion.identity);
    }

    [MenuItem("Easy Vehicle System/AI/NewPath(Advance)", false, 10)]
    static void CreateAiPathGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("AI Path(Advance)");
        go.AddComponent<ESAIPath_Single>();
        
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("Easy Vehicle System/NitroPlacer", false, 10)]
    static void CreateNitroPlacerGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("NitroPlacer");
        go.AddComponent<ESNitroPlacer>();

        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

}
#endif
