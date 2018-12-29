using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleProto.Data
{
    /// <summary>
    /// Base class for all data-assets
    /// </summary>
    public class DataAsset : ScriptableObject
    {
        [SerializeField] private string _assetGuid = "";

        /// <summary>
        /// Gets unique identifier of the asset
        /// </summary>
        internal string AssetGuid
        {
            get { return _assetGuid; }
            private set { _assetGuid = value; }
        }
        
#if UNITY_EDITOR

        private static readonly Dictionary<string, DataAsset> AssetsMap = new Dictionary<string, DataAsset>();

        private void CheckGuid()
        {
            if (!string.IsNullOrEmpty(AssetGuid))
            {
                DataAsset existed;
                if (!AssetsMap.TryGetValue(AssetGuid, out existed))
                {
                    AssetsMap.Add(AssetGuid, this);
                    return;
                }

                if (existed == null || existed == this)
                {
                    AssetsMap[AssetGuid] = this;
                    return;
                }
            }

            AssetGuid = Guid.NewGuid().ToString("N");
            AssetsMap.Add(AssetGuid, this);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        protected virtual void OnEnable()
        {
            CheckGuid();
        }
#endif
    }
}
