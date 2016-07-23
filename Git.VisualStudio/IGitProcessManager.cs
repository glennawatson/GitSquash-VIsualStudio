namespace Git.VisualStudio
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a process running GIT.
    /// </summary>
    public interface IGitProcessManager
    {
        /// <summary>
        /// Runs a new GIT instance.
        /// </summary>
        /// <param name="gitArguments">The arguments to pass to GIT.</param>
        /// <param name="token">A cancellation token to allow for process cancellation.</param>
        /// <param name="extraEnvironmentVariables">Environment variables to pass.</param>
        /// <param name="callerMemberName">The caller of the process.</param>
        /// <returns>A task which will return the exit code from GIT.</returns>
        Task<GitCommandResponse> RunGit(string gitArguments, CancellationToken token, IDictionary<string, string> extraEnvironmentVariables = null, [CallerMemberName] string callerMemberName = null);
    }
}