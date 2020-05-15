using System;
using System.IO;
using MikuMikuManager.Services;
using UniRx;
using UnityEngine;

namespace MikuMikuManager.Data
{
    /// <summary>
    ///     Represent a mmd object
    /// </summary>
    public class MMDObject
    {
        /// <summary>
        /// Constructor for the mmd objects 
        /// </summary>
        /// <param name="filePath">File name</param>
        /// <param name="rootPath">Root path of this file</param>
        /// <param name="watchedFolder">Folder by watching</param>
        /// <param name="friendlyName">Friendly name</param>
        public MMDObject(string filePath, string rootPath, string watchedFolder, string friendlyName = "")
        {
            FilePath = filePath;
            IsFavored = new ReactiveProperty<bool>(false);
            var path = $"{filePath}.png";
            PreviewPath =new ReactiveProperty<string>(File.Exists(path) ? path : string.Empty);
            FileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            FriendlyName = new ReactiveProperty<string>(friendlyName);
            RootPath = rootPath;
            WatchedFolder = watchedFolder;
            Tags = new ReactiveCollection<Tag>();
            IsFavored.Value = MMDObjectXML.LoadMMDObjectXML(this).IsFavored;

            IsFavored
                .Skip(1)
                .DistinctUntilChanged()
                .Subscribe(x => MMDObjectXML.SaveToXML(this));
        }

        /// <summary>
        ///     Full file path
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     File Name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Root path of this file
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        ///     Path of preview image
        ///     Return string.empty if there's no preview image
        /// </summary>
        public ReactiveProperty<string> PreviewPath { get; set; }

        /// <summary>
        ///     Friendly name
        /// </summary>
        public ReactiveProperty<string> FriendlyName { get; set; }

        /// <summary>
        ///     Tags of this mmd file
        /// </summary>
        public ReactiveCollection<Tag> Tags { get; set; }

        /// <summary>
        ///     The folder where be watched
        /// </summary>
        public string WatchedFolder { get; }

        /// <summary>
        ///     Is this mmd object being favored
        /// </summary>
        public ReactiveProperty<bool> IsFavored { get; set; }
    }

    /// <summary>
    ///     Tag for mmd object
    /// </summary>
    public class Tag
    {
        /// <summary>
        ///     Tag constructor
        /// </summary>
        /// <param name="name">Name of this tag</param>
        /// <param name="color">Color of this tag</param>
        public Tag(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public string Name { get; }
        public Color Color { get; }
    }

    public enum TagType
    {
        None,
        Artist,
        NotUsed,
        Copyright,
        Character,
        Circle,
        Faults
    }

    public static class TagExtensions
    {
        public static Color GetColor(this TagType tagType)
        {
            switch (tagType)
            {
                case TagType.None: return new Color(118, 118, 118, 255);
                case TagType.Artist: return new Color(202, 80, 16, 255);
                case TagType.Character: return new Color(16, 137, 62, 255);
                case TagType.Copyright: return new Color(194, 57, 179, 255);
                case TagType.Circle: return new Color(45, 125, 154, 255);
                case TagType.Faults: return new Color(232, 17, 35, 255);
                case TagType.NotUsed: return new Color(118, 118, 118, 255);
                default: throw new Exception("Get Color Exception");
            }
        }
    }
}