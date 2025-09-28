using UnityEngine;
using UnityEditor;
using System.Collections;

class MaterialImportSettings : AssetPostprocessor {
	public override int GetPostprocessOrder () { return -10; }

	void OnPreprocessModel ()
	{
		// EDF - ignore RIDE assets
		if (assetPath.StartsWith("Assets/Ride/") ||
			assetPath.StartsWith("Assets/Ride_Art/") ||
			assetPath.StartsWith("Assets/Ride_Dependencies/") ||
			assetPath.StartsWith("Assets/VH"))
			return;


		ModelImporter modelImporter = (ModelImporter) assetImporter;
		// -------MATERIAL NAME
		modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
		// -------MATERIAL SEARCH
		modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;
	}
}
