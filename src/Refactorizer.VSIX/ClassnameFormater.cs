namespace Refactorizer.VSIX
{
    internal class ClassnameFormater
    {
        public string FormatClassFullName(string namesapceName, string className)
        {
            return $"{namesapceName}.{className}";
        }
    }
}