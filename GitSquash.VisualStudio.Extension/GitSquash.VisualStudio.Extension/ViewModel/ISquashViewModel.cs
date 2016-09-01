namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    using Git.VisualStudio;

    /// <summary>
    /// View model for performing git squash rebase actions.
    /// </summary>
    public interface ISquashViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets a command that performs the squash.
        /// </summary>
        ICommand Squash { get; }

        /// <summary>
        /// Gets a command which will perform a rebase.
        /// </summary>
        ICommand Rebase { get; }

        /// <summary>
        /// Gets a command that will continue a rebase.
        /// </summary>
        ICommand ContinueRebase { get; }

        /// <summary>
        /// Gets a command which will show the conflicts page.
        /// </summary>
        ICommand ViewConflictsPage { get; }

        /// <summary>
        /// Gets a command which will navigate to the changes page.
        /// </summary>
        ICommand ViewChangesPage { get; }

        /// <summary>
        /// Gets a command that will abort a rebase.
        /// </summary>
        ICommand AbortRebase { get; }

        /// <summary>
        /// Gets a command which will cancel the current operation.
        /// </summary>
        ICommand CancelOperation { get; }

        /// <summary>
        /// Gets a command which will force a push.
        /// </summary>
        ICommand PushForce { get; }

        /// <summary>
        /// Gets a command which will pull the origin.
        /// </summary>
        ICommand FetchOrigin { get; }

        /// <summary>
        /// Gets a command which will skip.
        /// </summary>
        ICommand Skip { get; }

        /// <summary>
        /// Gets or sets the wrapper for the GIT squash operation.
        /// </summary>
        IGitSquashWrapper SquashWrapper { get; set; }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        GitBranch CurrentBranch { get; }

        /// <summary>
        /// Gets or sets the currently selected commit.
        /// </summary>
        GitCommit SelectedCommit { get; set; }

        /// <summary>
        /// Gets a command which will change the current branch.
        /// </summary>
        ICommand ChangeBranch { get; }

        /// <summary>
        /// Gets or sets the commit message to be used when squashed.
        /// </summary>
        string CommitMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we are going to do a force
        /// push once the rebase/squash has finished.
        /// </summary>
        bool DoForcePush { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is currently a rebase in operation.
        /// </summary>
        bool IsRebaseInProgress { get; }

        /// <summary>
        /// Gets a value indicating whether there are any conflicts in the branch at the moment.
        /// </summary>
        bool IsConflicts { get; }

        /// <summary>
        /// Gets or sets a value indicating whether we should apply a rebase after squashing.
        /// </summary>
        bool ApplyRebase { get; set; }

        /// <summary>
        /// Gets a value indicating whether the repository is dirty.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Gets a collection of branches for the project.
        /// </summary>
        IList<GitBranch> Branches { get; }

        /// <summary>
        /// Gets a collection of commits for the current branch.
        /// </summary>
        IList<GitCommit> BranchCommits { get; }

            /// <summary>
        /// Gets or sets the selected rebase branch.
        /// </summary>
        GitBranch SelectedRebaseBranch { get; set; }

        /// <summary>
        /// Gets the response of a git command.
        /// </summary>
        GitCommandResponse GitCommandResponse { get; }

        /// <summary>
        /// Gets the log options.
        /// </summary>
        GitLogOptionsViewModel LogOptions { get; }

        /// <summary>
        /// Gets a value indicating whether the previous operation success.
        /// </summary>
        bool? OperationSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the git process is currently busy.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Refreshes the display.
        /// </summary>
        void Refresh();
    }
}
