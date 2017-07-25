using System;
using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    public interface IModel
    {
        Guid Id { get; }

        string Name { get; set; }

        IModel Parent { get; set; }

        ICollection<IModel> OutReferences { get; set; }

        ICollection<IModel> InReferences { get; set; }

        bool HasChildren { get; }

        bool IsHarmfull { get; }
    }
}
