using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ESRaceController))]
public class ESRaceControllerEditor : Editor
{
    public ESRaceController myscript;


    public override void OnInspectorGUI()
    {

        myscript = target as ESRaceController;
        //

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.HelpBox(" This is the Distance of vehicles Away From  Nodes", MessageType.Info);
            float DistanceApartFromNode = new float();
            Transform PathNode = null;
            EditorGUI.BeginChangeCheck();
            DistanceApartFromNode = EditorGUILayout.FloatField("DistanceApart", myscript.DistanceApartFromNode);
            PathNode = EditorGUILayout.ObjectField("PathParent", myscript.nodeparent, typeof(Transform), true) as Transform;
           
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myscript, "debugmode");
                myscript.DistanceApartFromNode = DistanceApartFromNode;
                myscript.nodeparent = PathNode;
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
        for (int i = 0; i < myscript.m_vehiclesettings.Count; i++)
        {
            GUILayout.Space(5);
            GUI.color = Color.gray;
            EditorGUILayout.BeginVertical("Box");
           
           // myscript.m_vehiclesettingsfoldout[i] = EditorGUILayout.Foldout(myscript.m_vehiclesettingsfoldout[i], myscript.m_vehiclesettings[i].VehicleName);

            myscript.m_vehiclesettingsfoldout[i] = EditorGUILayout.BeginToggleGroup(myscript.m_vehiclesettings[i].VehicleName, myscript.m_vehiclesettingsfoldout[i]);

            EditorGUI.BeginChangeCheck();

            string _name = "";
            Transform _vehicleprefab = null;
            bool _usecustom = new bool();
            Vector3 _eulerangles = Vector3.zero;
            Transform Startpoint = null;
            Vector3 _eulerstartrot = Vector3.zero;
            Vector3 OffsetPos = Vector3.zero;
           
         
                  _name =  EditorGUILayout.TextField("Name", myscript.m_vehiclesettings[i].VehicleName);
                _vehicleprefab = (Transform)EditorGUILayout.ObjectField ("VehiclePrefab",myscript.m_vehiclesettings[i]._VehiclePrefab,
                    typeof(Transform),true) as Transform;
               
                _usecustom = EditorGUILayout.Toggle("UseCutsomRotation", myscript.m_vehiclesettings[i].UseCutsomRotation);
                if (_usecustom)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("Use this to edit the rotation of vehicle prefabs that are respawned facing opposite direction", MessageType.Info);
                     GUI.color = Color.yellow;
                    _eulerangles = EditorGUILayout.Vector3Field("EulerAngles", myscript.m_vehiclesettings[i]._EulerAngles);
                    GUILayout.Space(15);
                }
                GUI.color = Color.gray;
                Startpoint  = EditorGUILayout.ObjectField("StartPoint",myscript.m_vehiclesettings[i].StartPoint,typeof(Transform),true) as Transform;
                OffsetPos = EditorGUILayout.Vector3Field("OffsetPosition", myscript.m_vehiclesettings[i].OffsetPostion);
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("use this to edit direction of your  vehicle on game start", MessageType.Info);
                _eulerstartrot = EditorGUILayout.Vector3Field("StartRot", myscript.m_vehiclesettings[i].EulerStartRot);
               
            EditorGUILayout.EndToggleGroup();

         
            EditorGUILayout.EndVertical();
            //
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myscript, "Changed Field");
                myscript.m_vehiclesettings[i].VehicleName = _name;
                myscript.m_vehiclesettings[i]._VehiclePrefab = _vehicleprefab;
                myscript.m_vehiclesettings[i].UseCutsomRotation = _usecustom;
                myscript.m_vehiclesettings[i].StartPoint = Startpoint;
                myscript.m_vehiclesettings[i].EulerStartRot = _eulerstartrot;
                myscript.m_vehiclesettings[i]._EulerAngles = _eulerangles;
                myscript.m_vehiclesettings[i].OffsetPostion = OffsetPos;
            }
        }

        GUI.color = Color.white;
        GUILayout.Space(15f);

        EditorGUILayout.HelpBox("click 'add vehicle' to add more vehicles from scene ", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Vehicle", GUILayout.Width(200), GUILayout.Height(20)))
            {
                Undo.RecordObject(myscript, "Add Vehicle");
                ESRaceController.VehicleSettings VS = new ESRaceController.VehicleSettings();
                myscript.m_vehiclesettings.Add(VS);
                //System.Array.Resize(ref myscript.vehicle, myscript.m_vehiclesettings.Count);
                System.Array.Resize(ref myscript.m_vehiclesettingsfoldout, myscript.m_vehiclesettings.Count);
              
                //System.Array.Resize(ref my_preset.wheelcolliderpreset, my_preset.addsurface.Count);
                EditorUtility.SetDirty(myscript);
            }
            if (GUILayout.Button("Clear", GUILayout.Width(160), GUILayout.Height(20)))
            {
                Undo.RecordObject(myscript, "Add Vehicle");
                myscript.m_vehiclesettings.Clear();
                ESRaceController.VehicleSettings VS = new ESRaceController.VehicleSettings();
                myscript.m_vehiclesettings.Add(VS);
                //System.Array.Resize(ref myscript.vehicle, 0);
                System.Array.Resize(ref myscript.m_vehiclesettingsfoldout, myscript.m_vehiclesettings.Count);

                //System.Array.Resize(ref my_preset.wheelcolliderpreset, my_preset.addsurface.Count);
                EditorUtility.SetDirty(myscript);
            }
            EditorGUILayout.EndHorizontal();
            ArrayEditor("nodes");
           
        if (GUI.changed)
        {
            EditorUtility.SetDirty(myscript);
        }
    }
    private void ArrayEditor(string word)
    {
        SerializedProperty MyArrayprop = serializedObject.FindProperty(word);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(MyArrayprop, true);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            //EditorGUIUtility.LookLikeControls();
        }
        serializedObject.ApplyModifiedProperties();
    }	
}
