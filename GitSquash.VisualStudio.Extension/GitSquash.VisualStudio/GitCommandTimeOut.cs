namespace GitSquash.VisualStudio
{
    /// <summary>
    /// A response when there has been a time out from git.
    /// </summary>
    public class GitCommandTimeOut : GitCommandResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommandTimeOut"/> class.
        /// </summary>
        /// <param name="commandOutput">The output.</param>
        public GitCommandTimeOut(string commandOutput)
            : base(false, $"The command '{commandOutput}' is taking longer than expected. You might be prompted for information such as credentials. Please run the command from command line to find out what is blocking the process")
        {
        }
    }
}
