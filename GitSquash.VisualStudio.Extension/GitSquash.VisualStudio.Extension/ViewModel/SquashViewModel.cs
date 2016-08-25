namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Git.VisualStudio;

    using Properties;

    using ReactiveUI;

    /// <summary>
    /// The view model for performing squashs.
    /// </summary>
    public class SquashViewModel : ReactiveObject, ISquashViewModel
    {
        private readonly ReactiveCommand<object> refresh;

        private readonly ReactiveCommand<string> updateCommitMessage;

        private readonly ObservableAsPropertyHelper<bool?> isOperationSuccess;

        private readonly ObservableAsPropertyHelper<bool> isBusy;

        private readonly ReactiveCommand<GitCommandResponse> fetchOrigin;
        private readonly ReactiveCommand<GitCommandResponse> pushForce;
        private readonly ReactiveCommand<object> cancelOperation;
        private readonly ReactiveCommand<GitCommandResponse> abortRebase;
        private readonly ReactiveCommand<GitCommandResponse> continueRebase;
        private readonly ReactiveCommand<GitCommandResponse> rebase;
        private readonly ReactiveCommand<GitCommandResponse> squash;

        private bool applyRebase = true;

        private IList<GitCommit> branchCommits;

        private IList<GitBranch> branches;

        private string commitMessage;

        private GitBranch currentBranch;

        private bool forcePush = true;

        private GitCommandResponse gitCommandResponse;

        private bool isConflicts;

        private bool isDirty;

        private GitBranch rebaseBranch;

        private bool rebaseInProgress;

        private GitCommit selectedCommit;

        private GitLogOptionsViewModel gitLogOptions;

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

            this.isOperationSuccess = this.WhenAnyValue(x => x.GitCommandResponse).Select(x => x?.Success).ToProperty(this, x => x.OperationSuccess, out this.isOperationSuccess);

            var canSquashObservable = this.WhenAnyValue(x => x.CurrentBranch, x => x.SquashWrapper, x => x.SelectedCommit, x => x.CommitMessage, x => x.IsBusy, x => x.IsDirty)
                .Select(x => x.Item1 != null && x.Item2 != null && x.Item3 != null && string.IsNullOrWhiteSpace(x.Item4) == false && x.Item5 == false && x.Item6 == false);
            var canRebase = this.WhenAnyValue(x => x.IsRebaseInProgress, x => x.IsDirty, x => x.IsBusy, x => x.SelectedRebaseBranch)
                .Select(x => x.Item1 == false && x.Item2 == false && x.Item3 == false && x.Item4 != null);
            var canCancel = this.WhenAnyValue(x => x.IsBusy);
            var canContinueRebase = this.WhenAnyValue(x => x.IsBusy, x => x.SquashWrapper, x => x.IsRebaseInProgress)
                .Select(x => x.Item1 == false && x.Item2 != null && x.Item3);

            this.cancelOperation = ReactiveCommand.Create(canCancel);
            this.cancelOperation.Subscribe(_ => this.PerformCancelOperation());

            this.squash = this.GenerateGitCommand(canSquashObservable, this.PerformSquash);
            this.rebase = this.GenerateGitCommand(canRebase, this.PerformRebase);
            this.continueRebase = this.GenerateGitCommand(canContinueRebase, this.PerformContinueRebase);
            this.abortRebase = this.GenerateGitCommand(canContinueRebase, this.PerformAbortRebase);
            this.pushForce = this.GenerateGitCommand(this.PerformPushForce);
            this.fetchOrigin = this.GenerateGitCommand(this.PerformFetchOrigin);
            var updateCommand = ReactiveCommand.CreateAsyncTask(async _ => await this.PerformUpdateCommitMessage(CancellationToken.None));
            updateCommand.Subscribe(x => this.CommitMessage = x);
            this.updateCommitMessage = updateCommand;

            this.refresh = ReactiveCommand.Create();
            this.refresh.Subscribe(_ => this.RefreshInternal());

            this.isBusy = new[]
            {
                this.squash.IsExecuting, this.rebase.IsExecuting, this.continueRebase.IsExecuting,
                this.cancelOperation.IsExecuting, this.abortRebase.IsExecuting,
                this.fetchOrigin.IsExecuting, this.pushForce.IsExecuting, this.refresh.IsExecuting,
                this.updateCommitMessage.IsExecuting
            }.Merge().ToProperty(this, x => x.IsBusy, out this.isBusy);

            this.WhenAnyValue(x => x.SelectedCommit).InvokeCommand(this.updateCommitMessage);
            this.LogOptions = new GitLogOptionsViewModel { Value = Settings.Default.HistorySetting };
            this.DoForcePush = Settings.Default.PerformPush;
            this.ApplyRebase = Settings.Default.PerformRebaseAfterSquash;

            this.WhenAnyValue(x => x.LogOptions, x => x.DoForcePush, x => x.ApplyRebase).Skip(1).Subscribe(x =>
            {
                Settings.Default.HistorySetting = x.Item1?.Value ?? GitLogOptions.BranchOnlyAndParent;
                Settings.Default.PerformPush = x.Item2;
                Settings.Default.PerformRebaseAfterSquash = x.Item3;
                Settings.Default.Save();
            });

            this.LogOptions.WhenAnyValue(x => x.Value).Skip(1).Subscribe(x =>
            {
                Settings.Default.HistorySetting = x;
                Settings.Default.Save();

                this.Refresh();
            });
        }

        /// <inheritdoc />
        public ICommand Squash => this.squash;

        /// <inheritdoc />
        public ICommand Rebase => this.rebase;

        /// <inheritdoc />
        public ICommand ContinueRebase => this.continueRebase;

        /// <inheritdoc />
        public ICommand ViewConflictsPage { get; }

        /// <inheritdoc />
        public ICommand ViewChangesPage { get; }

        /// <inheritdoc />
        public ICommand AbortRebase => this.abortRebase;

        /// <inheritdoc />
        public ICommand CancelOperation => this.cancelOperation;

        /// <inheritdoc />
        public ICommand PushForce => this.pushForce;

        /// <inheritdoc />
        public ICommand FetchOrigin => this.fetchOrigin;

        /// <inheritdoc />
        public bool IsConflicts
        {
            get
            {
                return this.isConflicts;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isConflicts, value);
            }
        }

        /// <inheritdoc />
        public bool IsBusy => this.isBusy.Value;

        /// <inheritdoc />
        public bool ApplyRebase
        {
            get
            {
                return this.applyRebase;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.applyRebase, value);
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
                this.RaiseAndSetIfChanged(ref this.squashWrapper, value);
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
                this.RaiseAndSetIfChanged(ref this.currentBranch, value);
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
                this.RaiseAndSetIfChanged(ref this.selectedCommit, value);
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
                this.RaiseAndSetIfChanged(ref this.rebaseInProgress, value);
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
                this.RaiseAndSetIfChanged(ref this.branches, value);
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
                this.RaiseAndSetIfChanged(ref this.branchCommits, value);
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
                this.RaiseAndSetIfChanged(ref this.rebaseBranch, value);
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
                this.RaiseAndSetIfChanged(ref this.gitCommandResponse, value);
            }
        }

        /// <inheritdoc />
        public GitLogOptionsViewModel LogOptions
        {
            get
            {
                return this.gitLogOptions;
            }

            private set
            {
                this.RaiseAndSetIfChanged(ref this.gitLogOptions, value);
            }
        }

        /// <inheritdoc />
        public bool? OperationSuccess => this.isOperationSuccess.Value;

        /// <inheritdoc />
        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isDirty, value);
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
                this.RaiseAndSetIfChanged(ref this.commitMessage, value);
            }
        }

        /// <inheritdoc />
        public bool DoForcePush
        {
            get
            {
                return this.forcePush;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.forcePush, value);
            }
        }

        /// <inheritdoc />
        public void Refresh()
        {
            if (this.refresh.CanExecute(null))
            {
                this.refresh.Execute(null);
            }
        }

        private async void RefreshInternal()
        {
            string oldCommitText = this.CommitMessage;
            GitCommit oldCommit = this.SelectedCommit;

            this.Branches = await this.SquashWrapper.GetBranches(CancellationToken.None);
            this.CurrentBranch = await this.SquashWrapper.GetCurrentBranch(CancellationToken.None);

            if (this.Branches?.Contains(this.SelectedRebaseBranch) == false)
            {
                this.SelectedRebaseBranch = this.Branches?.FirstOrDefault(x => x.FriendlyName == "origin/master");
            }

            this.IsRebaseInProgress = this.SquashWrapper.IsRebaseHappening();
            this.IsConflicts = await this.SquashWrapper.HasConflicts(CancellationToken.None);
            this.IsDirty = await this.SquashWrapper.IsWorkingDirectoryDirty(CancellationToken.None) && !await this.SquashWrapper.HasConflicts(CancellationToken.None);
            this.BranchCommits = await this.SquashWrapper.GetCommitsForBranch(this.CurrentBranch, CancellationToken.None, this.LogOptions.Value);
            this.SelectedCommit = this.BranchCommits.FirstOrDefault(x => x == oldCommit);
            this.CommitMessage = string.IsNullOrWhiteSpace(oldCommitText) ? this.CommitMessage : oldCommitText;
        }

        private void PerformCancelOperation()
        {
            try
            {
                this.tokenSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private ReactiveCommand<GitCommandResponse> GenerateGitCommand(IObservable<bool> canSquashObservable, Func<CancellationToken, Task<GitCommandResponse>> func)
        {
            ReactiveCommand<GitCommandResponse> command = ReactiveCommand.CreateAsyncTask(canSquashObservable, async _ =>
            {
                try
                {
                    this.tokenSource?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                this.tokenSource = new CancellationTokenSource();
                return await func(this.tokenSource.Token);
            });

            command.Subscribe(x =>
            {
                this.GitCommandResponse = x;
                this.Refresh();
            });

            command.ThrownExceptions.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ex => this.GitCommandResponse = new GitCommandResponse(false, ex.Message, null, 0));

            return command;
        }

        private ReactiveCommand<GitCommandResponse> GenerateGitCommand(Func<CancellationToken, Task<GitCommandResponse>> func)
        {
            ReactiveCommand<GitCommandResponse> command = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                try
                {
                    this.tokenSource?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                this.tokenSource = new CancellationTokenSource();
                return await func(this.tokenSource.Token);
            });

            command.Subscribe(x =>
            {
                this.GitCommandResponse = x;
                this.Refresh();
            });

            command.ThrownExceptions.ObserveOn(RxApp.MainThreadScheduler).Subscribe(ex => this.GitCommandResponse = new GitCommandResponse(false, ex.Message, null, 0));

            return command;
        }

        private async Task<GitCommandResponse> PerformPushForce(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }

            GitCommandResponse forcePushOutput = await this.SquashWrapper.PushForce(token);
            return forcePushOutput;
        }

        private async Task<GitCommandResponse> PerformFetchOrigin(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }

            GitCommandResponse result = await this.SquashWrapper.FetchOrigin(token);
            return result;
        }

        private async Task<GitCommandResponse> PerformSquash(CancellationToken token)
        {
            GitCommandResponse squashOutput = await this.SquashWrapper.Squash(token, this.CommitMessage, this.SelectedCommit);

            if (squashOutput.Success == false || token.IsCancellationRequested)
            {
                return squashOutput;
            }

            GitCommandResponse forcePushOutput = null;
            if (this.DoForcePush)
            {
                forcePushOutput = await this.PerformPushForce(token);
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

            if (this.ApplyRebase && this.DoForcePush)
            {
                forcePushOutput = await this.PerformPushForce(token);
            }

            StringBuilder sb = new StringBuilder();
            if (squashOutput.Success)
            {
                sb.AppendLine(squashOutput.OutputMessage);
            }

            if (forcePushOutput != null && forcePushOutput.Success)
            {
                sb.AppendLine(forcePushOutput.OutputMessage);
            }

            if (rebaseOutput != null && rebaseOutput.Success)
            {
                sb.AppendLine(rebaseOutput.OutputMessage);
            }

            return new GitCommandResponse(true, sb.ToString(), null, 0);
        }

        private async Task<GitCommandResponse> PerformRebase(CancellationToken token)
        {
            var rebaseOutput = await this.SquashWrapper.Rebase(token, this.SelectedRebaseBranch);

            GitCommandResponse forcePushOutput = null;
            if (this.DoForcePush)
            {
                forcePushOutput = await this.SquashWrapper.PushForce(token);

                if (forcePushOutput.Success == false || token.IsCancellationRequested)
                {
                    return forcePushOutput;
                }
            }

            StringBuilder sb = new StringBuilder();
            if (rebaseOutput != null && rebaseOutput.Success)
            {
                sb.AppendLine(rebaseOutput.OutputMessage);
            }

            if (forcePushOutput != null && forcePushOutput.Success)
            {
                sb.AppendLine(forcePushOutput.OutputMessage);
            }

            return new GitCommandResponse(true, sb.ToString(), null, 0);
        }

        private Task<GitCommandResponse> PerformAbortRebase(CancellationToken token)
        {
            return this.SquashWrapper.Abort(token);
        }

        private Task<GitCommandResponse> PerformContinueRebase(CancellationToken token)
        {
            return this.SquashWrapper.Continue(token);
        }

        private Task<string> PerformUpdateCommitMessage(CancellationToken token)
        {
            return this.SquashWrapper.GetCommitMessages(this.SelectedCommit, token);
        }
    }
}