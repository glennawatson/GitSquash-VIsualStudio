namespace GitSquash.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Git.VisualStudio;

    using LibGit2Sharp;

    using PropertyChanged;

    /// <summary>
    /// A wrapper that performs the git squash operations.
    /// </summary>
    [ImplementPropertyChanged]
    public class GitSquashWrapper : IGitSquashWrapper
    {

        private readonly string repoDirectory;

        private readonly IGitProcessManager gitProcess;

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
        }

        /// <inheritdoc />
        public string GetCommitMessages(GitCommit startCommit)
        {
            if (startCommit == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            using (Repository repository = this.GetRepository())
            {
                foreach (Commit commit in repository.Head.Commits.TakeWhile(x => x.Sha != startCommit.Sha))
                {
                    sb.AppendLine(commit.Message.Trim());
                }
            }

            return TrimEmptyLines(sb.ToString());
        }

        /// <inheritdoc />
        public GitBranch GetCurrentBranch()
        {
            using (Repository repository = this.GetRepository())
            {
                return new GitBranch(repository.Head.FriendlyName);
            }
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> PushForce(CancellationToken token)
        {
            using (Repository repository = this.GetRepository())
            {
                if (repository.Head.IsTracking == false)
                {
                    return new GitCommandResponse(false, "The branch has not been pushed before.");
                }

                return await this.gitProcess.RunGit("push -f", token);
            }
        }

        /// <inheritdoc />
        public IEnumerable<GitCommit> GetCommitsForBranch(GitBranch branch, int number = 25)
        {
            using (Repository repository = this.GetRepository())
            {
                Branch internalBranch = repository.Branches.FirstOrDefault(x => x.FriendlyName == branch.FriendlyName);

                if (internalBranch == null)
                {
                    return Enumerable.Empty<GitCommit>();
                }

                return internalBranch.Commits.Take(number).Select(x => new GitCommit(x.Sha, x.MessageShort)).ToList();
            }
        }

        /// <inheritdoc />
        public bool IsRebaseHappening()
        {
            bool isFile = Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-apply"));

            return isFile || Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-merge"));
        }

        /// <inheritdoc />
        public bool IsWorkingDirectoryDirty()
        {
            using (Repository repository = this.GetRepository())
            {
                return repository.RetrieveStatus().IsDirty;
            }
        }

        /// <inheritdoc />
        public bool HasConflicts()
        {
            using (Repository repository = this.GetRepository())
            {
                return repository.Index.IsFullyMerged == false;
            }
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Squash(CancellationToken token, string newCommitMessage, GitCommit startCommit)
        {
            using (Repository repository = this.GetRepository())
            {
                if (repository.RetrieveStatus().IsDirty)
                {
                    return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.");
                }
            }

            string rewriterName;
            string commentWriterName;
            if (this.GetWritersName(out rewriterName, out commentWriterName) == false)
            {
                return new GitCommandResponse(false, "Cannot get valid paths to GIT parameters");
            }

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, newCommitMessage);

            var environmentVariables = new Dictionary<string, string> { { "COMMENT_FILE_NAME", fileName } };

            return await this.gitProcess.RunGit($"-c core.quotepath=false -c \"sequence.editor=\'{rewriterName}\'\" -c \"core.editor=\'{commentWriterName}\'\" rebase -i  {startCommit.Sha}", token, environmentVariables);
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
            using (Repository repository = this.GetRepository())
            {
                if (repository.RetrieveStatus().IsDirty)
                {
                    return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.");
                }
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
                return new GitCommandResponse(false, "Cannot get valid paths to GIT parameters");
            }

            return await this.gitProcess.RunGit($"-c core.quotepath=false -c \"core.editor=\'{commentWriterName}\'\"  rebase --continue", token);
        }

        /// <inheritdoc />
        public IList<GitBranch> GetBranches()
        {
            using (Repository repository = this.GetRepository())
            {
                return repository.Branches.OrderBy(x => x.FriendlyName).Select(x => new GitBranch(x.FriendlyName)).ToList();
            }
        }

        private static string TrimEmptyLines(string input)
        {
            input = input.Trim('\r', '\n');
            input = input.Trim();

            return input;
        }


        private Repository GetRepository()
        {
            return new Repository(this.repoDirectory);
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