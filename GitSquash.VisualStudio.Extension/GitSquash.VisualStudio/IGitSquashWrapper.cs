// <copyright file="IGitSquashWrapper.cs" company="Glenn Watson">
// Copyright (c) Glenn Watson. All rights reserved.
// </copyright>

namespace GitSquash.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A wrapper around git that will provide the git squash actions.
    /// </summary>
    public interface IGitSquashWrapper : IDisposable
    {
        /// <summary>
        /// Determines if there are any conflicts. 
        /// </summary>
        /// <returns>True if there is a conflict, false otherwise.</returns>
        bool HasConflicts();

        /// <summary>
        /// Determines if there is currently a rebase in operation.
        /// </summary>
        /// <returns>True if rebase in operation, false otherwise.</returns>
        bool IsRebaseHappening();

        /// <summary>
        /// Determines if the working directory is dirty or not.
        /// </summary>
        /// <returns>True if the working directory is dirty, false otherwise.</returns>
        bool IsWorkingDirectoryDirty();

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="newCommitMessage">The new commit message for the squashed commit.</param>
        /// <param name="startCommit">The commit to start the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        Task<GitCommandResponse> Squash(string newCommitMessage, GitCommit startCommit);

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="parentBranch">The branch parent to parent the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        Task<GitCommandResponse> Rebase(GitBranch parentBranch);

        /// <summary>
        /// Aborts a attempt to squash/rebase.
        /// </summary>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> Abort();

        /// <summary>
        /// Indicates we want to continue after a conflict was found.
        /// </summary>
        /// <param name="commitMessage">The commit message.</param>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> Continue(string commitMessage);

        /// <summary>
        /// Gets a list of remote branches.
        /// </summary>
        /// <returns>The remote branches.</returns>
        IList<GitBranch> GetBranches();

        /// <summary>
        /// Gets the commit message since the parent branch.
        /// </summary>
        /// <param name="startCommit">The start commit.</param>
        /// <returns>All the commit messages</returns>
        string GetCommitMessages(GitCommit startCommit);

        /// <summary>
        /// Gets the current branch of the repository.
        /// </summary>
        /// <returns>The current branch.</returns>
        GitBranch GetCurrentBranch();

        /// <summary>
        /// Performs a push to the GIT repository using a force.
        /// </summary>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> PushForce();

        /// <summary>
        /// Fetches the origin.
        /// </summary>
        /// <returns>Details about the operation.</returns>
        Task<GitCommandResponse> FetchOrigin();

        /// <summary>
        /// Gets the commits for a branch.
        /// </summary>
        /// <param name="branch">The branch to get the details for.</param>
        /// <param name="number">The number of commits to retrieve for.</param>
        /// <returns>A collection of commits.</returns>
        IEnumerable<GitCommit> GetCommitsForBranch(GitBranch branch, int number = 25);
    }
}
