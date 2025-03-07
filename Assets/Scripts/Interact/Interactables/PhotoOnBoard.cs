using System.Linq;
using UnityEngine;

public class PhotoOnBoard : MonoBehaviour
{
    public PhotoMetadata Metadata { get; private set; }
    public string PhotoPath { get; private set; }
    public string ObjectiveName { get; private set; }
    
    [SerializeField] private Material defaultSecretMaterial;
    
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (GetComponent<MeshFilter>() == null)
            {
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreatePhotoMesh();
            }
        }
    }
    
    public void Initialize(Material material, PhotoMetadata metadata, string path, string objectiveName = null, Material secretMaterial = null)
    {
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer is missing! Adding one...");
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = material;
        Metadata = metadata;
        PhotoPath = path;
        ObjectiveName = objectiveName;
        BoxCollider boxCollider;
        if (!TryGetComponent(out boxCollider))
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        boxCollider.size = new Vector3(1.1f, 1.1f, 0.1f);
        ApplySecretMaterial(secretMaterial);
    }
    private void ApplySecretMaterial(Material passedSecretMaterial = null)
    {
        Transform transform = base.transform.Find("Secret");
        if (transform != null)
        {
            Material material = passedSecretMaterial;
            if (material == null && !string.IsNullOrEmpty(ObjectiveName))
            {
                PhotoObjective photoObjective = PhotoObjectivesManager.Instance.GetLevels().SelectMany((LevelObjectives level) => level.objectives).FirstOrDefault((PhotoObjective obj) => obj.objectName == this.ObjectiveName);
                material = ((photoObjective != null) ? photoObjective.secretMaterial : null);
            }
            MeshRenderer component = transform.GetComponent<MeshRenderer>();
            if (component != null)
            {
                component.material = (material ?? defaultSecretMaterial);
            }
        }
    }
    
    private Mesh CreatePhotoMesh()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}