using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GlobalData;

public class AssetManager : MonoBehaviour
{
    [SerializeField] private Mesh tileDefault;

    //Addressables.LoadAsset<GameObject>("AssetAddress");
    //Addressables.Instantiate<GameObject>("AssetAddress");
    //yes, you can call LoadAsset() twice, and you'll get a ref count of 2, but won't actually re-load the thing. 
    //You'll need two ReleaseAsset() calls to get the ref count back to 0.
    //Using Addressables instantiation intefraces will load the asset, then immediately adds it to your Scene.

    private Dictionary<string, GameObject> tileCache = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> unitBaseCache = new Dictionary<string, GameObject>();
    private Dictionary<string, UnitBaseData> unitDataCache = new Dictionary<string, UnitBaseData>();

    public IUnit InstantiateUnit(bool initial, Vector3Int localPlace, string asset, int unitTeam)
    {
        if (asset != "")
        {
            Debug.Log("asset" + asset);
            if (!unitBaseCache.ContainsKey(asset))
            {
                if (File.Exists(unitModAssetPath + asset + '/' + asset))
                {
                    AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(unitModAssetPath + asset + '/' + asset);
                    GameObject myLoadedGameObject = myLoadedAssetBundle.LoadAsset<GameObject>(asset);
                    myLoadedGameObject.name = asset;
                    //myLoadedGameObject.transform.parent = rootGameObject.transform;;l'
                    unitBaseCache[asset] = myLoadedGameObject;
                }
                else
                {
                    GameObject myLoadedGameObject = Resources.Load(unitAssetPath + asset + '/' + asset) as GameObject;
                    //myLoadedGameObject.transform.parent = rootGameObject.transform;
                    if (myLoadedGameObject != null)
                    {
                        unitBaseCache[asset] = myLoadedGameObject;
                    }
                    else { return null; }
                }
            }
            GameObject p = unitBaseCache[asset];
            //p = Instantiate(p, new Vector3(0, 0, 0), rotation);
            p = InstantiateWithName(p, asset);
            GameObject rootGameObject = new GameObject();
            //rootGameObject.transform.rotation = rotation;
            rootGameObject.AddComponent<Unit>();
            //rootGameObject.AddComponent<UnitAnimator>();
            p.transform.parent = rootGameObject.transform;
            IUnit unit = rootGameObject.GetComponent<IUnit>();
            UnitBaseData data = LoadUnitBaseData(asset);
            unit.Load(initial, localPlace, data, unitTeam);
            return unit;
        }
        return null;
    }

    public IBuilding InstantiateBuilding(bool initial, Vector3Int localPlace, string asset, int buildingTeam)
    {
        if (asset != "")
        {
            if (!unitBaseCache.ContainsKey(asset))
            {
                if (File.Exists(buildingModAssetPath + asset + '/' + asset))
                {
                    AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(buildingModAssetPath + asset + '/' + asset);
                    GameObject myLoadedGameObject = myLoadedAssetBundle.LoadAsset<GameObject>(asset);
                    myLoadedGameObject.name = asset;
                    //myLoadedGameObject.transform.parent = rootGameObject.transform;
                    unitBaseCache[asset] = myLoadedGameObject;
                }
                else
                {
                    GameObject myLoadedGameObject = Resources.Load(buildingAssetPath + asset + '/' + asset) as GameObject;
                    //myLoadedGameObject.transform.parent = rootGameObject.transform;
                    if (myLoadedGameObject != null)
                    {
                        unitBaseCache[asset] = myLoadedGameObject;
                    }
                    else { return null; }
                }
            }
            GameObject p = unitBaseCache[asset];
            //p = Instantiate(p, new Vector3(0, 0, 0), rotation);
            p = InstantiateWithName(p, asset);
            GameObject rootGameObject = new GameObject();
            //rootGameObject.transform.rotation = rotation;
            rootGameObject.AddComponent<Building>();
            //rootGameObject.AddComponent<UnitAnimator>();
            p.transform.parent = rootGameObject.transform;
            IBuilding building = rootGameObject.GetComponent<IBuilding>();
            UnitBaseData data = LoadUnitBaseData(asset);
            building.Load(initial, localPlace, data, buildingTeam);
            return building;
        }
        return null;
    }

    public GameObject InstantiateModel(string model)
    {
        if (model != "")
        {
            if (!unitBaseCache.ContainsKey(model))
            {
                if (File.Exists(buildingModAssetPath + model + '/' + model))
                {
                    AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(buildingModAssetPath + model + '/' + model);
                    GameObject myLoadedGameObject = myLoadedAssetBundle.LoadAsset<GameObject>(model);
                    myLoadedGameObject.name = model;
                    unitBaseCache[model] = myLoadedGameObject;
                }
                else
                {
                    GameObject myLoadedGameObject = Resources.Load(buildingAssetPath + model + '/' + model) as GameObject;
                    if (myLoadedGameObject != null)
                    {
                        unitBaseCache[model] = myLoadedGameObject;
                    }
                    else { return null; }
                }
            }
            GameObject p = unitBaseCache[model];
            //p = Instantiate(p, new Vector3(0, 0, 0), rotation);
            p = InstantiateWithName(p, model);
            return p;
        }
        return null;
    }

