using System.Collections;
using System.Collections.Generic;

using UnityEngine;


[DefaultExecutionOrder(10)]
public class TextureAtlasMaker : MonoBehaviour
{
    private Texture2D textureAtlas, transparentAtlas;
    [SerializeField] private Material solidBlocks, transparentBlocks;
    void Start()
    {
        List<AtlasSection> sections = new();
        List<AtlasSection> transparentSections = new();
        foreach (Block block in WorldData.instance.blocks)
        {

            if (!block.isWater && block.faceTextures.Count > 0)
            {
                for (int i = 0; i < block.faceTextures.Count; i++)
                {
                    if (block.renderNeighbourFaces)
                    {
                        transparentSections.Add(new AtlasSection(block.faceTextures[i].texture, block.faceTextures[i].uv));
                    }
                    else
                    {
                        sections.Add(new AtlasSection(block.faceTextures[i].texture, block.faceTextures[i].uv));
                    }
                }
            }
        }
        textureAtlas = GenerateTextureAtlas(1024, sections.ToArray());
        transparentAtlas = GenerateTextureAtlas(1024, transparentSections.ToArray());

        solidBlocks.mainTexture = textureAtlas;
        transparentBlocks.mainTexture = transparentAtlas;
    }
    public Texture2D InsertTexture(Texture2D textureToInsert, int rowIndex, int columnIndex)
    {
        Texture2D targetTexture = new Texture2D(1024, 1024);
        if (targetTexture == null || textureToInsert == null)
        {
            Debug.LogError("Please assign both target and texture to insert.");
            return null;
        }

        if (rowIndex < 0 || rowIndex >= 32 || columnIndex < 0 || columnIndex >= 32)
        {
            Debug.LogError("Row or column index is out of bounds.");
            return null;
        }

        int insertX = columnIndex * 32; // Calculate the x position to start insertion.
        int insertY = (31 - rowIndex) * 32; // Calculate the y position to start insertion (invert the row index).

        if (insertX + textureToInsert.width > targetTexture.width || insertY + textureToInsert.height > targetTexture.height)
        {
            Debug.LogError("Insertion coordinates are out of bounds.");
            return null;
        }

        // Get the pixels from the target texture.
        Color[] targetPixels = targetTexture.GetPixels();

        // Get the pixels from the texture to insert.
        Color[] insertPixels = textureToInsert.GetPixels();

        // Loop through the 32x32 texture to insert and copy it into the target texture.
        for (int y = 0; y < textureToInsert.height; y++)
        {
            for (int x = 0; x < textureToInsert.width; x++)
            {
                int targetIndex = (insertY + y) * targetTexture.width + (insertX + x);
                int insertIndex = y * textureToInsert.width + x;

                // Ensure the alpha channel of the inserted texture is not transparent.
                if (insertPixels[insertIndex].a > 0)
                {
                    targetPixels[targetIndex] = insertPixels[insertIndex];
                }
            }
        }

        // Set the modified pixels back to the target texture.
        targetTexture.SetPixels(insertX, insertY, textureToInsert.width, textureToInsert.height, targetPixels);
        targetTexture.Apply();
        return targetTexture;
    }
    public static Texture2D GenerateTextureAtlas(int atlasSize, AtlasSection[] atlasSections)
    {
        // Create the atlas texture
        Texture2D atlasTexture = new Texture2D(atlasSize, atlasSize);

        // Initialize with transparent color
        Color[] clearColor = new Color[atlasSize * atlasSize];
        for (int i = 0; i < clearColor.Length; i++)
        {
            clearColor[i] = i % 2 == 3 ? Color.white : Color.black;
        }
        atlasTexture.SetPixels(clearColor);

        // Dictionary to track UVs and their corresponding textures
        Dictionary<Vector2, Texture2D> uvToTexture = new Dictionary<Vector2, Texture2D>();

        foreach (var section in atlasSections)
        {
            Sprite sprite = section.sprite;
            int columnIndex = (int)section.uv.x;
            int rowIndex = (int)section.uv.y;

            // Calculate position in the atlas based on row and column indices
            int x = columnIndex * 32; // Assuming 32 is the size of each texture
            int y = (31 - rowIndex) * 32; // Invert the row index

            // Ensure the sprite fits within the atlas
            if (x + 32 <= atlasSize && y + 32 <= atlasSize)
            {
                // Copy the sprite texture to the atlas
                Color[] colors = sprite.texture.GetPixels();
                atlasTexture.SetPixels(x, y, 32, 32, colors);

                // Add the UV and texture to the dictionary
                uvToTexture[new Vector2(columnIndex, rowIndex)] = atlasTexture;
            }
            else
            {
                Debug.LogError("Sprite is too large for the atlas at row " + rowIndex + " and column " + columnIndex);
            }
        }

        // Apply changes and set the texture to be readable
        atlasTexture.Apply();
        atlasTexture.wrapMode = TextureWrapMode.Clamp;
        atlasTexture.filterMode = FilterMode.Point;
        return atlasTexture;
    }




    public class AtlasSection
    {
        public Sprite sprite;
        public Vector2 uv;

        public AtlasSection(Sprite sprite, Vector2 uv)
        {
            this.sprite = sprite;
            this.uv = uv;
        }
    }
}