using System;

namespace TextureSplitter
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static string[] Args;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Args = args;
            using (var game = new Game1())
                game.Run();
        }
    }
}
