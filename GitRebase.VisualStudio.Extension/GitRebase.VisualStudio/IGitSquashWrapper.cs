namespace GitRebase.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IGitSquashWrapper
    {
        string CurrentBranch { get; }

        void Abort();

        void Continue();
    }
}
