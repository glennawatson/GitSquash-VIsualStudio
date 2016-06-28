namespace GitRebase.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation.Git.Controls.Extensibility;

    public interface IGitSquashWrapper
    {
        IHistoryCommitItem ParentItem { get; }

        void Squash(); 

        void Abort();

        void Continue();
    }
}
