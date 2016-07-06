namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class VSVersion
    {
        private static readonly object MLock = new object();
        private static Version mVsVersion;
        private static Version mOsVersion;

        public static Version FullVersion
        {
            get
            {
                lock (MLock)
                {
                    if (mVsVersion == null)
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                            string verName = fvi.ProductVersion;

                            for (int i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }

                            mVsVersion = new Version(verName);
                        }
                        else
                        {
                            mVsVersion = new Version(0, 0); // Not running inside Visual Studio!
                        }
                    }
                }

                return mVsVersion;
            }
        }

        public static Version OSVersion => mOsVersion ?? (mOsVersion = Environment.OSVersion.Version);

        public static bool VS2012OrLater => FullVersion >= new Version(11, 0);

        public static bool VS2010OrLater => FullVersion >= new Version(10, 0);

        public static bool VS2008OrOlder => FullVersion < new Version(9, 0);

        public static bool VS2005 => FullVersion.Major == 8;

        public static bool VS2008 => FullVersion.Major == 9;

        public static bool VS2010 => FullVersion.Major == 10;

        public static bool VS2012 => FullVersion.Major == 11;
    }
}
