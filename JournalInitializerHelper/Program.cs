using EncryptedJournal;
namespace JournalInitializerHelper
{
    class HelperClass
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine(Cryption.GetStableHashCode(Console.ReadLine()));
            CharacterGroups(4);
        }

        static void CharacterGroups(int groupCount)
        {
            Random rnd = new Random();
            string allCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()-_=+[{]};:'\"\\|,<.>/? ";
            string[] groups = new string[groupCount];
            int[] groupCounts = new int[groupCount];
            for (int i = 0; i < groupCount; i++)
            {
                groupCounts[i] = rnd.Next(allCharacters.Length / groupCount * 2 / 5, allCharacters.Length / groupCount);
            }
            if (groupCounts.Sum() > allCharacters.Length)
            {
                int until = groupCounts.Sum() - allCharacters.Length;
                int countIndex = 0;
                for (int i = 0; i < until; i++)
                {
                    groupCounts[countIndex]--;
                    countIndex++;
                    if (countIndex >= groupCounts.Length)
                    {
                        countIndex = 0;
                    }
                }
            }
            if (groupCounts.Sum() < allCharacters.Length)
            {
                int until = allCharacters.Length - groupCounts.Sum();
                int countIndex = 0;
                for (int i = 0; i < until; i++)
                {
                    groupCounts[countIndex]++;
                    countIndex++;
                    if (countIndex >= groupCounts.Length)
                    {
                        countIndex = 0;
                    }
                }
            }
            for (int i = 0; i < groupCount; i++)
            {
                groups[i] = "";
                for (int j = 0; j < groupCounts[i]; j++)
                {
                    int index = rnd.Next(0, allCharacters.Length);
                    groups[i] += allCharacters[index];
                    allCharacters = allCharacters.Remove(index, 1);
                }
            }
            Console.WriteLine("{ \"" + string.Join("\", \"", groups) + "\" }");
        }
    }
}