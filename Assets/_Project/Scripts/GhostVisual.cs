using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tints every renderer on a ghost preview object and makes it semi-transparent,
/// so the preview is visually distinct from committed pieces. The original
/// materials are never modified; instances are created and reused instead.
/// </summary>
public class GhostVisual : MonoBehaviour
{
    [SerializeField] private Color validColor = new Color(0.2f, 1f, 0.3f, 0.45f);
    [SerializeField] private Color neutralColor = new Color(1f, 1f, 1f, 0.35f);

    private readonly List<Material> instancedMaterials = new List<Material>();

    private void Awake()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                Material instance = new Material(renderer.sharedMaterials[i]);
                MakeTransparent(instance);
                materials[i] = instance;
                instancedMaterials.Add(instance);
            }

            renderer.materials = materials;
        }
    }

    /// <summary>Switches the ghost between its snapping and free-placement colours.</summary>
    public void SetSnapping(bool isSnapping)
    {
        Color target = isSnapping ? validColor : neutralColor;

        foreach (Material material in instancedMaterials)
        {
            material.color = target;
        }
    }

    /// <summary>Reconfigures a Standard shader material for transparent rendering.</summary>
    private static void MakeTransparent(Material material)
    {
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLYBLEND_ON");
        material.renderQueue = 3000;
    }

    private void OnDestroy()
    {
        foreach (Material material in instancedMaterials)
        {
            Destroy(material);
        }
    }
}