namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Git.VisualStudio;

    using Microsoft.TeamFoundation.MVVM;

    using ViewModelBase = GalaSoft.MvvmLight.ViewModelBase;

    /// <summary>
    /// The view model for performing squashs.
    /// </summary>
    public class SquashViewModel : ViewModelBase, ISquashViewModel
    {
        private bool applyRebase = true;

        private IList<GitCommit> branchCommits;

        private IList<GitBranch> branches;

        private string commitMessage;

        private GitBranch currentBranch;

        private bool forcePush = true;

        private GitCommandResponse gitCommandResponse;

        private bool isBusy;

        private bool isConflicts;

        private bool isDirty;

        private GitBranch rebaseBranch;

        private bool rebaseInProgress;

        private GitCommit selectedCommit;

        private IGitSquashWrapper squashWrapper;

        private CancellationTokenSource tokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashViewModel" /> class.
        /// </summary>
        /// <param name="squashWrapper">Our model that we retrieve data from.</param>
        /// <param name="changeBranch">A command to change the current branch.</param>
        /// <param name="showConflicts">A command to resolve conflicts.</param>
        /// <param name="showChanges">A command to show the changes tab.</param>
        public SquashViewModel(IGitSquashWrapper squashWrapper, ICommand changeBranch, ICommand showConflicts, ICommand showChanges)
        {
            if (squashWrapper == null)
            {
                throw new ArgumentNullException(nameof(squashWrapper));
            }

            if (changeBranch == null)
            {
                throw new ArgumentNullException(nameof(changeBranch));
            }

            if (showConflicts == null)
            {
                throw new ArgumentNullException(nameof(showConflicts));
            }

            if (showChanges == null)
            {
                throw new ArgumentNullException(nameof(showChanges));
            }

            this.SquashWrapper = squashWrapper;
            this.ChangeBranch = changeBranch;
            this.ViewConflictsPage = showConflicts;
            this.ViewChangesPage = showChanges;

            this.PropertyChanged += this.OnPropertyChanged;

            this.Refresh();

            this.Squash = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformSquash), this.CanPerformSquash);
            this.Rebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformRebase), this.CanPerformRebase);
            this.ContinueRebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformContinueRebase), this.CanContinueRebase);
            this.AbortRebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformAbortRebase), this.CanContinueRebase);
            this.CancelOperation = new RelayCommand(this.PerformCancelOperation, _ => this.tokenSource != null);
        }

        /// <inheritdoc />
        public ICommand Squash { get; }

        /// <inheritdoc />
        public ICommand Rebase { get; }

        /// <inheritdoc />
        public ICommand ContinueRebase { get; }

        /// <inheritdoc />
        public ICommand ViewConflictsPage { get; }

        /// <inheritdoc />
        public ICommand ViewChangesPage { get; }

        /// <inheritdoc />
        public ICommand AbortRebase { get; }

        /// <inheritdoc />
        public ICommand CancelOperation { get; }

        /// <inheritdoc />
        public bool IsConflicts
        {
            get
            {
                return this.isConflicts;
            }

            set
            {
                this.Set(ref this.isConflicts, value);
            }
        }

        /// <inheritdoc />
        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            set
            {
                this.Set(ref this.isBusy, value);
            }
        }

        /// <inheritdoc />
        public bool ApplyRebase
        {
            get
            {
                return this.applyRebase;
            }

            set
            {
                this.Set(ref this.applyRebase, value);
            }
        }

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
        public GitBranch CurrentBranch
        {
            get
            {
                return this.currentBranch;
            }

            set
            {
                this.Set(ref this.currentBranch, value);
            }
        }

        /// <inheritdoc />
        public GitCommit SelectedCommit
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
        public IList<GitBranch> Branches
        {
            get
            {
                return this.branches;
            }

            private set
            {
                this.Set(ref this.branches, value);
            }
        }

        /// <inheritdoc />
        public IList<GitCommit> BranchCommits
        {
            get
            {
                return this.branchCommits;
            }

            private set
            {
                this.Set(ref this.branchCommits, value);
            }
        }

        /// <inheritdoc />
        public GitBranch SelectedRebaseBranch
        {
            get
            {
                return this.rebaseBranch;
            }

            set
            {
                this.Set(ref this.rebaseBranch, value);
            }
        }

        /// <inheritdoc />
        public GitCommandResponse GitCommandResponse
        {
            get
            {
                return this.gitCommandResponse;
            }

            set
            {
                this.Set(ref this.gitCommandResponse, value);
                this.RaisePropertyChanged(nameof(this.OperationSuccess));
            }
        }

        /// <inheritdoc />
        public bool? OperationSuccess => this.GitCommandResponse?.Success;

        /// <inheritdoc />
        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }

            set
            {
                this.Set(ref this.isDirty, value);
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

        /// <inheritdoc />
        public void Refresh()
        {
            string oldCommitText = this.CommitMessage;
            GitCommit oldCommit = this.SelectedCommit;

            this.Branches = this.SquashWrapper.GetBranches();
            this.CurrentBranch = this.SquashWrapper.GetCurrentBranch();

            if (this.Branches?.Contains(this.SelectedRebaseBranch) == false)
            {
                this.SelectedRebaseBranch = this.Branches?.FirstOrDefault(x => x.FriendlyName == "origin/master");
            }

            this.IsRebaseInProgress = this.SquashWrapper.IsRebaseHappening();
            this.IsConflicts = this.SquashWrapper.HasConflicts();
            this.IsDirty = this.SquashWrapper.IsWorkingDirectoryDirty() && !this.SquashWrapper.HasConflicts();
            this.BranchCommits = this.SquashWrapper.GetCommitsForBranch(this.CurrentBranch).ToList();
            this.SelectedCommit = this.BranchCommits.FirstOrDefault(x => x == oldCommit);
            this.CommitMessage = string.IsNullOrWhiteSpace(oldCommitText) ? this.CommitMessage : oldCommitText;
        }

        private void PerformCancelOperation(object argument)
        {
            try
            {
                this.tokenSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private async Task ExecuteGitBackground(Func<CancellationToken, Task<GitCommandResponse>> func)
        {
            this.IsBusy = true;
            try
            {
                try
                {
                    this.tokenSource?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                this.tokenSource = new CancellationTokenSource();
                GitCommandResponse rebaseOutput = await func(this.tokenSource.Token);
                this.GitCommandResponse = rebaseOutput;
                this.Refresh();
            }
            catch (Exception ex)
            {
                this.GitCommandResponse = new GitCommandResponse(false, ex.Message);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task<GitCommandResponse> PerformSquash(CancellationToken token)
        {
            GitCommandResponse squashOutput = await this.SquashWrapper.Squash(token, this.CommitMessage, this.SelectedCommit);

            if (squashOutput.Success == false || token.IsCancellationRequested)
            {
                return squashOutput;
            }

            GitCommandResponse forcePushOutput;
            if (this.ForcePush)
            {
                forcePushOutput = await this.SquashWrapper.PushForce(token);

                if (forcePushOutput.Success == false || token.IsCancellationRequested)
                {
                    return forcePushOutput;
                }
            }

            GitCommandResponse rebaseOutput = null;

            if (this.ApplyRebase)
            {
                rebaseOutput = await this.SquashWrapper.Rebase(token, this.SelectedRebaseBranch);

                if (rebaseOutput.Success == false || token.IsCancellationRequested)
                {
                    return rebaseOutput;
                }
            }

            forcePushOutput = null;
            if (this.ForcePush)
            {
                forcePushOutput = await this.SquashWrapper.PushForce(token);

                if (forcePushOutput.Success == false || token.IsCancellationRequested)
                {
                    return forcePushOutput;
                }
            }

            StringBuilder sb = new StringBuilder();
            if (squashOutput != null && squashOutput.Success)
            {
                sb.AppendLine(squashOutput.CommandOutput);
            }

            if (forcePushOutput != null && forcePushOutput.Success)
            {
                sb.AppendLine(forcePushOutput.CommandOutput);
            }

            if (rebaseOutput != null && rebaseOutput.Success)
            {
                sb.AppendLine(rebaseOutput.CommandOutput);
            }

            return new GitCommandResponse(true, sb.ToString());
        }

        private Task<GitCommandResponse> PerformRebase(CancellationToken token)
        {
            return this.SquashWrapper.Rebase(token, this.SelectedRebaseBranch);
        }

        private Task<GitCommandResponse> PerformAbortRebase(CancellationToken token)
        {
            return this.SquashWrapper.Abort(token);
        }

        private Task<GitCommandResponse> PerformContinueRebase(CancellationToken token)
        {
            return this.SquashWrapper.Continue(token);
        }

        private bool CanContinueRebase(object param)
        {
            return this.SquashWrapper != null && this.SquashWrapper.IsRebaseHappening() && this.IsBusy == false;
        }

        private bool CanPerformRebase(object parameter)
        {
            return this.SquashWrapper.IsRebaseHappening() == false && this.SquashWrapper.IsWorkingDirectoryDirty() == false && this.IsBusy == false && this.SelectedRebaseBranch != null;
        }

        private bool CanPerformSquash(object param)
        {
            return this.SquashWrapper != null && this.SquashWrapper.GetCurrentBranch().FriendlyName == this.CurrentBranch.FriendlyName && this.SelectedCommit != null && string.IsNullOrWhiteSpace(this.CommitMessage) == false && this.IsBusy == false && this.SquashWrapper.IsWorkingDirectoryDirty() == false;
        }

        private void UpdateCommitMessage(GitCommit commit)
        {
            this.CommitMessage = this.SquashWrapper?.GetCommitMessages(commit);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedCommit":
                    this.UpdateCommitMessage(this.SelectedCommit);
                    break;
            }
        }
    }
}