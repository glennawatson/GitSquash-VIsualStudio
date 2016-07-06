namespace GitRebase.VisualStudio
{
    /// <summary>
    /// A response after we have initiated a command to git.
    /// </summary>
    public class GitCommandResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommandResponse"/> class.
        /// </summary>
        /// <param name="success">If the command was successful.</param>
        /// <param name="commandOutput">The command output.</param>
        public GitCommandResponse(bool success, string commandOutput)
        {
            this.Success = success;
            this.CommandOutput = commandOutput;
        }

        /// <summary>
        /// Gets a value indicating whether the git command was successful.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Gets the output from the command.
        /// </summary>
        public string CommandOutput { get; private set; }
    }
}
