using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace OfflineWebsiteViewer
{
    class RegistryRecentProjectsPersister
    {
        private const string KeyName = "RECENT";

        public int MaxProjects { get; }

        public IEnumerable<string> RecentProjects => GetRecentProjects();

        public RegistryRecentProjectsPersister(int maxProjects = 10)
        {
            MaxProjects = maxProjects;
        }

        public void AddProject(string path)
        {
            var projects = GetRecentProjects();

            if (projects.Contains(path))
                projects.Remove(path);

            projects.Insert(0, path);

            if (projects.Count > MaxProjects)
                projects = projects.GetRange(0, MaxProjects);
            SaveToRegistry(projects.ToArray());
        }

        public void RemoveProject(string path)
        {
            var projects = GetRecentProjects();

            if (projects.Contains(path))
            {
                projects.Remove(path);
                SaveToRegistry(projects.ToArray());
            }
        }

        public void Clear()
        {
            var key = OpenRegistryKey();
            key.DeleteValue(KeyName);
            key.Close();
        }

        private static RegistryKey OpenRegistryKey()
        {
            var subkey = Registry.CurrentUser.OpenSubKey("Software", true);
            if (subkey == null)
                throw new Exception("No software key found in registry");

            var key = subkey.OpenSubKey(AppDomain.CurrentDomain.FriendlyName, true);

            // Create a new key if not exists
            if (key == null)
            {
                key = subkey.CreateSubKey(AppDomain.CurrentDomain.FriendlyName);

                if (key == null)
                    throw new Exception("Cannot create a registry key");
            }

            subkey.Close();
            return key;
        }

        private static List<string> GetRecentProjects()
        {
            var key = OpenRegistryKey();
            var values = key.GetValue(KeyName) as string[];
            key.Close();
            if (values != null)
                return new List<string>(values);
            return new List<string>();
        }

        private static void SaveToRegistry(string[] values)
        {
            var key = OpenRegistryKey();
            key.SetValue(KeyName, values, RegistryValueKind.MultiString);
            key.Close();
        }
    }
}
