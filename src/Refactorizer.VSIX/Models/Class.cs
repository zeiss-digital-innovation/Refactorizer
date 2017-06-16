using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Class : IModel
    {
        public Class(Guid id, string fullName, string name)
        {
            Id = id;
            FullName = fullName;
            Name = name;
        }

        public string FullName { get; }

        public string Name { get; set; }

        public List<Class> References { get; set; } = new List<Class>();

        public Guid Id { get; }
    }
}