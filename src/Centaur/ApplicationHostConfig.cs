namespace Centaur
{
    public class ApplicationHostConfig
    {
        public string Path { get; private set; }

        public ApplicationHostConfig(string path)
        {
            Path = path;
        }
    }
}