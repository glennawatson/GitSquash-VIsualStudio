namespace Git.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper which manages branch history.
    /// </summary>
    public class BranchManager : IBranchManager
    {
        private readonly GitProcessManager gitProcessManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchManager"/> class.
        /// </summary>
        /// <param name="repoPath">The directory to the repo.</param>
        /// <param name="logger">The logger to use.</param>
        public BranchManager(string repoPath, IOutputLogger logger)
        {
            this.gitProcessManager = new GitProcessManager(repoPath, logger ?? new DebugOutputLog());
        }

        /// <inheritdoc />
        public async Task<IList<GitBranch>> GetLocalBranches(CancellationToken token)
        {
            GitCommandResponse result = await this.gitProcessManager.RunGit("branch", token);

            IList<GitBranch> branches = new List<GitBranch>();
            if (!result.Success)
            {
                return branches;
            }

            string[] array = result.ProcessOutput.ToArrayOfLines();
            foreach (string line in array)
            {
                string branch = line.Substring(2);
                branches.Add(new GitBranch(branch, false));
            }

            return branches;
        }

        /// <inheritdoc />
        public async Task<IList<GitBranch>> GetRemoteBranches(CancellationToken token)
        {
            GitCommandResponse result = await this.gitProcessManager.RunGit("branch -r", token);

            IList<GitBranch> branches = new List<GitBranch>();

            if (result.Success == false)
            {
                return branches;
            }

            string[] array = result.ProcessOutput.ToArrayOfLines();
            foreach (string line in array)
            {
                int arrowPos = line.IndexOf(" -> ", StringComparison.InvariantCulture);
                string branch = line;
                if (arrowPos != -1)
                {
                    branch = line.Substring(0, arrowPos);
                }

                branches.Add(new GitBranch(branch.Trim(), true));
            }

            return branches;
        }

        /// <inheritdoc />
        public async Task<IList<GitBranch>> GetLocalAndRemoteBranches(CancellationToken token)
        {
            var local = await this.GetLocalBranches(token);

            if (token.IsCancellationRequested)
            {
                return null;
            }

            var remote = await this.GetRemoteBranches(token);

            return local.Concat(remote).OrderBy(x => x.FriendlyName).ToList();
        }

        /// <inheritdoc />
        public async Task<GitBranch> GetCurrentCheckedOutBranch(CancellationToken token)
        {
            GitCommandResponse result = await this.gitProcessManager.RunGit("branch", token);

            if (!result.Success)
            {
                return null;
            }

            string[] array = result.ProcessOutput.ToArrayOfLines();
            foreach (string line in array)
            {
                if (line.StartsWith("*"))
                {
                    return new GitBranch(line.Substring(2), false);
                }
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<bool> IsMergeConflict(CancellationToken token)
        {
            GitCommandResponse result = await this.gitProcessManager.RunGit("ls-files -u", token);

            if (result.Success)
            {
                return false;
            }

            string[] array = result.ProcessOutput.ToArrayOfLines();

            return array.Length > 0;
        }

        /// <inheritdoc />
        public async Task<IList<GitCommit>> GetCommitsForBranch(GitBranch branch, int skip, int limit, GitLogOptions logOptions, CancellationToken token)
        {
            string arguments = await this.ExtractLogParameter(branch, skip, limit, logOptions, "HEAD", token);

            var result = await this.gitProcessManager.RunGit("log " + arguments, token);

            if (result.Success == false)
            {
                return new List<GitCommit>();
            }

            string[] lines = result.ProcessOutput.Split(new[] { '\u001e' }, StringSplitOptions.RemoveEmptyEntries);

            IList<GitCommit> results = ConvertStringToGitCommits(lines);

            return results.OrderBy(x => x.DateTime).ToList();
        }

        /// <inheritdoc />
        public async Task<string> GetCommitMessagesAfterParent(GitCommit parent, CancellationToken token)
        {
            string arguments = await this.ExtractLogParameter(await this.GetCurrentCheckedOutBranch(token), 0, 0, GitLogOptions.None, $"{parent.Sha}..HEAD", token);

            var result = await this.gitProcessManager.RunGit("log " + arguments, token);

            if (result.Success == false)
            {
                return string.Empty;
            }

            string[] lines = result.ProcessOutput.Split(new[] { '\u001e' }, StringSplitOptions.RemoveEmptyEntries);

            IList<GitCommit> results = ConvertStringToGitCommits(lines);

            return string.Join(Environment.NewLine, results.Select(x => x.MessageLong));
        }

        /// <inheritdoc />
        public async Task<bool> IsWorkingDirectoryDirty(CancellationToken token)
        {
            var result = await this.gitProcessManager.RunGit("diff-files --quiet", token);

            return result.ReturnCode == 1;
        }

        /// <inheritdoc />
        public async Task<GitBranch> GetRemoteBranch(GitBranch branch, CancellationToken token)
        {
            if (branch.IsRemote)
            {
                return branch;
            }

            var result = await this.gitProcessManager.RunGit(" -lvv", token);

            if (result.Success == false)
            {
                return null;
            }

            string[] lines = result.ProcessOutput.ToArrayOfLines();
            
            foreach (string line in lines)
            {
                var matches = Regex.Matches(line, @"^[* ][* ]([a-ZA-Z0-9/]*)[ ]*[a-zA-Z0-9]+ \[([a-zA-Z0-9/]*)\]");

                if (matches.Count != 2)
                {
                    continue;
                }

                var branchName = matches[0].Value;

                if (branchName.Equals(branch.FriendlyName, StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    continue;
                }

                return new GitBranch(matches[1].Value, true);
            }

            return null;
        }

        private static IList<GitCommit> ConvertStringToGitCommits(string[] lines)
        {
            IList<GitCommit> results = new List<GitCommit>();
            foreach (string line in lines.Select(x => x.Trim('\r', '\n').Trim()))
            {
                string[] fields = line.Split('\u001f');

                if (fields.Length != 11)
                {
                    continue;
                }

                string changeset = fields[0];
                string changesetShort = fields[1];
                string[] parents = fields[2].Split(' ');
                DateTime commitDate;
                DateTime.TryParse(fields[3], out commitDate);
                string committer = $"{fields[4]} <{fields[5]}>";
                string author = $"{fields[6]} <{fields[7]}>";
                string refs = fields[8];
                string messageShort = fields[9];
                string messageLong = fields[10].Trim('\r', '\n').Trim();

                results.Add(new GitCommit(changeset, messageShort, messageLong, commitDate, author, committer, changesetShort, parents));
            }

            return results;
        }

        private async Task<string> ExtractLogParameter(GitBranch branch, int skip, int limit, GitLogOptions logOptions, string revisionRange, CancellationToken token)
        {
            StringBuilder arguments = new StringBuilder();

            arguments.Append($"{revisionRange} ");

            if (branch != null)
            {
                arguments.Append($"--branches={branch.FriendlyName} ");
            }

            if (skip > 0)
            {
                arguments.Append($" --skip={skip}");
            }

            if (limit > 0)
            {
                arguments.Append($" --max-count={limit}");
            }

            arguments.Append(" --full-history");

            if (logOptions.HasFlag(GitLogOptions.TopologicalOrder))
            {
                arguments.Append(" --topo-order");
            }

            if (!logOptions.HasFlag(GitLogOptions.IncludeMerges))
            {
                arguments.Append(" --no-merges --first-parent");
            }
  
            StringBuilder formatString = new StringBuilder("--format=%H\u001f%h\u001f%P\u001f");
            formatString.Append("%ci");
            formatString.Append("\u001f%cn\u001f%ce\u001f%an\u001f%ae\u001f%d\u001f%s\u001f%B\u001e");
            arguments.Append(" " + formatString);
            arguments.Append(" --decorate=full");
            arguments.Append(" --date=iso");

            StringBuilder ignoreBranches = new StringBuilder("--not ");

            if (logOptions.HasFlag(GitLogOptions.BranchOnlyAndParent))
            {
                var branches = await this.GetLocalBranches(token);

                foreach (var testBranch in branches)
                {
                    if (testBranch != branch)
                    {
                        ignoreBranches.Append($"{testBranch.FriendlyName} ");
                    }
                }

                arguments.Append($" {ignoreBranches} -- ");
            }

            return arguments.ToString();
        }
    }
}
