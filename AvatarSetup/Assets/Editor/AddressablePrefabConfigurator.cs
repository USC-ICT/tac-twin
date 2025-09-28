using Ride;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;
using VHAssets;

public static class AddressablePrefabConfigurator
{
    private const string m_addressableGroupName = "CC_Characters";
    private static readonly HashSet<string> m_targetParts = new() { "Shirt", "Pants", "Helmet", "Vest", "Boots" };
    private const float m_normalMapStrengthValue = 0.3f;
    private const string PRESERVE_SPECULAR_PROPERTY = "_EnableBlendModePreserveSpecularLighting";
    private const string ALPHA_CLIPPING_PROPERTY = "_AlphaCutoffEnable";
    private const string NORMAL_VALUE_PROPERTY = "_NormalScale";
    private const string FACE_DECAL_PATH = "Assets/Art/Decals/Prefabs/FaceBloodDecal.prefab";
    private static readonly Vector3 FACE_DECAL_POSITION = new(-0.06f, 0.044f, 0.09f);
    private static readonly Vector3 FACE_DECAL_ROTATION = new(0f, 140f, 0f);
    private const string VEST_DECAL_PATH = "Assets/Art/Decals/Prefabs/VestBloodDecal.prefab";
    private static readonly Vector3 VEST_DECAL_POSITION = new(0.148f, 0.085f, -0.078f);
    private static readonly Vector3 VEST_DECAL_ROTATION = new(114.262f, 38.586f, 41.618f);
    private const string PORTRAIT_PREFAB_PATH = "Assets/Prefabs/Button_PortraitSelect.prefab";

    [MenuItem("Ride/Addressables/Configure Prefabs")]
    public static void ConfigureAddressablePrefabs()
    {
        List<string> modifiedPrefabs = new();
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable settings not found!");
            return;
        }

        AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.name == m_addressableGroupName);
        if (group == null)
        {
            Debug.LogError($"Addressable group '{m_addressableGroupName}' not found!");
            return;
        }

        foreach (AddressableAssetEntry entry in group.entries)
        {
            if (entry.AssetPath.Contains("Portrait") || entry.AssetPath.Contains("portrait"))
                continue;

            if (entry.AssetPath.EndsWith(".prefab") )
                if (ConfigurePrefab(entry.AssetPath))
                    modifiedPrefabs.Add(Path.GetFileNameWithoutExtension(entry.AssetPath));
        }

        Debug.Log(modifiedPrefabs.Count > 0
            ? $"Modified Prefabs: {string.Join(", ", modifiedPrefabs)}"
            : "No prefabs were modified.");
    }

    private static bool ConfigurePrefab(string prefabAssetPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
        if (prefab == null)
        {
            Debug.LogError($"Could not load prefab at {prefabAssetPath}");
            return false;
        }

        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (prefabInstance == null)
        {
            Debug.LogError($"Failed to instantiate prefab {prefab.name}");
            return false;
        }

        bool prefabModified = false;
        prefabModified |= ConfigureSkinnedMeshRenderers(prefabInstance);
        prefabModified |= AddBloodDecal(prefabInstance, "CC_Base_Head", FACE_DECAL_PATH, FACE_DECAL_POSITION, FACE_DECAL_ROTATION);
        prefabModified |= AddBloodDecal(prefabInstance, "CC_Base_R_RibsTwist", VEST_DECAL_PATH, VEST_DECAL_POSITION, VEST_DECAL_ROTATION);
        prefabModified |= AdjustClaviclePosition(prefabInstance, "CC_Base_L_Clavicle", 0.2f);
        prefabModified |= AdjustClaviclePosition(prefabInstance, "CC_Base_R_Clavicle", 0.2f);

        if (prefabModified)
            PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstance, prefabAssetPath, InteractionMode.AutomatedAction);

        Object.DestroyImmediate(prefabInstance);
        return prefabModified;
    }

    private static bool ConfigureSkinnedMeshRenderers(GameObject prefab)
    {
        bool prefabModified = false;
        SkinnedMeshRenderer[] skinnedMeshRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinnedMeshRenderers)
        {
            string objName = smr.gameObject.name;
            //Set UpdateWhenOffscreen to true for all child SkinnedMeshRenderer components
            if (!objName.EndsWith("_BoneRoot") && !smr.updateWhenOffscreen)
            {
                smr.updateWhenOffscreen = true;
                prefabModified = true;
            }

            if (m_targetParts.Contains(objName) && smr.sharedMaterial != null)
            {
                Material material = smr.sharedMaterial;
                //Set Surface Input -> Normal Map value to 0.3 for target parts
                if (material.HasProperty(NORMAL_VALUE_PROPERTY) &&
                    Mathf.Abs(material.GetFloat(NORMAL_VALUE_PROPERTY) - m_normalMapStrengthValue) > 0.001f)
                {
                    material.SetFloat(NORMAL_VALUE_PROPERTY, m_normalMapStrengthValue);
                    prefabModified = true;
                }
            }

            if(objName == "Lenses")
                prefabModified |= ConfigureLenses(smr);
        }
        return prefabModified;
    }

    private static bool ConfigureLenses(SkinnedMeshRenderer lenses)
    {
        bool prefabModified = false;
        //Turn off shadow casting
        if (lenses.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
        {
            lenses.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            prefabModified = true;
        }

        //Disable preserve specular highlights and enable alpha clipping
        if (lenses.sharedMaterial != null)
        {
            Material material = lenses.sharedMaterial;
            if (material.HasProperty(PRESERVE_SPECULAR_PROPERTY) &&
                material.GetFloat(PRESERVE_SPECULAR_PROPERTY) != 0)
            {
                material.SetFloat(PRESERVE_SPECULAR_PROPERTY, 0);
                prefabModified = true;
            }

            // Enable Alpha Clipping
            if (material.HasProperty(ALPHA_CLIPPING_PROPERTY) &&
                material.GetFloat(ALPHA_CLIPPING_PROPERTY) != 1)
            {
                material.SetFloat(ALPHA_CLIPPING_PROPERTY, 1);
                prefabModified = true;
            }
        }
        return prefabModified;
    }

    private static bool AddBloodDecal(GameObject prefab, string parentName, string prefabPath, Vector3 localPosition, Vector3 localRotation)
    {
        Transform parentTransform = VHUtils.FindChildRecursive(prefab, parentName).transform;
        if (parentTransform == null)
        {
            Debug.LogError($"Parent {parentName} not found in prefab {prefab.name}");
            return false;
        }

        if (parentTransform.Find(Path.GetFileNameWithoutExtension(prefabPath)) != null)
            return false;

        GameObject decalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (decalPrefab == null)
        {
            Debug.LogError($"Decal prefab at {prefabPath} could not be loaded!");
            return false;
        }

        GameObject decalInstance = Object.Instantiate(decalPrefab, parentTransform);
        decalInstance.transform.localPosition = localPosition;
        decalInstance.transform.localRotation = Quaternion.Euler(localRotation);
        decalInstance.name = decalPrefab.name;

        return true;
    }

    private static bool AdjustClaviclePosition(GameObject prefab, string clavicleName, float targetY)
    {
        //fix shoulder position in prefab, outside of the avatar
        Transform clavicle = VHUtils.FindChildRecursive(prefab, clavicleName).transform;
        if (clavicle == null)
        {
            Debug.LogError($"Clavicle {clavicleName} not found in prefab {prefab.name}");
            return false;
        }
        if (Mathf.Abs(clavicle.localPosition.y - targetY) > 0.001f)
        {
            clavicle.localPosition = new Vector3(clavicle.localPosition.x, targetY, clavicle.localPosition.z);
            return true;
        }
        return false;
    }

    [MenuItem("Ride/Addressables/Generate Portrait Prefabs")]
    private static void GeneratePortraitPrefab()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("Addressable settings not found!"); return; }

        GameObject basePortraitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PORTRAIT_PREFAB_PATH);
        if (basePortraitPrefab == null) { Debug.LogError($"Base portrait prefab not found at {PORTRAIT_PREFAB_PATH}!"); return; }

        AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.name == m_addressableGroupName);
        if (group == null) { Debug.LogError($"Addressable group '{m_addressableGroupName}' not found!"); return; }


        List<AddressableAssetEntry> entriesCopy = group.entries.ToList();
        foreach (AddressableAssetEntry entry in entriesCopy)
        {
            if (!entry.AssetPath.EndsWith(".prefab")) continue;

            GameObject addressablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);
            if (addressablePrefab == null) continue;

            PortraitTexture sourcePortrait = addressablePrefab.GetComponent<PortraitTexture>();
            if (sourcePortrait == null) continue;

            // Instantiate base prefab
            GameObject portraitInstance = PrefabUtility.InstantiatePrefab(basePortraitPrefab) as GameObject;
            if (portraitInstance == null) continue;
            RawImage portraitImage = portraitInstance.GetComponentInChildren<RawImage>();
            TextMeshProUGUI portraitLabel = portraitInstance.GetComponentInChildren<TextMeshProUGUI>();
            //PortraitTexture portraitComponent = newPortraitInstance.GetComponent<PortraitTexture>();
            //if (portraitComponent == null)
            //{
            //    Debug.LogError("Base portrait prefab must have a PortraitTexture component!");
            //    Object.DestroyImmediate(newPortraitInstance);
            //    continue;
            //}


            // Assign properties
            portraitImage.texture = sourcePortrait.m_portrait;
            portraitLabel.text = entry.labels.FirstOrDefault(label => label != "CC_Character") ?? string.Empty;

            // Save to same folder as addressable
            string directory = Path.GetDirectoryName(entry.AssetPath);
            string newPath = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(entry.AssetPath)}_Portrait.prefab");
            if (File.Exists(newPath)) { Debug.Log($"Skipping: Portrait prefab already exists for {entry.AssetPath}"); continue; }
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            PrefabUtility.SaveAsPrefabAsset(portraitInstance, newPath);
            Object.DestroyImmediate(portraitInstance);

            // Mark new portrait prefab as addressable
            AddressableAssetEntry newEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newPath), group);

            // Set simplified addressable name
            string simplifiedAddress = Path.GetFileNameWithoutExtension(newPath);
            newEntry.address = simplifiedAddress;

            // Add 'Portrait' addressable label
            string label = $"{simplifiedAddress}";
            if (!settings.GetLabels().Contains(label))
                settings.AddLabel(label);

            newEntry.SetLabel(label, true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Portrait prefabs generated successfully.");
    }
}
