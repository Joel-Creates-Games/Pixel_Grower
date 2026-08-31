using UnityEngine;

/// <summary>
/// Attach this to a GameObject named “ImagePacket” (or tagged appropriately) in your initial scene.
/// It will persist across scene loads, carry one Texture2D payload, and self-destruct on retrieval.
/// </summary>
public class ImagePacket : MonoBehaviour
{
    public static ImagePacket Instance { get; private set; }
    [SerializeField] private Texture2D _payload;

    void Awake()
    {
        // Enforce singleton and persist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Store a Texture2D in the packet before loading the next scene.
    /// </summary>
    public void SetImage(Texture2D image)
    {
        _payload = image;
    }

    /// <summary>
    /// Retrieve the stored Texture2D, destroy this packet object, and clear the instance.
    /// Returns null if no payload was set or if already retrieved.
    /// </summary>
    public Texture2D RetrieveAndDestroy()
    {
        Texture2D result = _payload;
        _payload = null;

        // Clean up
        Instance = null;
        Destroy(gameObject);

        return result;
    }

    /// <summary>
    /// Peek at the image without destroying the packet.
    /// </summary>
    public Texture2D Peek()
    {
        return _payload;
    }
}
