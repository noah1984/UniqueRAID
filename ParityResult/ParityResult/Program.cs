// Copyright (C) 2017 Noah Allen
//
// This file is part of UniqueRAID.
//
// UniqueRAID is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// UniqueRAID is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ParityResult
{
    class Program
    {
        static void Main(string[] args)
        {
            bool inputFlag = false;
            while (!inputFlag)
            {
                inputFlag = true;
                Console.Write("Enter filename: ");
                string inputFilename = Console.ReadLine();
                string dirName = Path.GetDirectoryName(inputFilename);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilename);
                string fileNameWithoutExtBlock = "";
                string fileExtension = Path.GetExtension(inputFilename);
                int inputBlock = 0;
                try
                {
                    inputBlock = int.Parse(fileNameWithoutExtension.Substring(fileNameWithoutExtension.LastIndexOf('_') + 1));
                    fileNameWithoutExtBlock = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.LastIndexOf('_'));
                }
                catch
                {
                        
                    inputFlag = false;
                }
                if (!inputFlag)
                {
                    Console.WriteLine("Invalid File");
                }
                else
                {
                    Console.Write("Enter segments: ");
                    string userInput = Console.ReadLine();
                    int segments = 0;
                    try
                    {
                        segments = int.Parse(userInput);
                    }
                    catch
                    {
                        
                        inputFlag = false;
                    }
                    if (!inputFlag)
                    {
                        Console.WriteLine("Invalid input: numbers only");
                    }
                    else
                    {

                        Console.Write("Enter block size: ");
                        userInput = Console.ReadLine();
                        int blockSize = 0;
                        try
                        {
                            blockSize = int.Parse(userInput);
                        }
                        catch
                        {

                            inputFlag = false;
                        }
                        if (!inputFlag)
                        {
                            Console.WriteLine("Invalid input: numbers only");
                        }
                        else
                        {
                            bool multipleMissing = false;
                            Hashtable fileBytes = new Hashtable();
                            string[] filenames = new string[segments];
                            int firstBlock = 0;
                            int missingBlock = 0;
                            int mod = inputBlock % segments;
                            if (mod == 0)
                            {
                                firstBlock = inputBlock - segments + 1;
                            }
                            else
                            {
                                firstBlock = inputBlock - mod + 1;
                            }
                            for (int i = 0; i < segments; ++i)
                            {
                                filenames[i] = dirName + "\\" + fileNameWithoutExtBlock + "_" + (firstBlock + i) + fileExtension;
                                if (!File.Exists(filenames[i]))
                                {
                                    if (missingBlock != 0)
                                    {
                                        Console.WriteLine("Cannot rebuild: multiple segments missing");
                                        multipleMissing = true;
                                        break;
                                    }
                                    missingBlock = firstBlock + i;
                                }
                                else
                                {
                                    FileStream inputFile = new FileStream(filenames[i], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                                    byte[] inputFileBytes = new byte[blockSize];
                                    inputFile.Read(inputFileBytes, 0, inputFileBytes.Length);
                                    inputFile.Close();
                                    fileBytes[filenames[i]] = inputFileBytes;
                                }
                            }
                            if (!multipleMissing)
                            {
                                if (missingBlock == 0)
                                {
                                    //perform data verification
                                    Console.WriteLine("Perform data verification");
                                }
                                else if(fileBytes.Count > 0)
                                {
                                    ArrayList fileBytesValues = new ArrayList(fileBytes.Values);
                                    Console.WriteLine();
                                    byte[] missingBuffer = (byte[])((byte[])fileBytesValues[0]).Clone();
                                    for (int i = 1; i < fileBytesValues.Count; ++i)
                                    {
                                        byte[] tempBuffer = (byte[])fileBytesValues[i];
                                        for (int j = 0; j < tempBuffer.Length; ++j)
                                        {
                                            missingBuffer[j] = (byte)(missingBuffer[j] ^ tempBuffer[j]);
                                        }
                                    }
                                    for (int i = 0; i < filenames.Length; ++i)
                                    {
                                        Console.Write("Segment " + (i + 1));
                                        if (!fileBytes.ContainsKey(filenames[i]))
                                        {
                                            Console.WriteLine(" (recovered)");
                                            FileStream outputFile = new FileStream(filenames[i], FileMode.Create);
                                            outputFile.Write(missingBuffer, 0, missingBuffer.Length);
                                            outputFile.Close();
                                            fileBytes[filenames[i]] = missingBuffer;
                                        }
                                        else if (i + 1 == inputBlock - firstBlock + 1)
                                        {
                                            Console.WriteLine(" (selected)");
                                        }
                                        else
                                        {
                                            Console.WriteLine();
                                        }
                                        byte[] tempBytes = (byte[])fileBytes[filenames[i]];
                                        Console.Write("        ");
                                        for (int j = 0; j < tempBytes.Length; ++j)
                                        {
                                            Console.Write("Byte {0}  ", (j + 1).ToString().PadRight(3, ' '));
                                        }
                                        Console.WriteLine();
                                        Console.Write("Binary  ");
                                        for (int j = 0; j < tempBytes.Length; ++j)
                                        {
                                            string singleCharStr = FromBytes(new byte[] { tempBytes[j] });
                                            Console.Write(ToBinary(singleCharStr) + "  ");
                                        }
                                        Console.WriteLine();
                                        Console.Write("Hex     ");
                                        for (int j = 0; j < tempBytes.Length; ++j)
                                        {
                                            string singleCharStr = FromBytes(new byte[] { tempBytes[j] });
                                            Console.Write("0x" + ToHex(singleCharStr) + "      ");
                                        }
                                        Console.WriteLine();
                                        Console.Write("Char    ");
                                        for (int j = 0; j < tempBytes.Length; ++j)
                                        {
                                            string singleCharStr = FromBytes(new byte[] { tempBytes[j] });
                                            Console.Write(singleCharStr + "         ");
                                        }
                                        Console.WriteLine();
                                        Console.Write("Decimal ");
                                        for (int j = 0; j < tempBytes.Length; ++j)
                                        {
                                            string singleCharStr = FromBytes(new byte[] { tempBytes[j] });
                                            Console.Write(((int)tempBytes[j]).ToString().PadRight(3, ' ') + "       ");
                                        }
                                        Console.WriteLine();
                                        Console.WriteLine();
                                        //Console.WriteLine("'" + FromBytes((byte[])fileBytes[filenames[i]]) + "', " + ToHex(fileBytes[filenames[i]]) + ", ");
                                            
                                    }
                                    Console.WriteLine("Successful segment recovery");
                                }
                                else
                                {
                                    Console.WriteLine("Problem with files being read");
                                }
                            }
                        }
                    }
                    
                }
                
                Console.ReadLine();
            }
        }

        public static string ToHex(string ascii)
        {
            string hex = "";
            for (int i = 0; i < ascii.Length; ++i)
            {
                int tmp = ascii[i];
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }
        public static string ToBinary(string ascii)
        {
            string binary = "";
            byte[] asciiBytes = Encoding.UTF8.GetBytes(ascii);
            for (int i = 0; i < asciiBytes.Length; ++i)
            {
                binary += Convert.ToString(asciiBytes[i], 2);
            }
            return binary.PadLeft(8, '0');
        }
        public static string FromBytes(byte[] byteArray)
        {
            return Encoding.UTF8.GetString(byteArray);
        }
    }
}
