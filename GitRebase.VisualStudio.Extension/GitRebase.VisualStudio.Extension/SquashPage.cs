namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.TeamFoundation.Controls;
    using Microsoft.VisualStudio.Shell;
    using TeamExplorer.Common;

    [TeamExplorerPage(GitSquashPackage.RebasePageGuidString, Undockable = true)]
    public class SquashPage : TeamExplorerBasePage 
    {
        private static ITeamExplorer teamExplorer;

        public SquashPage([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            teamExplorer = (ITeamExplorer)serviceProvider.GetService(typeof(ITeamExplorer));
        }

        public void ShowPage(string pageGuid)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => teamExplorer.NavigateToPage(new Guid(pageGuid), null)));
        }
    }
}
