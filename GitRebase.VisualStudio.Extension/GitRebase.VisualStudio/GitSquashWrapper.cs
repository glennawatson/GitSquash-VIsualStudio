namespace GitSquash.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    using LibGit2Sharp;

    using PropertyChanged;

    /// <summary>
    /// A wrapper that performs the git squash operations.
    /// </summary>
    [ImplementPropertyChanged]
    public class GitSquashWrapper : IGitSquashWrapper
    {
        private static StringBuilder output = new StringBuilder();

        private static StringBuilder error = new StringBuilder();

        private readonly IGitSquashOutputLogger outputLogger;

        private readonly string repoDirectory;

        private readonly Repository repository;

        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSquashWrapper" /> class.
        /// </summary>
        /// <param name="repoDirectory">The directory where the repository is located</param>
        /// <param name="outputLogger">The output logger to output git transactions.</param>
        public GitSquashWrapper(string repoDirectory, IGitSquashOutputLogger outputLogger)
        {
            this.repoDirectory = repoDirectory;
            this.outputLogger = outputLogger;

            this.repository = new Repository(repoDirectory);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string GetCommitMessages(GitCommit startCommit)
        {
            if (startCommit == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            foreach (Commit commit in this.repository.Head.Commits.TakeWhile(x => x.Sha != startCommit.Sha))
            {
                sb.AppendLine(commit.Message);
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public GitBranch GetCurrentBranch()
        {
            return new GitBranch(this.repository.Head.FriendlyName);
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> PushForce()
        {
            if (this.repository.Head.IsTracking == false)
            {
                return new GitCommandResponse(false, "The branch has not been pushed before.");
            }

            return await this.RunGit("push -f");
        }

        /// <inheritdoc />
        public IEnumerable<GitCommit> GetCommitsForBranch(GitBranch branch, int number = 25)
        {
            Branch internalBranch = this.repository.Branches.FirstOrDefault(x => x.FriendlyName == branch.FriendlyName);

            if (internalBranch == null)
            {
                return Enumerable.Empty<GitCommit>();
            }

            return internalBranch.Commits.Take(number).Select(x => new GitCommit(x.Sha, x.MessageShort));
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
            return this.repository.RetrieveStatus().IsDirty;
        }

        /// <inheritdoc />
        public bool HasConflicts()
        {
            return this.repository.Index.IsFullyMerged == false;
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Squash(string newCommitMessage, GitCommit startCommit)
        {
            if (this.repository.RetrieveStatus().IsDirty)
            {
                return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.");
            }

            string location = Assembly.GetExecutingAssembly().Location;

            if (string.IsNullOrWhiteSpace(location))
            {
                return new GitCommandResponse(false, "Cannot find assembly location");
            }

            string directoryName;
            try
            {
                directoryName = Path.GetDirectoryName(location);
            }
            catch (ArgumentException ex)
            {
                return new GitCommandResponse(false, ex.Message);
            }
            catch (PathTooLongException ex)
            {
                return new GitCommandResponse(false, ex.Message);
            }

            if (directoryName == null)
            {
                return new GitCommandResponse(false, "Cannot find assembly location");
            }

            string rewriterName = Path.Combine(directoryName, "rebasewriter.exe");
            string commentWriterName = Path.Combine(directoryName, "commentWriter.exe");

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, newCommitMessage);

            var environmentVariables = new Dictionary<string, string> { { "COMMENT_FILE_NAME", fileName } };

            return await this.RunGit($"-c core.quotepath=false -c \"sequence.editor=\'{rewriterName}\'\" -c \"core.editor=\'{commentWriterName}\'\" rebase -i  {startCommit.Sha}", environmentVariables);
        }

        /// <inheritdoc />
        public Task<GitCommandResponse> FetchOrigin()
        {
            Task<GitCommandResponse> response = this.RunGit("fetch -v origin");

            return response;
        }

        /// <inheritdoc />
        public async Task<GitCommandResponse> Rebase(GitBranch parentBranch)
        {
            if (this.repository.RetrieveStatus().IsDirty)
            {
                return new GitCommandResponse(false, "Cannot rebase: You have unstaged changes.");
            }

            GitCommandResponse response = await this.FetchOrigin();

            if (response.Success == false)
            {
                return response;
            }

            return await this.RunGit($"rebase  {parentBranch.FriendlyName}");
        }

        /// <inheritdoc />
        public Task<GitCommandResponse> Abort()
        {
            return this.RunGit("rebase --abort");
        }

        /// <inheritdoc />
        public Task<GitCommandResponse> Continue(string commitMessage)
        {
            return this.RunGit("rebase --continue");
        }

        /// <inheritdoc />
        public IList<GitBranch> GetBranches()
        {
            return this.repository.Branches.OrderBy(x => x.FriendlyName).Select(x => new GitBranch(x.FriendlyName)).ToList();
        }

        /// <summary>
        /// Disposes of any objects contained in the class.
        /// </summary>
        /// <param name="isDisposing">If the ,method is being called by the dispose method.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                this.repository?.Dispose();
            }

            this.isDisposed = true;
        }

        private static Process CreateGitProcess(string arguments, string repoDirectory)
        {
            string gitInstallationPath = GitHelper.GetGitInstallationPath();
            string pathToGit = Path.Combine(Path.Combine(gitInstallationPath, "bin\\git.exe"));
            return new Process { StartInfo = { CreateNoWindow = true, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, FileName = pathToGit, Arguments = arguments, WorkingDirectory = repoDirectory }, EnableRaisingEvents = true };
        }

        private static Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (s, ea) => tcs.SetResult(process.ExitCode);

            bool started = process.Start();
            if (!started)
            {
                // you may allow for the process to be re-used (started = false) 
                // but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private async Task<GitCommandResponse> RunGit(string gitArguments, IDictionary<string, string> extraEnvironmentVariables = null, [CallerMemberName] string callerMemberName = null)
        {
            this.outputLogger.WriteLine($"execute: git {gitArguments}");
            error = new StringBuilder();
            output = new StringBuilder();

            using (Process process = CreateGitProcess(gitArguments, this.repoDirectory))
            {
                if (extraEnvironmentVariables != null)
                {
                    foreach (KeyValuePair<string, string> kvp in extraEnvironmentVariables)
                    {
                        process.StartInfo.EnvironmentVariables.Add(kvp.Key, kvp.Value);
                    }
                }

                process.ErrorDataReceived += this.OnErrorReceived;
                process.OutputDataReceived += this.OnOutputDataReceived;

                int returnValue = await RunProcessAsync(process);

                if (returnValue == 1)
                {
                    return new GitCommandResponse(false, $"{callerMemberName} failed. See output window.");
                }

                return new GitCommandResponse(true, $"{callerMemberName} succeeded.");
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null)
            {
                return;
            }

            output.Append(dataReceivedEventArgs.Data);
            this.outputLogger.WriteLine(dataReceivedEventArgs.Data);
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null)
            {
                return;
            }

            if (!dataReceivedEventArgs.Data.StartsWith("fatal:", StringComparison.OrdinalIgnoreCase))
            {
                this.outputLogger.WriteLine(dataReceivedEventArgs.Data);
                return;
            }

            error = new StringBuilder();
            error.Append(dataReceivedEventArgs.Data);
            Debug.WriteLine(dataReceivedEventArgs.Data);
            this.outputLogger.WriteLine($"Error: {dataReceivedEventArgs.Data}");
        }
    }
}