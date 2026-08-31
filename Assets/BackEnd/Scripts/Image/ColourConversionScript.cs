using UnityEngine;

public class ImageToColorArray : MonoBehaviour
{
    [Header("Assign your source image here")]
    public Texture2D sourceImage;

    [Header("Resized output")]
    public Texture2D resizedImage;

    [Header("Color data from resized image")]
    public Color[] pixelColors;

    [Header("Grid Reference")]
    [SerializeField] private GridGenerator gridGenerator;

    ImagePacket imagePacket;

    [ContextMenu("Resize and Extract Colors")]


    private void Start()
    {
        RetrieveImage();
        ResizeAndExtractColors();
    }
    public void ResizeAndExtractColors()
    {
        if (sourceImage == null)
        {
            Debug.LogError("Source image not assigned.");
            return;
        }

        // Step 1: Resize to 32x32
        resizedImage = new Texture2D(32, 32, TextureFormat.RGBA32, false);

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float u = (float)x / 32f;
                float v = (float)y / 32f;

                Color sampledColor = sourceImage.GetPixelBilinear(u, v);
                resizedImage.SetPixel(x, y, sampledColor);
            }
        }

        resizedImage.Apply();

        // Step 2: Extract colors into array
        pixelColors = resizedImage.GetPixels(); // returns Color[32*32] in row-major order

        Debug.Log("Resized image and extracted pixel colors.");
        ApplyColoursToPlants();
    }

    public void ApplyColoursToPlants()
    {
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator reference not assigned.");
            return;
        }

        gridGenerator.ColourPlants(pixelColors);
    }

    public void RetrieveImage()
    {
        if (GameObject.Find("ImagePacket") != null)
        {
            imagePacket = GameObject.Find("ImagePacket").GetComponent<ImagePacket>();
            sourceImage = imagePacket.RetrieveAndDestroy();
        }
    }

}