using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Globalization;

namespace SplitCompressor
{
    class Program
    {
        //  Примеры аргументов приложения

        //  Разделение и сжатие файла
        //  /compress "D:\Documents\manual.pdf" /out "D:\Documents\archive\manual.pdf.gz" /partsize 0.5
        //  /compress "D:\Documents\manual.pdf" /out "D:\Documents\archive\manual.pdf.gz" /partsize 1

        // Распаковка и объединение файла
        //  /decompress "D:\Documents\archive\manual.pdf" /out "D:\Documents\manual.pdf"

        static void Main(string[] args)
        {
            const int MB_SIZE = 1024 * 1024;

            string compressFilePath = null;
            string decompressFilePath = null;
            string outFilePath = null;
            string partSizeStr = null;
            int partSize = MB_SIZE;
            compressFilePath = GetParameter(args, "/compress");
            decompressFilePath = GetParameter(args, "/decompress");
            outFilePath = GetParameter(args, "/out");
            partSizeStr = GetParameter(args, "/partsize");

            CompressorSplitter compressorSplitter = null;
            DecompressorConcatenator decompressorConcatenator = null;
            Thread compressThread = null;
            Thread decompressThread = null;
            Thread progressThread = null;

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
                    compressorSplitter = new CompressorSplitter();
                    compressThread = new Thread(() => compressorSplitter.Run(compressFilePath, outFilePath, partSize));
                    compressThread.Start();
                    progressThread = RunProgressThread(compressorSplitter);
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
                    decompressorConcatenator = new DecompressorConcatenator();
                    decompressThread = new Thread(() => decompressorConcatenator.Run(decompressFilePath, outFilePath));
                    decompressThread.Start();
                    progressThread = RunProgressThread(decompressorConcatenator);
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
            if (progressThread != null)
            {
                progressThread.Join();
            }
            Console.WriteLine("Waiting for completion of final operations...");
            if (compressThread != null)
            {
                compressThread.Join();
            }
            if (decompressThread != null)
            {
                decompressThread.Join();
            }
            Console.WriteLine("Completed");
            Console.WriteLine("Press any key to exit...");
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

        private static Thread RunProgressThread(ParallelTask parallelTask)
        {
            Thread thread = new Thread(() =>
            {
                Console.WriteLine("Processing...");
                int completed = parallelTask.CompletedCount;
                int total = parallelTask.TotalCount;
                Console.Write($"\rProcessed {completed} of {total} parts                                        ");
                while (!parallelTask.IsCompleted)
                {
                    completed = parallelTask.CompletedCount;
                    total = parallelTask.TotalCount;
                    Console.Write($"\rProcessed {completed} of {total} parts                                        ");
                    Thread.Sleep(200);
                }
                completed = parallelTask.CompletedCount;
                total = parallelTask.TotalCount;
                Console.Write($"\rProcessed {completed} of {total} parts                                        ");
                Console.WriteLine();
                Console.WriteLine("Processing completed.");
            });
            thread.Start();
            return thread;
        }

        private static void PrintWrongParamsMessage()
        {
            Console.WriteLine("Wrong parameters passed.");
            Console.Write("Parameter usage:\r\n" +
                "Compression: /compress <src_file_path> /out <dest_file_path> /partsize <size_Mb>\r\n" +
                "Decompression: /decompress <origin_file_path> /out <out_file_path>\r\n" +
                "<origin_file_path> is passed without archive extension and part number index\r\n");
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
