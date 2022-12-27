namespace EncryptedJournal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                var charArgs = new List<char>();
                foreach (var item in args)
                {
                    foreach (var character in item)
                    {
                        charArgs.Add(character);
                    }
                }
                Cryption.Run(charArgs.Distinct());
            }
        }
    }

    public static class Cryption
    {
		static readonly int KeyHash = -1872639815;

		static readonly string WorkingDir = @".";

        #region Data for all methods
		static readonly string[] charGroups = { "aábcdeéfghiíjklmnoóöőpqrstuúüűvwxyz", "AÁBCDEÉFGHIÍJKLMNOÓÖŐPQRSTUÚÜŰVWXYZ", "0123456789", "~!@#$%^&*()-_=+[{]};:'|,<.>/? " };

        static bool InputFlag = false;
        static bool OutputFlag = false;
        static bool KeyStrokeFlag = false;
        static bool LastFileFlag = false;
        static bool FileFlag = false;
        static bool SecretFlag = false;

        static int FileIndex = -1;

        static string? InputFile = "";
        static string? OutputFile = "";

        #region Options
        static readonly string Options =
            "You need to provide the right password first,\n" +
            "then you can enter the journal.\n" +
            "Options:\n" +
                "\ti - Input mode, compatible with Output mode,\n" +
                    "\t\tyou can break it with Ctrl + C\n" +
                "\to - Output mode, compatible with Input mode\n" +
                "\te - Encrypt mode, you can provide an input\n" +
                    "\t\tand it prints out encrypted\n" +
                "\td - Decrypt mode, you can provide an encrypted input\n" +
                    "\t\tand it prints out decrypted\n" +
                "\tm - Import mode, you can encrypt regular text,\n" +
                    "\t\tasks for an input file and an output file\n" +
                "\tx - Export mode, you can decrypt regular text,\n" +
                    "\t\tasks for an input file and an output file\n" +
                "\tw - You can set a new working directory for\n" +
                    "\t\tthe current run\n" +
                "\ts - An option for Input mode,\n" +
                    "\t\tthe input will be hidden (secret)\n" +
                "\tk - An option for Output mode,\n" +
                    "\t\tyou need to provide a keystroke\n" +
                    "\t\tfor every entry to be printed\n" +
                "\tf - An option for Output mode,\n" +
                    "\t\toutputs the file specified by its index\n" +
                    "\t\t(eg: 0 - \"Journal_0\")\n" +
                "\tl - An option for Output mode,\n" +
                    "\t\toutputs the last file\n" +
                "\tg - Get info (working directory, characterGroups)\n" +
                "\th - Outputs this help message\n" +
            "One of the six modes is mandatory, if you provide\n" +
            "both Output mode and Input mode, Output mode comes first.\n" +
            "Encrypt mode, Decrypt mode, Import mode and\n" +
            "Export mode are incompatible with all other modes.\n" +
            "You can break Input mode, Encrypt mode and\n" +
            "Decrypt mode with Ctrl + C.\n" +
            "Note: For Encrypt mode, Decrypt mode, Import mode\n" +
            "and Export mode whichever you pass as an option comes first.";
        #endregion
        #endregion

        /// <summary>
        /// Main method that runs.
        /// </summary>
        /// <param name="args"></param>
        public static void Run(IEnumerable<char> args)
        {
            if (args.Contains('h') || HiddenInput().GetStableHashCode() != KeyHash)
            {
                Console.WriteLine(Options);
                return;
            }
            Console.WriteLine("Password OK");
            if (args.Contains('g'))
            {
                Console.WriteLine("\n_.-Working Directory-._\n" + WorkingDir + "\n\n_.-Charcter Groups-._\n");
                foreach (var item in charGroups)
                {
                    Console.WriteLine(item);
                }
                return;
            }
            Directory.SetCurrentDirectory(WorkingDir);
            if (args.Contains('w'))
            {
                Console.Write("WorkingDirectory: ");
                Directory.SetCurrentDirectory(Console.ReadLine());
            }
            foreach (var item in args)
            {
                switch (item)
                {
                    case 'i': InputFlag = true; break;
                    case 'o': OutputFlag = true; break;
                    case 'k': KeyStrokeFlag = true; break;
                    case 'f': FileFlag = true; break;
                    case 'l': LastFileFlag = true; break;
                    case 's': SecretFlag = true; break;
                    case 'e': EncryptMode(); return;
                    case 'd': DecryptMode(); return;
                    case 'm': ImportMode(); return;
                    case 'x': ExportMode(); return;
                    default:
                        break;
                }
            }
            if (FileFlag && !LastFileFlag)
            {
                Console.Write("FileIndex: ");
                _ = int.TryParse(Console.ReadLine(), out FileIndex);
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
            string input = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                if (key.Key == ConsoleKey.Backspace && input != "")
                {
                    input = input.Remove(input.Length - 1, 1);
                }
                else
                {
                    input += key.KeyChar;
                }
            }
            return input;
        }

        static void EncryptMode()
        {
            while (true)
            {
            InputStart:
                string? toEncrypt = Console.ReadLine();
                if (toEncrypt == null || toEncrypt == "") goto InputStart;
                Console.WriteLine(Encrypt(toEncrypt));
            }
        }

        static void DecryptMode()
        {
            while (true)
            {
            InputStart:
                string? toDecrypt = Console.ReadLine();
                if (toDecrypt == null || toDecrypt == "") goto InputStart;
                Console.WriteLine(Decrypt(toDecrypt));
            }
        }

        static void ImportMode()
        {
            Console.WriteLine("You asked for import mode.");
            Console.Write("InputFile: ");
            InputFile = Console.ReadLine();
            if (!File.Exists(InputFile))
            {
                throw new Exception("File does not exists.");
            }
            Console.Write("OutpuFile: ");
            OutputFile = Console.ReadLine();
            if (File.Exists(OutputFile))
            {
                Console.Write("WARNING! File already exists, do you want to proceed? (yes/anything else) ");
                if (Console.ReadLine() != "yes")
                {
                    return;
                }
            }
            var input = File.ReadAllLines(InputFile);
            var output = input.Select(s => Encrypt(s)).ToArray();
            File.WriteAllLines(OutputFile, output);
        }

        static void ExportMode()
        {
            Console.WriteLine("You asked for export mode.");
            Console.Write("InputFile: ");
            InputFile = Console.ReadLine();
            if (!File.Exists(InputFile))
            {
                throw new Exception("File does not exists.");
            }
            Console.Write("OutpuFile: ");
            OutputFile = Console.ReadLine();
            if (File.Exists(OutputFile))
            {
                Console.Write("WARNING! File already exists, do you want to proceed? (yes/anything else) ");
                if (Console.ReadLine() != "yes")
                {
                    return;
                }
            }
            var input = File.ReadAllLines(InputFile);
            var output = input.Select(s => Decrypt(s)).ToArray();
            File.WriteAllLines(OutputFile, output);
        }

        #region Input mode
        static void InputStream()
        {
            while (true)
            {
            InputStart:
                string? toEncrypt;
                if (SecretFlag)
                {
                    toEncrypt = HiddenInput();
                }
                else
                {
                    toEncrypt = Console.ReadLine();
                }
                if (toEncrypt == null || toEncrypt == "") goto InputStart;
                JournalEntryAppendix(Encrypt(DateTime.Now.ToString("F") + $" - {toEncrypt}"));
            }
        }

        static string Encrypt(string input)
        {
            var firstTransformation = input.Select(c => CharacterAlteringEncrypt(c));
            var secondTransformation = firstTransformation.Select((c, i) => i % 2 != 0 && i % 5 != 0 ? c : char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToArray();
            var temp = new string(Enumerable.Range(0, secondTransformation.Length).Where(i => (i + 1) % 3 == 0).Select(i => secondTransformation[i]).Reverse().ToArray());
            var thirdTransformation = new string(Enumerable.Range(0, secondTransformation.Length).Where(i => (i + 1) % 3 != 0).Select(i => secondTransformation[i]).ToArray()) + temp;
            return thirdTransformation;
        }

        static void JournalEntryAppendix(string entry)
        {
            if (!File.Exists("journal_0"))
            {
                var fileCreation = File.Create("journal_0");
                fileCreation.Close();
            }
            var journal = Directory.GetFiles(".").Where(s => s.Contains("journal")).OrderBy(s => s).Last();
            if (new FileInfo(journal).Length > 20480)
            {
                journal = "journal_" + (int.Parse(journal.Split('_')[1]) + 1);
                var fileCreation = File.Create(journal);
                fileCreation.Close();
            }
            File.AppendAllLines(journal, new string[] { entry });
            if (SecretFlag)
            {
                Console.WriteLine("New entry successfully added.");
            }
        }
        #endregion

        #region Output mode
        static void OutputStream()
        {
            var journals = Directory.GetFiles(".").Where(s => s.Contains("journal")).OrderBy(s => s);
            if (FileFlag && File.Exists($"journal_{FileIndex}"))
            {
                journals = new string[] { $"journal_{FileIndex}" }.OrderBy(s => s);
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
                        reordering += entry[^1];
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
            try
            {
                int groupIndex = Enumerable.Range(0, charGroups.Length).Where(g => charGroups[g].Contains(character)).FirstOrDefault();
                int newIndex = charGroups[groupIndex].IndexOf(character) + (charGroups[groupIndex].Length / 3);
                return newIndex > charGroups[groupIndex].Length - 1 ? charGroups[groupIndex][newIndex - charGroups[groupIndex].Length] : charGroups[groupIndex][newIndex];
            }
            catch (Exception)
            {
                return character;
            }
        }

        static char CharacterAlteringDecrypt(char character)
        {
            try
            {
                int groupIndex = Enumerable.Range(0, charGroups.Length).Where(g => charGroups[g].Contains(character)).First();
                int newIndex = charGroups[groupIndex].IndexOf(character) - (charGroups[groupIndex].Length / 3);
                return newIndex < 0 ? charGroups[groupIndex][newIndex + charGroups[groupIndex].Length] : charGroups[groupIndex][newIndex];
            }
            catch (Exception)
            {
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
