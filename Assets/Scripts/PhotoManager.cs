using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhotoManager : MonoBehaviour
{
    public static PhotoManager Instance { get; private set; }
    
    public List<PhotoData> capturedPhotos = new List<PhotoData>();
    
    [System.Serializable]
    public class PhotoData
    {
        public Texture2D photoTexture;
        public PhotoMetadata metadata;
        public string photoPath;
        public Material secretMaterial; // Store secret material
        public string objectiveName; // Store objective name
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddPhoto(Texture2D photo, PhotoMetadata metadata, string photoPath)
    {
        PhotoObjective lastCompletedObjective = PhotoObjectivesManager.Instance.GetLastCompletedObjective();
        if (lastCompletedObjective == null)
        {
            Debug.LogError("No completed objective found");
            return;
        }
        Debug.Log("Photo captured for objective: " + lastCompletedObjective.objectName + ". Assigning secret material.");
        capturedPhotos.Add(new PhotoData
        {
            photoTexture = photo,
            metadata = metadata,
            photoPath = photoPath,
            objectiveName = lastCompletedObjective.objectName,
            secretMaterial = lastCompletedObjective.secretMaterial
        });
        ApplySecretMaterial(photo, lastCompletedObjective.secretMaterial);
    }

    private void ApplySecretMaterial(Texture2D photo, Material secretMaterial)
    {
        if (photo == null || secretMaterial == null)
        {
            Debug.LogWarning("Cannot apply secret material: Photo or material is null");
            return;
        }
        GameObject gameObject = new GameObject("CapturedPhoto");
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = CreateQuadMesh();
        meshFilter.mesh = mesh;
        renderer.material.mainTexture = photo;
        GameObject gameObject2 = new GameObject("Secret");
        gameObject2.transform.SetParent(gameObject.transform);
        MeshFilter meshFilter2 = gameObject2.AddComponent<MeshFilter>();
        Renderer renderer2 = gameObject2.AddComponent<MeshRenderer>();
        meshFilter2.mesh = mesh;
        renderer2.material = secretMaterial;
        gameObject2.transform.localPosition = Vector3.forward * 0.01f;
        Debug.Log("Secret material applied to photo with material: " + secretMaterial.name);
    }
private Mesh CreateQuadMesh()
{
    Mesh mesh = new Mesh();
    
    Vector3[] vertices = new Vector3[4]
    {
        new Vector3(-0.5f, -0.5f, 0),
        new Vector3(0.5f, -0.5f, 0),
        new Vector3(-0.5f, 0.5f, 0),
        new Vector3(0.5f, 0.5f, 0)
    };
    
    Vector2[] uvs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };
    
    int[] triangles = new int[6]
    {
        0, 2, 1,
        2, 3, 1
    };
    
    mesh.vertices = vertices;
    mesh.uv = uvs;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    
    return mesh;
}

public void TransferPhotosToPhotoBoard(PhotoBoard targetPhotoBoard)
{
    foreach (PhotoData photoData in capturedPhotos)
    {
        targetPhotoBoard.AddPhoto(photoData.photoTexture, photoData.metadata, photoData.photoPath,
            photoData.objectiveName, photoData.secretMaterial);
    }

    capturedPhotos.Clear();
}
}
