using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyIconDisplay
{
    static bool _hierarchyHasFocus = false;
    static EditorWindow _hierarchyEditorWindow;

    static HierarchyIconDisplay()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHiearchyWindowItemOnGUI;
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (_hierarchyEditorWindow == null)
        {
            _hierarchyEditorWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor"));
        }

        _hierarchyHasFocus = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == _hierarchyEditorWindow;
    }

    private static void OnHiearchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        Component[] components = obj.GetComponents<Component>();
        if (components == null || components.Length == 0)
            return;

        Component component = components.Length > 1 ? components[1] : components[0];
        if (component == null) return;
 
        Texture icon = AssetPreview.GetMiniThumbnail(component);
        if (icon == null) return;

        Type type = component.GetType();
    
        GUIContent content = new GUIContent
        {
            image = icon,
            tooltip = type.Name
        };

        bool isSelected = Selection.instanceIDs.Contains(instanceID);
        bool isHovering = selectionRect.Contains(Event.current.mousePosition);

        Color color = UnityEditorBackgroundColor.Get(isSelected, isHovering, _hierarchyHasFocus);
        Rect backgroundRect = selectionRect;
        Rect iconRect = new Rect(selectionRect.x, selectionRect.y, 16, 16);
        backgroundRect.width = 18.5f;

        EditorGUI.DrawRect(backgroundRect, color);
        EditorGUI.LabelField(iconRect, content);

        bool isPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null;
        if (isPrefab)
        {
            Rect prefabIconRect = new Rect(selectionRect.xMax - 18.5f, selectionRect.y, 18.5f, selectionRect.height);
            Texture prefabIcon = EditorGUIUtility.IconContent("Prefab Icon").image;
            GUI.Label(prefabIconRect, new GUIContent(prefabIcon));
        }
    }
}

public static class UnityEditorBackgroundColor
{
    static readonly Color k_defaultColor = new Color(0.7834f, 0.7834f, 0.7834f);
    static readonly Color k_defaultProColor = new Color(0.2196f, 0.2196f, 0.2196f);

    static readonly Color k_selectedColor = new Color(0.22745f, 0.447f, 0.6902f);
    static readonly Color k_selectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);

    static readonly Color k_selectedUnFocusedColor = new Color(0.68f, 0.68f, 0.68f);
    static readonly Color k_selectedUnFocusedProColor = new Color(0.3f, 0.3f, 0.3f);

    static readonly Color k_hoveredColor = new Color(0.698f, 0.698f, 0.698f);
    static readonly Color k_hoveredProColor = new Color(0.2706f, 0.2706f, 0.2706f);

    public static Color Get(bool isSelected, bool isHovered, bool isWindowFocused)
    {
        if (isSelected)
        {
            return isWindowFocused
                ? EditorGUIUtility.isProSkin ? k_selectedProColor : k_selectedColor
                : EditorGUIUtility.isProSkin ? k_selectedUnFocusedProColor : k_selectedUnFocusedColor;
        }
        else if (isHovered)
        {
            return EditorGUIUtility.isProSkin ? k_hoveredProColor : k_hoveredColor;
        }
        else
        {
            return EditorGUIUtility.isProSkin ? k_defaultProColor : k_defaultColor;
        }
    }
}
