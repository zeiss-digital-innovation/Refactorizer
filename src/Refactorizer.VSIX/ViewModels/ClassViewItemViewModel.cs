using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class ClassViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Class _class;

        public ClassViewItemViewModel(Class @class, TreeViewItemViewModel parent) : base(parent)
        {
            _class = @class;
        }

        protected override void Loadchildren()
        {
        }

        public override string Name => _class.Name;
    }
}