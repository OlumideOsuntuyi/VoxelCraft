using UnityEngine;

[DefaultExecutionOrder(5)]
public class ChunkComponent : MonoBehaviour
{
    private bool created;
    private bool contains => World.world.chunks.ContainsKey(coord);
    public bool drawMesh;
    public Vector2Int coord;
    public int layerToDraw = -1;
    public Chunk chunk => contains ? World.world.chunks[coord] : null;

    public int fullDraws;
    public int updating;
    private void Awake()
    {
        hideFlags = HideFlags.NotEditable;
    }
    private void Update()
    {
        if(created && contains)
        {
            if (drawMesh)
            {
                DrawMesh();
            }
        }
    }
    void DrawMesh()
    {
        if(layerToDraw < 0)
        {
            for (int i = 0; i < chunk.subChunks.Count; i++)
            {
                World.world.chunks[coord].subChunks[i].CreateMesh();
                drawMesh = false;
                layerToDraw = -1;
            }
            fullDraws++;
        }
        else if (World.world.chunks[coord].subChunks[layerToDraw]._isEditable)
        {
            World.world.chunks[coord].subChunks[layerToDraw].CreateMesh();
            drawMesh = false;
            layerToDraw = -1;
        }
    }
    internal void Set(Vector2Int coord)
    {
        this.coord = coord;
        created = true;
        drawMesh = false;
    }
    internal void DestoySelf()
    {
        if (World.world.chunks.ContainsKey(coord))
        {
            World.world.chunks.Remove(coord);
        }
        Destroy(gameObject);
    }
}
