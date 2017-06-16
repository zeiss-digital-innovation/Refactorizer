using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    internal class Document : IModel
    {
        public Document(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; set; }

        public List<Class> Classes { get; set; } = new List<Class>();

        public Guid Id { get; }

        public List<Document> References { get; } = new List<Document>();
    }
}