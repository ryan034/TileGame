using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    private static GlobalManager globalManager;

    private TileManager tileManager;
    private PlayerManager playerManager;

    private AssetManager assetManager;
    private EventsManager eventsManager;
    private UnitTransformManager unitTransformManager;

    [SerializeField] private GameObject assetManagerPrefab;
    [SerializeField] private GameObject eventsManagerPrefab;
    [SerializeField] private GameObject unitTransformManagerPrefab;

    public static TileManager TileManager
    {
        get
        {
            if (globalManager.tileManager == null)
            {
                globalManager.tileManager = new TileManager();
            }
            return globalManager.tileManager;
        }
    }

    public static PlayerManager PlayerManager
    {
        get
        {
            if (globalManager.playerManager == null)
            {
                globalManager.playerManager = new PlayerManager();
            }
            return globalManager.playerManager;
        }
    }

    public static AssetManager AssetManager
    {
        get
        {
            if (globalManager.assetManager == null)
            {
                globalManager.assetManager = Instantiate(globalManager.assetManagerPrefab).GetComponent<AssetManager>();
            }
            return globalManager.assetManager;
        }
    }

    public static EventsManager EventsManager
    {
        get
        {
            if (globalManager.eventsManager == null)
            {
                globalManager.eventsManager = Instantiate(globalManager.eventsManagerPrefab).GetComponent<EventsManager>();
            }
            return globalManager.eventsManager;
        }
    }

    public static UnitTransformManager UnitTransformManager
    {
        get
        {
            if (globalManager.unitTransformManager == null)
            {
                globalManager.unitTransformManager = Instantiate(globalManager.unitTransformManagerPrefab).GetComponent<UnitTransformManager>();
            }
            return globalManager.unitTransformManager;
        }
    }

    private void Awake()
    {
        if (globalManager == null)
        {
            globalManager = this;
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
        PlayerManager.EndAndStartNextTurn();
    }
}