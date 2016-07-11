//------------------------------------------------------------------------------
// <copyright file="GitRebasePackage.cs" company="Glenn Watson">
//     Copyright (c) Glenn Watson.  All rights reserved.
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//------------------------------------------------------------------------------
namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Microsoft.TeamFoundation.Git.Controls.Extensibility;
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

        private IHistoryExt2 gitHistory;

        private IGitSquashWrapper squashWrapper;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            IServiceContainer serviceContainer = this;

            this.gitService = (IGitExt)serviceContainer.GetService(typeof(IGitExt));
            this.gitHistory = (IHistoryExt2)serviceContainer.GetService(typeof(IHistoryExt2));

            serviceContainer.AddService(typeof(IGitSquashWrapper), this.CreateGitWrapperService, true);
        }

        private object CreateGitWrapperService(IServiceContainer container, Type serviceType)
        {
            if (typeof(IGitSquashWrapper) != serviceType)
            {
                return null;
            }

            if (this.squashWrapper != null)
            {
                return this.squashWrapper;
            }

            var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var customGuid = new Guid("27AF351D-6A16-47E5-8D9D-0EF16C348395");
            if (outWindow != null)
            {
                outWindow.CreatePane(ref customGuid, "Git Commit Squash", 1, 1);
                IVsOutputWindowPane outputWindow;
                outWindow.GetPane(ref customGuid, out outputWindow);
            }

            if (this.gitService.ActiveRepositories.FirstOrDefault() != null)
            {
                IGitRepositoryInfo gitRepositoryInfo = this.gitService.ActiveRepositories.FirstOrDefault();
                if (gitRepositoryInfo == null)
                {
                    return this.squashWrapper;
                }

                string path = gitRepositoryInfo.RepositoryPath;
                this.TraceWriteLine("Creating Wrapper service with path: " + path);
                this.squashWrapper = new GitSquashWrapper(this.gitService.ActiveRepositories.First().RepositoryPath);
            }
            else
            {
                this.TraceWriteLine("Creating Wrapper service.");
                this.squashWrapper = null;
            }

            return this.squashWrapper;
        }

        private void TraceWriteLine(string msg)
        {
            Trace.WriteLine("**********" + msg);
        }
    }
}