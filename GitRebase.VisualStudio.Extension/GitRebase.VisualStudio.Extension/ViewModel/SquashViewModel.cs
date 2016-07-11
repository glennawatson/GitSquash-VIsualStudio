namespace GitRebase.VisualStudio.Extension.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    using LibGit2Sharp;

    /// <summary>
    /// The view model for performing squashs.
    /// </summary>
    public class SquashViewModel : ViewModelBase, ISquashViewModel
    {
        private IGitSquashWrapper squashWrapper;

        private Commit selectedCommit;

        private string commitMessage;

        private bool forcePush = true;

        private bool rebaseInProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashViewModel"/> class.
        /// </summary>
        /// <param name="squashWrapper">Our model that we retrieve data from.</param>
        /// <param name="changeBranch">A command to change the current branch.</param>
        public SquashViewModel(IGitSquashWrapper squashWrapper, ICommand changeBranch)
        {
            this.SquashWrapper = squashWrapper;
            this.ChangeBranch = changeBranch;

            this.Squash = new RelayCommand(this.PerformSquash, this.CanPerformSquash);

            this.PropertyChanged += this.OnPropertyChanged;
            this.CurrentBranch = this.SquashWrapper?.GetCurrentBranch();

            this.IsRebaseInProgress = this.squashWrapper.IsRebaseHappening();
        }

        /// <inheritdoc />
        public ICommand Squash { get; }

        /// <inheritdoc />
        public IGitSquashWrapper SquashWrapper
        {
            get
            {
                return this.squashWrapper;
            }

            set
            {
                this.Set(ref this.squashWrapper, value);
            }
        }

        /// <inheritdoc />
        public Branch CurrentBranch { get; }

        /// <inheritdoc />
        public Commit SelectedCommit
        {
            get
            {
                return this.selectedCommit;
            }

            set
            {
                this.Set(ref this.selectedCommit, value);
            }
        }

        /// <inheritdoc />
        public bool IsRebaseInProgress
        {
            get
            {
                return this.rebaseInProgress;
            }

            set
            {
                this.Set(ref this.rebaseInProgress, value);
            }
        }

        /// <inheritdoc />
        public ICommand ChangeBranch { get; }

        /// <inheritdoc />
        public string CommitMessage
        {
            get
            {
                return this.commitMessage;
            }

            set
            {
                this.Set(ref this.commitMessage, value);
            }
        }

        /// <inheritdoc />
        public bool ForcePush
        {
            get
            {
                return this.forcePush;
            }

            set
            {
                this.Set(ref this.forcePush, value);
            }
        }

        private void PerformSquash()
        {
            if (this.SquashWrapper == null)
            {
                return;
            }

            this.SquashWrapper.Squash(this.CommitMessage, this.SelectedCommit);

            if (this.ForcePush)
            {
                this.SquashWrapper.PushForce();
            }
        }

        private bool CanPerformSquash()
        {
            return this.SquashWrapper != null && this.SquashWrapper.GetCurrentBranch() == this.CurrentBranch
                   && this.SelectedCommit != null && string.IsNullOrWhiteSpace(this.CommitMessage) == false;
        }

        private void UpdateCommitMessage(Commit commit)
        {
            this.CommitMessage = this.SquashWrapper?.GetCommitMessages(commit);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentBranch")
            {
                this.SelectedCommit = this.CurrentBranch?.Commits.FirstOrDefault();
            }
            else if (e.PropertyName == "SelectedCommit")
            {
                this.UpdateCommitMessage(this.SelectedCommit);
            }
        }
    }
}
