using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace GitSquash.VisualStudio.UnitTests
{
    public class WritersUnitTest
    {
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

            File.WriteAllText(path, "This is my awesome commit. I don't want this.");

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
    }
}
