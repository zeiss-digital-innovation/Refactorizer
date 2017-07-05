using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Class : IModel
    {
        public Class(Guid id, string fullName, string name, IModel parent)
        {
            Id = id;
            FullName = fullName;
            Name = name;
            Parent = parent;
        }

        public string FullName { get; }

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public ICollection<IModel> References { get; set; } = new List<IModel>();

        public Guid Id { get; }
    }
}