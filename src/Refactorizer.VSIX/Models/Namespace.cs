using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Namespace : IModel
    {
        public Namespace(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public List<Class> Classes { get; set; } = new List<Class>();

        public List<Namespace> References { get; set; } = new List<Namespace>();

        public Guid Id { get; }
    }
}