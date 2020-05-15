using MikuMikuManager.Data;
using System.Collections.Generic;

public interface IPreviewBuilder
{
    //  Methods:
    /// <summary>
    /// Start to render the mmd objects which doesn't have a preview image
    /// </summary>
    void StartRender();

    /// <summary>
    /// Start to render the specified mmd object
    /// </summary>
    void StartRender(MMDObject mmdObject);
}
