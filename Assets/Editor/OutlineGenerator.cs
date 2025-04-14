using UnityEngine;
using UnityEditor;
using System.IO;

public class OutlineGenerator : EditorWindow
{
    // Source texture to generate outline from.
    Texture2D sourceTexture = null;
    // Color to use for the outline.
    Color outlineColor = Color.white;
    // How many pixels out to look for an edge (1 means one pixel thick outline).
    int thickness = 1;
    // Alpha threshold to determine if a pixel is “opaque”.
    float alphaThreshold = 0.1f;

    [MenuItem("Tools/Outline Generator")]
    static void Init()
    {
        OutlineGenerator window = (OutlineGenerator)GetWindow(typeof(OutlineGenerator));
        window.titleContent = new GUIContent("Outline Generator");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Outline Generator", EditorStyles.boldLabel);

        // Select source texture.
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);
        // Choose the color of your outline.
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);
        // Choose how thick the outline should be.
        thickness = EditorGUILayout.IntSlider("Thickness", thickness, 1, 10);
        // Set a threshold for detecting opaque pixels.
        alphaThreshold = EditorGUILayout.Slider("Alpha Threshold", alphaThreshold, 0f, 1f);

        if (sourceTexture == null)
        {
            EditorGUILayout.HelpBox("Please assign a source texture.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Generate Outline Texture"))
        {
            Texture2D outlineTex = GenerateOutline(sourceTexture, outlineColor, thickness, alphaThreshold);
            // Prompt to save the new texture asset.
            string path = EditorUtility.SaveFilePanelInProject("Save Outline Texture", 
                                sourceTexture.name + "_Outline.png", "png", 
                                "Please enter a file name to save the outline texture to");
            if (!string.IsNullOrEmpty(path))
            {
                byte[] bytes = outlineTex.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Outline Generated", "The outline texture has been saved to:\n" + path, "OK");
            }
        }
    }

    /// <summary>
    /// Generates a new Texture2D that contains only the outline of the non-transparent area in the source.
    /// For each pixel that is transparent in the source, we check if any pixel within the specified thickness
    /// is opaque. If yes, that pixel is part of the border.
    /// </summary>
    Texture2D GenerateOutline(Texture2D source, Color outlineColor, int thickness, float threshold)
    {
        int width = source.width;
        int height = source.height;
        Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);

        // Grab all source pixels at once for efficiency.
        Color[] srcPixels = source.GetPixels();
        Color[] resultPixels = new Color[srcPixels.Length];

        // Loop over every pixel in the texture.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                float currentAlpha = srcPixels[index].a;

                if (currentAlpha > threshold)
                {
                    // This pixel is inside the shape; we leave it transparent in the outline texture.
                    resultPixels[index] = Color.clear;
                }
                else
                {
                    bool isEdge = false;
                    // Check surrounding pixels within the thickness range.
                    for (int dy = -thickness; dy <= thickness && !isEdge; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= height)
                            continue;
                        for (int dx = -thickness; dx <= thickness && !isEdge; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= width)
                                continue;
                            int nIndex = nx + ny * width;
                            if (srcPixels[nIndex].a > threshold)
                            {
                                isEdge = true;
                            }
                        }
                    }
                    // Mark as outline color if adjacent to an opaque pixel.
                    resultPixels[index] = isEdge ? outlineColor : Color.clear;
                }
            }
        }
        result.SetPixels(resultPixels);
        result.Apply();

        return result;
    }
}
