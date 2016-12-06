namespace Centaur
{
    public class IISExpressConfig
    {
        public string Path { get; private set; }
        public string Site { get; private set; }

        public IISExpressConfig(string path)
        {
            Path = path;
        }
        public IISExpressConfig(string path, string site)
        {
            Path = path;
            Site = site;
        }
    }
}