// <copyright file="IGitSquashWrapper.cs" company="Glenn Watson">
// Copyright (c) Glenn Watson. All rights reserved.
// </copyright>

namespace GitRebase.VisualStudio
{
    using System;
    using System.Collections.Generic;

    using LibGit2Sharp;

    /// <summary>
    /// A wrapper around git that will provide the git squash actions.
    /// </summary>
    public interface IGitSquashWrapper : IDisposable
    {
        /// <summary>
        /// Determines if there is currently a rebase in operation.
        /// </summary>
        /// <returns>True if rebase in operation, false otherwise.</returns>
        bool IsRebaseHappening();

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="newCommitMessage">The new commit message for the squashed commit.</param>
        /// <param name="startCommit">The commit to start the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        GitCommandResponse Squash(string newCommitMessage, Commit startCommit);

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        /// <param name="newCommitMessage">The new commit message for the squashed commit.</param>
        /// <param name="parentBranch">The branch parent to parent the rebase/squash from.</param>
        /// <returns>Details about the commit.</returns>
        GitCommandResponse Squash(string newCommitMessage, Branch parentBranch);

        /// <summary>
        /// Aborts a attempt to squash/rebase.
        /// </summary>
        void Abort();

        /// <summary>
        /// Indicates we want to continue after a conflict was found.
        /// </summary>
        void Continue();

        /// <summary>
        /// Gets a list of remote branches.
        /// </summary>
        /// <returns>The remote branches.</returns>
        IEnumerable<Branch> GetRemoteBranches();

        /// <summary>
        /// Gets the commit message since the parent branch.
        /// </summary>
        /// <param name="startCommit">The start commit.</param>
        /// <returns>All the commit messages</returns>
        string GetCommitMessages(Commit startCommit);

        /// <summary>
        /// Gets the current branch of the repository.
        /// </summary>
        /// <returns>The current branch.</returns>
        Branch GetCurrentBranch();

        /// <summary>
        /// Performs a push to the GIT repository using a force.
        /// </summary>
        /// <returns>Details about the operation.</returns>
        GitCommandResponse PushForce();
    }
}
