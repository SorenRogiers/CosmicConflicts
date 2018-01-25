using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum MarchingSquaresPatterns
{
    Empty =0,
    
    //Corners
    TopLeftCorner = 4,
    TopRightCorner = 1,
    BottomLeftCorner = 8,
    BottomRightCorner = 2,

    //Sides
    TopSide = 10,
    RightSide = 3,
    BottomSide =5,
    LeftSide = 12,

    //Diagonals
    DiagonalLeftToRight = 9,
    DiagonalRightToLeft = 6,

    //Empty corners
    TopLeftEmpty = 11,
    TopRightEmpty = 14,
    BottomLeftempty =7,
    BottomRightEmpty = 13,

    Full = 15,
}

/* SmoothMeshCreator
 * *****************
 * Transforms the data from the quadtree into a mesh.
 * Using marching squares to create a smooth world instead of a blocky one.
 * Regenerate the world when we did damage to it
 */ 
 //TODO Serious optimizations left.

public class SmoothMeshCreator : MonoBehaviour {

    public bool Generate;
    public QuadtreeComponent QuadtreeComponent;
    public Material VoxelMaterial;

    private GameObject _currentMesh;
    private readonly Color[] _voxelColorCodes = new Color[]
    {
        Color.clear,
        Color.red,
        Color.green,
        Color.blue
    };
    private bool _sendMessage = false;

