using System;
using System.Linq;
using System.Reflection;

namespace LtScience.APIClients
{
    internal static class InstalledMods
    {
        // Properties
        private static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

        internal static bool IsSnacksInstalled
        {
            get { return IsModInstalled("Snacks"); }
        }

        internal static bool IsTacInstalled
        {
            get { return IsModInstalled("TacLifeSupport"); }
        }

        internal static bool IsUsiInstalled
        {
            get { return IsModInstalled("USILifeSupport"); }
        }

        // Methods
        private static bool IsModInstalled(string assemblyName)
        {
            try
            {
                Assembly assembly = (from a in Assemblies where a.FullName.Split(',')[0] == assemblyName select a).First();
                return assembly != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
