using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride;
using Ride.Terrain;
using Ride.UI;
using UnityEngine.SceneManagement;

/// <summary>
///  Controller for changing VH Sandbox terrain
/// </summary>
public class EnvironmentSwapper : MonoBehaviour
{
    //[SerializeField] TerrainLoader terrainLoader;
    [SerializeField] EnvironmentDatabase environmentDatabase;
    [SerializeField] RideButtonUGUI swapTerrainButton;
    [SerializeField] RideDropdownTMPro terrainDropDown;
    [SerializeField] RideDropdownTMPro lodDropDown;
    [SerializeField] RideToggleUGUI loadTreesToggle;

    const string DEFAULT_OPTION = "Default Plane";

    public ITerrain m_terrain;

    //public TerrainSO terrainToLoad { get; private set; }
    public Vector3 terrainCenter { get; private set; }

    private void Start()
    {
        List<string> envNames = new();
        envNames.Add(DEFAULT_OPTION);

        foreach (var env in environmentDatabase.Environments)
        {
            //envNames.Add(env.terrainSO.name);
        }

        terrainDropDown.m_dropdown.ClearOptions();
        terrainDropDown.m_dropdown.AddOptions(envNames);
        swapTerrainButton.onClick.AddListener(SwapTerrain);
        terrainDropDown.m_dropdown.onValueChanged.AddListener(CheckSelectedOption);
        CheckSelectedOption(0);

        //terrainLoader.m_overrideTerrainDefaults = true;


        LoadNewEnvironment();
    }

    public void SwapTerrain()
    {
#if false
        if (terrainDropDown.selection == 0)
        {
            SetEnvironmentParams(null);
        }
        else
        {
            terrainLoader.m_terrain = string.Empty;
            terrainLoader.m_overrideTerrainDefaults = false;
            SetEnvironmentParams(
                environmentDatabase[terrainDropDown.selection - 1].terrainSO,
                environmentDatabase[terrainDropDown.selection - 1].loadPosition);
        }

        StartCoroutine(TerrainSwapRoutine());
#endif
    }

    IEnumerator TerrainSwapRoutine()
    {
        DestroyCurrentEnvironment();

        yield return null;  //have to wait a frame, otherwise we get null errors

        LoadNewEnvironment();
    }



#if false
    void SetEnvironmentParams(TerrainSO terrainSO, Vector3 loadPosition = default)
    {
        terrainToLoad = terrainSO;
        terrainLoader.m_terrainSO = terrainSO;
        terrainCenter = loadPosition;
        terrainLoader.m_startLoadingPoint.position = -loadPosition;

        if (!terrainToLoad) return;

        terrainLoader.m_collisionLod = terrainSO.lods[0];
        terrainLoader.m_lod = terrainSO.lods[lodDropDown.selection];
        terrainLoader.m_loadTrees = loadTreesToggle.isOn;
    }
#endif

    void LoadNewEnvironment()
    {
        //m_terrain = terrainLoader.LoadTerrain();
    }

    void DestroyCurrentEnvironment()
    {
        Globals.api.terrainSystem.DestroyTerrain(m_terrain);
    }

    void CheckSelectedOption(int option)
    {
        lodDropDown.m_dropdown.ClearOptions();

        if (option != 0)
        {
            loadTreesToggle.isInteractable = true;
            lodDropDown.isInteractable = true;

            //lodDropDown.m_dropdown.AddOptions(environmentDatabase[terrainDropDown.selection - 1].terrainSO.lods);
            //lodDropDown.selection = lodDropDown.numItems - 1;
        }
        else
        {
            loadTreesToggle.isOn = false;
            loadTreesToggle.isInteractable = false;
            lodDropDown.isInteractable = false;
        }
    }
}
