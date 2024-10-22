﻿using MikuMikuManager.Data;
using MikuMikuManager.Services;
using MMD;
using MMD.PMX;
using System;
using System.Collections;
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
        public ReactiveCollection<MMDObject> MmdObjects { get; set; }

        /// <summary>
        /// Indicate if is in rendering now
        /// </summary>
        public ReactiveProperty<bool> IsRendering { get; set; } = new ReactiveProperty<bool>(false);

        /// <summary>
        /// Indicate if is in saving now
        /// </summary>
        public ReactiveProperty<bool> IsSaving { get; set; } = new ReactiveProperty<bool>(false);

        public MMDObject CurrentMmdObject { get; private set; }

        #endregion

        #region Public Members
        public static GameObject container;
        /// <summary>
        /// Static instances
        /// </summary>
        public static PreviewBuilder Instance => container.GetComponent<PreviewBuilder>();

        public GameObject parent;
        public Camera rtCamera;

        public int renderWidth = 1000;
        public int renderHeight = 2000;

        #endregion

        private void Awake()
        {
            container = transform.gameObject;
        }

        private void Start()
        {
            MmdObjects = new ReactiveCollection<MMDObject>();
        }

        private void FixedUpdate()
        {
            // Save rt ass♂ we can
            if (!IsRendering.Value || IsSaving.Value) return;
            if (MmdObjects.Count != 0)
            {
                //OnRenderStart();    //  Notify that start render
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                try
                {
                    CurrentMmdObject = MmdObjects.First();
                    var rt = new RenderTexture(renderWidth, renderHeight, 0, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.sRGB)
                    {
                        name = $"{CurrentMmdObject.FileName}",
                        depth = 0,
                        anisoLevel = 0,
                        dimension = TextureDimension.Tex2D,
                        antiAliasing = 8
                    };

                    rtCamera.targetTexture = rt;
                    IsSaving.Value = true;

                    CreatePmx();
                    StartCoroutine(TakePhoto(rt));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    rtCamera.targetTexture = null;
                }
            }
            else
            {
                IsRendering.Value = false;
                rtCamera.targetTexture = null;
            }
        }

        public void StartRender()
        {
            MmdObjects = MMMServices.Instance.ObservedMMDObjects
                .Where(x => x.PreviewPath.Value == string.Empty)
                .ToReactiveCollection();

            IsRendering.Value = true;
        }

        public void StartRender(MMDObject mmdObject)
        {
            MmdObjects.Add(mmdObject);
            IsRendering.Value = true;
        }

        /// <summary>
        /// Create pmx from pmx list
        /// </summary>
        private void CreatePmx()
        {
            var modelAgent = new ModelAgent(CurrentMmdObject.FilePath);
            PMXFormat pmxFormat;
            try
            {
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

        private IEnumerator TakePhoto(RenderTexture renderTexture)
        {
            yield return Observable.TimerFrame(2).ToYieldInstruction();

            // Take photo and set can save rt false
            SaveRenderTextureToFile(CurrentMmdObject.FilePath + ".png", renderTexture);

            Destroy(_fbxGameObject);
            MmdObjects.Remove(CurrentMmdObject);
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

            var path = $"{CurrentMmdObject.FilePath}.png";
            CurrentMmdObject.PreviewPath.Value = path;


            IsSaving.Value = false;
        }

        #endregion
    }
}