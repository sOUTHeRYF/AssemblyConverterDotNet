namespace Unity
{
    using System;

    [Flags]
    internal enum MethodFlag
    {
        HasImageEffectOpaqueAttribute = 2,
        HasImageEffectTransformsToLDRAttribute = 4,
        None = 0,
        Static = 1
    }
}

