// Copyright 2024 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Uralstech.UXR.Utilities.Editor
{
    public static class AssetMenuExtensions
    {
        private const string PackageName = "com.uralstech.uxr.utilities";
        private const string XrInputFieldPrefabPath = "Runtime/Prefabs/UI/TextInputField.prefab";
        private const string XrInputFieldWithVoicePrefabPath = "Runtime/Prefabs/UI/TextInputFieldWithVoice.prefab";

        [MenuItem("GameObject/UI/XR Input Field", false, 10)]
        private static void CreateXRInputField()
        {
            // Instantiate the prefab
            if (!InstantiatePrefab(XrInputFieldPrefabPath, out string prefabPath))
                Debug.LogError($"Could not find prefab for XR Input Field at path: {prefabPath}. Verify the path and ensure it exists in the package.");
        }

        [MenuItem("GameObject/UI/XR Input Field with Voice Input", false, 10)]
        private static void CreateXRInputFieldWithVoice()
        {
            // Instantiate the prefab
            if (!InstantiatePrefab(XrInputFieldWithVoicePrefabPath, out string prefabPath))
                Debug.LogError($"Could not find prefab for XR Input Field with Voice at path: {prefabPath}. Verify the path and ensure it exists in the package.");
        }

        [MenuItem("GameObject/XR/XR Keyboard Manager", false, 10)]
        private static void CreateXRKeyboardManager()
        {
            // 1. Create a new empty game object.
            GameObject keyboardManager = new(nameof(XRKeyboardManager));

            // 2. Add the XRKeyboardManager Component to the new GameObject.
            keyboardManager.AddComponent<XRKeyboardManager>();

            // 3. Place it in the scene
            keyboardManager.transform.SetParent(Selection.activeTransform, false);

            // 4. Handle Undo
            Undo.RegisterCreatedObjectUndo(keyboardManager, $"Create {nameof(XRKeyboardManager)}");

            // 5. Select the created game object.
            Selection.activeGameObject = keyboardManager;
        }

        private static bool InstantiatePrefab(string relativePrefabPath, out string prefabPath, bool overridePackagePathCache = false)
        {
            prefabPath = string.Empty;

            // 1. Find the package path
            string packagePath = GetPackagePath(overridePackagePathCache);
            if (string.IsNullOrEmpty(packagePath))
            {
                Debug.LogError($"Could not find package path for {PackageName}.");
                return false;
            }

            // 2. Construct the prefab path relative to the package root.
            // Use forward slashes for path since it can be used for both Windows and Unix paths.
            prefabPath = Path.Combine(packagePath, relativePrefabPath);

            // 3. Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return !overridePackagePathCache && InstantiatePrefab(relativePrefabPath, out prefabPath, true);

            // 4. Instantiate the prefab
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // 5. Place it in the scene
            instance.transform.SetParent(Selection.activeTransform, false); // Parent to current selection

            // 6. Set name
            instance.name = prefab.name;

            // 7. Handle undo
            Undo.RegisterCreatedObjectUndo(instance, $"Create {prefab.name}");

            // 8. Select created object
            Selection.activeGameObject = instance;
            return true;
        }

        private static string s_packagePathCached = string.Empty;

        // Helper method to get the package path
        private static string GetPackagePath(bool overrideCached)
        {
            if (!string.IsNullOrEmpty(s_packagePathCached) && !overrideCached)
                return s_packagePathCached;

            s_packagePathCached = UnityEditor.PackageManager.PackageInfo.FindForPackageName(PackageName)?.assetPath;
            return s_packagePathCached;
        }
    }
}
