namespace Unity
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class OperationContext
    {
        private readonly Unity.MetadataContainer metadataContainer;
        private readonly List<ModuleContext> moduleContexts = new List<ModuleContext>();

        public OperationContext()
        {
            this.SkipMetadata = false;
            this.RemoveDebuggableAttribute = true;
            this.metadataContainer = new Unity.MetadataContainer(this);
        }

        public void AddModule(ModuleDefinition module)
        {
            ModuleContext item = new ModuleContext(this, module);
            this.moduleContexts.Add(item);
            if (item.IsUnityEngine)
            {
                this.UnityEngineModuleContext = item;
            }
        }

        public TypeDefinition GetCorLibType(string fullName)
        {
            return (this.CorLib.GetType(fullName) ?? this.CorLib.ExportedTypes.Single<ExportedType>(t => (t.FullName == fullName)).Resolve());
        }

        public FieldReference Import(ModuleDefinition module, FieldReference field)
        {
            return this.ModuleContexts.Single<ModuleContext>(c => (c.Module == module)).Import(field);
        }

        public MethodReference Import(ModuleDefinition module, MethodReference method)
        {
            return this.ModuleContexts.Single<ModuleContext>(c => (c.Module == module)).Import(method);
        }

        public bool IsSystemAssembly(string name)
        {
            using (List<ModuleContext>.Enumerator enumerator = this.moduleContexts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Module.Assembly.Name.Name == name)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SetIs64(bool is64)
        {
            this.Is64 = is64;
        }

        public void SetPaths(IEnumerable<string> paths)
        {
            this.AssemblyResolver.AddPaths(paths);
        }

        public void SetPlatform(Unity.Platform platform, string projectLockFile)
        {
            this.Platform = platform;
            if (platform == Unity.Platform.UAP)
            {
                this.AssemblyResolver = new UWPAssemblyResolver(projectLockFile);
            }
            else
            {
                this.AssemblyResolver = new PlatformAssemblyResolver(platform);
            }
            this.CorLib = this.AssemblyResolver.Resolve("mscorlib").MainModule;
            this.ArrayType = this.GetCorLibType("System.Array");
            this.Int32Type = this.GetCorLibType("System.Int32");
            this.IntPtrType = this.GetCorLibType("System.IntPtr");
            this.StringType = this.GetCorLibType("System.String");
            this.ObjectType = this.GetCorLibType("System.Object");
            this.TypeType = this.GetCorLibType("System.Type");
        }

        public void SetWinRTBridge(AssemblyDefinition assembly)
        {
            TypeDefinition type = assembly.MainModule.GetType("WinRTBridge", "GCHandledObjects");
            this.GCHandledObjectsObjectToGCHandleMethod = type.Methods.Single<MethodDefinition>(InnerClass.FuncA ?? (InnerClass.FuncA = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.SetWinRTBridgeA)));
            this.GCHandledObjectsObjectToGCHandleRetainMethod = type.Methods.Single<MethodDefinition>(InnerClass.FuncB ?? (InnerClass.FuncB = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.SetWinRTBridgeB)));
            this.GCHandledObjectsGCHandleToObjectMethod = type.Methods.Single<MethodDefinition>(InnerClass.FuncC ?? (InnerClass.FuncC = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.SetWinRTBridgeC)));
            this.GCHandledObjectsGCHandleToPinnedArrayObjectMethod = type.Methods.Single<MethodDefinition>(InnerClass.FuncD ?? (InnerClass.FuncD = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.SetWinRTBridgeD)));
            TypeDefinition definition2 = assembly.MainModule.GetType("WinRTBridge", "TypeInformation");
            this.TypeInformationGetTypeFromTypeIdMethod = definition2.Methods.Single<MethodDefinition>(InnerClass.FuncE ?? (InnerClass.FuncE = new Func<MethodDefinition, bool>(InnerClass.InnerInstance.SetWinRTBridgeE)));
            this.TypeInformationGetTypeFromTypeIdMethod.ReturnType = this.TypeType;
        }

        public TypeReference ArrayType { get; private set; }

        public IPlatformAssemblyResolver AssemblyResolver { get; private set; }

        public ModuleDefinition CorLib { get; private set; }

        public MethodDefinition GCHandledObjectsGCHandleToObjectMethod { get; private set; }

        public MethodDefinition GCHandledObjectsGCHandleToPinnedArrayObjectMethod { get; private set; }

        public MethodDefinition GCHandledObjectsObjectToGCHandleMethod { get; private set; }

        public MethodDefinition GCHandledObjectsObjectToGCHandleRetainMethod { get; private set; }

        public TypeReference Int32Type { get; private set; }

        public TypeReference IntPtrType { get; private set; }

        public bool Is64 { get; private set; }

        public bool IsWSA
        {
            get
            {
                if (((this.Platform != Unity.Platform.WSA80) && (this.Platform != Unity.Platform.WSA81)) && (this.Platform != Unity.Platform.WP81))
                {
                    return (this.Platform == Unity.Platform.UAP);
                }
                return true;
            }
        }

        public Unity.MetadataContainer MetadataContainer
        {
            get
            {
                return this.metadataContainer;
            }
        }

        public IReadOnlyList<ModuleContext> ModuleContexts
        {
            get
            {
                return this.moduleContexts;
            }
        }

        public TypeReference ObjectType { get; private set; }

        public Unity.Platform Platform { get; private set; }

        public bool RemoveDebuggableAttribute { get; set; }

        public bool SkipMetadata { get; set; }

        public TypeReference StringType { get; private set; }

        public MethodDefinition TypeInformationGetTypeFromTypeIdMethod { get; private set; }

        public TypeReference TypeType { get; private set; }

        public ModuleContext UnityEngineModuleContext { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class InnerClass
        {
            public static readonly OperationContext.InnerClass InnerInstance = new OperationContext.InnerClass();
            public static Func<MethodDefinition, bool> FuncA;
            public static Func<MethodDefinition, bool> FuncB;
            public static Func<MethodDefinition, bool> FuncC;
            public static Func<MethodDefinition, bool> FuncD;
            public static Func<MethodDefinition, bool> FuncE;

            internal bool SetWinRTBridgeA(MethodDefinition m)
            {
                return (m.Name == "ObjectToGCHandle");
            }

            internal bool SetWinRTBridgeB(MethodDefinition m)
            {
                return (m.Name == "ObjectToGCHandleRetain");
            }

            internal bool SetWinRTBridgeC(MethodDefinition m)
            {
                return (m.Name == "GCHandleToObject");
            }

            internal bool SetWinRTBridgeD(MethodDefinition m)
            {
                return (m.Name == "GCHandleToPinnedArrayObject");
            }

            internal bool SetWinRTBridgeE(MethodDefinition m)
            {
                return (m.Name == "GetTypeFromTypeId");
            }
        }
    }
}

