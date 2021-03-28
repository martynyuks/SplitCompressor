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
                    if (!File.Exists(compressFilePathes[0]))
                    {
                        throw new FileNotFoundException(FormattableString.Invariant($"File not found: \"{compressFilePathes[0]}\""));
                    }
                    compressorSplitter = new CompressorSplitter(compressFilePathes[0], compressFilePathes[1], partSize,
                        Environment.ProcessorCount);
                    compressThread = new Thread(() => compressorSplitter.Run());
                    compressThread.Start();
                    progressThread = RunProgressThread(compressorSplitter, true);
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
                    if (!File.Exists(decompressFilePathes[0]))
                    {
                        throw new FileNotFoundException(FormattableString.Invariant($"File not found: \"{decompressFilePathes[0]}\""));
                    }
                    decompressorConcatenator = new DecompressorConcatenator(decompressFilePathes[0], decompressFilePathes[1],
                        Environment.ProcessorCount);
                    decompressThread = new Thread(() => decompressorConcatenator.Run());
                    decompressThread.Start();
                    progressThread = RunProgressThread(decompressorConcatenator, false);
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
            if (compressThread != null)
            {
                compressThread.Join();
            }
            if (decompressThread != null)
            {
                decompressThread.Join();
            }
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

        private static Thread RunProgressThread(ArchiveParallelProcessor parallelProcessor, bool compressOrDecompress)
        {
            Thread thread = new Thread(() =>
            {
                string processOperation = compressOrDecompress ? "Compressing" : "Decompressing";
                string processProgress = compressOrDecompress ? "Compressed" : "Decompressed";
                Console.Write(
                    FormattableString.Invariant($"{processOperation}...\r\n") +
                    FormattableString.Invariant($"Source file: \"{parallelProcessor.SrcFilePath}\"\r\n") +
                    FormattableString.Invariant($"Destination file: \"{parallelProcessor.DstFilePath}\"\r\n"));
                double progress = GetProgress(parallelProcessor.CompletedCount, parallelProcessor.TotalCount);
                Console.Write(FormattableString.Invariant($"\r{processProgress} {progress:F0} %                                        "));
                while (!parallelProcessor.IsCompleted)
                {
                    progress = GetProgress(parallelProcessor.CompletedCount, parallelProcessor.TotalCount);
                    Console.Write(FormattableString.Invariant($"\r{processProgress} {progress:F0} %                                        "));
                    Thread.Sleep(200);
                }
                progress = GetProgress(parallelProcessor.CompletedCount, parallelProcessor.TotalCount);
                Console.Write(FormattableString.Invariant($"\r{processProgress} {progress:F0} %                                        "));
                Console.WriteLine();
                Console.WriteLine(FormattableString.Invariant($"{processOperation} completed."));
            });
            thread.Start();
            return thread;
        }

        private static double GetProgress(int completed, int total)
        {
            if (total > 0)
            {
                return (double)completed / total * 100.0;
            }
            else
            {
                return 0.0;
            }
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
