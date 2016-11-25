using System;

namespace Hecate
{
    /// <summary>
    /// Description of MainClass.
    /// </summary>
    public static class MainClass
    {
        public static void Main()
        {
            // Begin console interface
            System.Console.WriteLine("Hecate Story Generator");
            
            
            // Initialize generator
            StoryGenerator generator = new StoryGenerator();
            generator.ParseRuleDirectory("E:\\Code\\C#\\Hecate\\Data");
            
            do {
                string result = generator.Generate("[=>story]");
                System.Console.WriteLine("\n== STORY\n" + result);
            } while (System.Console.ReadKey().KeyChar != 'q');
            
            
        }
    }
}
