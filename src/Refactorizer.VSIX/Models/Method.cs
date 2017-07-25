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

        public string FullName { get; set; }

        public IModel Parent { get; set; }

        public ICollection<IModel> OutReferences { get; set; } = new List<IModel>();

        public ICollection<IModel> InReferences { get; set; } = new List<IModel>();

        public Method(Guid id, string name, string fullName, IModel parent)
        {
            Id = id;
            Name = name;
            Parent = parent;
            FullName = fullName;
        }

        public string ReturnType { get; set; }

        public string Parameter { get; set; }

        public string Signature => (ReturnType == null ? string.Empty : $"{ReturnType} ") + $"{Name} ({Parameter})";

        public bool HasChildren => false;

        public bool IsHarmfull => OutReferences.Count < InReferences.Count;

        public AccessLevel AccessLevel { get; set; } = AccessLevel.Public;
    }
}
