namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell;
    using TeamExplorer.Common;

    /// <summary>
    /// Represents a page in the team explorer
    /// relating to the squash action.
    /// </summary>
    [TeamExplorerPage(GitSquashPackage.SquashPageGuidString, Undockable = true)]
    public class SquashPage : TeamExplorerBasePage 
    {
        private static ITeamExplorer teamExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashPage"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        [ImportingConstructor]
        public SquashPage([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            teamExplorer = (ITeamExplorer)serviceProvider.GetService(typeof(ITeamExplorer));
           
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

        public override void Initialize(object sender, PageInitializeEventArgs e)
        {
            base.Initialize(sender, e);

            this.Title = "Squash";
        }
    }
}
