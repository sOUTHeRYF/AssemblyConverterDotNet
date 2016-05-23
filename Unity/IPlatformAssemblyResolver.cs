namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;

    internal interface IPlatformAssemblyResolver : IAssemblyResolver
    {
        void AddAssembly(AssemblyDefinition assembly);
        void AddPaths(IEnumerable<string> paths);

        string CurrentDirectory { get; set; }
    }
}

