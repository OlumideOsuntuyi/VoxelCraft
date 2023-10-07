
using UnityEngine;

[ExecuteAlways]
public class ThreadsManager : MonoBehaviour
{
    public static ThreadsManager instance;
    public ChunkLoadingManager chunkLoadingManager;
    private void Awake()
    {
        instance = this;
    }
    public bool isThreadActive;
    void Update()
    {
        if(chunkLoadingManager)
        {
            if(chunkLoadingManager.chunkUpdateThread != null)
            {
                bool isactive = chunkLoadingManager.chunkUpdateThread.IsAlive;
                if (!Application.isPlaying && isThreadActive)
                {
                    chunkLoadingManager.chunkUpdateThread.Abort();
                }
                isactive = chunkLoadingManager.chunkUpdateThread.IsAlive;
                isThreadActive = isThreadActive || isactive;
            }
        }
    }
}