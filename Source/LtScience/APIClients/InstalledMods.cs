/*
 * L-Tech Scientific Industries Continued
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * Copyright © 2016, linuxgurugamer
 * 
 * Kerbal Space Program is Copyright © 2011-2016 Squad. See http://kerbalspaceprogram.com/.
 * This project is in no way associated with nor endorsed by Squad.
 * 
 * This file is part of Olympic1's L-Tech (Continued). Original author of L-Tech is 'ludsoe' on the KSP Forums.
 * This file was not part of the original L-Tech but was written by Arne Peirs.
 * Copyright © 2015-2016, Arne Peirs (Olympic1)
 * 
 * Continues to be licensed under the MIT License.
 * See <https://opensource.org/licenses/MIT> for full details.
 */

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
            get
            {
                return IsModInstalled("Snacks");
            }
        }

        internal static bool IsTacInstalled
        {
            get
            {
                return IsModInstalled("TacLifeSupport");
            }
        }

        internal static bool IsUsiInstalled
        {
            get
            {
                return IsModInstalled("USILifeSupport");
            }
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
