using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class FieldViewItemViewModel : DependencyTreeViewItemViewModel
    {
        public FieldViewItemViewModel(DependencyTreeViewItemViewModel parent, IModel relatedModel) : base(parent, relatedModel)
        {
        }

        public override string Name => (RelatedModel as Field)?.Signature ?? RelatedModel.Name;
    }
}
