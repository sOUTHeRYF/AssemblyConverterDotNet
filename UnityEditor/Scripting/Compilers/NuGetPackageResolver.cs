namespace UnityEditor.Scripting.Compilers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using UnityEditor;

    internal sealed class NuGetPackageResolver
    {
        public NuGetPackageResolver()
        {
            this.TargetMoniker = "UAP,Version=v10.0";
        }

        private string ConvertToWindowsPath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private string GetPackagesPath()
        {
            string packagesDirectory = this.PackagesDirectory;
            if (!string.IsNullOrEmpty(packagesDirectory))
            {
                return packagesDirectory;
            }
            packagesDirectory = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(packagesDirectory))
            {
                return packagesDirectory;
            }
            return Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), ".nuget"), "packages");
        }

        public void Resolve()
        {
            List<string> list = new List<string>();
            string str = this.ConvertToWindowsPath(this.GetPackagesPath());
            foreach (KeyValuePair<string, object> pair in (Dictionary<string, object>) ((Dictionary<string, object>) ((Dictionary<string, object>) Json.Deserialize(File.ReadAllText(this.ProjectLockFile)))["targets"])[this.TargetMoniker])
            {
                object obj2;
                Dictionary<string, object> dictionary = (Dictionary<string, object>) pair.Value;
                if (dictionary.TryGetValue("compile", out obj2))
                {
                    Dictionary<string, object> dictionary2 = (Dictionary<string, object>) obj2;
                    char[] separator = new char[] { '/' };
                    string[] textArray1 = pair.Key.Split(separator);
                    string str2 = textArray1[0];
                    string str3 = textArray1[1];
                    string path = Path.Combine(Path.Combine(str, str2), str3);
                    if (!Directory.Exists(path))
                    {
                        throw new Exception(string.Format("Package directory not found: \"{0}\".", path));
                    }
                    foreach (string str5 in dictionary2.Keys)
                    {
                        if (!string.Equals(Path.GetFileName(str5), "_._", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string str6 = Path.Combine(path, this.ConvertToWindowsPath(str5));
                            if (!File.Exists(str6))
                            {
                                throw new Exception(string.Format("Reference not found: \"{0}\".", str6));
                            }
                            list.Add(str6);
                        }
                    }
                    if (dictionary.ContainsKey("frameworkAssemblies"))
                    {
                        throw new NotImplementedException("Support for \"frameworkAssemblies\" property has not been implemented yet.");
                    }
                }
            }
            this.ResolvedReferences = list.ToArray();
        }

        public string PackagesDirectory { get; set; }

        public string ProjectLockFile { get; set; }

        public string[] ResolvedReferences { get; private set; }

        public string TargetMoniker { get; set; }
    }
}

