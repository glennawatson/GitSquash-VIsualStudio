namespace GitRebase.VisualStudio.Extension.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    using LibGit2Sharp;

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
        /// Gets or sets the wrapper for the git squash operation.
        /// </summary>
        IGitSquashWrapper SquashWrapper { get; set;  }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        Branch CurrentBranch { get; }

        /// <summary>
        /// Gets or sets the currently selected commit.
        /// </summary>
        Commit SelectedCommit { get; set; }

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
        bool ForcePush { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is currently a rebase in operation.
        /// </summary>
        bool IsRebaseInProgress { get; }
    }
}
