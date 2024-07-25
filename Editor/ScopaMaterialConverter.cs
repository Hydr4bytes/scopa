using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScopaMaterialConverter : MonoBehaviour
{
    [MenuItem("Assets/Scopa/Export Materials...")]
    private static void ExportTextures()
    {
        var materialList = UnityExtensions.RecursiveMaterialSearch();
        var previews = GeneratePreviews(materialList);

        string dirPath = EditorUtility.SaveFolderPanel("Save Textures", Application.dataPath, "Texures");

        if (Directory.Exists(dirPath))
        {
            foreach (var preview in previews)
            {
                byte[] bytes = preview.EncodeToPNG();
                string filename = preview.name + ".png";

                File.WriteAllBytes(Path.Combine(dirPath, filename), bytes);

                Debug.Log($"Exported Texture {preview.name}.png");
            }
        }
    }

    private static List<Texture2D> GeneratePreviews(List<Material> materials)
    {
        int layerToUse = 8;

        RenderTexture rendTextr = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        rendTextr.Create();

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.layer = layerToUse;

        GameObject camObject = new GameObject();
        Camera cam = camObject.AddComponent<Camera>();
        cam.cameraType = CameraType.Preview;
        cam.orthographic = true;
        cam.orthographicSize = 0.5f;
        cam.transform.position = new Vector3(0f, 0f, -1f);
        cam.targetTexture = rendTextr;
        cam.cullingMask = 1 << layerToUse;

        var previewScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(previewScene);

        SceneManager.MoveGameObjectToScene(camObject, previewScene);
        SceneManager.MoveGameObjectToScene(cube, previewScene);

        RenderTexture.active = rendTextr;

        List<Texture2D> previews = new List<Texture2D>();
        foreach (var material in materials)
        {
            cube.GetComponent<Renderer>().material = material;

            cam.Render();
            Texture2D outputTexture = new Texture2D(512, 512, TextureFormat.RGB24, false);
            outputTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            outputTexture.Apply();
            outputTexture.name = material.name;

            previews.Add(outputTexture);
        }

        Object.DestroyImmediate(camObject);
        Object.DestroyImmediate(cube);
        DestroyImmediate(rendTextr);

        EditorSceneManager.CloseScene(previewScene, true);

        return previews;
    }
}