    void Update()
    {
        if (QuadtreeComponent.Quadtree != null)
        {
            if(!_sendMessage)
            {
                QuadtreeComponent.Quadtree.QuadtreeUpdated += (obj, args) => { Generate = true; }; //Regenerate mesh when quadtree is updated.
                _sendMessage = true;
            }
        }

        //Destroy the current mesh and replace it with a new one.
        if (Generate)
        {
            GameObject newGeneratedMesh = GenerateMesh();

            if (_currentMesh != null)
                Destroy(_currentMesh);

            _currentMesh = newGeneratedMesh;

            Generate = false;
        }
    }
    private GameObject GenerateMesh()
    {
        GameObject chunk = new GameObject();
        chunk.name = "Voxel Chunk";
        chunk.transform.parent = this.transform;
        chunk.transform.localPosition = Vector3.zero;
        chunk.transform.tag = "Ground";
        chunk.layer = 8;

        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var colors = new List<Color>();

        var leafs = QuadtreeComponent.Quadtree.GetLeafNodes().OrderBy((node => node.Position.x)).ThenBy(node=> node.Position.y).ToArray();

        int size = (int)Mathf.Pow(2, QuadtreeComponent.Depth) - 1;
        for(int x=0; x < size;++x)
        {
            for(int y=0; y < size;++y)
            {
                int top = x + y * (size + 1);
                int bottom = x + ((y+1) * (size + 1));

                var topLeft = leafs[top];
                var topRight = leafs[top+1];
                var bottomLeft = leafs[bottom];
                var bottomRight = leafs[bottom+1];
                InsertMesh(vertices, triangles, uvs, normals, colors, new Quadtree<int>.QuadtreeNode<int>[] { topLeft, topRight, bottomLeft, bottomRight });
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();


        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();
        Rigidbody rigidbody = chunk.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.solverIterations = 6;
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;


        meshRenderer.material = VoxelMaterial;
        meshCollider.sharedMesh = mesh;
        meshFilter.mesh = mesh;

        return chunk;
    }
    private void InsertMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        int type = 0;

        for (int i = 0; i < points.Length; ++i)
        {
            if (points[i].Data != 0)
            {
                type |= 1 << 3 - i;
            }
        }

        switch ((MarchingSquaresPatterns)type)
        {
            case MarchingSquaresPatterns.Full:
                BuildSquareFull(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.TopLeftEmpty:
                BuildSquareTopLeftEmpty(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.TopRightEmpty:
                BuildSquareTopRightEmpty(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.BottomLeftempty:
                BuildSquareBottomLeftEmpty(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.BottomRightEmpty:
                BuildSquareBottomRightEmpty(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.LeftSide:
                BuildSquareLeftSide(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.RightSide:
                BuildSquareRightSide(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.TopSide:
                BuildSquareTopSide(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.BottomSide:
                BuildSquareBottomSide(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.DiagonalLeftToRight:
                BuildSquareDiagonalLeftToRight(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.DiagonalRightToLeft:
                BuildSquareDiagonalRightToLeft(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.BottomLeftCorner:
                BuildSquareBottomLeftCorner(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.TopRightCorner:
                BuildSquareTopRightCorner(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.TopLeftCorner:
                BuildSquareTopLeftCorner(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.BottomRightCorner:
                BuildSquareBottomRightCorner(vertices, triangles, uvs, normals, colors, points);
                break;
            case MarchingSquaresPatterns.Empty:
                break;
        }

    }

    private void BuildSquareFull(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareTopSide(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareLeftSide(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size *0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size *0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size *0.5f) + Vector3.right * (points[0].Size *0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareRightSide(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(points[3].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareBottomSide(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);


        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 1);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareBottomLeftCorner(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
    }
    private void BuildSquareTopLeftCorner(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
    }
    private void BuildSquareBottomRightCorner(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));
        vertices.Add(points[2].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
    }
    private void BuildSquareTopRightCorner(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 UpperLeft = new Vector3(points[0].Position.x, points[0].Position.y - points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[3].Position, 0.5f));

        uvs.Add(UpperLeft);
        uvs.Add(UpperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(UpperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(Color.red);
        colors.Add(Color.red);
        colors.Add(Color.red);
    }
    private void BuildSquareDiagonalLeftToRight(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[2].Position, points[0].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size *0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.left * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.left * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 5);

        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 5);

        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 5);

        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 4);

        colors.Add(Color.red);
        colors.Add(Color.red);
        colors.Add(Color.red);
        colors.Add(Color.red);
        colors.Add(Color.red);
        colors.Add(Color.red);
    }
    private void BuildSquareDiagonalRightToLeft(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(points[2].Position);
        vertices.Add(Vector2.Lerp(points[2].Position, points[0].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * (points[0].Size*0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size - Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) - Vector3.right * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 5);
        triangles.Add(initialIndex + 0);

        triangles.Add(initialIndex + 5);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 4);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
    }
    private void BuildSquareBottomLeftEmpty(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(points[1].Position);
        vertices.Add(points[3].Position);
        vertices.Add(points[2].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[2].Position, 0.5f));


        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * (points[0].Size * 0.5f));
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.left * (points[0].Size * 0.5f) );
        uvs.Add(upperLeft - Vector3.right * (points[0].Size * 0.5f) + Vector3.down * (points[0].Size * 0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 4);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareTopRightEmpty(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(points[2].Position);


        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * (points[0].Size * 0.5f) + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size * 0.5f);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 4);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareBottomRightEmpty(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(points[1].Position);
        vertices.Add(points[3].Position);
        vertices.Add(Vector2.Lerp(points[3].Position, points[2].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[2].Position, points[0].Position, 0.5f));

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * (points[0].Size *0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size *0.5f));

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 4);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 3);

        triangles.Add(initialIndex + 3);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
    private void BuildSquareTopLeftEmpty(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals, List<Color> colors, Quadtree<int>.QuadtreeNode<int>[] points)
    {
        Vector3 upperLeft = new Vector3(points[0].Position.x - points[0].Size * 0.5f, points[0].Position.y + points[0].Size * 0.5f, 0);
        int initialIndex = vertices.Count;

        vertices.Add(points[0].Position);
        vertices.Add(Vector2.Lerp(points[0].Position, points[1].Position, 0.5f));
        vertices.Add(Vector2.Lerp(points[1].Position, points[3].Position, 0.5f));
        vertices.Add(points[3].Position);
        vertices.Add(points[2].Position);

        uvs.Add(upperLeft);
        uvs.Add(upperLeft + Vector3.right * (points[0].Size*0.5f));
        uvs.Add(upperLeft + Vector3.down * (points[0].Size *0.5f) + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size + Vector3.right * points[0].Size);
        uvs.Add(upperLeft + Vector3.down * points[0].Size);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        triangles.Add(initialIndex + 0);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 4);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 1);
        triangles.Add(initialIndex + 2);

        triangles.Add(initialIndex + 4);
        triangles.Add(initialIndex + 2);
        triangles.Add(initialIndex + 3);

        colors.Add(_voxelColorCodes[points[0].Data]);
        colors.Add(_voxelColorCodes[points[1].Data]);
        colors.Add(_voxelColorCodes[points[2].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
        colors.Add(_voxelColorCodes[points[3].Data]);
    }
}
