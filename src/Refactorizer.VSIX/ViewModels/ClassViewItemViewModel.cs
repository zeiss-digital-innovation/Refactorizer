using System;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class ClassViewItemViewModel : DependencyTreeViewItemViewModel
    {
        public ClassViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        protected override void Loadchildren()
        {
        }
    }
}