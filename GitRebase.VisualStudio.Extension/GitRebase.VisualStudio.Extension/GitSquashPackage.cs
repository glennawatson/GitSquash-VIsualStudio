//------------------------------------------------------------------------------
// <copyright file="GitRebasePackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Design;
    using System.Linq;

    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideService(typeof(IGitSquashWrapper))]
    public sealed class GitSquashPackage : Package
    {
        private IGitSquashWrapper squashWrapper;

        private IGitExt gitService;

        /// <summary>
        /// GitRebasePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "b9e7e4a0-2cd8-4787-bc1a-7581babc3b88";

        /// <summary>
        /// A guid to reference the rebase page.
        /// </summary>
        public const string RebasePageGuidString = "87C014D4-0102-43FA-B3AC-25B7033A13D5";

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSquashPackage"/> class.
        /// </summary>
        public GitSquashPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        private object CreateGitWrapperService(IServiceContainer container, Type serviceType)
        {
            if (typeof(IGitSquashWrapper) == serviceType)
            {
                if (this.squashWrapper != null)
                {
                    return this.squashWrapper;
                }

                IVsOutputWindowPane outputWindow;
                var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                var customGuid = new Guid("27AF351D-6A16-47E5-8D9D-0EF16C348395");
                if (outWindow != null)
                {
                    outWindow.CreatePane(ref customGuid, "Git Commit Squash", 1, 1);
                    outWindow.GetPane(ref customGuid, out outputWindow);
                }
                if (this.gitService.ActiveRepositories.FirstOrDefault() != null)
                {
                    IGitRepositoryInfo gitRepositoryInfo = this.gitService.ActiveRepositories.FirstOrDefault();
                    if (gitRepositoryInfo != null)
                    {
                        string path = gitRepositoryInfo.RepositoryPath;
                        this.TraceWriteLine("Creating Wrapper service with path: " + path);
                        this.squashWrapper = new GitSquashWrapper(path, events, new OutputWindowLogger(outputWindow), projects, Translator);
                    }
                }
                else
                {
                    this.TraceWriteLine("Creating Wrapper service.");
                    this.squashWrapper = new GitSquashWrapper(new OutputWindowLogger(outputWindow), projects, Translator);
                }

                return this.squashWrapper;
            }

            return null;
        }

        private void TraceWriteLine(string msg)
        {
            System.Diagnostics.Trace.WriteLine("**********" + msg);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override
        void Initialize()
        {
            base.Initialize();

            IServiceContainer serviceContainer = this;

            serviceContainer.AddService(typeof(IGitSquashWrapper), this.CreateGitWrapperService, true);

            this.gitService = (IGitExt)serviceContainer.GetService(typeof(IGitExt));

        }
    }
}
