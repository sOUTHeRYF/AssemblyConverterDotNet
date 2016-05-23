namespace Unity
{
    using Microsoft.Win32;
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class PlatformAssemblyResolver : IPlatformAssemblyResolver, IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> assemblies;
        private readonly List<string> frameworkPaths;
        private readonly string platformPath;

        public PlatformAssemblyResolver(Platform platform)
        {
            string str;
            this.assemblies = new Dictionary<string, AssemblyDefinition>();
            this.frameworkPaths = new List<string>();
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            switch (platform)
            {
                case Platform.DotNet45:
                    this.frameworkPaths.Add(Path.Combine(folderPath, @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"));
                    str = WSA_GetPlatformPath(Platform.WSA80);
                    break;

                case Platform.UAP:
                    this.frameworkPaths.Add(Path.Combine(folderPath, @"Reference Assemblies\Microsoft\Framework\.NETCore\v4.5.1"));
                    str = UAP_GetPlatformPath();
                    break;

                case Platform.WP80:
                {
                    this.frameworkPaths.Add(WP80_GetFrameworkPath());
                    string path = WP80_GetLibrariesPath();
                    if (Directory.Exists(path))
                    {
                        this.frameworkPaths.Add(path);
                    }
                    string str4 = WP80_GetAdSDKPath();
                    if (Directory.Exists(str4))
                    {
                        this.frameworkPaths.Add(str4);
                    }
                    str = WP80_GetPlatformPath();
                    break;
                }
                case Platform.WP81:
                    this.frameworkPaths.Add(WP81_GetFrameworkPath());
                    str = WP81_GetPlatformPath();
                    break;

                case Platform.WSA80:
                    this.frameworkPaths.Add(Path.Combine(folderPath, @"Reference Assemblies\Microsoft\Framework\.NETCore\v4.5"));
                    str = WSA_GetPlatformPath(platform);
                    break;

                case Platform.WSA81:
                    this.frameworkPaths.Add(Path.Combine(folderPath, @"Reference Assemblies\Microsoft\Framework\.NETCore\v4.5.1"));
                    str = WSA_GetPlatformPath(platform);
                    break;

                default:
                    throw new Exception(string.Format("Unknown platform {0}.", platform));
            }
            if (!Directory.Exists(this.frameworkPaths[0]))
            {
                throw new Exception(string.Format("Framework not found at \"{0}\".", this.frameworkPaths[0]));
            }
            if (!string.IsNullOrEmpty(str) && !File.Exists(str))
            {
                throw new FileNotFoundException(string.Format("Windows.winmd not found at \"{0}\".", Path.GetDirectoryName(str)));
            }
            this.platformPath = str;
        }

        public void AddAssembly(AssemblyDefinition assembly)
        {
            if (this.assemblies.ContainsKey(assembly.Name.Name))
            {
                throw new Exception(string.Format("Assembly \"{0}\" already added.", assembly.Name.Name));
            }
            this.assemblies.Add(assembly.Name.Name, assembly);
        }

        public void AddPaths(IEnumerable<string> paths)
        {
            this.frameworkPaths.AddRange(paths);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return this.Resolve(name, new ReaderParameters());
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return this.Resolve(fullName, new ReaderParameters());
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition assembly;
            if (this.assemblies.TryGetValue(name.Name, out assembly))
            {
                return assembly;
            }
            List<string> list = new List<string>(this.frameworkPaths.Count + 1);
            if (!string.IsNullOrEmpty(this.CurrentDirectory))
            {
                list.Add(this.CurrentDirectory);
            }
            list.AddRange(this.frameworkPaths);
            foreach (string str in list)
            {
                string[] textArray1 = new string[] { ".winmd", ".dll" };
                foreach (string str2 in textArray1)
                {
                    string path = Path.Combine(str, name.Name + str2);
                    if ((name.IsWindowsRuntime && (name.Name == "Windows")) && !string.IsNullOrEmpty(this.platformPath))
                    {
                        path = this.platformPath;
                    }
                    if (File.Exists(path))
                    {
                        if (parameters.AssemblyResolver == null)
                        {
                            parameters.AssemblyResolver = this;
                        }
                        assembly = ModuleDefinition.ReadModule(path, parameters).Assembly;
                        this.assemblies.Add(assembly.Name.Name, assembly);
                        return assembly;
                    }
                }
            }
            throw new FileNotFoundException(string.Format("Assembly \"{0}\" file not found.", name.FullName));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return this.Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }

        private static string UAP_GetPlatformPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue("InstallationFolder");
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\10");
            }
            return Path.Combine(str, @"UnionMetadata\Facade\Windows.winmd");
        }

        private static string WP80_GetAdSDKPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhone\v8.0\AssemblyFoldersEx\WPAdSDK"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue(null);
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft SDKs\Windows Phone\v8.0\ExtensionSDKs\MSAdvertising\6.1\References\CommonConfiguration\neutral");
            }
            return str;
        }

        private static string WP80_GetFrameworkPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhone\v8.0\Framework Reference Assemblies"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue(null);
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\WindowsPhone\v8.0");
            }
            return str;
        }

        private static string WP80_GetLibrariesPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhone\v8.0\AssemblyFoldersEx\Windows Phone"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue(null);
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft SDKs\Windows Phone\v8.0\Libraries");
            }
            return str;
        }

        private static string WP80_GetPlatformPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhone\v8.0"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue("InstallationFolder");
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Phone Kits\8.0");
            }
            return Path.Combine(str, @"Windows MetaData\Windows.winmd");
        }

        private static string WP81_GetFrameworkPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhoneApp\v8.1\Framework Reference Assemblies"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue(null);
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework\WindowsPhoneApp\v8.1");
            }
            return str;
        }

        private static string WP81_GetPlatformPath()
        {
            string str = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\WindowsPhoneApp\v8.1"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue("InstallationFolder");
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Phone Kits\8.1");
            }
            return Path.Combine(str, @"References\CommonConfiguration\Neutral\Windows.winmd");
        }

        private static string WSA_GetPlatformPath(Platform platform)
        {
            string str = null;
            string str2;
            if (platform != Platform.WSA80)
            {
                if (platform != Platform.WSA81)
                {
                    throw new NotSupportedException(string.Format("Platform {0} is not supported.", platform));
                }
            }
            else
            {
                str2 = "8.0";
                goto Label_0032;
            }
            str2 = "8.1";
        Label_0032:
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v" + str2))
            {
                if (key != null)
                {
                    str = (string) key.GetValue("InstallationFolder");
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Windows Kits\" + str2);
            }
            return Path.Combine(str, @"References\CommonConfiguration\Neutral\Windows.winmd");
        }

        public string CurrentDirectory { get; set; }
    }
}

