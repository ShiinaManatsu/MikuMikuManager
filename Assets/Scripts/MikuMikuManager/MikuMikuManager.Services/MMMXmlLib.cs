using MikuMikuManager.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace MikuMikuManager.Services
{
    public class MMDObjectXML
    {
        public bool IsFavored;

        public static void SaveToXML(MMDObject @object)
        {
            var path = $"{@object.FileName}.xml";
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
                var res= serializer.Deserialize(reader) as MMDObjectXML;
                reader.Close();
                return res;
            }
            else
            {
                return new MMDObjectXML() { IsFavored = false };
            }
        }
    }

    public class AppSettingsXML
    {
        [XmlArray("Folders")]
        public string[] WatchedFolders;

        public void SaveToXML()
        {
            var path = Application.temporaryCachePath;
            Debug.Log(path);
            TextWriter writer = new StreamWriter($"{path}/AppSettings.xml");
            var serializer = new XmlSerializer(typeof(AppSettingsXML));
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static AppSettingsXML LoadAppSettingsXML()
        {
            var path = $"{Application.temporaryCachePath}/AppSettings.xml";
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(AppSettingsXML));
                var reader = new StreamReader(path);
                var res= serializer.Deserialize(reader) as AppSettingsXML;
                reader.Close();
                return res;
            }
            else
            {
                var xml = new AppSettingsXML() { WatchedFolders = new string[] { } };
                xml.SaveToXML();
                return xml;
            }
        }
    }
}