using MikuMikuManager.Data;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace MikuMikuManager.Services
{
    /// <summary>
    /// Represents a pmx xml file
    /// </summary>
    public class MMDObjectXML
    {
        public bool IsFavored;

        public static void SaveToXML(MMDObject @object)
        {
            Debug.Log("SaveToXML");
            var path = $"{@object.FilePath}.xml";
            TextWriter writer = new StreamWriter(path);
            var serializer = new XmlSerializer(typeof(MMDObjectXML));
            serializer.Serialize(writer, new MMDObjectXML() { IsFavored = @object.IsFavored.Value });
            writer.Close();
        }

        public static MMDObjectXML LoadMMDObjectXML(MMDObject @object)
        {
            var path = $"{@object.FilePath}.xml";
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(MMDObjectXML));
                var reader = new StreamReader(path);
                var res = serializer.Deserialize(reader) as MMDObjectXML;
                reader.Close();
                return res;
            }
            else
            {
                return new MMDObjectXML() { IsFavored = false };
            }
        }
    }

    /// <summary>
    /// Represents a app setting xml file
    /// </summary>
    public class AppSettingsXML
    {
        [XmlArray("Folders")]
        public string[] WatchedFolders { get; set; }

        [XmlArray("SpecifiedMMDObject")]
        public string[] SpecifiedMmdObject { get; set; }

        public void SaveToXml()
        {
            var path = Application.temporaryCachePath;
            Debug.Log(path);
            TextWriter writer = new StreamWriter($"{path}/AppSettings.xml");
            var serializer = new XmlSerializer(typeof(AppSettingsXML));
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static AppSettingsXML LoadAppSettingsXml()
        {
            var path = $"{Application.temporaryCachePath}/AppSettings.xml";
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(AppSettingsXML));
                var reader = new StreamReader(path);
                var res = serializer.Deserialize(reader) as AppSettingsXML;
                reader.Close();
                return res;
            }
            else
            {
                var xml = new AppSettingsXML { WatchedFolders = new string[] { }, SpecifiedMmdObject = new string[] { } };
                xml.SaveToXml();
                return xml;
            }
        }
    }
}