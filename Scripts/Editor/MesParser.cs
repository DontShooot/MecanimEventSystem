using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MecanimEventSystem.Tools
{
    public class MesParser
    {
        private const string SectionBeginsToken = "---";

        private const string StateBeginToken = "State:";
        private const string StateNameToken = "m_Name:";
        private const string TagToken = "m_Tag:";
        private const string StateLayerToken = "m_ParentStateMachine:";

        private const string LayerBeginToken = "StateMachine:";
        private const string LayerNameToken = "m_Name:";
        private const string LayerDefaultStateToken = "m_DefaultState:";

        private const string SectionIdSeparatorToken = "&";
        private const string DataSeparatorToken = ": ";

        private class LayerData
        {
            public string Name;
            public string SectionId;
            public string DefaultStateName;
        }

        public static void ProcessMesEntity(MesEntity entity)
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                Debug.LogError("MecanimEventSystem can work only with asset's text serialization");
                return;
            }

            RuntimeAnimatorController controller = entity.OverrideController;

            if (controller == null)
            {
                var animator = entity.GetComponent<Animator>();
                if (animator == null)
                    animator = entity.GetComponentInChildren<Animator>();

                if (animator == null)
                {
                    Debug.LogError("Can't find animator component in " + entity.name);
                    return;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Animation controlles is not defined in " + animator.name + ". You can set Animation controller in MESEntity Override controller");
                    return;
                }

                controller = animator.runtimeAnimatorController;
            }
            ParseAnimationController(GetControllerPath(controller), entity);
        }

        private static void ParseAnimationController(string path, MesEntity entity)
        {
            AssetDatabase.SaveAssets();

            //Файл парсится два раза, чтобы избежать зависимости от порядка определения стэйт машин (слоев) и стэйтов

            var layersData = ParseAnimationLayers(path);
            
            if (layersData != null)
                ParseAnimationStates(path, entity, layersData);
        }

        private static List<LayerData> ParseAnimationLayers(string path)
        {
            try
            {
                var layersData = new List<LayerData>();
                using (StreamReader sr = File.OpenText(path))
                {
                    LayerData currentLayer = null;
                    string sectionId = null;
                    while (!sr.EndOfStream)
                    {
                        
                        var line = sr.ReadLine();
                        
                        if (line.Contains(SectionBeginsToken))
                        {
                            sectionId = ParseData(line, SectionIdSeparatorToken);
                            sectionId = Regex.Replace(sectionId, "[^.0-9]", "");

                            continue;
                        }

                        if (line == LayerBeginToken)
                        {
                            currentLayer = new LayerData() {SectionId = sectionId};
                            layersData.Add(currentLayer);
                        }

                        if (currentLayer == null) continue;

                        if (line.Contains(LayerNameToken))
                        {
                            string layerName = ParseData(line, DataSeparatorToken);
                            if (string.IsNullOrEmpty(sectionId) || string.IsNullOrEmpty(layerName))
                            {
                                Debug.LogError("Failed to parse layer name " + layerName + " in section " + sectionId + " of line: " + line);
                                return null;
                            }

                            currentLayer.Name = layerName;
                            
                        } else if (line.Contains(LayerDefaultStateToken))
                        {
                            var defaultState = ParseData(line, LayerDefaultStateToken);
                            defaultState = Regex.Replace(defaultState, "[^.0-9]", "");
                            currentLayer.DefaultStateName = defaultState;
                            currentLayer = null;
                        }
                    }
                }

                return layersData;
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse layers");
                Debug.LogError(ex.Message);
                return null;
            }
        }

        private static void ParseAnimationStates(string path, MesEntity entity, List<LayerData> layersData)
        {
            try
            {
                List<MesStateData> allStatesData = new List<MesStateData>();
                using (StreamReader sr = File.OpenText(path))
                {
                    MesStateData stateData = null;
                    string sectionId = null;
                    while (!sr.EndOfStream)
                    {

                        var line = sr.ReadLine();

                        if (line.Contains(SectionBeginsToken))
                        {
                            sectionId = ParseData(line, SectionIdSeparatorToken);
                            sectionId = Regex.Replace(sectionId, "[^.0-9]", "");
                        }

                        if (line == StateBeginToken)
                        {
                            stateData = new MesStateData();
                            stateData.Id = sectionId;
                            continue;
                        }

                        if (stateData == null) continue;

                        if (!ParseState(line, stateData, layersData))
                        {
                            if (stateData.IsValid())
                                allStatesData.Add(stateData);
                            else
                                Debug.LogError("State data is not valid");
                            stateData = null;
                        }
                    }
                }

                StoreStatesData(entity, allStatesData);
                Debug.Log("Entity updated");
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception while parsing controller");
                Debug.Log(ex.Message);
            }
        }

        private static bool ParseState(string line, MesStateData stateData, List<LayerData> layersData)
        {

            if (line.Contains(StateNameToken))
            {
                string nameData = ParseData(line, DataSeparatorToken);
                if (string.IsNullOrEmpty(nameData))
                {
                    Debug.LogError("State name is empty");
                    return false;
                }

                stateData.StateName = nameData;

                return true;
            }

            if (line.Contains(StateLayerToken))
            {
                string layerIdData = ParseData(line, DataSeparatorToken);
                layerIdData = Regex.Replace(layerIdData, "[^.0-9]", "");


                foreach (var layer in layersData)
                {
                    if (layer.SectionId == layerIdData)
                    {
                        stateData.LayerName = layer.Name;
                        stateData.IsDefaultState = stateData.Id == layer.DefaultStateName;
                        return true;
                    }
                }

                return false;
            }

            if (line.Contains(TagToken))
            {
                string tagData = ParseData(line, DataSeparatorToken);
                if (string.IsNullOrEmpty(tagData))
                    return true;                

                try
                {
                    int tag = Convert.ToInt32(tagData, 2);
                    stateData.Tag = tag;
                }
                catch (Exception ex)
                {
                    Debug.Log("Failed to parse tag. Please do not override state tag. " + ex.Message);
                }

                Debug.Log("Tag line: " + ParseData(line, DataSeparatorToken));

                return true;
            }

            if (line.Contains(SectionBeginsToken))
            {
                return false;
            }

            return true;
        }

        private static string ParseData(string line, string separator)
        {
            int tokenIdx = line.LastIndexOf(separator);

            if (tokenIdx == -1)
            {
                Debug.LogError("Failed to parse line <" + line + "> with token <" + separator + ">");
                return null;
            }

            int startingIdx = tokenIdx + separator.Length;

            return line.Substring(startingIdx, line.Length - startingIdx);
        }

        private static void StoreStatesData(MesEntity entity, List<MesStateData> statesData)
        {
            entity.StoreStates(statesData);
        }

        private static string GetControllerPath(RuntimeAnimatorController controller)
        {
            var projectPath = Application.dataPath;
            int assetsIdx = projectPath.IndexOf("Assets");

            projectPath = projectPath.Substring(0, assetsIdx);

            var controllerPath = AssetDatabase.GetAssetPath(controller);

            return (projectPath + controllerPath);
        }
    }
}