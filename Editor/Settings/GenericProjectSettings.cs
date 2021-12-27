using System.IO;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    /// <summary>
    ///     Helper base class to handle IO between any Unity-serializable class and the ProjectSettings folder.
    ///     Uses Unity <see cref="JsonUtility"/> with the class instance. A subclass of <see cref="ScriptableObject"/>
    ///     is not really needed but may increases compatibility with something like inspector editor code.
    /// </summary>
    /// <remarks>
    ///     I saw a public <c>UnityEditor.ScriptableSingleton</c> with almost identical purpose
    ///     except that <c>FilePathAttribute</c> required for it is <c>internal</c>. WHY??
    /// </remarks>
    internal abstract class GenericProjectSettings<T> where T : GenericProjectSettings<T>, new()
    {
        private static T cachedSettings;

        private static T cachedDummy;

        /// <summary>
        ///     Point this to a constant number. It allows you to deprecate the old serialized file and start over.
        ///     If file loaded has <see cref="SerializedVersion"/> lower than this, the loaded instance will be
        ///     a new instance instead.
        /// </summary>
        protected abstract int DataVersion { get; }

        /// <summary>
        ///     Point this to a field that is serialized. When loading back this can be used to compare with
        ///     <see cref="DataVersion"/> if we should trust the serialized file or not.
        /// </summary>
        protected abstract int SerializedVersion { get; set; }

        /// <summary>
        ///     Overridable file name to use for the settings file.
        /// </summary>
        protected virtual string FileName => typeof(T).Name;

        private string settingsPath => "ProjectSettings/" + FileName + ".asset";

        private static T dummy
        {
            get
            {
                if (cachedDummy == null)
                {
                    cachedDummy = new T();
                }

                return cachedDummy;
            }
        }

        /// <summary>
        ///     A singleton of the saved settings file. Can create a new one automatically or return
        ///     a cached instance in the case of repeated use.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (cachedSettings != null && cachedSettings.SerializedVersion >= dummy.DataVersion)
                {
                    return cachedSettings;
                }

                var settings = new T();

                if (File.Exists(dummy.settingsPath))
                {
                    var assetJson = File.ReadAllText(dummy.settingsPath);
                    JsonUtility.FromJsonOverwrite(assetJson, settings);
                    if (settings.SerializedVersion < dummy.DataVersion)
                    {
                        settings = new T();
                    }
                }

                cachedSettings = settings;
                return settings;
            }
        }

        /// <summary>
        ///     Return the settings to the initial state and save to disk.
        /// </summary>
        public void Reset()
        {
            cachedSettings = new T();
            Save();
        }

        /// <summary>
        ///     Commit all changes of this instance to disk.
        /// </summary>
        public void Save()
        {
            if (cachedSettings != null)
            {
                SerializedVersion = dummy.DataVersion;
                File.WriteAllText(settingsPath, JsonUtility.ToJson(cachedSettings, true));
            }
        }
    }
}