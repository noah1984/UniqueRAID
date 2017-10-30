using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FailureReportGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string userInput = "";
            string folder = "";
            while (userInput.ToLower() != "exit" && userInput.ToLower() != "quit")
            {
                if (folder == "")
                {
                    Console.Write("Enter folder: ");
                    userInput = Console.ReadLine();
                    if (!Directory.Exists(userInput))
                    {
                        if (userInput.ToLower() != "exit" && userInput.ToLower() != "quit")
                        {
                            Console.WriteLine("Folder not found");
                        }
                    }
                    else
                    {
                        folder = userInput;
                    }
                }
                else
                {
                    bool inputFlag = true;
                    Console.Write("Enter drives: ");
                    userInput = Console.ReadLine();
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
                        if (userInput.ToLower() != "exit" && userInput.ToLower() != "quit")
                        {
                            Console.WriteLine("Invalid input: numbers only");
                        }
                    }
                    else if (totalDrives < 6 || totalDrives > 32)
                    {
                        Console.WriteLine("Invalid input: drives must be between 6 and 32, inclusive");
                    }
                    else
                    {
                        int[][] drives = new int[totalDrives][];
                        int segments = 0;
                        int totalSections = 0;
                        FileStream failureReport = new FileStream(folder + "\\" + totalDrives + "_drive_failure_report.txt", FileMode.Create);
                        byte[] baNewLine = Encoding.UTF8.GetBytes("\r\n");
                        if (totalDrives % 2 == 0) //even drive count
                        {
                            segments = totalDrives / 2;
                            totalSections = segments;
                            for (int i = 0; i < totalDrives; ++i)
                            {
                                int j = i + 1;
                                if (j > segments)
                                {
                                    j = totalDrives - i;
                                }
                                drives[i] = new int[1] { j };
                            }
                            for (int i = 0; i < totalDrives; ++i)
                            {
                                int currentSection = i + 1;
                                if (i >= segments)
                                {
                                    currentSection = totalDrives - i;
                                }
                                string driveSections = String.Format("{0,-9}", "Drive " + (i + 1) + ":");
                                driveSections += String.Format("{0,3}", currentSection);
                                Console.WriteLine(driveSections);
                                byte[] baDriveSections = Encoding.UTF8.GetBytes(driveSections);
                                failureReport.Write(baDriveSections, 0, baDriveSections.Length);
                                failureReport.Write(baNewLine, 0, baNewLine.Length);
                            }
                            failureReport.Write(baNewLine, 0, baNewLine.Length);
                        }
                        else //odd drive count
                        {
                            segments = (totalDrives - 1) / 2;
                            totalSections = totalDrives * segments;
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
                                byte[] baDriveSections = Encoding.UTF8.GetBytes(driveSections);
                                failureReport.Write(baDriveSections, 0, baDriveSections.Length);
                                failureReport.Write(baNewLine, 0, baNewLine.Length);
                            }
                            failureReport.Write(baNewLine, 0, baNewLine.Length);
                        }
                        //failureReport.Close();
                        Console.WriteLine();
                        for (int i = 1; i < totalDrives; ++i)
                        {
                            int totalCominations = 0;
                            int totalFailures = 0;
                            string failureHeader = i + " drive failure:";
                            Console.WriteLine(failureHeader);
                            byte[] baFailureHeader = Encoding.UTF8.GetBytes(failureHeader);
                            failureReport.Write(baFailureHeader, 0, baFailureHeader.Length);
                            failureReport.Write(baNewLine, 0, baNewLine.Length);

                            for (int j = 0; j < Math.Pow(2, totalDrives); ++j)
                            {
                                if (CountOneBits(j) == i)
                                {
                                    ++totalCominations;
                                    string binaryStr = Convert.ToString(j, 2).PadLeft(totalDrives, '0');
                                    byte[] baBinaryStr = Encoding.UTF8.GetBytes(binaryStr);
                                    List<int> remainingSections = new List<int>();
                                    for (int k = 0; k < binaryStr.Length; ++k)
                                    {
                                        if (binaryStr[k] == '0')
                                        {
                                            int currentSection = k + 1;
                                            if (k >= segments)
                                            {
                                                currentSection = totalDrives - k;
                                            }
                                            if (!remainingSections.Contains(currentSection))
                                            {
                                                remainingSections.Add(currentSection);
                                            }
                                        }
                                    }
                                    bool oneMissing = false;
                                    bool fail = false;
                                    for (int k = 0; k < totalDrives; ++k)
                                    {
                                        int currentSection = k + 1;
                                        if (k >= segments)
                                        {
                                            currentSection = totalDrives - k;
                                        }
                                        if (k % segments == 0)
                                        {
                                            oneMissing = false;
                                        }
                                        if (!remainingSections.Contains(currentSection))
                                        {
                                            if (oneMissing)
                                            {
                                                fail = true;
                                                break;
                                            }
                                            oneMissing = true;
                                        }
                                    }
                                    string resultStr = binaryStr;
                                    if (fail)
                                    {
                                        ++totalFailures;
                                        resultStr += " BAD";
                                    }
                                    Console.WriteLine(resultStr);
                                    byte[] baResultStr = Encoding.UTF8.GetBytes(resultStr);
                                    failureReport.Write(baResultStr, 0, baResultStr.Length);
                                    failureReport.Write(baNewLine, 0, baNewLine.Length);
                                }
                            }
                            string failureSummary = totalFailures + " out of " + totalCominations;
                            Console.WriteLine(failureSummary);
                            Console.WriteLine();
                            byte[] baFailureSummary = Encoding.UTF8.GetBytes(failureSummary);
                            failureReport.Write(baFailureSummary, 0, baFailureSummary.Length);
                            failureReport.Write(baNewLine, 0, baNewLine.Length);
                            if (totalFailures > 0 && totalFailures == totalCominations)
                            {
                                break;
                            }
                            else
                            {
                                failureReport.Write(baNewLine, 0, baNewLine.Length);
                            }
                        }
                        failureReport.Close();
                    }
                }
            }
            Console.WriteLine("Done");
            Console.Read();
        }
        public static short CountOneBits(int number)
        {
            short count = 0;
            for (; (number != 0); number &= (number - 1))
                ++count;
            return count;
        }
    }
}
