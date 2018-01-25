using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuadtreeIndex
{
    //If something is bottom the first index is 1
    //If something is right the second index is 1
    TopLeft     = 0, //00
    TopRight    = 1, //01
    BottomLeft  = 2, //10
    BottomRight = 3, //11
}

/* Quadtree
 * ********
 * Quadtree holds the data to generate the world
 * 
 */ 
public class Quadtree<TType> where TType : IComparable
{
    private QuadtreeNode<TType>[] _nodes;
    private int _depth;

    public event EventHandler QuadtreeUpdated;

    public Quadtree(Vector2 position, float size, int depth)
    {
        _depth = depth;
        _nodes = BuildQuadtree(position, size, depth);
    }

    private QuadtreeNode<TType>[] BuildQuadtree(Vector2 position,float size,int depth)
    {
        int length = 0;

        for (int i =0;i <= depth;++i)
        {
            length += (int)Mathf.Pow(4, i);
        }

        var quadtree = new QuadtreeNode<TType>[length];

        quadtree[0] = new QuadtreeNode<TType>(position, size, 0);
        BuildQuadtreeRecursively(quadtree, 0);

        return quadtree;
    }
    private void BuildQuadtreeRecursively(QuadtreeNode<TType>[] quadtree,int index)
    {
        if(quadtree[index].Depth >= this._depth)
            return; //We're done if we hit a leaf node

        //if we have a tree - the root is equal to 0.
        //4 x 0 = 0 but if you add one you get the next node. +2 +3 +4 gives the other nodes.
        //Then if you're at position 1 and you multiply by 4 then you get 4, add 1 gives you 5 which is the first node in the seconde depth.
        int nextNode = 4 * index;

        Vector2 deltaX = new Vector2(quadtree[index].Size / 4, 0);
        Vector2 deltaY = new Vector2(0, quadtree[index].Size / 4);

        quadtree[nextNode + 1] = new QuadtreeNode<TType>(quadtree[index].Position - deltaX + deltaY, quadtree[index].Size / 2, quadtree[index].Depth + 1);
        quadtree[nextNode + 2] = new QuadtreeNode<TType>(quadtree[index].Position + deltaX + deltaY, quadtree[index].Size / 2, quadtree[index].Depth + 1);
        quadtree[nextNode + 3] = new QuadtreeNode<TType>(quadtree[index].Position - deltaX - deltaY, quadtree[index].Size / 2, quadtree[index].Depth + 1);
        quadtree[nextNode + 4] = new QuadtreeNode<TType>(quadtree[index].Position + deltaX - deltaY, quadtree[index].Size / 2, quadtree[index].Depth + 1);

        BuildQuadtreeRecursively(quadtree, nextNode + 1);
        BuildQuadtreeRecursively(quadtree, nextNode + 2);
        BuildQuadtreeRecursively(quadtree, nextNode + 3);
        BuildQuadtreeRecursively(quadtree, nextNode + 4);
    }

    public void InsertExplosion(Vector2 position,float radius, TType data)
    {
        var leafNodes = new LinkedList<QuadtreeNode<TType>>(); //linked list has constant time insert and deletes.
        CircleSearch(leafNodes, position,radius,_nodes,0);

        foreach (var quadtreeNode in leafNodes)
        {
            quadtreeNode.Data = data;
        }

        NotifyQuadtreeUpdate();
    }

    private void NotifyQuadtreeUpdate() //Send message out that if our quadtree is updated then we want to update our mesh too.
    {
        if (QuadtreeUpdated != null)
        {
            QuadtreeUpdated(this, new EventArgs());
        }
    }

    //Leaf nodes are going to be at the end of our array
    //just return the sub array from those elements to the end.
    public IEnumerable<QuadtreeNode<TType>> GetLeafNodes()
    {
        int leafNodes = (int)Mathf.Pow(4, _depth);
        for(int i = _nodes.Length - leafNodes; i < _nodes.Length; ++i)
        {
            yield return _nodes[i];
        }
    }

    private static int GetIndexAtPosition(Vector2 lookUpPosition, Vector2 nodePosition)
    {
        int index = 0;

        index |= lookUpPosition.x > nodePosition.x ? 1 : 0;
        index |= lookUpPosition.y < nodePosition.y ? 2 : 0;

        return index;
    }


    public void CircleSearch(LinkedList<QuadtreeNode<TType>> nodeList, Vector2 targetPosition,float radius,QuadtreeNode<TType>[] quadtree,int index)
    {
        if (quadtree[index].Depth >= this._depth)
        {
            nodeList.AddLast(quadtree[index]);
            return;
        }

        int nextNode = 4 * index;
        for (int i = 1; i <= 4; ++i)
        {
            if (IsInsideCircle(targetPosition, radius,quadtree[nextNode +i]))
            {
                CircleSearch(nodeList, targetPosition, radius, quadtree, nextNode + i);
            }
        }
    }

    public bool IsInsideCircle(Vector2 position, float radius,QuadtreeNode<TType> node)
    {
        Vector2 difference = node.Position - position;
        difference.x = Math.Max(0, Mathf.Abs(difference.x) - node.Size / 2);
        difference.y = Math.Max(0, Mathf.Abs(difference.y) - node.Size / 2);

        return difference.magnitude < radius;
    }

    private class QuadtreeNode<TType> where TType : IComparable
    {
        private Vector2 _position;
        private float _size;
        private TType _data;
        private int _depth;

        public QuadtreeNode(Vector2 position, float size,int depth,TType data = default(TType))
        {
            _position = position;
            _size = size;
            _data = data;
            _depth = depth;
        }

        public float Size
        {
            get { return _size; }
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public TType Data
        {
            get { return _data; }
            internal set { _data = value; } //Only things in the same namespace/library can see it now. Basically one up from protected.
        }

        public int Depth
        {
            get { return _depth; }
        }
    }
}