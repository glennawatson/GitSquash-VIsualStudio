namespace GitSquash.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The minimum requirement for a class to be considered a valid package for Visual Studio
    ///     is to implement the IVsPackage interface and register itself with the shell.
    ///     This package uses the helper classes defined inside the Managed Package Framework (MPF)
    ///     to do it: it derives from the Package class that provides the implementation of the
    ///     IVsPackage interface and uses the registration attributes defined in the framework to
    ///     register itself and its components with the shell. These attributes tell the pkgdef creation
    ///     utility what data to put into .pkgdef file.
    ///     </para>
    ///     <para>
    ///     To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in
    ///     .vsixmanifest file.
    ///     </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideService(typeof(IGitSquashWrapper))]
    public sealed class GitSquashPackage : Package
    {
        /// <summary>
        /// GitRebasePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "b9e7e4a0-2cd8-4787-bc1a-7581babc3b88";

        /// <summary>
        /// A guid to reference the rebase page.
        /// </summary>
        public const string SquashPageGuidString = "87C014D4-0102-43FA-B3AC-25B7033A13D5";

        /// <summary>
        /// A guid that references our navigation item.
        /// </summary>
        public const string SquashNavigationItemGuidString = "168177EF-3080-4640-9631-3363E5974E1A";

        private IGitExt gitService;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.TraceWriteLine("Package Initialization: Starting");
            IServiceContainer serviceContainer = this;
            ServiceCreatorCallback callback = this.CreateGitWrapperService;
            serviceContainer.AddService(typeof(IGitSquashWrapper), callback, true);
            this.gitService = (IGitExt)this.GetService(typeof(IGitExt));

            this.TraceWriteLine("Package Initialization: Done");
        }

        private void TraceWriteLine(string msg)
        {
            Trace.WriteLine("**********" + msg);
        }

        private object CreateGitWrapperService(IServiceContainer container, Type serviceType)
        {
            this.TraceWriteLine("Service Requested: " + serviceType.FullName);
            if (typeof(IGitSquashWrapper) == serviceType)
            {
                IVsOutputWindowPane outputWindow;
                var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                var customGuid = new Guid("E53E7910-5B4F-4C5D-95BE-92BD439178E6");
                outWindow.CreatePane(ref customGuid, "Git Squash", 1, 1);
                outWindow.GetPane(ref customGuid, out outputWindow);
                IGitSquashWrapper wrapper = null;
                if (this.gitService.ActiveRepositories.FirstOrDefault() != null)
                {
                    string path = this.gitService.ActiveRepositories.FirstOrDefault()?.RepositoryPath;
                    this.TraceWriteLine("Creating Wrapper service with path: " + path);
                    wrapper = new GitSquashWrapper(path, new OutputWindowLogger(outputWindow));
                }

                return wrapper;
            }

            throw new ArgumentException();
        }
    }
}