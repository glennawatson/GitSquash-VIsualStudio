namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    using GalaSoft.MvvmLight.Command;

    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

    using TeamExplorer.Common;

    using View;
    using ViewModel;

    /// <summary>
    /// Represents a page in the team explorer
    /// relating to the squash action.
    /// </summary>
    [TeamExplorerPage(GitSquashPackage.SquashPageGuidString, Undockable = true)]
    public class SquashPage : TeamExplorerBasePage 
    {
        private readonly ITeamExplorer teamExplorer;
        private readonly IGitExt gitService;

        private SquashView view;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashPage"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        [ImportingConstructor]
        public SquashPage([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.teamExplorer = (ITeamExplorer)serviceProvider.GetService(typeof(ITeamExplorer));

            this.gitService = (IGitExt)serviceProvider.GetService(typeof(IGitExt));
            this.gitService.PropertyChanged += (sender, e) => this.Refresh();
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
                new Action(() => this.teamExplorer.NavigateToPage(new Guid(pageGuid), null)));
        }

        /// <inheritdoc />
        public override void Initialize(object sender, PageInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            this.Title = "Squash";

            if (this.AreGitToolsInstalled() == false)
            {
                this.ShowPage(TeamExplorerPageIds.GitInstallThirdPartyTools);
                return;
            }

            this.view = new SquashView();

            this.PageContent = this.view;

            this.Refresh();
        }

        /// <inheritdoc />
        public override void Refresh()
        {
            var showBranches = new RelayCommand(() => this.ShowPage(TeamExplorerPageIds.GitBranches));
            this.view.ViewModel = new SquashViewModel(new GitSquashWrapper(this.GetRepositoryDirectory()), showBranches);
        }

        private string GetRepositoryDirectory()
        {
            return this.gitService.ActiveRepositories.FirstOrDefault()?.RepositoryPath;
        }

        private bool AreGitToolsInstalled()
        {
            var gitInstallationPath = GitHelper.GetGitInstallationPath();
            string pathToGit = Path.Combine(Path.Combine(gitInstallationPath, "bin\\git.exe"));

            return File.Exists(pathToGit);
        }
    }
}
