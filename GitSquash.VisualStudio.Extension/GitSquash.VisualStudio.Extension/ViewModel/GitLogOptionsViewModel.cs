namespace GitSquash.VisualStudio.Extension.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Git.VisualStudio;

    /// <summary>
    /// A set of values for log options.
    /// </summary>
    public class GitLogOptionsViewModel : ObservableCollection<GitLogOptionViewModel>
    {
        /// <summary>
        /// Gets or sets the value of the git log options.
        /// </summary>
        public GitLogOptions Value
        {
            get
            {
                return this.Where(option => option.Enabled).Aggregate(GitLogOptions.None, (current, option) => current | option.Value);
            }

            set
            {
                foreach (GitLogOptionViewModel oldOption in this)
                {
                    oldOption.PropertyChanged -= this.OnOptionChanged;
                }

                this.Clear();

                foreach (GitLogOptions logOption in Enum.GetValues(typeof(GitLogOptions)))
                {
                    if (logOption == GitLogOptions.None)
                    {
                        continue;
                    }

                    GitLogOptionViewModel option = new GitLogOptionViewModel(logOption) { Enabled = value.HasFlag(logOption) };
                    option.PropertyChanged += this.OnOptionChanged;

                    this.Add(option);
                }
            }
        }

        private void OnOptionChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs("Value"));
        }
    }
}
