using UnityEngine;
using UnityEditor;

namespace MecanimEventSystem.Tools
{
    [CustomEditor(typeof (MesEntity))]
    public class MesEntityInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
                GUI.enabled = false;
            
            if (GUILayout.Button("Update"))
            {
                MesParser.ProcessMesEntity((MesEntity) target);
            }
        }
    }
}