using System;
using MMD;
using MMD.PMX;
using UnityEditor;
using UnityEngine;

namespace PreviewBuilder
{
    /// <summary>
    ///     Test class for preview builder
    /// </summary>
    public class Test : Editor
    {
        [MenuItem("MMM/LoadModelTemp")]
        [ExecuteInEditMode]
        public static void LoadT()
        {
            CleanAssetDatabase();
            var path = EditorUtility.OpenFilePanel("Overwrite with png", "", "*");
            Debug.Log(path);
            var model_agent = new ModelAgent(path);

            PMXFormat pmx_format;
            try
            {
                //PMX読み込みを試みる
                pmx_format = PMXLoaderScript.Import(model_agent.file_path_);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
                //PMXとして読み込めなかったら
                //PMDとして読み込む
                var pmd_format = PMDLoaderScript.Import(model_agent.file_path_);
                pmx_format = PMXLoaderScript.PMD2PMX(pmd_format);
            }

            var fbxGameObject = PMXConverter.CreateGameObject(pmx_format, false,
                PMXConverter.AnimationType.LegacyAnimation, false, 1f);

            fbxGameObject.transform.SetParent(GameObject.Find("Parent").transform);
            fbxGameObject.transform.localScale = new Vector3(0.085f, 0.085f, 0.085f);
            fbxGameObject.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        }

        [MenuItem("MMM/CleanAssetDatabase")]
        public static void CleanAssetDatabase()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        [MenuItem("MMM/Show xml path")]
        public static void ShowXml()
        {
            Debug.Log($"{Application.temporaryCachePath}/AppSettings.xml");
        }
    }
}