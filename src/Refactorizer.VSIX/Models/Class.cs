﻿using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Class : IModel
    {
        public Class(Guid id, string name, IModel parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
        }

        public string FullName => ClassnameFormater.FullName((Parent as Namespace)?.Name, Name);

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public ICollection<IModel> References { get; set; } = new List<IModel>();

        public bool HasChildren => Methods.Count > 0;

        public Guid Id { get; }

        public bool IsInterface { get; set; } = false;

        public List<Method> Methods { get; set; } = new List<Method>();

        public List<Property> Properties { get; set; } = new List<Property>();

        public List<Field> Fields { get; set; } = new List<Field>();

        public AccessLevel AccessLevel { get; set; } = AccessLevel.Public;

        public List<Namespace> ReferencedNamespaces { get; set; } = new List<Namespace>();
    }
}