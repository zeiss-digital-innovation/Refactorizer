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

        public List<Namespace> Namespaces { get; set; } = new List<Namespace>();

        public List<Document> Documents { get; set; } = new List<Document>();

        public List<Project> References { get; } = new List<Project>();

        public Project(Guid id, ProjectId projectId, string name)
        {
            Id = id;
            ProjectId = projectId;
            Name = name;
        }
    }
}
