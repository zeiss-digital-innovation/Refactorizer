using System.Windows.Media;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.ViewModels
{
    class ProjectViewItemViewModel : TreeViewItemViewModel
    {
        private readonly Project _project;

        public ProjectViewItemViewModel(Project project, TreeViewItemViewModel parent) : base(parent)
        {
            _project = project;
            AddDummy();
        }

        protected override void Loadchildren()
        {
            foreach (var ns in _project.Namespaces)
            {
                Children.Add(new NamespaceViewItemViewModel(ns, this));
            }
        }

        public override string Name => _project.Name;
    }
}
