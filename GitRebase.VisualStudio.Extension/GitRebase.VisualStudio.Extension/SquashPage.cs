namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;

    using GitRebase.VisualStudio.Extension.View;
    using GitRebase.VisualStudio.Extension.ViewModel;

    using Microsoft.TeamFoundation.Controls;
    using Microsoft.TeamFoundation.Git.Controls.Extensibility;
    using Microsoft.TeamFoundation.MVVM;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

    using TeamExplorer.Common;

    /// <summary>
    /// Represents a page in the team explorer
    /// relating to the squash action.
    /// </summary>
    [TeamExplorerPage(GitSquashPackage.SquashPageGuidString, Undockable = true)]
    public class SquashPage : TeamExplorerBasePage 
    {
        private ITeamExplorer teamExplorer;

        private IGitExt gitExt;

        private IGitSquashWrapper squashWrapper;

        private dynamic uiService;


        /// <summary>
        /// Initializes a new instance of the <see cref="SquashPage"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        [ImportingConstructor]
        public SquashPage([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.teamExplorer = (ITeamExplorer)serviceProvider.GetService(typeof(ITeamExplorer));
            this.gitExt = (IGitExt)serviceProvider.GetService(typeof(IGitExt));
        }

        /// <summary>
        /// Shows a particular page based on the provided GUID.
        /// Handy for showing other team explorer pages.
        /// </summary>
        /// <param name="pageGuid">The guid to show.</param>
        public void ShowPage(string pageGuid)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => teamExplorer.NavigateToPage(new Guid(pageGuid), null)));
        }

        /// <inheritdoc />
        public override object GetExtensibilityService(Type serviceType)
        {
            if (serviceType == typeof(IGitSquashWrapper))
            {
                return this.squashWrapper;
            }

            return base.GetExtensibilityService(serviceType);
        }

        /// <inheritdoc />
        public override void Initialize(object sender, PageInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            this.Title = "Squash";

            this.squashWrapper = this.GetService<IGitSquashWrapper>();

            this.PageContent = new SquashView
                                 {
                                     ViewModel = new SquashViewModel(this.squashWrapper)
                                 };

            if (this.AreGitToolsInstalled() == false)
            {
                this.ShowPage(TeamExplorerPageIds.GitInstallThirdPartyTools);
            }
        }

        private bool AreGitToolsInstalled()
        {
            var gitInstallationPath = GitHelper.GetGitInstallationPath();
            string pathToGit = Path.Combine(Path.Combine(gitInstallationPath, "bin\\git.exe"));

            return File.Exists(pathToGit);
        }
    }
}
