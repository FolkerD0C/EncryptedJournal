public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 0)
        {
            Cryption.Run(args);
        }
    }
}

public static class Cryption
{
    static readonly int KeyHash = 0; //You need to hash a password first with the GetStableHashCode method

    #region Data for all methods
    //These can be rearranged to new mixed groups
    static readonly string alphabeticLowerCase = "abcdefghijklmnopqrstuvwxyz";
    static readonly string alphabeticUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static readonly string numeric = "0123456789";
    static readonly string nonAlphanumeric = "\'\"+!%/=()\\|[]<>#&@{},?.:-_ ";

    static bool InputFlag = false;
    static bool OutputFlag = false;
    static bool KeyStrokeFlag = false;
    static bool LastFileFlag = false;
    static bool FileFlag = false;
    static bool SecretFlag = false;

    static int FileIndex = -1;

    static readonly string Options =
        "You need to provide the right password first,\n" +
        "then you can enter the journal.\n" +
        "Options:\n" +
            "\ti - Input mode, compatible with Output mode\n" +
            "\to - Output mode, compatible with Input mode\n" +
            "\tk - An option for Output mode,\n" +
                "\t\tyou need to provide a keystroke\n" +
                "\t\tfor every entry to be printed\n" +
            "\tf - An option for Output mode,\n" +
                "\t\toutputs the file specified by its index\n" +
                "\t\t(eg: 0 - \"Journal_0\")\n" +
            "\tl - An option for Output mode,\n" +
                "\t\toutputs the last file\n" +
            "\ts - An option for Input mode,\n" +
                "\t\tthe input will be hidden (secret)" +
            "\th - Outputs this help message\n" +
        "Output mode or Input mode are mandatory,\n" +
        "if you provide both, Output mode comes first.";
    #endregion

    /// <summary>
    /// Main method that runs.
    /// </summary>
    /// <param name="args"></param>
    public static void Run(string[] args)
    {
        if (args.Contains("h") || HiddenInput().GetStableHashCode() != KeyHash)
        {
            Console.WriteLine(Options);
            return;
        }
        foreach (var item in args)
        {
            switch (item)
            {
                case "i": InputFlag = true; break;
                case "o": OutputFlag = true; break;
                case "k": KeyStrokeFlag = true; break;
                case "f": FileFlag = true; break;
                case "l": LastFileFlag = true; break;
                case "s": SecretFlag = true; break;
                default:
                    break;
            }
        }
        if (FileFlag && !LastFileFlag)
        {
            Console.Write("FileIndex: ");
            int.TryParse(Console.ReadLine(), out FileIndex);
        }
        if (InputFlag && OutputFlag)
        {
            OutputStream();
            InputStream();
        }
        else if (OutputFlag) OutputStream();
        else if (InputFlag) InputStream();
    }

    //https://stackoverflow.com/questions/23433980/c-sharp-console-hide-the-input-from-console-window-while-typing
    static string HiddenInput()
    {
        string password = "";
        while (true)
        {
            var key = System.Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
                break;
            password += key.KeyChar;
        }
        return password;
    }

    #region Input mode
    public static void InputStream()
    {
        while (true)
        {
        InputStart:
            string toEncrypt = "";
            if (SecretFlag)
            {
                toEncrypt = HiddenInput();
            }
            else
            {
                toEncrypt = Console.ReadLine();
            }
            if (toEncrypt == null || toEncrypt == "") goto InputStart;
            Encrypt(toEncrypt);
        }
    }

