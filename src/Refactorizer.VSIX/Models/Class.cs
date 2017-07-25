using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Class : IModel
    {
        public Class(Guid id, string name, IModel parent, string path)
        {
            Id = id;
            Name = name;
            Parent = parent;
            Path = path;
        }

        public string FullName => ClassnameFormater.FullName((Parent as Namespace)?.Name, Name);

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public ICollection<IModel> OutReferences { get; set; } = new List<IModel>();

        public ICollection<IModel> InReferences { get; set; } = new List<IModel>();

        public bool HasChildren => Methods.Count > 0;

        public bool IsHarmfull => OutReferences.Count < InReferences.Count;

        public Guid Id { get; }

        public bool IsInterface { get; set; } = false;

        public List<Method> Methods { get; set; } = new List<Method>();

        public List<Property> Properties { get; set; } = new List<Property>();

        public List<Field> Fields { get; set; } = new List<Field>();

        public AccessLevel AccessLevel { get; set; } = AccessLevel.Public;

        public List<Namespace> ReferencedNamespaces { get; set; } = new List<Namespace>();

        public string Path { get; set; }
    }
}