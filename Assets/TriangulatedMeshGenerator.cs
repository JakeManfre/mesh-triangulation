using System.Collections.Generic;
using UnityEngine;

public class TriangulatedMeshGenerator : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private Bounds _bounds;
    private List<Vector3> _vertices;
    private List<int> _triangleIndices = new();
    private LinkedList<int> _unusedVertices = new();
    private HashSet<int> _reflexVertices = new();
    private LinkedListNode<int> _currentUnusedVertexNode;

    private void Reset()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        _mesh = new();

        _vertices = new()
        {
            new(-4, 0, 2), // 0
            new(-1, 0, 4), // 1
            new(0, 0, -1), // 2
            new(2, 0, 2),  // 3

            new(2, 0, 1),  // 5
            
            new(4, 0, -1), // 4
            
             //new(3, 0, -1),

            new(2, 0, -3), // 6 - colinear point with vertices 2 and 8
            new(-3, 0, 0), // 7
            new(-2, 0, 1), // 8
        };

        GenerateMesh();
        
        _mesh.SetVertices(_vertices);
        _mesh.SetTriangles(_triangleIndices, 0);

        GenerateUvs();

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _meshFilter.sharedMesh = _mesh;
    }

    private void GenerateMesh()
    {
        Initialize();
        Triangulate();
        Debug.Log("done with triangulation");
    }

    private void Initialize()
    {
        _bounds = new();
        for (int i = 0; i < _vertices.Count; i += 1)
        {
            _unusedVertices.AddLast(i);
            _bounds.Encapsulate(_vertices[i]);
            var nextIndex = (i + 1) % _vertices.Count;
            var previousIndex = i - 1 < 0 ? _vertices.Count - 1 : i - 1;
            if (IsVertexReflex(i, nextIndex, previousIndex))
            {
                _reflexVertices.Add(i);
            }
        }
    }

    private void Triangulate()
    {
        _currentUnusedVertexNode = _unusedVertices.Last;

        while (_unusedVertices.Count > 2)
        {
            _currentUnusedVertexNode = GetNextUnusedIndexNode(_currentUnusedVertexNode);
            var nextUnusedVertexNode = GetNextUnusedIndexNode(_currentUnusedVertexNode);
            var previousUnusedVertexNode = GetPreviousUnusedIndexNode(_currentUnusedVertexNode);

            if (_reflexVertices.Contains(_currentUnusedVertexNode.Value))
                continue;

            var isAnyReflexVertexInsideTriangle = IsAnyReflexVertexInsideTriangle(
                _currentUnusedVertexNode.Value,
                nextUnusedVertexNode.Value,
                previousUnusedVertexNode.Value);

            if (isAnyReflexVertexInsideTriangle)
                continue;

            _triangleIndices.Add(_currentUnusedVertexNode.Value);
            _triangleIndices.Add(nextUnusedVertexNode.Value);
            _triangleIndices.Add(previousUnusedVertexNode.Value);

            _unusedVertices.Remove(_currentUnusedVertexNode);

            UpdateReflexVertex(nextUnusedVertexNode);
            UpdateReflexVertex(previousUnusedVertexNode);
        }
    }

    private void GenerateUvs()
    {
        var extents = _bounds.extents;
        var width = _bounds.size.x;
        var height = _bounds.size.z;
        var uvs = new List<Vector2>();

        for (int i = 0; i < _vertices.Count; i += 1)
        {
            var vertex = _vertices[i];
            var x = (vertex.x - extents.x) / width;
            var y = (vertex.z - extents.y) / height;
            uvs.Add(new(x, y));
        }

        _mesh.SetUVs(0, uvs);
    }

    private bool IsAnyReflexVertexInsideTriangle(int currentVertexIndex, int nextVertexIndex, int previousVertexIndex)
    {
        foreach (var index in _reflexVertices)
        {
            if (index == nextVertexIndex || index == previousVertexIndex)
                continue;

            var triangleSideA = _vertices[nextVertexIndex] - _vertices[currentVertexIndex];
            var triangleSideB = _vertices[previousVertexIndex] - _vertices[nextVertexIndex];
            var triangleSideC = _vertices[currentVertexIndex] - _vertices[previousVertexIndex];

            var reflexPointVectorA = _vertices[index] - _vertices[currentVertexIndex];
            var reflexPointVectorB = _vertices[index] - _vertices[nextVertexIndex];
            var reflexPointVectorC = _vertices[index] - _vertices[previousVertexIndex];

            var triangleSideADeterminant = Determinant(triangleSideA, reflexPointVectorA);
            var triangleSideBDeterminant = Determinant(triangleSideB, reflexPointVectorB);
            var triangleSideCDeterminant = Determinant(triangleSideC, reflexPointVectorC);

            var isADeterminantNegative = triangleSideADeterminant < 0;
            var isBDeterminantNegative = triangleSideBDeterminant < 0;
            var isCDeterminantNegative = triangleSideCDeterminant < 0;

            if (isADeterminantNegative == isBDeterminantNegative && isADeterminantNegative == isCDeterminantNegative)
                return true;
        }

        return false;
    }

    private void UpdateReflexVertex(LinkedListNode<int> node)
    {
        if (!_reflexVertices.Contains(node.Value))
            return;

        var nextIndex = GetNextUnusedIndexNode(node).Value;
        var previousIndex = GetPreviousUnusedIndexNode(node).Value;

        if (IsVertexReflex(node.Value, nextIndex, previousIndex))
            return;

        _reflexVertices.Remove(node.Value);
    }

    private bool IsVertexReflex(int vertexIndex, int nextVertexIndex, int previousVertexIndex)
    {
        var previousVertex = _vertices[previousVertexIndex];
        var currentVertex = _vertices[vertexIndex];
        var nextVertex = _vertices[nextVertexIndex];

        var nextVector = nextVertex - currentVertex;
        var previousVector = previousVertex - currentVertex;

        var determinant = Determinant(nextVector, previousVector);
        return determinant >= 0;
    }

    private LinkedListNode<int> GetNextUnusedIndexNode(LinkedListNode<int> currentIndex)
    {
        if (currentIndex.Next != null)
            return currentIndex.Next;

        return _unusedVertices.First;
    }

    public LinkedListNode<int> GetPreviousUnusedIndexNode(LinkedListNode<int> currentIndex)
    {
        if (currentIndex.Previous != null)
            return currentIndex.Previous;

        return _unusedVertices.Last;
    }

    private float Determinant(Vector3 vectorA, Vector3 vectorB)
    {
        return vectorA.x * vectorB.z - vectorA.z * vectorB.x;
    }
}
