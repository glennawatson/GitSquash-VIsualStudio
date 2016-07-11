namespace GitRebase.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

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

        private readonly string repoDirectory;

        private readonly Repository repository;

        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitSquashWrapper"/> class.
        /// </summary>
        /// <param name="repoDirectory">The directory where the repository is located</param>
        public GitSquashWrapper(string repoDirectory)
        {
            this.repoDirectory = repoDirectory;

            this.repository = new Repository(repoDirectory);
        }

        /// <summary>
        /// Delegate that passes along a string.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="param">The string parameter.</param>
        public delegate void StringDelegate(object sender, string param);

        /// <summary>
        /// A event that occurs when there is output from the git command line.
        /// </summary>
        public event StringDelegate GitOutput;

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string GetCommitMessages(Commit startCommit)
        {
            if (startCommit == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (Commit commit in this.GetCurrentBranch().Commits.SkipWhile(x => x.Sha != startCommit.Sha))
            {
                sb.AppendLine(commit.Message);
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public Branch GetCurrentBranch()
        {
            return this.repository.Head;
        }

        /// <inheritdoc />
        public GitCommandResponse PushForce()
        {
            return this.RunGitFlow("push -f");
        }

        /// <inheritdoc />
        public bool IsRebaseHappening()
        {
            bool isFile = Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-apply"));

            return isFile || Directory.Exists(Path.Combine(this.repoDirectory, ".git/rebase-merge"));
        }

        /// <inheritdoc />
        public GitCommandResponse Squash(string newCommitMessage, Commit startCommit)
        {
            var rewriterName = Path.Combine(Assembly.GetExecutingAssembly().Location, "rebasewriter.exe");

            return this.RunGitFlow(
                $"rebase -i {startCommit.Sha}",
                new Dictionary<string, string> { { "GIT_INTERACTIVE_EDITOR", rewriterName } });
        }

        /// <inheritdoc />
        public GitCommandResponse Rebase(Branch parentBranch)
        {
            var response = this.RunGitFlow("fetch origin");

            if (response.Success == false)
            {
                return response;
            }

            var rewriterName = Path.Combine(Assembly.GetExecutingAssembly().Location, "rebasewriter.exe");

            return this.RunGitFlow(
                $"rebase -i {parentBranch.FriendlyName}",
                new Dictionary<string, string> { { "GIT_SEQUENCE_EDITOR", rewriterName } });
        }

        /// <inheritdoc />
        public void Abort()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Continue()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Branch> GetRemoteBranches()
        {
            return this.repository.Branches;
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

        private static Process CreateGitFlowProcess(string arguments, string repoDirectory)
        {
            var gitInstallationPath = GitHelper.GetGitInstallationPath();
            string pathToGit = Path.Combine(Path.Combine(gitInstallationPath, "bin\\git.exe"));
            return new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = pathToGit,
                    Arguments = arguments,
                    WorkingDirectory = repoDirectory
                }
            };
        }

        private GitCommandResponse RunGitFlow(string gitArguments, IDictionary<string, string> extraEnvironmentVariables = null)
        {
            error = new StringBuilder();
            output = new StringBuilder();

            using (var p = CreateGitFlowProcess(gitArguments, this.repoDirectory))
            {
                if (extraEnvironmentVariables != null)
                {
                    foreach (var kvp in extraEnvironmentVariables)
                    {
                        p.StartInfo.EnvironmentVariables.Add(kvp.Key, kvp.Value);
                    }
                }

                p.Start();
                p.ErrorDataReceived += this.OnErrorReceived;
                p.OutputDataReceived += this.OnOutputDataReceived;
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit(15000);
                if (!p.HasExited)
                {
                    p.WaitForExit(15000);
                    if (!p.HasExited)
                    {
                        return new GitCommandTimeOut("git " + p.StartInfo.Arguments);
                    }
                }

                if (error != null && error.Length > 0)
                {
                    return new GitCommandResponse(false, error.ToString());
                }

                return new GitCommandResponse(true, output.ToString());
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null)
            {
                return;
            }

            output.Append(dataReceivedEventArgs.Data);
            Debug.WriteLine(dataReceivedEventArgs.Data);
            this.GitOutput?.Invoke(this, dataReceivedEventArgs.Data);
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null
                || !dataReceivedEventArgs.Data.StartsWith("fatal:", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            error = new StringBuilder();
            error.Append(dataReceivedEventArgs.Data);
            Debug.WriteLine(dataReceivedEventArgs.Data);
        }
    }
}
