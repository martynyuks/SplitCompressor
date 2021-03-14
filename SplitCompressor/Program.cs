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
        //  /compress "D:\Documents\manual.pdf" "D:\Documents\manual.pdf.gz"

        // Распаковка и объединение файла
        //  /decompress "D:\Documents\manual.pdf.gz" "D:\Documents\manual.pdf"

        static void Main(string[] args)
        {
            const int MB_SIZE = 1024 * 1024;

            List<string> compressFilePathes = null;
            List<string> decompressFilePathes = null;
            int partSize = MB_SIZE;
            compressFilePathes = GetParameters(args, "/compress");
            decompressFilePathes = GetParameters(args, "/decompress");

            CompressorSplitter compressorSplitter = null;
            DecompressorConcatenator decompressorConcatenator = null;
            Thread compressThread = null;
            Thread decompressThread = null;
            Thread progressThread = null;

            if (compressFilePathes != null && decompressFilePathes == null)
            {
                try
                {
                    compressorSplitter = new CompressorSplitter();
                    compressThread = new Thread(() => compressorSplitter.Run(compressFilePathes[0], compressFilePathes[1], partSize));
                    compressThread.Start();
                    progressThread = RunProgressThread(compressorSplitter);
                }
                catch (Exception e)
                {
                    PrintErrorMessage(e);
                    return;
                }
            }
            else if (decompressFilePathes != null && compressFilePathes == null)
            {
                try
                {
                    decompressorConcatenator = new DecompressorConcatenator();
                    decompressThread = new Thread(() => decompressorConcatenator.Run(decompressFilePathes[0], decompressFilePathes[1]));
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

        private static List<string> GetParameters(string[] args, string paramName)
        {
            List<string> parameters = new List<string>();
            int argIndex = Array.FindIndex(args, arg => arg.Equals(paramName));
            if (argIndex >= 0)
            {
                argIndex++;
                while (argIndex < args.Length && !args[argIndex].StartsWith("/"))
                {
                    parameters.Add(args[argIndex]);
                    argIndex++;
                }
                if (parameters.Count > 0)
                {
                    return parameters;
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
                "Compression: /compress <src_file_path> <dest_file_path>\r\n" +
                "Decompression: /decompress <src_file_path> <dest_file_path>\r\n");
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
