using System.Text.Json;
using System.IO;
using System.Text;


public class Util
{

    //---------------------------------------------------------------------Special Properties---------------------------------------------------------------------//

    public static byte[] Name_CurrencyStruct = { 0x43, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x63, 0x79, 0x53, 0x74, 0x72, 0x75, 0x63, 0x74, 0x00 };  // Currency Struct
    public static byte[] Name_Current = { 0x43, 0x75, 0x72, 0x72, 0x65, 0x6E, 0x74, 0x00 };  // Current
    public static byte[] Name_TotalAcquired = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x41, 0x63, 0x71, 0x75, 0x69, 0x72, 0x65, 0x64, 0x00 };
    public static byte[] Name_TotalFoundInWorld = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x46, 0x6F, 0x75, 0x6E, 0x64, 0x49, 0x6E, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00 };
    public static byte[] Name_TotalFromBattle = { 0x54, 0x6F, 0x74, 0x61, 0x6C, 0x46, 0x72, 0x6F, 0x6D, 0x42, 0x61, 0x74, 0x74, 0x6C, 0x65, 0x00 };

    //---------------------------------------------------------------------Uniform Properties---------------------------------------------------------------------//


    public static byte[] IntProperty = { 0x49, 0x6E, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x00 };  // IntProperty
    public static byte[] valueSize_Integer = { 0x04, 0x00, 0x00, 0x00 };
    public static byte[] valueByteArray = { 0x00, 0x00, 0x00, 0x00 };


    public static bool StructFound;
    //---------------------------------------------------------------------Util Functions---------------------------------------------------------------------//

    public static byte[] IntToLittleEndian(int number)
    {
        byte[] bytes = BitConverter.GetBytes(number);
        if (BitConverter.IsLittleEndian)
            return bytes;
        else
            return bytes.Reverse().ToArray();
    }

    public static int LittleEndianToInt(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
            return BitConverter.ToInt32(bytes, 0);
        else
            return BitConverter.ToInt32(bytes.Reverse().ToArray(), 0);
    }

    public static void PrintBytes(byte[] bytes)
    {
        foreach (byte b in bytes)
        {
            Console.Write("0x{0:X2} ", b);
        }
        Console.WriteLine();
    }

    public static void ClearConsoleLines(int clearCount)
    {
        Console.SetCursorPosition(0, Console.CursorTop - clearCount);
        for (int i = 0; i < clearCount; i++)
        {
            Console.Write(new string(' ', Console.BufferWidth));
        }
        Console.SetCursorPosition(0, Console.CursorTop - clearCount);
    }

    public static void ConvertStrBytesToCSVariable(string str, string varName)
    {
        string[] hexValuesSplit = str.Split(' ');
        byte[] byteArray = new byte[hexValuesSplit.Length];
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < hexValuesSplit.Length; i++)
        {
            byteArray[i] = Convert.ToByte(hexValuesSplit[i], 16);
        }

        foreach (byte b in byteArray)
        {
            sb.Append("0x");
            sb.Append(b.ToString("X2"));
            sb.Append(", ");
        }

        string result = sb.ToString().TrimEnd(',', ' ');

        if (varName != "")
        {
            Console.WriteLine("public static byte[] " + varName + " = {" + result + "};");
        }


    }





    public static void LocateStructForBinaryReader(BinaryReader reader, byte[] structName)
    {
        int matchIndex = 0;

        while (!StructFound && matchIndex < structName.Length && reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte b = reader.ReadByte();
            if (b == structName[matchIndex])
            {
                matchIndex++;
                if (matchIndex == structName.Length)
                {
                    StructFound = true;  // struct located
                }
            }
        }
    }




    public static bool FindAndWriteValue(BinaryReader reader, BinaryWriter writer, byte[] propertyName, int writeValue)  // Writes value then returns if the value has been written to
    {

        int matchIndex = 0;
        int skipSize = 4;

        if (!StructFound)
        {
            LocateStructForBinaryReader(reader, Name_CurrencyStruct);
        }

        while (reader.BaseStream.Position < reader.BaseStream.Length && matchIndex < propertyName.Length)
        {
            byte b = reader.ReadByte();
            if (b == propertyName[matchIndex])
            {
                matchIndex++;
                if (matchIndex == propertyName.Length)
                {
                    reader.ReadBytes(skipSize); // Skip IntProperty Size
                    reader.ReadBytes(IntProperty.Length);  // Skip IntProperty

                    // Read data and handle potential issues
                    try
                    {
                        reader.ReadBytes(skipSize);  // Skip Value Size
                        reader.ReadBytes(skipSize);  // Skip Value ArrayIndex
                        byte[] writeValueBytes = IntToLittleEndian(writeValue);
                        writer.Write(writeValueBytes);
                        matchIndex = 0; // Reset for next property search
                        return true;

                    }
                    catch (EndOfStreamException)
                    {
                        // Handle end of stream gracefully (e.g., return default value)
                        matchIndex = 0; // Reset for next property search
                        return false; // Or throw an exception based on your application logic
                    }

                }
            }
            else
            {
                matchIndex = 0; // Reset match index if byte doesn't match
            }
        }

        return false; // Property not found
    }

    public int FindAndReadValue(BinaryReader reader, byte[] propertyName)
    {
        int matchIndex = 0;
        int skipSize = 4;

        if (!StructFound)
        {
            LocateStructForBinaryReader(reader, Name_CurrencyStruct);
        }

        while (reader.BaseStream.Position < reader.BaseStream.Length && matchIndex < propertyName.Length)
        {
            byte b = reader.ReadByte();
            if (b == propertyName[matchIndex])
            {
                matchIndex++;
                if (matchIndex == propertyName.Length)
                {
                    reader.ReadBytes(skipSize); // Skip IntProperty Size
                    reader.ReadBytes(IntProperty.Length);  // Skip IntProperty

                    // Read data and handle potential issues
                    try
                    {
                        reader.ReadBytes(skipSize);  // Skip Value Size
                        reader.ReadBytes(skipSize);  // Skip Value ArrayIndex
                        byte[] valueBytes = reader.ReadBytes(4);  // read value
                        return LittleEndianToInt(valueBytes);
                    }
                    catch (EndOfStreamException)
                    {
                        // Handle end of stream gracefully (e.g., return default value)
                        return -1; // Or throw an exception based on your application logic
                    }
                }
            }
            else
            {
                matchIndex = 0; // Reset match index if byte doesn't match
            }
        }

        return -1; // Property not found
    }


}





















/*
Currency Struct Default Property

Variable.IntProperty.Size.ArrayIndex.Value


<>


Current.IntProperty.4.0.MaxInt
43 75 72 72 65 6E 74 00 0C 00 00 00 49 6E 74 50 72 6F 70 65 72 74 79 00 04 00 00 00 00 00 00 00 FF FF FF 7F
<>
Current
43 75 72 72 65 6E 74 00
IntPropSize
0C 00 00 00
IntProperty
49 6E 74 50 72 6F 70 65 72 74 79 00
Size
04 00 00 00
ArrayIndex
00 00 00 00
Value
FF FF FF 7F
Next properties name size
0E 00 00 00

*/

/* Serialize
using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
{
    WriteProperty(writer, "Current", cs.Current);
    WriteProperty(writer, "TotalAcquired", cs.TotalAcquired);
    WriteProperty(writer, "TotalFoundInWorld", cs.TotalFoundInWorld);
    WriteProperty(writer, "TotalFromBattle", cs.TotalFromBattle);
}*/
