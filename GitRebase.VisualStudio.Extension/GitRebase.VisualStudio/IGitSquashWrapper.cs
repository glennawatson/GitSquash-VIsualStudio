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
        /// Gets or sets the parent item that the git squash will be performed on.
        /// </summary>
        string ParentBranch { get; set; }

        /// <summary>
        /// Performs the squash action.
        /// </summary>
        void Squash(); 

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
    }
}
