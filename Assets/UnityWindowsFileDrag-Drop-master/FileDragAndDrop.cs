using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;
using MikuMikuManager.Data;
using MikuMikuManager.Services;


public class FileDragAndDrop : MonoBehaviour
{
    void OnEnable ()
    {
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (var s in aFiles.Where(x => x.EndsWith(".pmx", StringComparison.OrdinalIgnoreCase)))
        {
            var mmdObject =new MMDObject(s, s.Remove(s.LastIndexOf("\\")),string.Empty);
            var builder = GameObject.Find("MMDRenderer")
                .GetComponent<PreviewBuilder.PreviewBuilder>();

            MMMServices.Instance.SpecifiedMmdObjects.Add(mmdObject);

            builder.StartRender(mmdObject);
        }
    }
}
