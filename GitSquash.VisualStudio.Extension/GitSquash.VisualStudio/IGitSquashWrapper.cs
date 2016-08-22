namespace GitSquash.VisualStudio
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Git.VisualStudio;

    /// <summary>
    /// A wrapper around GIT that will provide the GIT squash actions.
    /// </summary>
    public interface IGitSquashWrapper
    {
        /// <summary>
        /// Determines if there are any conflicts. 
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>True if there is a conflict, false otherwise.</returns>
        Task<bool> HasConflicts(CancellationToken token);

        /// <summary>
        /// Determines if there is currently a rebase in operation.
        /// </summary>
        /// <returns>True if rebase in operation, false otherwise.</returns>
        bool IsRebaseHappening();

        /// <summary>
        /// Determines if the working directory is dirty or not.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>True if the working directory is dirty, false otherwise.</returns>
        Task<bool> IsWorkingDirectoryDirty(CancellationToken token);

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <param name="newCommitMessage">The new commit message for the squashed commit.</param>
        /// <param name="startCommit">The commit to start the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        Task<GitCommandResponse> Squash(CancellationToken token, string newCommitMessage, GitCommit startCommit);

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <param name="parentBranch">The branch parent to parent the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        Task<GitCommandResponse> Rebase(CancellationToken token, GitBranch parentBranch);

        /// <summary>
        /// Aborts a attempt to squash/rebase.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> Abort(CancellationToken token);

        /// <summary>
        /// Indicates we want to continue after a conflict was found.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> Continue(CancellationToken token);

        /// <summary>
        /// Performs a push to the GIT repository using a force.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> PushForce(CancellationToken token);

        /// <summary>
        /// Fetches the origin.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> FetchOrigin(CancellationToken token);

        /// <summary>
        /// Gets a list of remote branches.
        /// </summary>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>The remote branches.</returns>
        Task<IList<GitBranch>> GetBranches(CancellationToken token);

        /// <summary>
        /// Gets the commit message since the parent branch.
        /// </summary>
        /// <param name="startCommit">The start commit.</param>
        /// <param name="token">A cancellation token that allows for the operation to be exited early.</param>
        /// <returns>All the commit messages</returns>
        Task<string> GetCommitMessages(GitCommit startCommit, CancellationToken token);

        /// <summary>
        /// Gets the current branch of the repository.
        /// </summary>
        /// <param name="token">The cancellation token able to cancel the task.</param>
        /// <returns>The current branch.</returns>
        Task<GitBranch> GetCurrentBranch(CancellationToken token);

        /// <summary>
        /// Gets the commits for a branch.
        /// </summary>
        /// <param name="branch">The branch to get the details for.</param>
        /// <param name="token">The cancellation token able to cancel the task.</param>
        /// <param name="number">The number of commits to retrieve for.</param>
        /// <returns>A collection of commits.</returns>
        Task<IList<GitCommit>> GetCommitsForBranch(GitBranch branch, CancellationToken token, int number = 25);
    }
}
