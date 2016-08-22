namespace Git.VisualStudio
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages and handling getting branch information.
    /// </summary>
    public interface IBranchManager
    {
        /// <summary>
        /// Gets a list of local branches.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The local branches.</returns>
        Task<IList<GitBranch>> GetLocalBranches(CancellationToken token);

        /// <summary>
        /// Gets a list of remote branches.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The remote branches.</returns>
        Task<IList<GitBranch>> GetRemoteBranches(CancellationToken token);

        /// <summary>
        /// Get a list of both the local and remote branches.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The local and remote branches.</returns>
        Task<IList<GitBranch>> GetLocalAndRemoteBranches(CancellationToken token);

        /// <summary>
        /// Gets the current checked out branch.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The local checked out branch.</returns>
        Task<GitBranch> GetCurrentCheckedOutBranch(CancellationToken token);

        /// <summary>
        /// Gets the commits for the specified branch.
        /// </summary>
        /// <param name="branch">The branch to get the commits for.</param>
        /// <param name="skip">Number of commits to skip.</param>
        /// <param name="limit">The number of commits to bring back.</param>
        /// <param name="logOptions">The options for how to order the git log.</param>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>A list of git commits.</returns>
        Task<IList<GitCommit>> GetCommitsForBranch(GitBranch branch, int skip, int limit, GitLogOptions logOptions, CancellationToken token);

        /// <summary>
        /// Gets the command messages after the specified parent object.
        /// </summary>
        /// <param name="parent">The parent to get the commit messages for.</param>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The commit messages</returns>
        Task<string> GetCommitMessagesAfterParent(GitCommit parent, CancellationToken token);

        /// <summary>
        /// Gets a remote branch of the specified branch.
        /// </summary>
        /// <param name="branch">The branch to get the </param>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>The commit messages</returns>
        Task<GitBranch> GetRemoteBranch(GitBranch branch, CancellationToken token);

        /// <summary>
        /// Determines if there are any changes in the working directory.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>If there are changes in the working directory.</returns>
        Task<bool> IsWorkingDirectoryDirty(CancellationToken token);

        /// <summary>
        /// Determines if there are any merge conflicts.
        /// </summary>
        /// <param name="token">A cancellation token to stop the process.</param>
        /// <returns>If there are any merge conflicts.</returns>
        Task<bool> IsMergeConflict(CancellationToken token);
    }
}
