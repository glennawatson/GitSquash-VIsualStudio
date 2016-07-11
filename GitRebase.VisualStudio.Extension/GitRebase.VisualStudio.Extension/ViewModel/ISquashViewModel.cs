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
        ICommand PerformSquash { get; }

        /// <summary>
        /// Gets or sets the wrapper for the git squash operation.
        /// </summary>
        IGitSquashWrapper SquashWrapper { get; set;  }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        Branch CurrentBranch { get; }

        /// <summary>
        /// Gets a command which will change the current branch.
        /// </summary>
        ICommand ChangeBranch { get; }
    }
}
