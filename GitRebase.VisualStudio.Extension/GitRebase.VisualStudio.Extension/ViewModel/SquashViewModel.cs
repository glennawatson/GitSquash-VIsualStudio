﻿namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
<<<<<<< 5c7304e108debaf031d656c9fa9d65da8d19137f
=======
    using System.Threading.Tasks;
>>>>>>> Fix
    using System.Windows.Input;

    using Microsoft.TeamFoundation.MVVM;

    using ViewModelBase = GalaSoft.MvvmLight.ViewModelBase;

    /// <summary>
    /// The view model for performing squashs.
    /// </summary>
    public class SquashViewModel : ViewModelBase, ISquashViewModel
    {
        private IGitSquashWrapper squashWrapper;

        private IList<GitBranch> branches;

        private GitCommandResponse gitCommandResponse;

        private GitCommit selectedCommit;

        private GitBranch rebaseBranch;

        private GitBranch currentBranch;

        private Branch rebaseBranch;

        private string commitMessage;

        private bool forcePush = true;

        private bool rebaseInProgress;

        private bool isConflicts;

        private bool applyRebase = true;

        private bool isBusy;

        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashViewModel"/> class.
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
<<<<<<< 5c7304e108debaf031d656c9fa9d65da8d19137f
            this.CurrentBranch = this.SquashWrapper?.GetCurrentBranch();
            this.Branches = this.SquashWrapper?.GetRemoteBranches();
=======
>>>>>>> Fix

            this.Refresh();

            this.Squash = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformSquash), this.CanPerformSquash);
            this.Rebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformRebase), this.CanPerformRebase);
            this.ContinueRebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformContinueRebase), this.CanContinueRebase);
            this.AbortRebase = new RelayCommand(async _ => await this.ExecuteGitBackground(this.PerformAbortRebase), this.CanContinueRebase);
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
<<<<<<< 5c7304e108debaf031d656c9fa9d65da8d19137f
        public IEnumerable<Branch> Branches { get; }

        /// <inheritdoc />
        public Branch SelectedRebaseBranch
=======
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
        public IEnumerable<GitCommit> BranchCommits => this.SquashWrapper.GetCommitsForBranch(this.CurrentBranch);

        /// <inheritdoc />
        public GitBranch SelectedRebaseBranch
>>>>>>> Fix
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
<<<<<<< 5c7304e108debaf031d656c9fa9d65da8d19137f
=======
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
>>>>>>> Fix
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
            var oldCommitText = this.CommitMessage;
            var oldCommit = this.SelectedCommit;

            this.Branches = this.SquashWrapper.GetBranches();
            this.CurrentBranch = this.SquashWrapper.GetCurrentBranch();

            if (this.Branches?.Contains(this.SelectedRebaseBranch) == false)
            {
                this.SelectedRebaseBranch = this.Branches?.FirstOrDefault(x => x.FriendlyName == "origin/master");
            }

            this.IsRebaseInProgress = this.SquashWrapper.IsRebaseHappening();
            this.IsConflicts = this.SquashWrapper.HasConflicts();
            this.IsDirty = this.SquashWrapper.IsWorkingDirectoryDirty() && !this.SquashWrapper.HasConflicts();
            this.SelectedCommit = this.BranchCommits.FirstOrDefault(x => x == oldCommit);
            this.CommitMessage = string.IsNullOrWhiteSpace(oldCommitText) ? this.CommitMessage : oldCommitText;
        }

        private async Task ExecuteGitBackground(Func<Task<GitCommandResponse>> func)
        {
            this.IsBusy = true;
            try
            {
                GitCommandResponse rebaseOutput = await func();
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

        private async Task<GitCommandResponse> PerformSquash()
        {
            GitCommandResponse squashOutput = await this.SquashWrapper.Squash(this.CommitMessage, this.SelectedCommit);

            if (squashOutput.Success == false)
            {
                return squashOutput;
            }

            GitCommandResponse forcePushOutput;
            if (this.ForcePush)
            {
                forcePushOutput = await this.SquashWrapper.PushForce();

                if (forcePushOutput.Success == false)
                {
                    return forcePushOutput;
                }
            }

            GitCommandResponse rebaseOutput = null;

            if (this.ApplyRebase)
            {
                rebaseOutput = await this.SquashWrapper.Rebase(this.SelectedRebaseBranch);

                if (rebaseOutput.Success == false)
                {
                    return rebaseOutput;
                }
            }

            forcePushOutput = null;
            if (this.ForcePush)
            {
                forcePushOutput = await this.SquashWrapper.PushForce();

                if (forcePushOutput.Success == false)
                {
                    return forcePushOutput;
                }
            }

            return new GitCommandResponse(true, $"{rebaseOutput?.CommandOutput}\r\n{squashOutput.CommandOutput}\r\n{forcePushOutput?.CommandOutput}");
        }

        private Task<GitCommandResponse> PerformRebase()
        {
            return this.SquashWrapper.Rebase(this.SelectedRebaseBranch);
        }

        private Task<GitCommandResponse> PerformAbortRebase()
        {
            return this.SquashWrapper.Abort();
        }

        private Task<GitCommandResponse> PerformContinueRebase()
        {
            return this.SquashWrapper.Continue(this.CommitMessage);
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
            return this.SquashWrapper != null && this.SquashWrapper.GetCurrentBranch().FriendlyName == this.CurrentBranch.FriendlyName
                   && this.SelectedCommit != null && string.IsNullOrWhiteSpace(this.CommitMessage) == false && this.IsBusy == false && this.SquashWrapper.IsWorkingDirectoryDirty() == false;
        }

        private void UpdateCommitMessage(GitCommit commit)
        {
            this.CommitMessage = this.SquashWrapper?.GetCommitMessages(commit);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentBranch":
                    this.SelectedCommit = this.BranchCommits.FirstOrDefault();
                    break;
                case "SelectedCommit":
                    this.UpdateCommitMessage(this.SelectedCommit);
                    break;
            }
        }
    }
}
