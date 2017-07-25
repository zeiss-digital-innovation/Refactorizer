using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactorizer.VSIX.Models
{
    class Property : IModel
    {
        public Guid Id { get; }

        public string Name { get; set; }

        public IModel Parent { get; set; }

        public ICollection<IModel> OutReferences { get; set; } = new List<IModel>();

        public ICollection<IModel> InReferences { get; set; } = new List<IModel>();

        public AccessLevel AccessLevel { get; set; } = AccessLevel.Public;

        public bool HasChildren => false;

        public string Signature { get; set; }

        public bool IsHarmfull => OutReferences.Count < InReferences.Count;

        public Property(Guid id, string name, IModel parent, string signature)
        {
            Id = id;
            Name = name;
            Parent = parent;
            Signature = signature;
        }
    }
}
