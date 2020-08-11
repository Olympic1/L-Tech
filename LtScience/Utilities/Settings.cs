/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * Copyright © 2016-2018, Jonathan Bayer (linuxgurugamer)
 * 
 * Kerbal Space Program is Copyright © 2011-2018 Squad. See https://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2018, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

using System.IO;
using System.Reflection;
using LtScience.Windows;
using UnityEngine;

namespace LtScience.Utilities
{
    internal static class Settings
    {
        #region Properties

        internal static bool loaded;

        private static ConfigNode settings;

        private static readonly string settingsPath = $"{KSPUtil.ApplicationRootPath}GameData/LTech/PluginData";
        private static readonly string settingsFile = $"{settingsPath}/Settings.cfg";

        // This value is assigned from AssemblyInfo.cs
        internal static readonly string curVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Settings
        internal static float resolution = 1f;
        internal const float minResolution = 0.5f;
        internal const float maxResolution = 3f;

        internal static float shuttertime = 0.2f;
        internal const float minShuttertime = 0.2f;
        internal const float maxShuttertime = 3f;

        internal static bool hideUiOnScreenshot;

#endregion

#region Methods

        private static ConfigNode LoadSettingsFile()
        {
            return settings ?? (settings = ConfigNode.Load(settingsFile) ?? new ConfigNode());
        }

        internal static void LoadSettings()
        {
            if (settings == null)
                LoadSettingsFile();

            if (settings != null)
            {
                ConfigNode windowsNode = settings.HasNode("LT_Windows") ? settings.GetNode("LT_Windows") : settings.AddNode("LT_Windows");
                ConfigNode settingsNode = settings.HasNode("LT_Settings") ? settings.GetNode("LT_Settings") : settings.AddNode("LT_Settings");

                // Load window positions
                WindowSettings.position = GetRectangle(windowsNode, "SettingsPosition", WindowSettings.position);
                //WindowSkylab.position = GetRectangle(windowsNode, "SkylabPosition", WindowSkylab.position);

                // Load settings
                resolution = settingsNode.HasValue("Resolution") ? float.Parse(settingsNode.GetValue("Resolution")) : resolution;
                shuttertime = settingsNode.HasValue("Shuttertime") ? float.Parse(settingsNode.GetValue("Shuttertime")) : shuttertime;

                hideUiOnScreenshot = settingsNode.HasValue("HideUIOnScreenshot") ? bool.Parse(settingsNode.GetValue("HideUIOnScreenshot")) : hideUiOnScreenshot;

                // Set the loaded flag
                loaded = true;
            }

            // Force styles to refresh/load
            Style.WindowStyle = null;

            // Lets make sure that the windows can be seen on the screen
            Addon.RepositionWindows();
        }

        internal static void SaveSettings()
        {
            if (loaded && (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsFlight))
            {
                if (settings == null)
                    settings = LoadSettingsFile();

                ConfigNode windowsNode = settings.HasNode("LT_Windows") ? settings.GetNode("LT_Windows") : settings.AddNode("LT_Windows");
                ConfigNode settingsNode = settings.HasNode("LT_Settings") ? settings.GetNode("LT_Settings") : settings.AddNode("LT_Settings");

                // Save window positions
                WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.position);
                //WriteRectangle(windowsNode, "SkylabPosition", WindowSkylab.position);

                // Save settings
                WriteValue(settingsNode, "Resolution", $"{resolution:0}");
                WriteValue(settingsNode, "Shuttertime", $"{shuttertime:0.#}");

                WriteValue(settingsNode, "HideUIOnScreenshot", hideUiOnScreenshot);

                if (!Directory.Exists(settingsPath))
                    Directory.CreateDirectory(settingsPath);

                settings.Save(settingsFile);
            }
        }

        internal static void ResetSettings()
        {
            resolution = 1f;
            shuttertime = 0.2f;
            hideUiOnScreenshot = false;
            SaveSettings();
        }

        private static Rect GetRectangle(ConfigNode node, string name, Rect value)
        {
            Rect thisRect = new Rect();
            try
            {
                ConfigNode rectNode = node.HasNode(name) ? node.GetNode(name) : node.AddNode(name);
                thisRect.x = rectNode.HasValue("x") ? int.Parse(rectNode.GetValue("x")) : value.x;
                thisRect.y = rectNode.HasValue("y") ? int.Parse(rectNode.GetValue("y")) : value.y;
                thisRect.width = rectNode.HasValue("width") ? int.Parse(rectNode.GetValue("width")) : value.width;
                thisRect.height = rectNode.HasValue("height") ? int.Parse(rectNode.GetValue("height")) : value.height;
            }
            catch
            {
                thisRect = value;
            }

            return thisRect;
        }

        private static void WriteRectangle(ConfigNode node, string name, Rect value)
        {
            ConfigNode rectNode = node.HasNode(name) ? node.GetNode(name) : node.AddNode(name);
            WriteValue(rectNode, "x", (int)value.x);
            WriteValue(rectNode, "y", (int)value.y);
            WriteValue(rectNode, "width", (int)value.width);
            WriteValue(rectNode, "height", (int)value.height);
        }

        private static void WriteValue(ConfigNode node, string name, object value)
        {
            if (node.HasValue(name))
                node.RemoveValue(name);

            node.AddValue(name, value.ToString());
        }

#endregion
    }
}
