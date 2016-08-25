namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Git.VisualStudio;
    using ReactiveUI;

    /// <summary>
    /// A option in a view model representing the git log options enum.
    /// </summary>
    public class GitLogOptionViewModel : ReactiveObject
    {
        private bool enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLogOptionViewModel"/> class.
        /// </summary>
        /// <param name="logOption">The log option this represents.</param>
        public GitLogOptionViewModel(GitLogOptions logOption)
        {
            this.Value = logOption;

            this.Name = Enum.GetName(typeof(GitLogOptions), logOption);
        }

        /// <summary>
        /// Gets the name of the enum.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the enum value.
        /// </summary>
        public GitLogOptions Value { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the log option is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.enabled, value);
            }
        }
    }
}
