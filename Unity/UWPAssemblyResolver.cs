namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using UnityEditor.Scripting.Compilers;

    internal sealed class UWPAssemblyResolver : IPlatformAssemblyResolver, IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> _assemblies = new Dictionary<string, AssemblyDefinition>();
        private readonly List<string> _paths = new List<string>();
        private readonly Dictionary<string, string> _resolvedReferences;
        private readonly Dictionary<string, string> _winmdReferences = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public UWPAssemblyResolver(string projectLockFile)
        {
            if (string.IsNullOrEmpty(projectLockFile))
            {
                throw new Exception("Project lock file not specified.");
            }
            NuGetPackageResolver resolver1 = new NuGetPackageResolver {
                ProjectLockFile = projectLockFile
            };
            resolver1.Resolve();
            string[] resolvedReferences = resolver1.ResolvedReferences;
            this._resolvedReferences = new Dictionary<string, string>(resolvedReferences.Length, StringComparer.InvariantCultureIgnoreCase);
            foreach (string str in resolvedReferences)
            {
                string fileName = Path.GetFileName(str);
                if (this._resolvedReferences.ContainsKey(fileName))
                {
                    throw new Exception(string.Format("Reference \"{0}\" already added as \"{1}\".", str, this._resolvedReferences[fileName]));
                }
                this._resolvedReferences.Add(fileName, str);
            }
            foreach (string str3 in UWPReferences.GetReferences())
            {
                string key = Path.GetFileName(str3);
                if (this._winmdReferences.ContainsKey(key))
                {
                    throw new Exception(string.Format("Reference \"{0}\" already added as \"{1}\".", str3, this._winmdReferences[key]));
                }
                this._winmdReferences.Add(key, str3);
            }
        }

        public void AddAssembly(AssemblyDefinition assembly)
        {
            if (this._assemblies.ContainsKey(assembly.Name.Name))
            {
                throw new Exception(string.Format("Assembly \"{0}\" already added.", assembly.Name.Name));
            }
            this._assemblies.Add(assembly.Name.Name, assembly);
        }

        public void AddPaths(IEnumerable<string> paths)
        {
            this._paths.AddRange(paths);
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
            if (!this._assemblies.TryGetValue(name.Name, out assembly))
            {
                string fileName = name.Name + ".winmd";
                string str2 = this.ResolveAssemblyPath(this._winmdReferences, fileName);
                if (str2 == null)
                {
                    fileName = name.Name + ".dll";
                    str2 = this.ResolveAssemblyPath(this._resolvedReferences, fileName);
                }
                if (str2 == null)
                {
                    throw new FileNotFoundException(string.Format("Assembly \"{0}\" file not found.", name.FullName));
                }
                if (parameters.AssemblyResolver == null)
                {
                    parameters.AssemblyResolver = this;
                }
                assembly = ModuleDefinition.ReadModule(str2, parameters).Assembly;
                this._assemblies.Add(assembly.Name.Name, assembly);
            }
            return assembly;
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return this.Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }

        public string ResolveAssemblyPath(Dictionary<string, string> references, string fileName)
        {
            string str = null;
            if (!references.TryGetValue(fileName, out str))
            {
                List<string> list = new List<string>(this._paths.Count + 1);
                if (!string.IsNullOrEmpty(this.CurrentDirectory))
                {
                    list.Add(this.CurrentDirectory);
                }
                list.AddRange(this._paths);
                using (List<string>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string path = Path.Combine(enumerator.Current, fileName);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }
            return str;
        }

        public string CurrentDirectory { get; set; }
    }
}

