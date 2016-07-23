namespace Git.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Properties;

    /// <summary>
    /// Manages and starts GIT processes.
    /// </summary>
    public class GitProcessManager : IGitProcessManager
    {
        private static IOutputLogger outputLogger;

        private readonly string repoDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitProcessManager"/> class.
        /// </summary>
        /// <param name="repoDirectory">The location of the GIT repository.</param>
        /// <param name="newOutputLogger">The output logger where to send output.</param>
        public GitProcessManager(string repoDirectory, IOutputLogger newOutputLogger)
        {
            this.repoDirectory = repoDirectory;
            outputLogger = newOutputLogger;
        }

        /// <summary>
        /// Runs a new instance of GIT.
        /// </summary>
        /// <param name="gitArguments">The arguments to pass to GIT.</param>
        /// <param name="token">A cancellation token with the ability to cancel the process.</param>
        /// <param name="extraEnvironmentVariables">Any environment variables to pass for the process.</param>
        /// <param name="callerMemberName">The member calling the process.</param>
        /// <returns>A task which will return the response from the GIT process.</returns>
        public async Task<GitCommandResponse> RunGit(string gitArguments, CancellationToken token, IDictionary<string, string> extraEnvironmentVariables = null, [CallerMemberName] string callerMemberName = null)
        {
            outputLogger.WriteLine($"execute: git {gitArguments}");

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

                int returnValue = await RunProcessAsync(process, token);

                if (returnValue == 1)
                {
                    return new GitCommandResponse(false, $"{callerMemberName} failed. See output window.");
                }

                return new GitCommandResponse(true, $"{callerMemberName} succeeded.");
            }
        }

        private static Process CreateGitProcess(string arguments, string repoDirectory)
        {
            string gitInstallationPath = GitHelper.GetGitInstallationPath();
            string pathToGit = Path.Combine(Path.Combine(gitInstallationPath, "bin\\git.exe"));
            return new Process { StartInfo = { CreateNoWindow = true, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, FileName = pathToGit, Arguments = arguments, WorkingDirectory = repoDirectory }, EnableRaisingEvents = true };
        }

        private static Task<int> RunProcessAsync(Process process, CancellationToken token)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            token.Register(() =>
            {
                process.Kill();
                outputLogger.WriteLine(Resources.KillingProcess);
            });

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

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null)
            {
                return;
            }

            outputLogger.WriteLine(dataReceivedEventArgs.Data);
        }

        private void OnErrorReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data == null)
            {
                return;
            }

            if (!dataReceivedEventArgs.Data.StartsWith("fatal:", StringComparison.OrdinalIgnoreCase))
            {
                outputLogger.WriteLine(dataReceivedEventArgs.Data);
                return;
            }

            outputLogger.WriteLine(string.Format(Resources.ErrorRunningGit, dataReceivedEventArgs.Data));
        }
    }
}
