using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class NamespaceViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Namespace _namespace;

        public NamespaceViewItemViewModel(Namespace @namespace, TreeViewItemViewModel parent) : base(parent)
        {
            _namespace = @namespace;
            AddDummy();
        }

        protected override void Loadchildren()
        {
            foreach (var @class in _namespace.Classes)
            {
                Children.Add(new ClassViewItemViewModel(@class, this));
            }
        }

        public override string Name => _namespace.Name;
    }
}