using Game.Riddles.CellsRiddle;
using UnityEditor;
using UnityEngine;

namespace Editor.InEditorTutorials
{
    [CustomPropertyDrawer(typeof(GridCellRow))]
public class GridCellRowDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var rowArray = property.FindPropertyRelative("row");
        var count = rowArray.arraySize;

        const float spacing = 2f;
        var totalSpacing = spacing * (count - 1);
        var elementWidth = (position.width - totalSpacing) / count;

        for (var i = 0; i < count; i++)
        {
            var elementRect = new Rect(
                position.x + i * (elementWidth + spacing),
                position.y,
                elementWidth,
                EditorGUIUtility.singleLineHeight
            );

            var element = rowArray.GetArrayElementAtIndex(i);

            element.objectReferenceValue = EditorGUI.ObjectField(
                elementRect,
                element.objectReferenceValue,
                typeof(GridCell),
                true);
        }

        EditorGUI.EndProperty();
    }
}
}