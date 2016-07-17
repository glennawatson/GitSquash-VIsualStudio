﻿namespace GitSquash.VisualStudio
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a branch in GIT.
    /// </summary>
    [DebuggerDisplay("{FriendlyName}")]
    public class GitBranch : IEquatable<GitBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitBranch"/> class.
        /// </summary>
        /// <param name="friendlyName">A human friendly name of the branch.</param>
        public GitBranch(string friendlyName)
        {
            this.FriendlyName = friendlyName;
        }

        /// <summary>
        /// Gets the description of the branch.
        /// </summary>
        public string FriendlyName { get;  }

        /// <summary>
        /// Operator for equality.
        /// </summary>
        /// <param name="left">The left comparison object.</param>
        /// <param name="right">The right comparison object.</param>
        /// <returns>If the two sides match.</returns>
        public static bool operator ==(GitBranch left, GitBranch right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if the two branch objects are not equal to each other.
        /// </summary>
        /// <param name="left">The left comparison object.</param>
        /// <param name="right">The right comparison object.</param>
        /// <returns>If the two sides do not equal each other.</returns>
        public static bool operator !=(GitBranch left, GitBranch right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Determines if two branches are equal to each other.
        /// </summary>
        /// <param name="other">The other branch to compare.</param>
        /// <returns>If they are equal to each other.</returns>
        public bool Equals(GitBranch other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || string.Equals(this.FriendlyName, other.FriendlyName);
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

            return obj.GetType() == this.GetType() && this.Equals((GitBranch)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.FriendlyName?.GetHashCode() ?? 0;
        }
    }
}
