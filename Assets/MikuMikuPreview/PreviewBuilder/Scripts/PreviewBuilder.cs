using MikuMikuManager.Data;
using MikuMikuManager.Services;
using MMD;
using MMD.PMX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// For parsing models 
/// Load them to the scene
/// Get preview save to the target folder
/// </summary>
namespace PreviewBuilder
{
    [ExecuteInEditMode]
    public class PreviewBuilder : MonoBehaviour, IPreviewBuilder
    {
        #region Private Members

        /// <summary>
        /// Notify that render completed
        /// </summary>
        public Action OnRenderComplete { get; set; }

        /// <summary>
        /// Notify that start render
        /// </summary>
        public Action OnRenderStart { get; set; }

        private static GameObject _fbxGameObject;

        /// <summary>
        /// List of all the mmd objects loaded
        /// </summary>
        private List<MMDObject> MmdObjects { get; set; }

        /// <summary>
        /// Indicate if is in rendering now
        /// </summary>
        private bool IsRendering { get; set; } = false;

        /// <summary>
        /// Indicate if is in saving now
        /// </summary>
        private bool IsSaving { get; set; } = false;

        private MMDObject _currentMmdObject;

        #endregion

        #region Public Members

        public GameObject parent;
        public Camera rtCamera;

        public int renderWidth = 1000;
        public int renderHeight = 2000;

        #endregion

        private void Start()
        {
            MmdObjects = new List<MMDObject>();
        }

        private void FixedUpdate()
        {
            // Save rt ass♂ we can
            if (!IsRendering || IsSaving) return;
            if (MmdObjects.Count != 0)
            {
                //OnRenderStart();    //  Notify that start render
                System.Windows.Forms.Cursor.Current= System.Windows.Forms.Cursors.WaitCursor;
                try
                {
                    _currentMmdObject = MmdObjects.First();
                    var rt = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.sRGB)
                    {
                        name = $"{_currentMmdObject.FileName}",
                        depth = 0,
                        anisoLevel = 0,
                        dimension = TextureDimension.Tex2D,
                        antiAliasing = 8
                    };

                    rtCamera.targetTexture = rt;
                    IsSaving = true;

                    CreatePmx();
                    StartCoroutine(TakePhoto(0.5f, rt));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    rtCamera.targetTexture = null;
                }
            }
            else
            {
                IsRendering = false;
                rtCamera.targetTexture = null;
            }

            // Take photo~ Update save rt statement
            // Statement.CanSaveRT = true;
        }

        public void StartRender()
        {
            var objects = from obj in MMMServices.Instance.ObservedMMDObjects
                          where obj.PreviewPath.Value == string.Empty
                          select obj;
            MmdObjects.AddRange(objects);

            IsRendering = true;
        }

        public void StartRender(MMDObject mmdObject)
        {
            MmdObjects.Add(mmdObject);
            IsRendering = true;
        }

        /// <summary>
        /// Create pmx from pmx list
        /// </summary>
        private void CreatePmx()
        {
            var modelAgent = new ModelAgent(_currentMmdObject.FilePath);
            PMXFormat pmxFormat;
            try
            {
                //PMX読み込みを試みる
                pmxFormat = PMXLoaderScript.Import(modelAgent.file_path_);
            }
            catch
            {
                var pmdFormat = PMDLoaderScript.Import(modelAgent.file_path_);
                pmxFormat = PMXLoaderScript.PMD2PMX(pmdFormat);
            }

            _fbxGameObject = PMXConverter.CreateGameObject(pmxFormat, false, PMXConverter.AnimationType.LegacyAnimation,
                false, 1f);

            _fbxGameObject.transform.SetParent(parent.transform);
            _fbxGameObject.transform.localScale = new Vector3(0.085f, 0.085f, 0.085f);
            _fbxGameObject.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        }

        private IEnumerator TakePhoto(float waitTime, RenderTexture renderTexture)
        {
            yield return new WaitForSeconds(waitTime);
            //yield return new WaitForEndOfFrame();

            // Take photo and set can save rt false
            SaveRenderTextureToFile(_currentMmdObject.FilePath + ".png", renderTexture);

            Destroy(_fbxGameObject);
            MmdObjects.Remove(_currentMmdObject);
            if (MmdObjects.Count == 0)
            {
                OnRenderComplete();
            }

            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        #region Helper Methods

        /// <summary>
        ///     Save render texture to png
        /// </summary>
        /// <param name="filePath">The path to save</param>
        /// <param name="renderTexture">Render texture going to be stored</param>
        private void SaveRenderTextureToFile(string filePath, RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            var tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, true);

            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            var path = $"{_currentMmdObject.FilePath}.png";
            _currentMmdObject.PreviewPath.Value = path;


            IsSaving = false;
        }

        #endregion
    }
}