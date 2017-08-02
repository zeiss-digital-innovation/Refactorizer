using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactorizer.VSIX.Models
{
    class Solution : ISolution
    {
        public List<IModel> Projects { get; set; } = new List<IModel>();
    }
}
