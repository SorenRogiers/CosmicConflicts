using UnityEngine;

public class QuadtreeComponent : Singleton<QuadtreeComponent>
{

    public float Size = 5;
    public int Depth = 2;

    private Quadtree<int> _quadtree;

    void Awake()
    {
        _quadtree = new Quadtree<int>(this.transform.position, Size, Depth);
    }

    public Quadtree<int> Quadtree
    {
        get { return _quadtree; }
    }
}
