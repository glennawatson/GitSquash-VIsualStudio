using System.Diagnostics;
using System.Threading;

namespace CommentWriter
{
    using System;
    using System.IO;

    /// <summary>
    /// A program to write a appropriate comment for a rebase.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Executes the program.
        /// </summary>
        /// <param name="args">Arguments about the rebase.</param>
        public static void Main(string[] args)
        {
            File.WriteAllText("c:\\temp\\in_comment_writer.txt", "Hello World");
            if (args.Length != 1)
            {
                return;
            }

            File.WriteAllText("c:\\temp\\in2_comment_writer.txt", "Hello World");

            Console.WriteLine("Hello World");

            string fileName = args[0];

            var tempCommentFileName = Environment.GetEnvironmentVariable("COMMENT_FILE_NAME");

            File.WriteAllText("c:\\temp\\COMMENT_FILE_NAME.txt", tempCommentFileName ?? "Empty");

            string[] lines = File.ReadAllLines(string.IsNullOrWhiteSpace(tempCommentFileName) ? fileName : tempCommentFileName);

            WriteCommentFile(fileName, lines);
        }

        private static void WriteCommentFile(string fileName, string[] lines)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.NewLine = "\n";

                for (int i = 0; i < lines.Length; ++i)
                {
                    if (i < (lines.Length - 1))
                    {
                        writer.WriteLine(lines[i]);
                    }
                    else
                    {
                        writer.Write(lines[i]);
                    }
                }
            }
        }
    }
}
