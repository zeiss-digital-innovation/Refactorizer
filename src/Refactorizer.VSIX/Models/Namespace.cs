using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Namespace : IModel
    {
        public Namespace(Guid id, string name, IModel parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
        }

        public string Name { get; set; }

        /// <inheritdoc />
        public IModel Parent { get; set; }

        public List<Class> Classes { get; set; } = new List<Class>();

        public ICollection<IModel> References { get; set; } = new List<IModel>();

        public Guid Id { get; }
    }
}