    static void Encrypt(string input)
    {
        input = DateTime.Now.ToString("F") + $" - {input}";
        var firstTransformation = input.Select(c => CharacterAlteringEncrypt(c));
        var secondTransformation = firstTransformation.Select((c, i) => i % 2 != 0 && i % 5 != 0 ? c : char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray();
        var temp = new string(Enumerable.Range(0, secondTransformation.Length).Where(i => (i + 1) % 3 == 0).Select(i => secondTransformation[i]).Reverse().ToArray());
        var thirdTransformation = new string(Enumerable.Range(0, secondTransformation.Length).Where(i => (i + 1) % 3 != 0).Select(i => secondTransformation[i]).ToArray()) + temp;
        JournalEntryAppendix(thirdTransformation);
    }

    static void JournalEntryAppendix(string entry)
    {
        if (!File.Exists("Journal_0"))
        {
            var fileCreation = File.Create("Journal_0");
            fileCreation.Close();
        }
        var journal = Directory.GetFiles(".").Where(s => s.Contains("Journal")).OrderBy(s => s).Last();
        if (new FileInfo(journal).Length > 102400)
        {
            journal = "Journal_" + (int.Parse(journal.Split('_')[1]) + 1);
            var fileCreation = File.Create(journal);
            fileCreation.Close();
        }
        File.AppendAllLines(journal, new string[] { entry });
    }
    #endregion

    #region Output mode
    public static void OutputStream()
    {
        var journals = Directory.GetFiles(".").Where(s => s.Contains("Journal")).OrderBy(s => s);
        if (FileFlag && File.Exists($"Journal_{FileIndex}"))
        {
            journals = new string[] { $"Journal_{FileIndex}" }.OrderBy(s => s);
        }
        if (LastFileFlag)
        {
            journals = new string[] { journals.Last() }.OrderBy(s => s);
        }
        foreach (var journal in journals)
        {
            var output = File.ReadAllLines(journal);
            foreach (var entry in output)
            {
                Console.WriteLine(Decrypt(entry));
                if (KeyStrokeFlag) Console.ReadKey(true);
            }
        }
    }

    static string Decrypt(string entry)
    {
        string reordering = "";
        int tempForCounting = 0;
        for (int j = 0; j < entry.Length; j++)
        {
            try
            {
                if ((j + 1 + tempForCounting) % 3 == 0)
                {
                    reordering += entry[entry.Length - 1];
                    entry = entry.Remove(entry.Length - 1);
                    reordering += entry[j];
                    tempForCounting++;
                }
                else
                {
                    reordering += entry[j];
                }
            }
            catch (Exception)
            {

            }
        }
        entry = reordering;
        var recasing = entry.Select((c, i) => i % 2 != 0 && i % 5 != 0 ? c : char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c));
        var remapping = recasing.Select(c => CharacterAlteringDecrypt(c));
        return new string(remapping.ToArray());
    }
    #endregion

    #region Character transformation
    static char CharacterAlteringEncrypt(char character)
    {
        int casingInSwitchStatement = alphabeticLowerCase.Contains(character) ? 0 : alphabeticUpperCase.Contains(character) ? 1 : numeric.Contains(character) ? 2 : nonAlphanumeric.Contains(character) ? 3 : -1;
        switch (casingInSwitchStatement)
        {
            case 0:
                int charIndex13 = alphabeticLowerCase.IndexOf(character) + 13;
                return charIndex13 > alphabeticLowerCase.Length - 1 ? alphabeticLowerCase[charIndex13 - alphabeticLowerCase.Length] : alphabeticLowerCase[charIndex13];
            case 1:
                int charIndex11 = alphabeticUpperCase.IndexOf(character) + 11;
                return charIndex11 > alphabeticUpperCase.Length - 1 ? alphabeticUpperCase[charIndex11 - alphabeticUpperCase.Length] : alphabeticUpperCase[charIndex11];
            case 2:
                int charIndex3 = numeric.IndexOf(character) + 3;
                return charIndex3 > numeric.Length - 1 ? numeric[charIndex3 - numeric.Length] : numeric[charIndex3];
            case 3:
                int charIndex7 = nonAlphanumeric.IndexOf(character) + 7;
                return charIndex7 > nonAlphanumeric.Length - 1 ? nonAlphanumeric[charIndex7 - nonAlphanumeric.Length] : nonAlphanumeric[charIndex7];
            default:
                return character;
        }
    }

    static char CharacterAlteringDecrypt(char character)
    {
        int casingInSwitchStatement = alphabeticLowerCase.Contains(character) ? 0 : alphabeticUpperCase.Contains(character) ? 1 : numeric.Contains(character) ? 2 : nonAlphanumeric.Contains(character) ? 3 : -1;
        switch (casingInSwitchStatement)
        {
            case 0:
                int charIndex13 = alphabeticLowerCase.IndexOf(character) - 13;
                return charIndex13 < 0 ? alphabeticLowerCase[charIndex13 + alphabeticLowerCase.Length] : alphabeticLowerCase[charIndex13];
            case 1:
                int charIndex11 = alphabeticUpperCase.IndexOf(character) - 11;
                return charIndex11 < 0 ? alphabeticUpperCase[charIndex11 + alphabeticUpperCase.Length] : alphabeticUpperCase[charIndex11];
            case 2:
                int charIndex3 = numeric.IndexOf(character) - 3;
                return charIndex3 < 0 ? numeric[charIndex3 + numeric.Length] : numeric[charIndex3];
            case 3:
                int charIndex7 = nonAlphanumeric.IndexOf(character) - 7;
                return charIndex7 < 0 ? nonAlphanumeric[charIndex7 + nonAlphanumeric.Length] : nonAlphanumeric[charIndex7];
            default:
                return character;
        }
    }
    #endregion

    //https://stackoverflow.com/questions/36845430/persistent-hashcode-for-strings
    static int GetStableHashCode(this string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}