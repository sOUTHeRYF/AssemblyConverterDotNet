namespace Unity
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal sealed class MetadataBuffer
    {
        private readonly MemoryStream stream = new MemoryStream();
        private readonly Dictionary<string, int> stringOffsets;
        private readonly MemoryStream stringStream;
        private readonly BinaryWriter writer;

        public MetadataBuffer()
        {
            this.writer = new BinaryWriter(this.stream);
            this.stringStream = new MemoryStream();
            this.stringOffsets = new Dictionary<string, int>();
        }

        private void AddAssembly32(AssemblyWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write32(0);
            this.AddString32(value.Name);
            this.Write32(0);
            int num = (value.Types != null) ? value.Types.Length : 0;
            this.Write32(num);
            this.Write32(0);
        }

        private void AddAssembly64(AssemblyWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write64(0L);
            this.AddString64(value.Name);
            this.Write64(0L);
            int num = (value.Types != null) ? value.Types.Length : 0;
            this.Write64((long) num);
            this.Write64(0L);
        }

        private void AddField32(FieldWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write32(0);
            this.Write32(0);
            this.AddString32(value.Name);
            this.Write32(0);
            this.Write32(value.Type.Offset);
        }

        private void AddField64(FieldWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write64(0L);
            this.Write64(0L);
            this.AddString64(value.Name);
            this.Write64(0L);
            this.Write64((long) value.Type.Offset);
        }

        private void AddMethod32(MethodWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write32(0);
            this.AddString32(value.Name);
            this.Write32(0);
            this.Write32((int) value.Flags);
            this.Write32(value.DeclaringType.Offset);
            this.Write32(value.ReturnType.Offset);
            int num = (value.Parameters != null) ? value.Parameters.Length : 0;
            this.Write32(num);
            this.Write32(0);
        }

        private void AddMethod64(MethodWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write64(0L);
            this.AddString64(value.Name);
            this.Write64(0L);
            this.Write32((int) value.Flags);
            this.Write32(0);
            this.Write64((long) value.DeclaringType.Offset);
            this.Write64((long) value.ReturnType.Offset);
            int num = (value.Parameters != null) ? value.Parameters.Length : 0;
            this.Write64((long) num);
            this.Write64(0L);
        }

        private int AddString(string value, int align)
        {
            int length;
            if (!this.stringOffsets.TryGetValue(value, out length))
            {
                length = (int) this.stringStream.Length;
                this.stringOffsets.Add(value, length);
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                this.stringStream.Write(bytes, 0, bytes.Length);
                int num2 = align - (bytes.Length & (align - 1));
                for (int i = 0; i < num2; i++)
                {
                    this.stringStream.WriteByte(0);
                }
            }
            return length;
        }

        private int AddString32(string value)
        {
            return this.AddString(value, 4);
        }

        private int AddString64(string value)
        {
            return this.AddString(value, 8);
        }

        private int AddStrings()
        {
            this.stringStream.WriteTo(this.stream);
            return this.CurrentOffset;
        }

        private void AddType32(TypeWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write32(0);
            this.Write32(value.Id);
            this.Write32(value.ClassId);
            this.Write32(value.Assembly.Offset);
            this.AddString32(value.Namespace);
            this.Write32(0);
            this.AddString32(value.Name);
            this.Write32(0);
            this.Write32(0);
            int num = (value.Interfaces != null) ? value.Interfaces.Length : 0;
            this.Write32(num);
            this.Write32(0);
            int num2 = (value.Methods != null) ? value.Methods.Length : 0;
            this.Write32(num2);
            this.Write32(0);
            int num3 = (value.Fields != null) ? value.Fields.Length : 0;
            this.Write32(num3);
            this.Write32(0);
            this.Write32((int) value.Flags);
            this.Write32(0);
        }

        private void AddType64(TypeWrapper value)
        {
            value.Offset = this.CurrentOffset;
            this.Write64(0L);
            this.Write32(value.Id);
            this.Write32(value.ClassId);
            this.Write64((long) value.Assembly.Offset);
            this.AddString64(value.Namespace);
            this.Write64(0L);
            this.AddString64(value.Name);
            this.Write64(0L);
            this.Write64(0L);
            int num = (value.Interfaces != null) ? value.Interfaces.Length : 0;
            this.Write64((long) num);
            this.Write64(0L);
            int num2 = (value.Methods != null) ? value.Methods.Length : 0;
            this.Write64((long) num2);
            this.Write64(0L);
            int num3 = (value.Fields != null) ? value.Fields.Length : 0;
            this.Write64((long) num3);
            this.Write64(0L);
            this.Write32((int) value.Flags);
            this.Write32(0);
            this.Write64(0L);
        }

        public void Initialize(AssemblyWrapper[] assemblies, bool is64)
        {
            if (is64)
            {
                this.InitializeGenerated64(assemblies);
            }
            else
            {
                this.InitializeGenerated32(assemblies);
            }
        }

        private void InitializeGenerated32(AssemblyWrapper[] assemblies)
        {
            if (assemblies == null)
            {
                this.Write32(0);
            }
            else
            {
                this.Write32(assemblies.Length);
                this.AddString32(string.Empty);
                foreach (AssemblyWrapper wrapper in assemblies)
                {
                    this.AddAssembly32(wrapper);
                }
                foreach (AssemblyWrapper wrapper2 in assemblies)
                {
                    if (wrapper2.Types != null)
                    {
                        this.Write32(wrapper2.Offset + 12, this.CurrentOffset);
                        foreach (TypeWrapper wrapper3 in wrapper2.Types)
                        {
                            this.AddType32(wrapper3);
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper4 in assemblies)
                {
                    if (wrapper4.Types != null)
                    {
                        foreach (TypeWrapper wrapper5 in wrapper4.Types)
                        {
                            if (wrapper5.BaseType != null)
                            {
                                this.Write32(wrapper5.Offset + 0x18, wrapper5.BaseType.Offset);
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper6 in assemblies)
                {
                    if (wrapper6.Types != null)
                    {
                        foreach (TypeWrapper wrapper7 in wrapper6.Types)
                        {
                            if (wrapper7.Interfaces != null)
                            {
                                this.Write32(wrapper7.Offset + 0x20, this.CurrentOffset);
                                foreach (TypeWrapper wrapper8 in wrapper7.Interfaces)
                                {
                                    this.Write32(wrapper8.Offset);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper9 in assemblies)
                {
                    if (wrapper9.Types != null)
                    {
                        foreach (TypeWrapper wrapper10 in wrapper9.Types)
                        {
                            if (wrapper10.Methods != null)
                            {
                                this.Write32(wrapper10.Offset + 40, this.CurrentOffset);
                                foreach (MethodWrapper wrapper11 in wrapper10.Methods)
                                {
                                    this.AddMethod32(wrapper11);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper12 in assemblies)
                {
                    if (wrapper12.Types != null)
                    {
                        foreach (TypeWrapper wrapper13 in wrapper12.Types)
                        {
                            if (wrapper13.Methods != null)
                            {
                                foreach (MethodWrapper wrapper14 in wrapper13.Methods)
                                {
                                    if (wrapper14.Parameters != null)
                                    {
                                        this.Write32(wrapper14.Offset + 0x18, this.CurrentOffset);
                                        foreach (TypeWrapper wrapper15 in wrapper14.Parameters)
                                        {
                                            this.Write32(wrapper15.Offset);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper16 in assemblies)
                {
                    if (wrapper16.Types != null)
                    {
                        foreach (TypeWrapper wrapper17 in wrapper16.Types)
                        {
                            if (wrapper17.Fields != null)
                            {
                                this.Write32(wrapper17.Offset + 0x30, this.CurrentOffset);
                                foreach (FieldWrapper wrapper18 in wrapper17.Fields)
                                {
                                    this.AddField32(wrapper18);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper19 in assemblies)
                {
                    if (wrapper19.Types != null)
                    {
                        foreach (TypeWrapper wrapper20 in wrapper19.Types)
                        {
                            if (wrapper20.DeclaringType != null)
                            {
                                this.Write32(wrapper20.Offset + 0x38, wrapper20.DeclaringType.Offset);
                            }
                        }
                    }
                }
                int stringsOffset = this.AddStrings();
                this.PatchStrings32(assemblies, stringsOffset);
            }
        }

        private void InitializeGenerated64(AssemblyWrapper[] assemblies)
        {
            if (assemblies == null)
            {
                this.Write64(0L);
            }
            else
            {
                this.Write64((long) assemblies.Length);
                this.AddString64(string.Empty);
                foreach (AssemblyWrapper wrapper in assemblies)
                {
                    this.AddAssembly64(wrapper);
                }
                foreach (AssemblyWrapper wrapper2 in assemblies)
                {
                    if (wrapper2.Types != null)
                    {
                        this.Write64(wrapper2.Offset + 0x18, (long) this.CurrentOffset);
                        foreach (TypeWrapper wrapper3 in wrapper2.Types)
                        {
                            this.AddType64(wrapper3);
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper4 in assemblies)
                {
                    if (wrapper4.Types != null)
                    {
                        foreach (TypeWrapper wrapper5 in wrapper4.Types)
                        {
                            if (wrapper5.BaseType != null)
                            {
                                this.Write64(wrapper5.Offset + 40, (long) wrapper5.BaseType.Offset);
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper6 in assemblies)
                {
                    if (wrapper6.Types != null)
                    {
                        foreach (TypeWrapper wrapper7 in wrapper6.Types)
                        {
                            if (wrapper7.Interfaces != null)
                            {
                                this.Write64(wrapper7.Offset + 0x38, (long) this.CurrentOffset);
                                foreach (TypeWrapper wrapper8 in wrapper7.Interfaces)
                                {
                                    this.Write64((long) wrapper8.Offset);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper9 in assemblies)
                {
                    if (wrapper9.Types != null)
                    {
                        foreach (TypeWrapper wrapper10 in wrapper9.Types)
                        {
                            if (wrapper10.Methods != null)
                            {
                                this.Write64(wrapper10.Offset + 0x48, (long) this.CurrentOffset);
                                foreach (MethodWrapper wrapper11 in wrapper10.Methods)
                                {
                                    this.AddMethod64(wrapper11);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper12 in assemblies)
                {
                    if (wrapper12.Types != null)
                    {
                        foreach (TypeWrapper wrapper13 in wrapper12.Types)
                        {
                            if (wrapper13.Methods != null)
                            {
                                foreach (MethodWrapper wrapper14 in wrapper13.Methods)
                                {
                                    if (wrapper14.Parameters != null)
                                    {
                                        this.Write64(wrapper14.Offset + 0x30, (long) this.CurrentOffset);
                                        foreach (TypeWrapper wrapper15 in wrapper14.Parameters)
                                        {
                                            this.Write64((long) wrapper15.Offset);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper16 in assemblies)
                {
                    if (wrapper16.Types != null)
                    {
                        foreach (TypeWrapper wrapper17 in wrapper16.Types)
                        {
                            if (wrapper17.Fields != null)
                            {
                                this.Write64(wrapper17.Offset + 0x58, (long) this.CurrentOffset);
                                foreach (FieldWrapper wrapper18 in wrapper17.Fields)
                                {
                                    this.AddField64(wrapper18);
                                }
                            }
                        }
                    }
                }
                foreach (AssemblyWrapper wrapper19 in assemblies)
                {
                    if (wrapper19.Types != null)
                    {
                        foreach (TypeWrapper wrapper20 in wrapper19.Types)
                        {
                            if (wrapper20.DeclaringType != null)
                            {
                                this.Write64(wrapper20.Offset + 0x68, (long) wrapper20.DeclaringType.Offset);
                            }
                        }
                    }
                }
                int stringsOffset = this.AddStrings();
                this.PatchStrings64(assemblies, stringsOffset);
            }
        }

        private void PatchStrings32(AssemblyWrapper[] assemblies, int stringsOffset)
        {
            foreach (AssemblyWrapper wrapper in assemblies)
            {
                this.Write32(wrapper.Offset + 4, stringsOffset + this.AddString32(wrapper.Name));
                if (wrapper.Types != null)
                {
                    foreach (TypeWrapper wrapper2 in wrapper.Types)
                    {
                        this.Write32(wrapper2.Offset + 0x10, stringsOffset + this.AddString32(wrapper2.Namespace));
                        this.Write32(wrapper2.Offset + 20, stringsOffset + this.AddString32(wrapper2.Name));
                        if (wrapper2.Methods != null)
                        {
                            foreach (MethodWrapper wrapper3 in wrapper2.Methods)
                            {
                                this.Write32(wrapper3.Offset + 4, stringsOffset + this.AddString32(wrapper3.Name));
                            }
                        }
                        if (wrapper2.Fields != null)
                        {
                            foreach (FieldWrapper wrapper4 in wrapper2.Fields)
                            {
                                this.Write32(wrapper4.Offset + 8, stringsOffset + this.AddString32(wrapper4.Name));
                            }
                        }
                    }
                }
            }
        }

        private void PatchStrings64(AssemblyWrapper[] assemblies, int stringsOffset)
        {
            foreach (AssemblyWrapper wrapper in assemblies)
            {
                this.Write64(wrapper.Offset + 8, (long) (stringsOffset + this.AddString64(wrapper.Name)));
                if (wrapper.Types != null)
                {
                    foreach (TypeWrapper wrapper2 in wrapper.Types)
                    {
                        this.Write64(wrapper2.Offset + 0x18, (long) (stringsOffset + this.AddString64(wrapper2.Namespace)));
                        this.Write64(wrapper2.Offset + 0x20, (long) (stringsOffset + this.AddString64(wrapper2.Name)));
                        if (wrapper2.Methods != null)
                        {
                            foreach (MethodWrapper wrapper3 in wrapper2.Methods)
                            {
                                this.Write64(wrapper3.Offset + 8, (long) (stringsOffset + this.AddString64(wrapper3.Name)));
                            }
                        }
                        if (wrapper2.Fields != null)
                        {
                            foreach (FieldWrapper wrapper4 in wrapper2.Fields)
                            {
                                this.Write64(wrapper4.Offset + 0x10, (long) (stringsOffset + this.AddString64(wrapper4.Name)));
                            }
                        }
                    }
                }
            }
        }

        private void Write32(int value)
        {
            this.writer.Write(value);
        }

        private void Write32(int offset, int value)
        {
            this.writer.Seek(offset, SeekOrigin.Begin);
            this.writer.Write(value);
            this.writer.Seek(0, SeekOrigin.End);
        }

        private void Write64(long value)
        {
            this.writer.Write(value);
        }

        private void Write64(int offset, long value)
        {
            this.writer.Seek(offset, SeekOrigin.Begin);
            this.writer.Write(value);
            this.writer.Seek(0, SeekOrigin.End);
        }

        private int CurrentOffset
        {
            get
            {
                return (int) this.stream.Length;
            }
        }

        public byte[] Data
        {
            get
            {
                return this.stream.ToArray();
            }
        }
    }
}

