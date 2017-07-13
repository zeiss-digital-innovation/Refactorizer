using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactorizer.VSIX.Models
{
    class Method : IModel
    {
        public Guid Id { get; }

        public string Name { get; set; }

        public IModel Parent { get; set; }

        public ICollection<IModel> References { get; set; } = new List<IModel>();

        public Method(Guid id, string name, IModel parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
        }

        public string ReturnType { get; set; }

        public string Parameter { get; set; }

        public string Signature => ReturnType == null ? "" : $"{ReturnType} " + $"{Name} (${Parameter}";

        public bool HasChildren => false;

        public AccessLevel AccessLevel { get; set; } = AccessLevel.Public;
    }
}
