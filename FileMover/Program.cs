using System;

namespace FileMover
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 2)
                {
                    string sourceDirectory = args[0];
                    string destinationDirectory = args[1];
                    FileMoverHelper helper = new FileMoverHelper(sourceDirectory, destinationDirectory);
                    helper.MoveFiles();
                }
                else
                {
                    Console.WriteLine("Source and destination directory required!!!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                throw;
            }

            Console.WriteLine("Done!");
        }
    }
}