    public Buff LoadBuff(string buffScript)
    {
        Buff loaded = new Buff(BuffXml.Load(buffScript));
        if (loaded == null)
        {
            loaded = new Buff(Resources.Load(buffScriptPath + buffScript) as BuffScript);
        }
        return loaded;
    }

    public UnitBaseData LoadUnitBaseData(string form)
    {
        if (!unitDataCache.ContainsKey(form))
        {
            //look for xml in modfile
            UnitBaseData loaded = new UnitBaseData(UnitBaseXml.Load(form));
            if (loaded == null)
            {
                loaded = new UnitBaseData(Resources.Load(unitBaseScriptPath + form) as UnitBaseScript);
            }
            if (loaded != null)
            {
                unitDataCache[form] = loaded;
            }
            else { return null; }
        }
        return unitDataCache[form];
    }

    public void SpawnMap(string mapName)
    {
        MapXml map = MapXml.Load(mapName);
        if (map == null)
        {
            MapScript mapScript = Resources.Load(mapScriptPath + mapName) as MapScript;
            if (mapScript != null)
            {
                //TileManager.globalInstance.LoadPlayers(mapScript.teamsTotal);
                foreach (MapScript.TileInfo t in mapScript.tilesInfo)
                {
                    InstantiateTile(t.prefab, t.localPlace, t.terrain, t.unit, t.building, t.unitTeam, t.buildingTeam);
                }
            }
        }
        else
        {
            //TileManager.globalInstance.LoadPlayers(map.teamsTotal);
            foreach (MapXml.TileInfo t in map.tilesInfo)
            {
                InstantiateTile(t.prefab, t.localPlace, t.terrain, t.unit, t.building, t.unitTeam, t.buildingTeam);
            }
        }
    }

    private GameObject InstantiateWithName(GameObject p, string name)
    {
        GameObject g = Instantiate(p);
        g.name = name;
        return g;
    }

    //private void InstantiateTile(string asset, Vector3Int localPlace, int terrain, int skyTerrain, string unit, string building, int unitTeam, int buildingTeam) => StartCoroutine(InstantiateTileCoRoutine(asset, localPlace, terrain, skyTerrain, unit, building, unitTeam, buildingTeam));
    private void InstantiateTile(string asset, Vector3Int localPlace, int terrain, string unit, string building, int unitTeam, int buildingTeam)
    {
        if (asset != "")
        {
            if (!tileCache.ContainsKey(asset))
            {
                //yield return bundleLoadRequest;
                if (File.Exists(tileModAssetPath + asset))
                {
                    //myLoadedAssetBundle = Resources.Load(tileAssetPath + asset) as AssetBundle;
                    //Debug.Log(tileAssetPath + asset);
                    //yield return bundleResourceRequest;
                    AssetBundle myLoadedAssetBundle = AssetBundle.LoadFromFile(tileModAssetPath + asset);
                    GameObject p = myLoadedAssetBundle.LoadAsset<GameObject>(asset);
                    p.AddComponent<TileObject>();
                    p.GetComponent<MeshFilter>().mesh = tileDefault;
                    tileCache[asset] = p;
                }
                else
                {
                    GameObject p = Resources.Load(tileAssetPath + asset) as GameObject;
                    if (p != null)
                    {
                        tileCache[asset] = p;
                    }
                    else { return; }
                }
            }
            GameObject prefab = tileCache[asset];
            prefab = Instantiate(prefab as GameObject);
            prefab.GetComponent<TileObject>().Load(localPlace, terrain);

            InstantiateBuilding(true, localPlace, building, buildingTeam);
            InstantiateUnit(true, localPlace, unit, unitTeam);
        }
    }
    /*
    private IEnumerator InstantiateTileCoRoutine(string asset, Vector3Int localPlace, int terrain, int skyTerrain, string unit, string building, int unitTeam, int buildingTeam)
    {
        if (!tileCache.ContainsKey(asset))
        {
            AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(tileModAssetPath + asset);
            yield return bundleLoadRequest;

            AssetBundle myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if (myLoadedAssetBundle == null)
            {
                Debug.Log(tileAssetPath + asset);
                ResourceRequest bundleResourceRequest = Resources.LoadAsync(tileAssetPath + asset);
                //Debug.Log(tileAssetPath + asset);
                yield return bundleResourceRequest;
                myLoadedAssetBundle = bundleResourceRequest.asset as AssetBundle;
            }
            if (!tileCache.ContainsKey(asset))
            {
                tileCache.Add(asset, myLoadedAssetBundle);
            }
        }

        AssetBundleRequest assetLoadRequest = tileCache[asset].LoadAssetAsync<GameObject>(asset);
        yield return assetLoadRequest;

        GameObject prefab = Instantiate(assetLoadRequest.asset as GameObject, new Vector3(0, 0, 0), rotation);

        //myLoadedAssetBundle.Unload(false);
        prefab.GetComponent<MeshFilter>().mesh = tileDefault;
        TileObject tile = prefab.AddComponent<TileObject>();
        tile.Load(localPlace, terrain, skyTerrain);

        InstantiateBuilding(localPlace, building, buildingTeam);
        InstantiateUnit(localPlace, unit, unitTeam);
    }

    private void Awake()
    {
        if (GlobalManager.AssetManager != this)
        {
            Destroy(gameObject);
        }
    }*/
}
