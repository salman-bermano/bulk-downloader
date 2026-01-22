using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        Console.Write("Enter the full path to your links file (e.g., C:\\Users\\You\\Desktop\\links.txt): ");
        string linksFilePath = Console.ReadLine()?.Trim().Trim('"') ?? "";

        if (!File.Exists(linksFilePath))
        {
            Console.WriteLine($"\nError: File not found at '{linksFilePath}'");
            Console.WriteLine("Please check the path and try again.");
            return 1;
        }

        string downloadDir = "bulk_downloads";
        Directory.CreateDirectory(downloadDir);

        Console.WriteLine($"\nFiles will be saved to the '{Path.GetFullPath(downloadDir)}' folder.");
        Console.WriteLine("Starting downloads...\n");

        string[] urls;
        try
        {
            urls = File.ReadAllLines(linksFilePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading file: {e}");
            return 1;
        }

        int fileCounter = 1;

        foreach (var raw in urls)
        {
            string url = raw?.Trim();
            if (string.IsNullOrEmpty(url))
                continue;

            try
            {
                Console.WriteLine($"--- Processing link #{fileCounter} ---");
                Console.WriteLine($"Downloading: {url}");

                Uri uri;
                try
                {
                    uri = new Uri(url);
                }
                catch (UriFormatException)
                {
                    Console.WriteLine($"Invalid URL: {url}\n");
                    continue;
                }

                string extension = Path.GetExtension(uri.AbsolutePath);
                string outputFilename = fileCounter + extension;
                string outputPath = Path.Combine(downloadDir, outputFilename);

                Console.WriteLine($"Saving as:   {outputPath}");

                var psi = new ProcessStartInfo
                {
                    FileName = "curl.exe",
                    Arguments = $"-L -o \"{outputPath}\" \"{url}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null)
                    {
                        Console.WriteLine("Failed to start curl.exe.\n");
                        return 1;
                    }

                    proc.WaitForExit();

                    if (proc.ExitCode != 0)
                    {
                        Console.WriteLine($"Failed to download {url}. 'curl.exe' returned exit code {proc.ExitCode}\n");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully downloaded {outputFilename}\n");
                        fileCounter++;
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.WriteLine("\nFATAL ERROR: 'curl.exe' not found.");
                Console.WriteLine("curl.exe is included in modern Windows 10/11.");
                Console.WriteLine("Please ensure it's installed and your system's PATH variable is set correctly.");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An unexpected error occurred for {url}: {e}\n");
            }
        }

        Console.WriteLine($"--- Download process finished ---");
        Console.WriteLine($"Processed {urls.Length} links. Saved {fileCounter - 1} files.");
        return 0;
    }
}