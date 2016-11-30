using System;
using System.IO;

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
                int stories = 72;
                string whole_text = stories + " cases of the Blackhearts Detective Agency\n\nBy Aarne Uotila\nNaNoGenMo 2016\nhttps://github.com/NaNoGenMo/2016/issues/111\n\n\n";
                for (int i = 0; i < stories; i++) {
                    whole_text += "Chapter " + (i + 1) + "\n\n";
                    string result = generator.Generate("[=>story]");
                    whole_text += result + "\n\n";
                    System.Console.WriteLine("\n== STORY\n" + result);
                }
                File.WriteAllText("blackhearts.txt", whole_text.Replace("\n", Environment.NewLine));
                
            } while (System.Console.ReadKey().KeyChar != 'q');
            
            
        }
    }
}
