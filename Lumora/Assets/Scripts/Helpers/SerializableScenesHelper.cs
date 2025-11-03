using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneField
{
    [SerializeField]
    private Object sceneAsset;

    [SerializeField]
    private string sceneName;
    public string SceneName()
    {
          return sceneName; 
    }

    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.sceneName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SerializableScenesHelper: PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        SerializedProperty sceneAsset = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneName = property.FindPropertyRelative("sceneName");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        if (sceneAsset != null)
        {
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
            if (sceneAsset.objectReferenceValue != null)
            {
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif