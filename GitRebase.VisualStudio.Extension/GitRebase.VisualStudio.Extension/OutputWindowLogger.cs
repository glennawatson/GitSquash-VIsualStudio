namespace GitSquash.VisualStudio.Extension
{
    using System;

    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Outputs text to a output window.
    /// </summary>
    public class OutputWindowLogger : IGitSquashOutputLogger
    {
        /// <summary>
        /// The output window pane.
        /// </summary>
        private readonly IVsOutputWindowPane output;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputWindowLogger"/> class.
        /// </summary>
        /// <param name="output">The output window.</param>
        public OutputWindowLogger(IVsOutputWindowPane output)
        {
            this.output = output;
        }

        /// <inheritdoc />
        public void WriteLine(string text)
        {
            this.output.Activate();
            this.output.OutputStringThreadSafe(text + Environment.NewLine);
        }
    }
}
