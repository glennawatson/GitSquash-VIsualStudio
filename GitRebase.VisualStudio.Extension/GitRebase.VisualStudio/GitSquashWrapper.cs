namespace GitRebase.VisualStudio
{
    using System;

    using Microsoft.TeamFoundation.Git.Controls.Extensibility;

    using PropertyChanged;

    [ImplementPropertyChanged]
    public class GitSquashWrapper : IGitSquashWrapper
    {
        public IHistoryCommitItem ParentItem { get; set; }

        public void Squash()
        {
        }

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public void Continue()
        {
            throw new NotImplementedException();
        }
    }
}
