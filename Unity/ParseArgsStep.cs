namespace Unity
{
    using System;
    using System.Collections.Generic;

    internal sealed class ParseArgsStep : Step
    {
        private readonly string[] args;

        public ParseArgsStep(string[] args)
        {
            this.args = args;
        }

        protected override IStepContext Execute()
        {
            StepContext context = new StepContext();
            Platform unknown = Platform.Unknown;
            string projectLockFile = null;
            List<string> paths = new List<string>();
            bool flag = true;
            foreach (string str2 in this.args)
            {
                string str4;
                string str5;
                if (str2[0] != '-')
                {
                    context.AddFileName(str2);
                }
                else
                {
                    string str3 = str2.Substring(1);
                    char[] separator = new char[] { '=' };
                    string[] strArray2 = str3.Split(separator, 2);
                    str4 = null;
                    if (strArray2.Length != 1)
                    {
                        str3 = strArray2[0];
                        str4 = strArray2[1];
                    }
                    str5 = str3.ToLowerInvariant();
                    if (!(str5 == "path"))
                    {
                        if (str5 != "platform")
                        {
                            if (str5 == "lock")
                            {
                                goto Label_01A2;
                            }
                            if (str5 == "bits")
                            {
                                goto Label_01BE;
                            }
                            if (str5 == "metadata")
                            {
                                goto Label_022D;
                            }
                            if (str5 != "removedebuggableattribute")
                            {
                                throw new Exception(string.Format("Unknown command \"{0}\".", str2));
                            }
                            goto Label_02C2;
                        }
                    }
                    else
                    {
                        paths.Add(str4);
                        continue;
                    }
                    if (string.IsNullOrEmpty(str4))
                    {
                        throw new Exception("Platform not specified.");
                    }
                    str5 = str4.ToLowerInvariant();
                    if (!(str5 == "dotnet45"))
                    {
                        if (str5 != "uap")
                        {
                            if (str5 == "wp80")
                            {
                                goto Label_0174;
                            }
                            if (str5 == "wp81")
                            {
                                goto Label_017B;
                            }
                            if (str5 == "wsa80")
                            {
                                goto Label_0182;
                            }
                            if (str5 != "wsa81")
                            {
                                throw new Exception(string.Format("Unknown platform \"{0}\".", str4));
                            }
                            goto Label_0189;
                        }
                    }
                    else
                    {
                        unknown = Platform.DotNet45;
                        continue;
                    }
                    unknown = Platform.UAP;
                }
                continue;
            Label_0174:
                unknown = Platform.WP80;
                continue;
            Label_017B:
                unknown = Platform.WP81;
                continue;
            Label_0182:
                unknown = Platform.WSA80;
                continue;
            Label_0189:
                unknown = Platform.WSA81;
                continue;
            Label_01A2:
                if (string.IsNullOrEmpty(str4))
                {
                    throw new Exception("Project lock file not specified.");
                }
                projectLockFile = str4;
                continue;
            Label_01BE:
                if (string.IsNullOrEmpty(str4))
                {
                    throw new Exception("Bits not specified.");
                }
                str5 = str4.ToLowerInvariant();
                if (!(str5 == "32"))
                {
                    if (str5 != "64")
                    {
                        throw new Exception(string.Format("Unexpected bits value \"{0}\".", str4));
                    }
                }
                else
                {
                    base.OperationContext.SetIs64(false);
                    continue;
                }
                base.OperationContext.SetIs64(true);
                continue;
            Label_022D:
                if (string.IsNullOrEmpty(str4))
                {
                    throw new Exception("Metadata not specified.");
                }
                str5 = str4.ToLowerInvariant();
                if ((!(str5 == "0") && !(str5 == "false")) && !(str5 == "no"))
                {
                    if (((str5 != "1") && (str5 != "true")) && (str5 != "yes"))
                    {
                        throw new Exception(string.Format("Unexpected metadata value \"{0}\".", str4));
                    }
                }
                else
                {
                    flag = false;
                    continue;
                }
                flag = true;
                continue;
            Label_02C2:
                str5 = str4.ToLowerInvariant();
                if (!(str5 == "0") && !(str5 == "false"))
                {
                    if ((str5 != "1") && (str5 != "true"))
                    {
                        throw new Exception(string.Format("Unexpected removeDebuggableAttribute value \"{0}\".", str4));
                    }
                }
                else
                {
                    base.OperationContext.RemoveDebuggableAttribute = false;
                    continue;
                }
                base.OperationContext.RemoveDebuggableAttribute = true;
            }
            if (unknown == Platform.Unknown)
            {
                throw new Exception("Platform not specified.");
            }
            base.OperationContext.SetPlatform(unknown, projectLockFile);
            base.OperationContext.SetPaths(paths);
            base.OperationContext.SkipMetadata = !flag;
            return context;
        }

        public sealed class StepContext : IStepContext
        {
            private readonly List<string> fileNames = new List<string>();

            public void AddFileName(string fileName)
            {
                this.fileNames.Add(fileName);
            }

            public string[] FileNames
            {
                get
                {
                    return this.fileNames.ToArray();
                }
            }
        }
    }
}

