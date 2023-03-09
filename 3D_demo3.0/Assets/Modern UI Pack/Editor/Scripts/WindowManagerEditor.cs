using UnityEngine;
using UnityEditor;

namespace Michsky.UI.ModernUIPack
{
    [CustomEditor(typeof(WindowManager))]
    public class WindowManagerEditor : Editor
    {
        private GUISkin customSkin;
        private WindowManager wmTarget;
        private UIManagerWindowManager tempUIM;
        private int currentTab;

        private void OnEnable()
        {
            wmTarget = (WindowManager)target;

            try { tempUIM = wmTarget.GetComponent<UIManagerWindowManager>(); }
            catch { }

            if (EditorGUIUtility.isProSkin == true) { customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Dark"); }
            else { customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Light"); }
        }

        public override void OnInspectorGUI()
        {
            MUIPEditorHandler.DrawComponentHeader(customSkin, "WM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[2];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Settings");

            currentTab = MUIPEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 1;

            GUILayout.EndHorizontal();

            var windows = serializedObject.FindProperty("windows");
            var currentWindowIndex = serializedObject.FindProperty("currentWindowIndex");
            var editMode = serializedObject.FindProperty("editMode");
            var useCulling = serializedObject.FindProperty("useCulling");
            var onWindowChange = serializedObject.FindProperty("onWindowChange");

            switch (currentTab)
            {
                case 0:
                    MUIPEditorHandler.DrawHeader(customSkin, "Content Header", 6);

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.PropertyField(windows, new GUIContent("Window Items"), true);
                    windows.isExpanded = true;

                    if (GUILayout.Button("+  Add a new window", customSkin.button))
                        wmTarget.AddNewItem();

                    GUILayout.EndVertical();

                    MUIPEditorHandler.DrawHeader(customSkin, "Events Header", 10);
                    EditorGUILayout.PropertyField(onWindowChange, new GUIContent("On Window Change"), true);
                    break;

                case 1:
                    MUIPEditorHandler.DrawHeader(customSkin, "Options Header", 6);
                    useCulling.boolValue = MUIPEditorHandler.DrawToggle(useCulling.boolValue, customSkin, "Use Window Culling");
                    editMode.boolValue = MUIPEditorHandler.DrawToggle(editMode.boolValue, customSkin, "Edit Mode");

                    if (wmTarget.windows.Count != 0)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Selected Window:"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        currentWindowIndex.intValue = EditorGUILayout.IntSlider(currentWindowIndex.intValue, 0, wmTarget.windows.Count - 1);

                        GUILayout.Space(2);
                        EditorGUILayout.LabelField(new GUIContent(wmTarget.windows[currentWindowIndex.intValue].windowName), customSkin.FindStyle("Text"));

                        if (editMode.boolValue == true)
                        {
                            EditorGUILayout.HelpBox("While in edit mode, you can change the visibility of windows by changing the selected window.", MessageType.Info);

                            for (int i = 0; i < wmTarget.windows.Count; i++)
                            {
                                if (i == currentWindowIndex.intValue)
                                    wmTarget.windows[currentWindowIndex.intValue].windowObject.GetComponent<CanvasGroup>().alpha = 1;
                                else
                                    wmTarget.windows[i].windowObject.GetComponent<CanvasGroup>().alpha = 0;
                            }
                        }

                        GUILayout.EndVertical();
                    }

                    else
                        EditorGUILayout.HelpBox("Window List is empty. Create a new item to see more options.", MessageType.Info);

                    MUIPEditorHandler.DrawHeader(customSkin, "UIM Header", 10);

                    if (tempUIM != null)
                    {
                        EditorGUILayout.HelpBox("This object is connected with UI Manager. Some parameters (such as colors, " +
                            "fonts or booleans) are managed by the manager.", MessageType.Info);

                        if (GUILayout.Button("Open UI Manager", customSkin.button))
                            EditorApplication.ExecuteMenuItem("Tools/Modern UI Pack/Show UI Manager");

                        if (GUILayout.Button("Disable UI Manager Connection", customSkin.button))
                        {
                            if (EditorUtility.DisplayDialog("Modern UI Pack", "Are you sure you want to disable UI Manager connection with the object? " +
                                "This operation cannot be undone.", "Yes", "Cancel"))
                            {
                                try { DestroyImmediate(tempUIM); }
                                catch { Debug.LogError("<b>[Window Manager]</b> Failed to delete UI Manager connection.", this); }
                            }
                        }
                    }

                    else if (tempUIM == null)
                        EditorGUILayout.HelpBox("This object does not have any connection with UI Manager.", MessageType.Info);

                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}