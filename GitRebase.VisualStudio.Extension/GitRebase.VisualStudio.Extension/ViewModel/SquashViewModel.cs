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
        /// <param name="changeBranch">A command to change the current branch.</param>
        public SquashViewModel(IGitSquashWrapper squashWrapper, ICommand changeBranch)
        {
            this.SquashWrapper = squashWrapper;

            this.PerformSquash = new RelayCommand(() => this.SquashWrapper.Squash(), () => this.SquashWrapper != null);
            this.ChangeBranch = changeBranch;


        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public ICommand PerformSquash { get; }

        /// <inheritdoc />
        public IGitSquashWrapper SquashWrapper { get; set; }

        /// <inheritdoc />
        public Branch CurrentBranch { get; }

        /// <inheritdoc />
        public ICommand ChangeBranch { get; }
    }
}
