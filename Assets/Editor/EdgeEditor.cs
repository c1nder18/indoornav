using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Edge))]
public class EdgeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Edge edge = (Edge)target;

        if (GUILayout.Button("Add Point"))
        {
            edge.AddPoint();

            // Force the inspector b update and repaint
            EditorUtility.SetDirty(edge);
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    }
}
