namespace Unity
{
    using System;

    [Flags]
    internal enum TypeFlag
    {
        Abstract = 8,
        GenericDefinition = 0x10,
        GenericInstance = 0x20,
        HasSharedBetweenAnimatorsAttribute = 2,
        Interface = 4,
        None = 0,
        SealedOnCppSide = 1
    }
}

