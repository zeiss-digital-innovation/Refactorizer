using System.Collections.Generic;

namespace Refactorizer.VSIX.Models
{
    public interface ISolution
    {
        List<IModel> Projects { get; set; }
    }
}