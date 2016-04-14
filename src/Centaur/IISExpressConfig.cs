namespace Centaur
{
    public class IISExpressConfig
    {
        public string Path { get; private set; }

        public IISExpressConfig(string path)
        {
            Path = path;
        }
    }
}