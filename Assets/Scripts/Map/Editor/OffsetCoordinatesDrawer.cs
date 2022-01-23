using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexOffsetCoordinates))]
public class OffsetCoordinatesDrawer : PropertyDrawer
{
    #region Unity Methods
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HexOffsetCoordinates coordinates = new HexOffsetCoordinates(
            property.FindPropertyRelative("col").intValue,
            property.FindPropertyRelative("row").intValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
    #endregion
}
