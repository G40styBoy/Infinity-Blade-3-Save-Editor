using System;
using System.IO;

class Program
{
    static int linesMade;


    public static byte[] Name_Current = { 0x43, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x00 };  // Current
    public static byte[] Name_TotalAcquired = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x41, 0x63, 0x71, 0x75, 0x69, 0x72, 0x65, 0x64, 0x00 };
    public static byte[] Name_TotalFoundInWorld = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x46, 0x6F, 0x75, 0x6E, 0x64, 0x49, 0x6E, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00 };
    public static byte[] Name_TotalFromBattle = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x46, 0x72, 0x6F, 0x6D, 0x42, 0x61, 0x74, 0x74, 0x6C, 0x65, 0x00 };

    static string relativePath = @".\SAVE\UnencryptedSave0.bin";
    static string saveFilePath = Path.GetFullPath(relativePath);

    public class CurrencyStruct
    {
        public int Current { get; set; }
        public int TotalAcquired { get; set; }
        public int TotalFoundInWorld { get; set; }
        public int TotalFromBattle { get; set; }
    }

    static void Main()
    {

        bool readF = File.Exists(saveFilePath);

        if (!readF)
        {
            Console.WriteLine("File not found. Press any key to exit.");
            Console.ReadKey();
            return;
        }

        using (BinaryReader reader = new BinaryReader(File.Open(saveFilePath, FileMode.Open)))
        {
            Stream stream = reader.BaseStream;  // Get the underlying stream
            BinaryWriter writer = new BinaryWriter(stream);  // Create writer instance


            while (true)
            {
                bool returnSetup = setupLines();

                if (!returnSetup)
                {
                    Console.ReadKey();
                    return;
                }

                writeLine("1. Edit Save File");
                writeLine("2. Exit");

                Console.Write("Enter your choice: ");
                linesMade++;

                var choice = Console.ReadLine();

                ClearConsoleLines(linesMade);

                switch (choice)
                {
                    case "1":
                        EditSaveFile(reader, writer);
                        break;
                    case "2":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                //Console.ReadLine();
            }
        }
    }

    static void EditSaveFile(BinaryReader reader, BinaryWriter writer)
    {
        while (true)
        {

            writeLine("1. Edit Currency");
            writeLine("2. Return Home");

            Console.Write("Enter your choice: ");
            linesMade++;

            var editChoice = Console.ReadLine();

            ClearConsoleLines(linesMade);

            switch (editChoice)
            {
                case "1":
                    EditCurrency(reader, writer);
                    break;
                case "2":
                    return;
                default:
                    writeLine("Invalid choice. Please try again.");
                    Console.ReadKey();
                    ClearConsoleLines(linesMade);
                    break;
            }

            //Console.ReadLine();
        }
    }

    static void InfoSection()
    {
        Console.Write("Not implemented yet.");
        Console.ReadKey();
    }

    static void EditCurrency(BinaryReader reader, BinaryWriter writer)
    {
        // goldCurrent = findGold
        // chipsCurrent = findChips
        Util util = new Util();
        bool buff1 = false;
        bool buff2 = false;

        if (reader.BaseStream.Position != 0)  // reset stream position
        {
            ResetStreamPosition(reader);
        }

        CurrencyStruct goldStruct = new CurrencyStruct
        {
            Current = util.FindAndReadValue(reader, Name_Current),
            TotalAcquired = util.FindAndReadValue(reader, Name_TotalAcquired),
            TotalFoundInWorld = util.FindAndReadValue(reader, Name_TotalFoundInWorld),
            TotalFromBattle = util.FindAndReadValue(reader, Name_TotalFromBattle)
        };

        ResetStructFind();

        CurrencyStruct chipsStruct = new CurrencyStruct
        {
            Current = util.FindAndReadValue(reader, Name_Current),
            TotalAcquired = util.FindAndReadValue(reader, Name_TotalAcquired),
            TotalFoundInWorld = 0,
            TotalFromBattle = 0
        };


        writeLine("Gold Stats");
        writeLine("Amount: " + goldStruct.Current);
        writeLine("Total Acquired: " + goldStruct.TotalAcquired);
        writeLine("Total Found In World: " + goldStruct.TotalFoundInWorld);
        writeLine("Total From Battle: " + goldStruct.TotalFromBattle);
        writeLine("");
        writeLine("Chips Stats");
        writeLine("Amount: " + chipsStruct.Current);
        writeLine("Total Acquired: " + chipsStruct.TotalAcquired);
        writeLine("");

        ResetStreamPosition(reader);

        Console.Write("New Gold Value: ");
        linesMade++;
        var goldNew = Console.ReadLine();
        ClearConsoleLines(1);

        Console.Write("New Chips Value: ");
        linesMade++;
        var chipsNew = Console.ReadLine();
        ClearConsoleLines(1);

        bool goldSuc = long.TryParse(goldNew, out long num1);
        bool chipSuc = long.TryParse(chipsNew, out long num2);

        try
        {
            if (!string.IsNullOrEmpty(goldNew) && !string.IsNullOrEmpty(chipsNew))
            {
                buff1 = Util.FindAndWriteValue(reader, writer, Name_Current, ConvertStringToInt(goldNew));
                ResetStructFind();
                buff2 = Util.FindAndWriteValue(reader, writer, Name_Current, ConvertStringToInt(chipsNew));
                FlushStream(writer);  // flush changes
            }
            if (buff1 && buff2)
            {
                ClearConsoleLines(linesMade);
                writeLine("Edits made! Press any key to continue...");
            }
            else
            {
                ClearConsoleLines(linesMade);
                writeLine("Something went wrong!");
                if (!buff1 || !buff2)
                {
                    writeLine("Gold returned: " + buff1);
                    writeLine("Chips returned: " + buff2);
                }
            }

        }

        catch (OverflowException)
        {
            writeLine("Number too big!");
        }

        Console.ReadKey();
        ClearConsoleLines(linesMade);
    }

    static void ClearConsoleLines(int numLines)
    {
        if (numLines <= 0)
            return;

        var currentLineCursor = Console.CursorTop;
        var newLineCursor = currentLineCursor - numLines;
        newLineCursor = newLineCursor < 0 ? 0 : newLineCursor;

        Console.SetCursorPosition(0, newLineCursor);
        for (int i = 0; i < numLines; i++)
        {
            Console.Write(new string(' ', Console.BufferWidth));
            linesMade--;
        }
        Console.SetCursorPosition(0, newLineCursor);
    }


    static void writeLine(string str)
    {
        Console.WriteLine(str);
        linesMade++;
    }

    static bool setupLines()
    {
        string foundFile = "";

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Infinity Blade 3 Save Editor");
        Console.ResetColor();
        Console.Write("Save File Loaded: ");
        if (File.Exists(saveFilePath))
        {
            foundFile = "True";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(foundFile + "\n\n");
        }
        else
        {
            foundFile = "False";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(foundFile + "\n\n");
            Console.ResetColor();
            Console.Write("Make sure the save file is located inside of the file \"SAVE\" so the program can locate it, and that its named \"UnencryptedSave0\".");
            Console.WriteLine("\n\nPress any key to exit program...");
            return false;  // file not found
        }

        Console.ResetColor();
        return true;  // file found
    }

    static void ResetStreamPosition(BinaryReader reader)
    {
        reader.BaseStream.Position = 0;
    }

    static void ResetStructFind()
    {
        Util.StructFound = false;
    }

    static int ConvertStringToInt(string str)
    {
        int result;
        if (int.TryParse(str, out result))
        {
            return result;
        }
        else
        {
            throw new FormatException("The string cannot be converted to an integer.");
        }
    }

    static void FlushStream(BinaryWriter writer)
    {
        writer.Flush();
    }

}
