namespace JournalInitializerHelper
{
    static class HelperClass
    {
        static void Main(string[] args)
        {
            string[] arr = new string[2];
            arr[0] = $"\t\tstatic readonly int KeyHash = {GetStableHashCode(args[0])};";
            arr[1] = CharacterGroups(int.Parse(args[1]));
            File.WriteAllLines(args[2], arr);
        }

        static string CharacterGroups(int groupCount)
        {
            Random rnd = new();
            string allCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()-_=+[{]};:'|,<.>/? áéíóöőúüűÁÉÍÓÖŐÚÜŰ";
            string copyStr = "";
            foreach (var item in allCharacters)
            {
                copyStr += item;
            }
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
            if (new string(copyStr.OrderBy(x => x).ToArray()) != new string(string.Join("", groups).OrderBy(x => x).ToArray()))
            {
                throw new Exception("The input and output does not match");
            }
            return "\t\tstatic readonly string[] charGroups = { \"" + string.Join("\", \"", groups) + "\" };";
        }

        static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = (hash1 << 5) + hash1 ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = (hash2 << 5) + hash2 ^ str[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }
    }
}