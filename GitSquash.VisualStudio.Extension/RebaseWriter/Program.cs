namespace RebaseWriter
{
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The main rebase writer application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point into the application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }

            string fileName = args[0];
            WriteRebaseLinesFromFile(fileName);
        }

        private static void WriteRebaseLinesFromFile(string fileName)
        {
            var lines = File.ReadAllLines(fileName);

            using (var writer = new StreamWriter(fileName))
            {
                writer.NewLine = "\n";

                bool firstCommitSeen = false;
                foreach (var line in lines)
                {
                    string strippedLine = Regex.Replace(line, "#.*", string.Empty).Trim();

                    if (string.IsNullOrWhiteSpace(strippedLine))
                    {
                        continue;
                    }

                    var commit = strippedLine.Split(' ');

                    if (firstCommitSeen == false)
                    {
                        writer.WriteLine($"pick {commit[1]}");
                        firstCommitSeen = true;
                    }
                    else
                    {
                        writer.WriteLine($"squash {commit[1]}");
                    }
                }
            }
        }
    }
}
