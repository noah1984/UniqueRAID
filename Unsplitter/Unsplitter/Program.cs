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

namespace Unsplitter
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
                if (fileNameWithoutExtension.LastIndexOf('_') == -1)
                {
                    fileNameWithoutExtBlock = fileNameWithoutExtension;
                }
                else
                {
                    fileNameWithoutExtBlock = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.LastIndexOf('_'));
                }
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
                    int totalDrives = segments * 2 + 1;
                    int totalSections = totalDrives * segments;
                    FileStream outputFile = new FileStream(dirName + "\\" + fileNameWithoutExtBlock + fileExtension, FileMode.Create);
                    Queue toWrite = new Queue();
                    for (int currentBlock = 1; currentBlock < totalSections; currentBlock += segments)
                    {
                        int missingBlock = 0;
                        bool multipleMissing = false;
                        Hashtable fileBytes = new Hashtable();
                        string[] filenames = new string[segments];
                        for (int i = 0; i < segments; ++i)
                        {
                            filenames[i] = dirName + "\\" + fileNameWithoutExtBlock + "_" + (currentBlock + i) + fileExtension;
                            if (!File.Exists(filenames[i]))
                            {
                                if (missingBlock != 0)
                                {
                                    Console.WriteLine("Cannot rebuild: multiple segments missing");
                                    multipleMissing = true;
                                    break;
                                }
                                missingBlock = currentBlock + i;
                            }
                            else
                            {
                                FileStream inputFile = new FileStream(filenames[i], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                                byte[] inputFileBytes = new byte[2];
                                inputFile.Read(inputFileBytes, 0, inputFileBytes.Length);
                                inputFile.Close();
                                fileBytes[filenames[i]] = inputFileBytes;
                                if (i != segments - 1)
                                {
                                    toWrite.Enqueue(inputFileBytes);
                                }
                            }
                        }

                        if (!multipleMissing)
                        {
                                
                            if (missingBlock % segments == 0 || missingBlock == 0)
                            {
                                //perform data verification
                                while(toWrite.Count > 0)
                                {
                                    byte[] tempBuffer = (byte[])toWrite.Dequeue();
                                    Console.WriteLine("Writting " + FromBytes(tempBuffer));
                                    outputFile.Write(tempBuffer, 0, tempBuffer.Length);
                                }
                            }
                            else if (fileBytes.Count > 0)
                            {

                                while (toWrite.Count > 0)
                                {
                                    toWrite.Dequeue();
                                }
                                ArrayList fileBytesValues = new ArrayList(fileBytes.Values);
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
                                    if (!fileBytes.ContainsKey(filenames[i]))
                                    {
                                        Console.WriteLine("Writting " + FromBytes(missingBuffer) + " (recovered)");
                                        outputFile.Write(missingBuffer, 0, missingBuffer.Length);
                                    }
                                    else if(i < segments - 1)
                                    {
                                        byte[] tempBytes = (byte[])fileBytes[filenames[i]];
                                        Console.WriteLine("Writting " + FromBytes(tempBytes));
                                        outputFile.Write(tempBytes, 0, tempBytes.Length);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Problem at missing block " + missingBlock);
                            }
                        }
                    }
                    outputFile.Close();
                    Console.WriteLine("File rebuild complete");
                    
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
