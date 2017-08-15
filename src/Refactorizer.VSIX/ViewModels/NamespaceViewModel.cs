using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Refactorizer.VSIX.Commands;
using Refactorizer.VSIX.Misc;
using Refactorizer.VSIX.Models;
using Refactorizer.VSIX.Refactorings;
using Refactorizer.VSIX.Views;

namespace Refactorizer.VSIX.ViewModels
{
    internal class NamespaceViewModel : DependencyTreeItemViewModel
    {
        private const char NamespaceDelimiter = '.';

        public ICommand Delete { get; set; }

        public ICommand Rename { get; set; }

        public NamespaceViewModel(SolutionViewModel root, DependencyTreeItemViewModel parent, IModel relatedModel,
            IRefactoringFactory refactoringFactory) : base(root, parent, relatedModel, refactoringFactory)
        {
            Delete = new RelayCommand(param => DeleteAction());
            Rename = new RelayCommand(param => RenameAction());

            var @namespace = RelatedModel as Namespace;
            if (@namespace == null)
                return;

            foreach (var @class in @namespace.Classes)
                Children.Add(new ClassViewModel(root, this, @class, refactoringFactory));
        }

        private void DeleteAction()
        {
            DialogManager.Create("Confirm delete", new DeleteConfirmDialog(async () =>
            {
                var refactoring = Refactoring as NamespaceRefactoring;
                if (refactoring == null)
                    return;

                var ns = RelatedModel as Namespace;
                if (ns == null)
                    return;

                var result = await refactoring.Delete(ns);
                if (result == ActionResult.Success)
                {
                    Parent.DeleteChild(this);
                }
                else
                {
                    MessageBox.Show("Delete failed");
                }
                DialogManager.Close();
            }, DialogManager.Close));
        }

        private void RenameAction()
        {
            var refactoring = Refactoring as NamespaceRefactoring;
            if (refactoring == null)
                return;

            var ns = RelatedModel as Namespace;
            if (ns == null)
                return;

            DialogManager.Create("Rename", new RenameDialog(async () =>
            {
                var content = DialogManager.GetContent();
                var viewModel = content.DataContext as RenameDialogViewModel;
                if (viewModel == null)
                    return;

                var newName = viewModel.Text;
                if (!ns.Name.Equals(newName))
                {
                    var oldName = ns.Name;
                    var result = await refactoring.Rename(ns, newName);
                    if (result == ActionResult.Success)
                    {
                        var oldNameSplit = oldName.Split(NamespaceDelimiter).ToList();
                        var newNameSplit = newName.Split(NamespaceDelimiter).ToList();
                        var newNamePart = string.Empty;
                        var oldNamePart = string.Empty;
                        var index = 0;
                        var mapOfChanges = new Dictionary<string, string>();
                        foreach (var split in newNameSplit)
                        {
                            if (index > oldNameSplit.Count)
                                break;

                            newNamePart += newNamePart == string.Empty ? string.Empty : NamespaceDelimiter.ToString();
                            newNamePart += split;

                            oldNamePart += oldNamePart == string.Empty ? string.Empty : NamespaceDelimiter.ToString();
                            oldNamePart += oldNameSplit[index];

                            if (! oldNamePart.Equals(newNamePart))
                                mapOfChanges.Add(oldNamePart, newNamePart);

                            index++;
                        }

                        foreach (var projectViewModel in Root.Projects)
                        {
                            foreach (var child in projectViewModel.Children)
                            {
                                var namespaceViewModel = child as NamespaceViewModel;
                                if (namespaceViewModel == null)
                                    continue;

                                var nsName = namespaceViewModel.RelatedModel.Name;
                                var nsSplit = nsName.Split(NamespaceDelimiter);
                                index = 0;
                                var rename = false;
                                foreach (var s in nsSplit)
                                {
                                    if (mapOfChanges.ContainsKey(s))
                                    {
                                        nsSplit[index] = mapOfChanges[s];
                                        rename = true;
                                    }
                                    index++;
                                }

                                if (!rename)
                                    continue;

                                var changeName = string.Join(NamespaceDelimiter.ToString(), nsSplit);
                                namespaceViewModel.Name = changeName;
                            }
                        }

                        Name = newName;
                    }
                }

                DialogManager.Close();
            }, DialogManager.Close, ns.Name));
        }
    }
}