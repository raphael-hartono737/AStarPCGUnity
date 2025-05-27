using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour
{

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData)
    {
        Mesh mesh = meshData.CreateMesh();
        meshFilter.sharedMesh = mesh;

        // Add or update the MeshCollider
        MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh; // Update collider mesh
    }

}

