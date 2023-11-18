using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardCodedMeshGenerator : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private List<Vector3> _vertices = new()
    {
        new(-4, 0, 2),
        new(-1, 0, 4),
        new(0, 0, -1),
        new(2, 0, 2),
        new(4, 0, -1),
        new(2, 0, -3),
        new(-3, 0, 0),
        new(-2, 0, 1),
    };

    private List<int> _triangles = new()
    {
        0, 1, 7, 
        1, 2, 7, 
        2, 6, 7, 
        3, 4, 2, 
        4, 5, 2, 
        5, 6, 2, 
    };

    private void Reset()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        Debug.Log("Drawing hard coded");
        _mesh = new();

        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        _mesh.Clear();
        GenerateHardCodedMesh();
        _mesh.RecalculateNormals();
        _meshFilter.sharedMesh = _mesh;
    }

    private void GenerateHardCodedMesh()
    {
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
    }
}
