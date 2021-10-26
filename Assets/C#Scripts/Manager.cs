using UnityEngine;

public class Manager : MonoBehaviour
{
    private static Manager manager;

    private TileManager tileManager;
    private PlayerManager playerManager;

    private AssetManager assetManager;
    private EventsManager eventsManager;

    [SerializeField] private GameObject assetManagerPrefab;
    [SerializeField] private GameObject eventsManagerPrefab;
    
    public static TileManager TileManager
    {
        get
        {
            if (manager.tileManager == null)
            {
                manager.tileManager = new TileManager();
            }
            return manager.tileManager;
        }
    }

    public static PlayerManager PlayerManager
    {
        get
        {
            if (manager.playerManager == null)
            {
                manager.playerManager = new PlayerManager();
            }
            return manager.playerManager;
        }
    }

    public static AssetManager AssetManager
    {
        get
        {
            if (manager.assetManager == null)
            {
                manager.assetManager = Instantiate(manager.assetManagerPrefab).GetComponent<AssetManager>();
            }
            return manager.assetManager;
        }
    }

    public static EventsManager EventsManager
    {
        get
        {
            if (manager.eventsManager == null)
            {
                manager.eventsManager = Instantiate(manager.eventsManagerPrefab).GetComponent<EventsManager>();
            }
            return manager.eventsManager;
        }
    }

    private void Awake()
    {
        if (manager == null)
        {
            manager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //GameObject myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "cursor")).LoadAsset<GameObject>("Cursor");
        //GameObject prefab = Instantiate(myLoadedAssetBundle, new Vector3(0, 0, 0), rotation);
        AssetManager.SpawnMap("bootybay");
        Pointer.globalInstance.Setup();
    }
}