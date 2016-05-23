namespace UnityEditor.Scripting.Compilers
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    internal static class UWPReferences
    {
        private static UWPExtension[] GetExtensions(string folder, string version)
        {
            string referencesFolder = Path.Combine(folder, "References");
            List<UWPExtension> list = new List<UWPExtension>();
            string[] directories = Directory.GetDirectories(Path.Combine(folder, "Extension SDKs"));
            for (int i = 0; i < directories.Length; i++)
            {
                string path = Path.Combine(directories[i], version, "SDKManifest.xml");
                if (File.Exists(path))
                {
                    try
                    {
                        UWPExtension item = new UWPExtension(path, referencesFolder);
                        list.Add(item);
                    }
                    catch
                    {
                    }
                }
            }
            return list.ToArray();
        }

        private static string[] GetPlatform(string folder, string version)
        {
            string uri = Path.Combine(folder, @"Platforms\UAP", version, "Platform.xml");
            XElement element1 = XDocument.Load(uri).Element("ApplicationPlatform");
            if (element1.Attribute("name").Value != "UAP")
            {
                throw new Exception(string.Format("Invalid platform manifest at \"{0}\".", uri));
            }
            return GetReferences(Path.Combine(folder, "References"), element1.Element("ContainedApiContracts"));
        }

        public static string[] GetReferences()
        {
            string str;
            Version version;
            int num;
            GetWindowsKit10(out str, out version);
            string str2 = version.ToString();
            if (version.Minor == -1)
            {
                str2 = str2 + ".0";
            }
            if (version.Build == -1)
            {
                str2 = str2 + ".0";
            }
            if (version.Revision == -1)
            {
                str2 = str2 + ".0";
            }
            HashSet<string> source = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            string item = Path.Combine(str, @"UnionMetadata\Facade\Windows.winmd");
            source.Add(item);
            string[] platform = GetPlatform(str, str2);
            for (num = 0; num < platform.Length; num++)
            {
                string str4 = platform[num];
                source.Add(str4);
            }
            UWPExtension[] extensions = GetExtensions(str, str2);
            for (num = 0; num < extensions.Length; num++)
            {
                foreach (string str5 in extensions[num].References)
                {
                    source.Add(str5);
                }
            }
            return source.ToArray<string>();
        }

        private static string[] GetReferences(string referencesFolder, XElement containedApiContractsElement)
        {
            List<string> list = new List<string>();
            foreach (XElement local1 in containedApiContractsElement.Elements("ApiContract"))
            {
                string str = local1.Attribute("name").Value;
                string str2 = local1.Attribute("version").Value;
                string path = Path.Combine(referencesFolder, str, str2, str + ".winmd");
                if (File.Exists(path))
                {
                    list.Add(path);
                }
            }
            return list.ToArray();
        }

        private static void GetWindowsKit10(out string folder, out Version version)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            folder = Path.Combine(folderPath, @"Windows Kits\10\");
            version = new Version(10, 0, 0x2800);
            try
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key2 = key.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0"))
                    {
                        if (key2 != null)
                        {
                            folder = (string) key2.GetValue("InstallationFolder", folder);
                            string input = (string) key2.GetValue("ProductVersion", version.ToString());
                            version = Version.Parse(input);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private sealed class UWPExtension
        {
            public UWPExtension(string manifest, string referencesFolder)
            {
                XElement element = XDocument.Load(manifest).Element("FileList");
                if (element.Attribute("TargetPlatform").Value != "UAP")
                {
                    throw new Exception(string.Format("Invalid extension manifest at \"{0}\".", manifest));
                }
                this.Name = element.Attribute("DisplayName").Value;
                XElement containedApiContractsElement = element.Element("ContainedApiContracts");
                this.References = UWPReferences.GetReferences(referencesFolder, containedApiContractsElement);
            }

            public string Name { get; private set; }

            public string[] References { get; private set; }
        }
    }
}

