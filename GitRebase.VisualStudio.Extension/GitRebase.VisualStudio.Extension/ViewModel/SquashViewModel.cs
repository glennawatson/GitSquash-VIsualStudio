namespace GitRebase.VisualStudio.Extension.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Command;

    using LibGit2Sharp;

    /// <summary>
    /// The view model for performing squashs.
    /// </summary>
    public class SquashViewModel : ISquashViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquashViewModel"/> class.
        /// </summary>
        /// <param name="squashWrapper">Our model that we retrieve data from.</param>
        public SquashViewModel(IGitSquashWrapper squashWrapper)
        {
            this.SquashWrapper = squashWrapper;

            this.BranchNames = this.SquashWrapper.GetRemoteBranches().ToList();

            this.PerformSquash = new RelayCommand(() => this.SquashWrapper.Squash(), () => this.SquashWrapper != null);
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public ICommand PerformSquash { get; }

        /// <inheritdoc />
        public IGitSquashWrapper SquashWrapper { get; set; }

        public IEnumerable<Branch> BranchNames { get; }
    }
}
