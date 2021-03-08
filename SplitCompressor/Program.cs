using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Globalization;

namespace SplitCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            const int MB_SIZE = 1024 * 1024;

            string compressFilePath = null;
            string decompressFilePath = null;
            string outFilePath = null;
            string partSizeStr;
            int partSize = MB_SIZE;
            compressFilePath = GetParameter(args, "/compress");
            decompressFilePath = GetParameter(args, "/decompress");
            outFilePath = GetParameter(args, "/out");
            partSizeStr = GetParameter(args, "/partsize");
            if (outFilePath == null)
            {
                PrintWrongParamsMessage();
                return;
            }
            if (compressFilePath != null && decompressFilePath == null)
            {
                try
                {
                    if (partSizeStr != null)
                    {
                        double partSizeD = double.Parse(partSizeStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                        partSize = (int)Math.Round(partSizeD * MB_SIZE);
                    }
                    Console.WriteLine("Processing...");
                    CompressorSplitter.Run(compressFilePath, outFilePath, partSize);
                }
                catch (Exception e)
                {
                    PrintErrorMessage(e);
                    return;
                }
            }
            else if (decompressFilePath != null && compressFilePath == null)
            {
                try
                {
                    Console.WriteLine("Processing...");
                    DecompressorConcatenator.Run(decompressFilePath, outFilePath);
                }
                catch (Exception e)
                {
                    PrintErrorMessage(e);
                    return;
                }
            }
            else
            {
                PrintWrongParamsMessage();
                return;
            }
            
            Console.WriteLine("Processing completed.");
            Console.ReadLine();
        }

        private static string GetParameter(string[] args, string paramName)
        {
            int argIndex = Array.FindIndex(args, arg => arg.Equals(paramName));
            if (argIndex >= 0)
            {
                argIndex++;
                if (argIndex < args.Length)
                {
                    return args[argIndex];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static void PrintWrongParamsMessage()
        {
            Console.WriteLine("Wrong parameters passed.");
            Console.Write("Parameter usage:\r\n" +
                "Compression: /compress <src_file_path> /out <dest_file_path> /partsize <size_Mb>\r\n" +
                "Decompression: /decompress <origin_file_path> /out <out_file_path>\r\n" +
                "<origin_file_path> is passed without archive extension and part number index");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private static void PrintErrorMessage(Exception e)
        {
            Console.WriteLine("Error occured.");
            Console.WriteLine(e.Message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
