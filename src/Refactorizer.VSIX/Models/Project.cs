using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MSProject = Microsoft.CodeAnalysis.Project;

namespace Refactorizer.VSIX.Models
{
    class Project : IModel
    {
        public Guid Id { get; }

        public ProjectId ProjectId => MSProject.Id;

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public List<Namespace> Namespaces { get; set; } = new List<Namespace>();

        public ICollection<IModel> OutReferences { get; set; } = new List<IModel>();

        public ICollection<IModel> InReferences { get; set; } = new List<IModel>();

        public MSProject MSProject { get; set; }

        public Project(Guid id, MSProject msProject, string name)
        {
            Id = id;
            MSProject = msProject;
            Name = name;
        }

        public bool IsHarmfull => OutReferences.Count < InReferences.Count;

        public bool HasChildren => Namespaces.Count > 0;
    }
}
