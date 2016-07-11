namespace GitSquash.VisualStudio.Extension
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Class to provide the current version of visual studio the product is running under.
    /// </summary>
    public static class VsVersion
    {
        private static readonly object MLock = new object();
        private static Version mVsVersion;
        private static Version mOsVersion;

        /// <summary>
        /// Gets a value indicating the current version of visual studio.
        /// </summary>
        public static Version FullVersion
        {
            get
            {
                lock (MLock)
                {
                    if (mVsVersion != null)
                    {
                        return mVsVersion;
                    }

                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                    if (File.Exists(path))
                    {
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                        string verName = fvi.ProductVersion;

                        for (int i = 0; i < verName.Length; i++)
                        {
                            if (char.IsDigit(verName, i) || verName[i] == '.')
                            {
                                continue;
                            }

                            verName = verName.Substring(0, i);
                            break;
                        }

                        mVsVersion = new Version(verName);
                    }
                    else
                    {
                        mVsVersion = new Version(0, 0); // Not running inside Visual Studio!
                    }
                }

                return mVsVersion;
            }
        }

        /// <summary>
        /// Gets a value that indicates the current operating system value.
        /// </summary>
        public static Version OsVersion => mOsVersion ?? (mOsVersion = Environment.OSVersion.Version);

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2012 or later. 
        /// </summary>
        public static bool Vs2012OrLater => FullVersion >= new Version(11, 0);

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2010 or later.
        /// </summary>
        public static bool Vs2010OrLater => FullVersion >= new Version(10, 0);

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2008 or later.
        /// </summary>
        public static bool Vs2008OrOlder => FullVersion < new Version(9, 0);

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2005.
        /// </summary>
        public static bool Vs2005 => FullVersion.Major == 8;

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2008.
        /// </summary>
        public static bool Vs2008 => FullVersion.Major == 9;

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2010.
        /// </summary>
        public static bool Vs2010 => FullVersion.Major == 10;

        /// <summary>
        /// Gets a value indicating whether we are running under Visual Studio 2012.
        /// </summary>
        public static bool Vs2012 => FullVersion.Major == 11;
    }
}
