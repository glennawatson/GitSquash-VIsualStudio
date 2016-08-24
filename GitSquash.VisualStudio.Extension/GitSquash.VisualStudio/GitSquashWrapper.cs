namespace GitSquash.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Git.VisualStudio;

    using PropertyChanged;

    /// <summary>
    /// A wrapper that performs the git squash operations.
    /// </summary>
    [ImplementPropertyChanged]
    public class GitSquashWrapper : IGitSquashWrapper
    {

        private readonly string repoDirectory;

        private readonly IGitProcessManager gitProcess;

        private readonly IBranchManager branchManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSquashWrapper" /> class.
        /// </summary>
        /// <param name="repoDirectory">The directory where the repository is located</param>
        /// <param name="logger">The output logger to output git transactions.</param>
        /// <param name="gitProcess">The git process to use.</param>
        public GitSquashWrapper(string repoDirectory, IOutputLogger logger, IGitProcessManager gitProcess = null)
        {
            this.repoDirectory = repoDirectory;
            this.gitProcess = gitProcess ?? new GitProcessManager(repoDirectory, logger);
            this.branchManager = new BranchManager(this.repoDirectory, null);
        }

        /// <inheritdoc />
        public async Task<string> GetCommitMessages(GitCommit startCommit, CancellationToken token)
        {
            if (startCommit == null)
            {
                return null;
            }

            var result = await this.branchManager.GetCommitMessagesAfterParent(startCommit, token);

            return result.TrimEmptyLines();
        }

        /// <inheritdoc />
        public Task<GitBranch> GetCurrentBranch(CancellationToken token)
        {
            return this.branchManager.GetCurrentCheckedOutBranch(token);
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> PushForce(CancellationToken token)
        {
            if (await this.branchManager.GetRemoteBranch(await this.branchManager.GetCurrentCheckedOutBranch(token), token) != null)
            {
                return new GitCommandResponse(false, "The branch has not been pushed before.", null, 0);
            }

            return await this.gitProcess.RunGit("push -f", token);
        }

        /// <inheritdoc />
        public Task<IList<GitCommit>> GetCommitsForBranch(GitBranch branch, CancellationToken token, int number = 25)
        {
            return this.branchManager.GetCommitsForBranch(branch, 0, number, GitLogOptions.BranchOnlyAndParent, token);
        }

        /// <inheritdoc />
        public bool IsRebaseHappening()
        {
            bool isFile = Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-apply"));

            return isFile || Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-merge"));
        }

        /// <inheritdoc />
        public Task<bool> IsWorkingDirectoryDirty(CancellationToken token)
        {
            return this.branchManager.IsWorkingDirectoryDirty(token);
        }

        /// <inheritdoc />
        public Task<bool> HasConflicts(CancellationToken token)
        {
            return this.branchManager.IsMergeConflict(token);
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Squash(CancellationToken token, string newCommitMessage, GitCommit startCommit)
        {
            if (await this.branchManager.IsWorkingDirectoryDirty(token))
            {
                return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.", null, 0);
            }

            string rewriterName;
            string commentWriterName;
            if (this.GetWritersName(out rewriterName, out commentWriterName) == false)
            {
                return new GitCommandResponse(false, "Cannot get valid paths to GIT parameters", null, 0);
            }

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, newCommitMessage);

            var environmentVariables = new Dictionary<string, string> { { "COMMENT_FILE_NAME", fileName } };

            return await this.gitProcess.RunGit($"-c \"sequence.editor=\'{rewriterName}\'\" -c \"core.editor=\'{commentWriterName}\'\" rebase -i  {startCommit.Sha}", token, environmentVariables);
        }

        /// <inheritdoc />
        public Task<GitCommandResponse> FetchOrigin(CancellationToken token)
        {
            Task<GitCommandResponse> response = this.gitProcess.RunGit("fetch -v origin", token);

            return response;
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Rebase(CancellationToken token, GitBranch parentBranch)
        {
            if (await this.branchManager.IsWorkingDirectoryDirty(token))
            {
                return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.", null, 0);
            }

            GitCommandResponse response = await this.FetchOrigin(token);

            if (response.Success == false)
            {
                return response;
            }

            return await this.gitProcess.RunGit($"rebase  {parentBranch.FriendlyName}", token);
        }

        /// <inheritdoc />
        public Task<GitCommandResponse> Abort(CancellationToken token)
        {
            return this.gitProcess.RunGit("rebase --abort", token);
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Continue(CancellationToken token)
        {
            string rewriterName;
            string commentWriterName;
            if (this.GetWritersName(out rewriterName, out commentWriterName) == false)
            {
                return new GitCommandResponse(false, "Cannot get valid paths to GIT parameters", null, 0);
            }

            return await this.gitProcess.RunGit($"-c core.quotepath=false -c \"core.editor=\'{commentWriterName}\'\"  rebase --continue", token);
        }

        /// <inheritdoc />
        public Task<IList<GitBranch>> GetBranches(CancellationToken token)
        {
            return this.branchManager.GetLocalAndRemoteBranches(token);
        }

        private bool GetWritersName(out string rebaseWriter, out string commentWriter)
        {
            rebaseWriter = null;
            commentWriter = null;

            try
            {

                string location = Assembly.GetExecutingAssembly().Location;

                if (string.IsNullOrWhiteSpace(location))
                {
                    return false;
                }

                string directoryName = Path.GetDirectoryName(location);

                if (string.IsNullOrWhiteSpace(directoryName))
                {
                    return false;
                }

                rebaseWriter = Path.Combine(directoryName, "rebasewriter.exe");
                commentWriter = Path.Combine(directoryName, "commentWriter.exe");
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
        }
    }
}