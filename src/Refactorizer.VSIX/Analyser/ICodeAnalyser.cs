using System.Threading.Tasks;
using Refactorizer.VSIX.Models;

namespace Refactorizer.VSIX.Analyser
{
    public interface ICodeAnalyser
    {
        /// <summary>
        ///     Generates a new dependency solution based on opened solution
        /// </summary>
        /// <returns></returns>
        Task<ISolution> GenerateDependencyTree();
    }
}