using System.IO;
using System.Xml.Serialization;

namespace PreviewBuilder.Xmls
{
    /// <summary>
    /// Definition of mmd fbx xml
    /// </summary>
    [XmlRoot("MMDModel", Namespace = "", IsNullable = false)]
    public class MMDModel
    {
        [XmlElement("textureList")] public TextureList TextureList { get; set; }

        [XmlElement("materialList")] public MaterialList MaterialList { get; set; }


        /// <summary>
        /// Deserialize a xml to mmd model
        /// </summary>
        /// <param name="filename">The location of xml file</param>
        /// <returns></returns>
        public static MMDModel GetMMDModel(string filename)
        {
            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MMDModel));
                // Call the Deserialize method to restore the object's state.
                return (MMDModel) serializer.Deserialize(reader) as MMDModel;
            }
        }
    }

    public class TextureList
    {
        [XmlElement("Texture")] public Texture[] Texture { get; set; }
    }

    public class MaterialList
    {
        [XmlElement("Material")] public Material[] Material { get; set; }
    }

    public class Texture
    {
        [XmlElement("fileName")] public string FileName { get; set; }
    }

    public class Material
    {
        [XmlElement("materialName")] public string MaterialName { get; set; }

        [XmlElement("textureID")] public int TextureID { get; set; }
    }
}