using UnityEngine;

/// <summary>
/// Draws an on-screen control reference and live counters using Unity's
/// immediate-mode GUI. Kept deliberately simple so the build is
/// self-explanatory without a Canvas hierarchy.
/// </summary>
public class HudDisplay : MonoBehaviour
{
    [SerializeField] private ConveyorPlacer placer;
    [SerializeField] private ProductSpawner spawner;
    [SerializeField] private string[] typeNames = { "Long belt", "Short belt" };
    [SerializeField] private ApplicationController appController;

    private GUIStyle panelStyle;
    private GUIStyle textStyle;
    private Texture2D panelTexture;

    private void Awake()
    {
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
        panelTexture.Apply();
    }

    private void OnDestroy()
    {
        Destroy(panelTexture);
    }

    private void OnGUI()
    {
        EnsureStyles();

        GUI.Box(new Rect(12f, 12f, 260f, 370f), GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(24f, 24f, 240f, 360f));

        GUILayout.Label("CONTROLS", textStyle);
        GUILayout.Label("Left click   Place conveyor", textStyle);
        GUILayout.Label("Right click  Delete conveyor", textStyle);
        GUILayout.Label("R            Rotate 90 degrees", textStyle);
        GUILayout.Label("1 / 2        Change type", textStyle);
        GUILayout.Label("Space        Start / stop products", textStyle);
        GUILayout.Label("WASD         Pan camera", textStyle);
        GUILayout.Label("Q / E        Orbit camera", textStyle);
        GUILayout.Label("T / G        Tilt camera", textStyle);
        GUILayout.Label("Scroll       Zoom", textStyle);
        GUILayout.Label("P            Pause / resume", textStyle);
        GUILayout.Label("Esc          Quit", textStyle);

        GUILayout.Space(8f);
        GUILayout.Label($"Selected: {CurrentTypeName()}", textStyle);

        if (placer != null)
        {
            GUILayout.Label($"Conveyors placed: {placer.PlacedSegments.Count}", textStyle);
        }

        if (spawner != null)
        {
            GUILayout.Label($"Products delivered: {spawner.DeliveredCount}", textStyle);
        }

        GUILayout.EndArea();

        if (appController != null && appController.IsPaused)
        {
            GUILayout.Space(4f);
            GUILayout.Label("PAUSED", textStyle);
        }
    }

    private string CurrentTypeName()
    {
        if (placer == null) return "-";

        int index = placer.SelectedIndex;
        return index >= 0 && index < typeNames.Length ? typeNames[index] : $"Type {index + 1}";
    }

    private void EnsureStyles()
    {
        if (textStyle != null) return;

        panelStyle = new GUIStyle(GUI.skin.box) { normal = { background = panelTexture } };
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = Color.white } };
    }
}