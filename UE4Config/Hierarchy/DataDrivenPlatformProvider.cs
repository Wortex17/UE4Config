﻿using System;
using System.Collections.Generic;
using UE4Config.Evaluation;
using UE4Config.Parsing;

namespace UE4Config.Hierarchy
{
    /// <summary>
    /// A class that supports the DataDrivenPlatformInfo setup introduced in Unreal Engine 4.23
    /// </summary>
    public class DataDrivenPlatformProvider
    {
        public IConfigFileProviderAutoPlatformModel FileProvider { get; private set; }

        public IReadOnlyDictionary<string, DataDrivenPlatformInfo> DataDrivenPlatformInfos => m_DataDrivenPlatformInfos;

        protected Dictionary<string, DataDrivenPlatformInfo> m_DataDrivenPlatformInfos { get; }  =
            new Dictionary<string, DataDrivenPlatformInfo>();

        public void Setup(IConfigFileProviderAutoPlatformModel configFileProvider)
        {
            FileProvider = configFileProvider;
        }

        public void CollectDataDrivenPlatforms()
        {
            if (FileProvider == null)
            {
                throw new NullReferenceException("FileProvider was null");
            }
            
            //Fetch all DataDrivenPlatformInfo.ini's, read them, construct platforms etc.
            var platforms = FileProvider.FindAllPlatforms();
            m_DataDrivenPlatformInfos.Clear();
            var dataDrivenPlatformInfos = m_DataDrivenPlatformInfos;
            ConfigIni dataDrivenPlatformConfig;
            foreach (var platformIdentifier in platforms)
            {
                if (FileProvider.LoadOrCreateDataDrivenPlatformConfig(platformIdentifier, out dataDrivenPlatformConfig))
                {
                    ParseDataDrivenPlatformInfos(dataDrivenPlatformConfig, info => dataDrivenPlatformInfos.Add(info.PlatformIdentifier, info));
                }
            }
        }

        protected void ParseDataDrivenPlatformInfos(ConfigIni dataDrivenPlatformConfig, Action<DataDrivenPlatformInfo> onDataDrivenPlatformInfo)
        {
            if (dataDrivenPlatformConfig == null)
            {
                return;
            }
            foreach (var iniSection in dataDrivenPlatformConfig.Sections)
            {
                const string platformInfoHeaderPrefix = "PlatformInfo ";
                string sectionName = iniSection.Name;
                if(sectionName.Length <= platformInfoHeaderPrefix.Length || !sectionName.StartsWith(platformInfoHeaderPrefix))
                    continue;
                
                var platformInfo = new DataDrivenPlatformInfo();
                platformInfo.PlatformIdentifier = iniSection.Name.Substring(platformInfoHeaderPrefix.Length);
                List<string> results = new List<string>();
                
                results.Clear();
                PropertyEvaluator.Default.EvaluatePropertyValues(dataDrivenPlatformConfig, sectionName, "TargetPlatformName", results);
                if (results.Count > 0)
                {
                    platformInfo.PlatformIdentifier = results[0];
                }
                
                results.Clear();
                PropertyEvaluator.Default.EvaluatePropertyValues(dataDrivenPlatformConfig, sectionName, "IniPlatformName", results);
                if (results.Count > 0)
                {
                    platformInfo.ParentPlatformIdentifier = results[0];
                }

                onDataDrivenPlatformInfo(platformInfo);
            }
        }

        public void RegisterDataDrivenPlatforms(IConfigReferenceTree referenceTree)
        {
            var dataDrivenPlatformInfos = m_DataDrivenPlatformInfos;
            var visitedPlatformIdentifiers = new List<string>();
            var hierarchyTemp = new List<DataDrivenPlatformInfo>();
            foreach (var dataDrivenPlatformInfo in dataDrivenPlatformInfos.Values)
            {
                //To add in correct order for valid tree structure, add roots&parents first
                if (visitedPlatformIdentifiers.Contains(dataDrivenPlatformInfo.PlatformIdentifier))
                {
                    continue;
                }
                hierarchyTemp.Clear();
                FindDataDrivenPlatformInfoHierarchy(dataDrivenPlatformInfo, dataDrivenPlatformInfos, ref hierarchyTemp);
                foreach (var dataDrivenPlatformInfoInHierarchy in hierarchyTemp)
                {
                    if (visitedPlatformIdentifiers.Contains(dataDrivenPlatformInfoInHierarchy.PlatformIdentifier))
                    {
                        continue;
                    }
                    
                    visitedPlatformIdentifiers.Add(dataDrivenPlatformInfoInHierarchy.PlatformIdentifier);
                    
                    IConfigPlatform parentPlatform = null;
                    if (!String.IsNullOrEmpty(dataDrivenPlatformInfoInHierarchy.ParentPlatformIdentifier))
                    {
                        parentPlatform = referenceTree.GetPlatform(dataDrivenPlatformInfoInHierarchy.ParentPlatformIdentifier);
                    }

                    referenceTree.RegisterPlatform(dataDrivenPlatformInfoInHierarchy.PlatformIdentifier,
                        parentPlatform);
                }
            }
        }

        protected void FindDataDrivenPlatformInfoHierarchy(DataDrivenPlatformInfo platformInfo, Dictionary<string, DataDrivenPlatformInfo> platformInfos, ref List<DataDrivenPlatformInfo> hierarchy)
        {
            DataDrivenPlatformInfo pivotPlatformInfo = platformInfo;
            do
            {
                if (hierarchy.Contains(pivotPlatformInfo))
                {
                    break; // Loop detected
                }

                hierarchy.Add(pivotPlatformInfo);
            } while (FindParentDataDrivenPlatformInfo(pivotPlatformInfo, platformInfos, out pivotPlatformInfo));
            
            hierarchy.Reverse();
        }
        
        protected bool FindParentDataDrivenPlatformInfo(DataDrivenPlatformInfo platformInfo, Dictionary<string, DataDrivenPlatformInfo> platformInfos, out DataDrivenPlatformInfo parentPlatformInfo)
        {
            if (!String.IsNullOrEmpty(platformInfo.ParentPlatformIdentifier))
            {
                if (platformInfos.TryGetValue(platformInfo.ParentPlatformIdentifier, out var parentPlatformInfoCandidate))
                {
                    parentPlatformInfo = parentPlatformInfoCandidate;
                    return true;
                }
                else
                {
                    //Create a stand-in for the unresolved platform
                    parentPlatformInfo = new DataDrivenPlatformInfo()
                    {
                        PlatformIdentifier = platformInfo.ParentPlatformIdentifier
                    };
                    return true;
                }
            }

            parentPlatformInfo = new DataDrivenPlatformInfo();
            return false;
        }
    }
}