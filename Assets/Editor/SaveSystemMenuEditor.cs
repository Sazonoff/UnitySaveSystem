using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnitySaveSystem.Saves;

public class SaveSystemMenuEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/Save System/Save System Menu")]
    public static void ShowWindow()
    {
        SaveSystemMenuEditor wnd = GetWindow<SaveSystemMenuEditor>();
        wnd.titleContent = new GUIContent("SaveSystemMenuEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset.CloneTree(root);
        root.Q<Label>("Tittle").text = $"Save System v.{SaveSystemInfo.VersionCode}";
        root.Q<Button>("OpenGitHubButton").RegisterCallback<ClickEvent>(OpenGitHubPage);
        root.Q<Button>("GeneratePreloadClassButton").RegisterCallback<ClickEvent>(GeneratePreloadClass);
        root.Q<Button>("OpenSaveFolderButton").RegisterCallback<ClickEvent>(OpenSaveFolder);
        root.Q<Button>("DeleteSavesButton").RegisterCallback<ClickEvent>(DeleteSaves);
    }

    private void DeleteSaves(ClickEvent evt)
    {
        SaveSystemHelperEditor.DeleteSaves();
    }

    private void OpenSaveFolder(ClickEvent evt)
    {
        SaveSystemHelperEditor.OpenSaveFolder();
    }

    private void GeneratePreloadClass(ClickEvent evt)
    {
        SaveSystemHelperEditor.GeneratePreloadSave();
    }

    private void OpenGitHubPage(ClickEvent evt)
    {
        SaveSystemHelperEditor.OpenGitHubPage();
    }
}