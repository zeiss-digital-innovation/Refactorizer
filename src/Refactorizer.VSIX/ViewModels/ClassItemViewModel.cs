using System;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class ClassItemViewModel : TreeItemViewModel
    {
        public ClassItemViewModel(TreeItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        protected override void Loadchildren()
        {
        }
    }
}