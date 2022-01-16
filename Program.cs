namespace DumpLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = new ServerOptions();
                new FileWatcher(options);
            }
            catch (System.Exception ex)
            {
                LogConsole.LogError(ex);
            }
        }
    }
}
