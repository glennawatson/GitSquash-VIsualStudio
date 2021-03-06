﻿namespace Git.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// A commit in GIT.
    /// </summary>
    [DebuggerDisplay("Id = {Sha}")]
    public class GitCommit : IEquatable<GitCommit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommit"/> class.
        /// </summary>
        /// <param name="sha">The Sha Id for this commit.</param>
        /// <param name="messageShort">A short message about the commit.</param>
        /// <param name="messageLong">The full commit message.</param>
        /// <param name="dateTime">The date time of the commit.</param>
        /// <param name="author">The author of the commit.</param>
        /// <param name="committer">The committer of the commit.</param>
        /// <param name="shaShort">The shorten version of the Sha hash.</param>
        /// <param name="parents">The parents of the commit.</param>
        public GitCommit(string sha, string messageShort, string messageLong, DateTime dateTime, string author, string committer, string shaShort, IReadOnlyList<string> parents)
        {
            this.Sha = sha;
            this.MessageShort = messageShort;
            this.MessageLong = messageLong;
            this.DateTime = dateTime;
            this.Author = author;
            this.Committer = committer;
            this.ShaShort = shaShort;
            this.Parents = parents;
        }
         
        /// <summary>
        /// Gets the Sha Id code.
        /// </summary>
        public string Sha { get; }

        /// <summary>
        /// Gets the short SHA value.
        /// </summary>
        public string ShaShort { get; }

        /// <summary>
        /// Gets the description of the commit.
        /// </summary>
        public string MessageShort { get; }

        /// <summary>
        /// Gets the full commit message.
        /// </summary>
        public string MessageLong { get; }

        /// <summary>
        /// Gets the date time of the commit.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Gets the author of the commit.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the commiter of the commit.
        /// </summary>
        public string Committer { get; }

        /// <summary>
        /// Gets a read only list of the parents of the commit.
        /// </summary>
        public IReadOnlyList<string> Parents { get; }

        /// <summary>
        /// Determines if two commits are equal to each other.
        /// </summary>
        /// <param name="left">The left side to compare.</param>
        /// <param name="right">The right side to compare.</param>
        /// <returns>If the commits are equal to each other.</returns>
        public static bool operator ==(GitCommit left, GitCommit right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if two commits are not equal to each other.
        /// </summary>
        /// <param name="left">The left side to compare.</param>
        /// <param name="right">The right side to compare.</param>
        /// <returns>If the commits are not equal to each other.</returns>
        public static bool operator !=(GitCommit left, GitCommit right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines if another instance of a commit is logically equal.
        /// </summary>
        /// <param name="other">The other commit.</param>
        /// <returns>If they are logically equal or not.</returns>
        public bool Equals(GitCommit other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || string.Equals(this.Sha, other.Sha);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == this.GetType() && this.Equals((GitCommit)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Sha?.GetHashCode() ?? 0;
        }
    }
}
