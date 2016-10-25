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
            generator.parseRuleFile("E:\\Code\\C#\\Hecate\\Data\\Test.hec");
            
            //string result = generator.generate("Eläin: [test]\nToinen: [test]");
            string result = generator.generate("A: [=>test]\nB: [kurba]\nC: [12 +3]");
            
            System.Console.WriteLine(result);
            
            // End on input
            System.Console.ReadKey();
            
            
        }
    }
}
