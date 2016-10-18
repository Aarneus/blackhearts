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
            generator.ParseRuleFile("E:\\Code\\C#\\Hecate\\Data\\Test.hec");
            
            // End on input
            System.Console.ReadKey();
            
            
        }
    }
}
