using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Refactorizer.VSIX.Models
{
    class Project : IModel
    {
        public Guid Id { get; }

        public ProjectId ProjectId { get; }

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public List<Namespace> Namespaces { get; set; } = new List<Namespace>();

        public ICollection<IModel> References { get; set; } = new List<IModel>();

        public Project(Guid id, ProjectId projectId, string name)
        {
            Id = id;
            ProjectId = projectId;
            Name = name;
        }

        public bool HasChildren => Namespaces.Count > 0;
    }
}
