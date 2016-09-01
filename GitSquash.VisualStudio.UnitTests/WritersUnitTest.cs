namespace GitSquash.VisualStudio.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Git.VisualStudio;

    using Xunit;

    public class WritersUnitTest
    {
        [Fact]
        public async void TestSquashWriter()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            Directory.CreateDirectory(tempDirectory);

            GitProcessManager local = new GitProcessManager(tempDirectory, null);

            var result = await local.RunGit("init", CancellationToken.None);

            result.Success.Should().Be(true, "Must be able to init");

            int numberCommits = 10;
            await this.GenerateCommits(numberCommits, tempDirectory, local, "master");

            var branchManager = new BranchManager(tempDirectory, null);

            var commits = await branchManager.GetCommitsForBranch(new GitBranch("master", false), 0, 0, GitLogOptions.None, CancellationToken.None);

            IGitSquashWrapper squashWrapper = new GitSquashWrapper(tempDirectory, null);
            GitCommandResponse squashOutput = await squashWrapper.Squash(CancellationToken.None, "Bye Cruel World", commits.Last());

            commits = await branchManager.GetCommitsForBranch(new GitBranch("master", false), 0, 0, GitLogOptions.None, CancellationToken.None);
            commits = commits.ToList();

            commits.Count.Should().Be(2);
            commits[0].MessageLong.Should().BeEquivalentTo("Bye Cruel World");
        }

        [Fact]
        public void TestCommentWriter()
        {
            string commentWritersName, rebaseWritersName;
            GitSquashWrapper.GetWritersName(out rebaseWritersName, out commentWritersName);

            File.Exists(commentWritersName).Should().BeTrue();

            string validCommit = "This is awesome";

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, validCommit);

            string path = Path.GetTempFileName();

            File.WriteAllText(path, @"This is my awesome commit. I don't want this.");

            Process process = new Process()
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = commentWritersName,
                    Arguments = path,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8                  
                },
                EnableRaisingEvents = true
            };

            process.StartInfo.EnvironmentVariables.Add("COMMENT_FILE_NAME", fileName);

            process.Start();

            StringBuilder output = new StringBuilder();

            process.ErrorDataReceived += (sender, e) => output.Append(e.Data);
            process.OutputDataReceived += (sender, e) => output.Append(e.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            string readAllText = File.ReadAllText(path);

            readAllText.Should().Be(validCommit);
        }

        private async Task GenerateCommits(int numberCommits, string directory, IGitProcessManager local, string branchName)
        {
            GitCommandResponse result;
            if (branchName != "master")
            {
                result = await local.RunGit($"branch {branchName}", CancellationToken.None);

                result.Success.Should().Be(true, $"Must be able create branch {branchName}");
            }

            await local.RunGit($"checkout {branchName}", CancellationToken.None);

            for (int i = 0; i < numberCommits; ++i)
            {
                File.WriteAllText(Path.Combine(directory, Path.GetRandomFileName()), @"Hello World" + i);
                result = await local.RunGit("add -A", CancellationToken.None);
                result.Success.Should().Be(true, "Must be able to add");
                result = await local.RunGit($"commit -m \"Commit {branchName}-{i}\"", CancellationToken.None);
                result.Success.Should().Be(true, "Must be able to commit");
            }
        }
    }
}
