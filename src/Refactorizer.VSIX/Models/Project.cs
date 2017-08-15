using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    class Project : IModel
    {
        public Guid Id { get; }

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public List<Namespace> Namespaces { get; set; } = new List<Namespace>();

        public ICollection<IModel> OutReferences { get; set; } = new List<IModel>();

        public ICollection<IModel> InReferences { get; set; } = new List<IModel>();

        public Project(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool IsHarmfull => OutReferences.Count < InReferences.Count;

        public bool HasChildren => Namespaces.Count > 0;
    }
}
