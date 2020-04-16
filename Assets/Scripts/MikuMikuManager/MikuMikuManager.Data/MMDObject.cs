using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MikuMikuManager.Services;

namespace MikuMikuManager.Data
{
    /// <summary>
    /// Represent a mmd object
    /// </summary>
    public class MMDObject
    {
        /// <summary>
        /// File Name
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Root path of this file
        /// </summary>
        public string RootPath { get; private set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public ReactiveProperty<string> FriendlyName { get; set; }

        /// <summary>
        /// Tags of this mmd file
        /// </summary>
        public ReactiveCollection<Tag> Tags { get; set; }

        /// <summary>
        /// The folder where be watched
        /// </summary>
        public string WatchedFolder { get; private set; }

        /// <summary>
        /// Is this mmd object being favored
        /// </summary>
        public ReactiveProperty<bool> IsFavored { get; set; }

        /// <summary>
        /// Constructor for the mmd objects
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="rootPath">Root path of this file</param>
        /// <param name="friendlyName">Friendly name</param>
        public MMDObject(string fileName, string rootPath, string watchedFolder, string friendlyName = "")
        {
            FileName = fileName;
            FriendlyName = new ReactiveProperty<string>(friendlyName);
            RootPath = rootPath;
            WatchedFolder = watchedFolder;
            Tags = new ReactiveCollection<Tag>();
            IsFavored = new ReactiveProperty<bool>(MMDObjectXML.LoadMMDObjectXML(this).IsFavored);

            IsFavored.ObserveEveryValueChanged(x => x.Value)
                .Subscribe(x => MMDObjectXML.SaveToXML(this));
        }
    }

    /// <summary>
    /// Tag for mmd object
    /// </summary>
    public class Tag
    {
        public string Name { get; private set; }
        public Color Color { get; private set; }

        /// <summary>
        /// Tag constructor
        /// </summary>
        /// <param name="name">Name of this tag</param>
        /// <param name="color">Color of this tag</param>
        public Tag(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }

    public enum TagType { None, Artist, NotUsed, Copyright, Character, Circle, Faults }

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
                default: throw new System.Exception("Get Color Exception");
            }
        }
    }
}