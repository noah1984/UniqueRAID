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

namespace noah1984.ParitySplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            string userInput = "";
            while (userInput.ToLower() != "exit" && userInput.ToLower() != "quit")
            {
                bool inputFlag = true;
                Console.Write("Enter Drives: ");
                userInput = Console.ReadLine();
                if(userInput.ToLower() == "exit" || userInput.ToLower() == "quit")
                {
                    break;
                }
                int totalDrives = 0;
                try
                {
                    totalDrives = int.Parse(userInput);
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
                    Console.Write("Enter filename: ");
                    string inputFilename = Console.ReadLine();
                    if (!File.Exists(inputFilename))
                    {
                        Console.WriteLine("File not found");
                        inputFlag = false;
                    }
                    else
                    {
                        //inputFilename = Path.GetFullPath(inputFilename);
                        FileInfo fileInformation = new FileInfo(inputFilename);
                        string dirName = Path.GetDirectoryName(inputFilename);
                        string inputFilenameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilename);
                        string inputExtension = Path.GetExtension(inputFilename);
                        FileStream inputFile = new FileStream(inputFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        List<byte[]> sectionBytes = new List<byte[]>();
                        byte[] buffer = new byte[2];
                        byte[] currentParity = new byte[2];
                        int bytesRead = 0;
                        int blockNumber = 1;
                        int[][] drives = new int[totalDrives][];
                        int segments = 0;
                        int totalSections = 0;
                        if (totalDrives % 2 == 0) //even drive count
                        {
                            segments = totalDrives / 2;
                            totalSections = segments;
                            if (fileInformation.Length != (segments - 1) * 2)
                            {
                                Console.WriteLine("The file must have a total bytes equal to " + (segments - 1) * 2);
                                inputFlag = false;
                            }
                            else
                            {
                                while ((bytesRead = inputFile.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    byte[] currentBuffer = (byte[])buffer.Clone();
                                    sectionBytes.Add(currentBuffer);
                                    if (blockNumber % segments == 1)
                                    {
                                        currentParity = (byte[])buffer.Clone();
                                    }
                                    else
                                    {
                                        for (int i = 0; i < currentParity.Length; ++i)
                                        {
                                            currentParity[i] = (byte)(currentParity[i] ^ buffer[i]);
                                        }
                                        if ((blockNumber + 1) % segments == 0)
                                        {
                                            ++blockNumber;
                                            sectionBytes.Add(currentParity);
                                        }
                                    }
                                    ++blockNumber;
                                }
                                inputFile.Close();
                                for (int i = 0; i < totalDrives; ++i)
                                {
                                    int j = i + 1;
                                    if (j > segments)
                                    {
                                        j = totalDrives - i;
                                    }
                                    drives[i] = new int[1] { j };
                                    string driveSections = String.Format("{0,-9}", "Drive " + (i + 1) + ":");
                                    driveSections += String.Format("{0,3}", j);
                                    Console.WriteLine(driveSections);
                                    string driveStr = "Drive" + (i + 1);
                                    if (!Directory.Exists(dirName + "\\" + totalDrives + "drive\\" + driveStr))
                                    {
                                        Directory.CreateDirectory(dirName + "\\" + totalDrives + "drive\\" + driveStr);
                                    }
                                    FileStream outputFile = new FileStream(dirName + "\\" + totalDrives + "drive\\" + driveStr + "\\" + inputFilenameWithoutExtension +
                                        "_" + j + inputExtension, FileMode.Create);
                                    outputFile.Write(sectionBytes[drives[i][0] - 1], 0, sectionBytes[drives[i][0] - 1].Length);
                                    outputFile.Close();
                                }
                            }
                        }
                        else //odd drive count
                        {
                            segments = (totalDrives - 1) / 2;
                            totalSections = totalDrives * segments;
                            if (fileInformation.Length != totalDrives * (segments - 1) * 2)
                            {
                                Console.WriteLine("The file must have a total bytes equal to " + totalDrives * (segments - 1) * 2);
                                inputFlag = false;
                            }
                            else
                            {
                                while ((bytesRead = inputFile.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    byte[] currentBuffer = (byte[])buffer.Clone();
                                    sectionBytes.Add(currentBuffer);
                                    if (blockNumber % segments == 1)
                                    {
                                        currentParity = (byte[])buffer.Clone();
                                    }
                                    else
                                    {
                                        for (int i = 0; i < currentParity.Length; ++i)
                                        {
                                            currentParity[i] = (byte)(currentParity[i] ^ buffer[i]);
                                        }
                                        if ((blockNumber + 1) % segments == 0)
                                        {
                                            ++blockNumber;
                                            sectionBytes.Add(currentParity);
                                        }
                                    }
                                    ++blockNumber;
                                }
                                inputFile.Close();
                                for (int i = 0; i < totalDrives; ++i)
                                {
                                    drives[i] = new int[segments * 2];
                                    int j = 0;
                                    int k = i;
                                    int l = 0;
                                    for (; j < segments; ++j, --k, ++l)
                                    {
                                        if (k < 0)
                                        {
                                            k = totalDrives - 1;
                                        }
                                        drives[i][l] = k * segments + j + 1;
                                    }
                                    for (--j; j >= 0; --j, --k, ++l)
                                    {
                                        if (k < 0)
                                        {
                                            k = totalDrives - 1;
                                        }
                                        drives[i][l] = k * segments + j + 1;
                                    }
                                }
                                for (int i = 0; i < drives.Length; ++i)
                                {
                                    string driveSections = String.Format("{0,-9}", "Drive " + (i + 1) + ":");
                                    for (int j = 0; j < drives[i].Length; ++j)
                                    {
                                        driveSections += String.Format("{0,4}", drives[i][j]);
                                    }
                                    Console.WriteLine(driveSections);
                                    string driveStr = "Drive" + (i + 1);

                                    if (!Directory.Exists(dirName + "\\" + totalDrives + "drive\\" + driveStr))
                                    {
                                        Directory.CreateDirectory(dirName + "\\" + totalDrives + "drive\\" + driveStr);
                                    }
                                    for (int j = 0; j < drives[i].Length; ++j)
                                    {
                                        FileStream outputFile = new FileStream(dirName + "\\" + totalDrives + "drive\\" + driveStr + "\\" + inputFilenameWithoutExtension +
                                            "_" + drives[i][j] + inputExtension, FileMode.Create);
                                        outputFile.Write(sectionBytes[drives[i][j] - 1], 0, sectionBytes[drives[i][j] - 1].Length);
                                        outputFile.Close();
                                    }
                                    
                                }
                            }
                        }
                        while (userInput.ToLower() != "exit" && userInput.ToLower() != "restart" && userInput.ToLower() != "quit")
                        {
                            bool wasValidBinary = true;
                            Console.WriteLine();
                            Console.Write("Enter failure combination (binary): ");
                            userInput = Console.ReadLine();
                            if (userInput.ToLower() == "exit" || userInput.ToLower() == "restart" || userInput.ToLower() == "quit")
                            {
                                if (userInput.ToLower() == "restart")
                                {
                                    Console.WriteLine("Restarting...");
                                    Console.WriteLine();
                                }
                                break;
                            }
                            for (int i = 0; i < userInput.Length; ++i)
                            {
                                if (userInput[i] != '0' && userInput[i] != '1')
                                {
                                    Console.WriteLine("Invalid input: binary only");
                                    wasValidBinary = false;
                                    break;
                                }
                            }
                            if (wasValidBinary)
                            {
                                if (userInput.Length != totalDrives)
                                {
                                    Console.WriteLine("Invalid input: must be " + totalDrives + "-bit binary");
                                }
                                else
                                {
                                    List<int> remainingSections = new List<int>();
                                    for (int i = 0; i < userInput.Length; ++i)
                                    {
                                        if (userInput[i] == '0')
                                        {
                                            for (int l = 0; l < drives[i].Length; ++l)
                                            {
                                                if (!remainingSections.Contains(drives[i][l]))
                                                {
                                                    remainingSections.Add(drives[i][l]);
                                                }
                                            }
                                        }
                                    }
                                    bool fail = false;
                                    for (int currentBlock = 1; currentBlock < totalSections; currentBlock += segments)
                                    {
                                        int missingBlock = -1;
                                        List<byte[]> remainingBytes = new List<byte[]>();
                                        for (int i = 0; i < segments; ++i)
                                        {
                                            if (!remainingSections.Contains(currentBlock + i))
                                            {
                                                if (missingBlock != -1)
                                                {
                                                    fail = true;
                                                    break;
                                                }
                                                missingBlock = i;
                                            }
                                            else
                                            {
                                                remainingBytes.Add(sectionBytes[currentBlock + i - 1]);
                                            }
                                        }
                                        for (int j = 0; j < segments - 1; ++j)
                                        {

                                            if (j == missingBlock && !fail)
                                            {
                                                byte[] missingBuffer = (byte[])((byte[])remainingBytes[0]).Clone();
                                                for (int i = 1; i < remainingBytes.Count; ++i)
                                                {
                                                    byte[] tempBuffer = (byte[])remainingBytes[i];
                                                    for (int k = 0; k < tempBuffer.Length; ++k)
                                                    {
                                                        missingBuffer[k] = (byte)(missingBuffer[k] ^ tempBuffer[k]);
                                                    }
                                                }
                                                Console.WriteLine("Found bytes: " + Encoding.UTF8.GetString(missingBuffer) + " (recovered)");
                                            }
                                            else if (remainingSections.Contains(currentBlock + j))
                                            {
                                                Console.WriteLine("Found bytes: " + Encoding.UTF8.GetString(sectionBytes[currentBlock + j - 1]));
                                            }
                                            else
                                            {
                                                Console.WriteLine("Unable to recover segment: " + (currentBlock + j));
                                            }
                                        }
                                    }
                                    Console.Write("Result for " + userInput + ": ");
                                    if (fail)
                                    {
                                        Console.WriteLine("BAD");
                                    }
                                    else
                                    {
                                        Console.WriteLine("GOOD");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
