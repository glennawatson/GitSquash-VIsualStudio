namespace Git.VisualStudio.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;
    using LibGit2Sharp;

    public class GitUnitTest
    {
        [Fact]
        public async void TestGitHistoryBranchOnly()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDirectory);

            GitProcessManager local = new GitProcessManager(tempDirectory, null);

            var result = await local.RunGit("init", CancellationToken.None);

            result.Success.Should().Be(true, "Must be able to init");

            int numberCommits = 10;
            await GenerateCommits(numberCommits, tempDirectory, local, "master");

            BranchManager branchManager = new BranchManager(tempDirectory, null);

            var commits = await branchManager.GetCommitsForBranch(new GitBranch("master", false), 0, 0, GitLogOptions.BranchOnlyAndParent, CancellationToken.None);
            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            using (Repository repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "master");
                branch.Should().NotBeNull();

                CheckCommits(branch.Commits, commits);
            }

            commits.Should().BeInDescendingOrder(x => x.DateTime);

            await GenerateCommits(numberCommits, tempDirectory, local, "test1");

            commits = await branchManager.GetCommitsForBranch(new GitBranch("test1", false), 0, 0, GitLogOptions.BranchOnlyAndParent, CancellationToken.None);

            commits.Count.Should().Be(numberCommits + 1, $"We have done {numberCommits + 1} commits");

            using (Repository repository = new Repository(tempDirectory))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "test1");
                branch.Should().NotBeNull();

                CheckCommits(branch.Commits.Take(11), commits);
            }
        }

        private static void CheckCommits(IEnumerable<Commit> repoCommits, IList<GitCommit> commits)
        {
            repoCommits.Select(x => x.Sha).ShouldAllBeEquivalentTo(commits.Select(x => x.Sha));
            repoCommits.Select(x => x.MessageShort).ShouldAllBeEquivalentTo(commits.Select(x => x.MessageShort));
        }

        [Fact]
        public async void TestFullHistory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDirectory);

            GitProcessManager local = new GitProcessManager(tempDirectory, null);

            var result = await local.RunGit("init", CancellationToken.None);

            result.Success.Should().Be(true, "Must be able to init");

            int numberCommits = 10;
            await GenerateCommits(numberCommits, tempDirectory, local, "master");
            BranchManager branchManager = new BranchManager(tempDirectory, null);

            var commits = await branchManager.GetCommitsForBranch(new GitBranch("master", false), 0, 0, GitLogOptions.BranchOnlyAndParent, CancellationToken.None);

            commits.Count.Should().Be(numberCommits, $"We have done {numberCommits} commits");

            commits.Should().BeInDescendingOrder(x => x.DateTime);

            result = await local.RunGit("branch test1", CancellationToken.None);
            result.Success.Should().Be(true, "Must be able create branch");

            result = await local.RunGit("checkout test1", CancellationToken.None);
            result.Success.Should().Be(true, "Must be able checkout branch");

            await GenerateCommits(numberCommits, tempDirectory, local, "master");

            commits = await branchManager.GetCommitsForBranch(new GitBranch("test1", false), 0, 0, GitLogOptions.None, CancellationToken.None);

            commits.Count.Should().Be(numberCommits * 2, $"We have done {numberCommits + 1} commits");
        }

        private async Task GenerateCommits(int numberCommits, string directory, IGitProcessManager local, string branchName)
        {
            GitCommandResponse result = null;
            if (branchName != "master")
            {
                result = await local.RunGit($"branch {branchName}", CancellationToken.None);

                result.Success.Should().Be(true, $"Must be able create branch {branchName}");
            }

            await local.RunGit($"checkout {branchName}", CancellationToken.None);

            for (int i = 0; i < numberCommits; ++i)
            {
                File.WriteAllText(Path.Combine(directory, Path.GetRandomFileName()), "Hello World" + i);
                result = await local.RunGit("add -A", CancellationToken.None);
                result.Success.Should().Be(true, "Must be able to add");
                result = await local.RunGit($"commit -m \"Commit {branchName}-{i}\"", CancellationToken.None);
                result.Success.Should().Be(true, "Must be able to commit");
            }

        }
    }
}
