namespace GitSquash.VisualStudio.Extension
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Linq;

    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;

    using TeamExplorer.Common;

    /// <summary>
    /// Represents a navigation item in the team explorer which will perform the squash action in git.
    /// </summary>
    [TeamExplorerNavigationItem(GitSquashPackage.SquashNavigationItemGuidString, 1500)]
    public class SquashNavigationItem : TeamExplorerBaseNavigationItem
    {
        private readonly IGitExt gitService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashNavigationItem"/> class.
        /// </summary>
        /// <param name="serviceProvider">The visual studio service provider.</param>
        [ImportingConstructor]
        public SquashNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            try
            {
                this.UpdateVisible();
                this.Image = Properties.Resources.SquashIcon;
                this.IsVisible = false;
                this.Text = Properties.Resources.SquashName;
                ITeamExplorer teamExplorer = this.GetService<ITeamExplorer>();
                teamExplorer.PropertyChanged += this.TeamExplorerOnPropertyChanged;
                this.gitService = (IGitExt)serviceProvider.GetService(typeof(IGitExt));
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        /// <summary>
        /// Executes the action of the navigation item.
        /// </summary>
        public override void Execute()
        {
            try
            {
                Logger.PageView("Navigate");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                this.ShowNotification(ex.Message, NotificationType.Error);
            }

            ITeamExplorer service = this.GetService<ITeamExplorer>();
            service?.NavigateToPage(new Guid(GitSquashPackage.SquashPageGuidString), null);
        }

        /// <summary>
        /// When the team explorer has changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void TeamExplorerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateVisible();
        }

        private void UpdateVisible()
        {
            this.IsVisible = this.gitService != null && this.gitService.ActiveRepositories.Any();
        }

        private void HandleException(Exception ex)
        {
            Logger.Exception(ex);
            this.ShowNotification(ex.Message, NotificationType.Error);
        }
    }
}
