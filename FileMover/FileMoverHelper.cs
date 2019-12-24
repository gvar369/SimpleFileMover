using Serilog;
using System;
using System.IO;
using System.Security.Cryptography;

namespace FileMover
{
    class FileMoverHelper
    {
        private readonly string _sourceDirectory;
        private readonly string _destinationDirectory;
        private readonly bool _verifyFileOnCopy;
        private readonly bool _deleteFileOnCopy;

        internal FileMoverHelper(string sourceDirectory, string destinationDirectory)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("log.txt",
                rollingInterval: RollingInterval.Month,
                rollOnFileSizeLimit: true)
            .CreateLogger();

            Log.Information("==============Starting File Move===========");

            _sourceDirectory = sourceDirectory;
            _destinationDirectory = destinationDirectory;
            _verifyFileOnCopy = true;
            _deleteFileOnCopy = true;
        }

        internal void MoveFiles()
        {
            try
            {
                if (SourceDirectoryExists())
                {
                    if (DestinationDirectoryExists())
                    {
                        CopyFiles();
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(_destinationDirectory))
                            Log.Warning("Destination directory required!!!");
                        else
                            Log.Warning("Unable to find destination directory");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_sourceDirectory))
                        Log.Warning("Source directory required!!!");
                    else
                        Log.Warning("Unable to find source directory");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to move files");
                throw;
            }
            Log.Information("==============Ending File Move===========");
            Log.CloseAndFlush();
        }

        private bool SourceDirectoryExists()
        {
            Log.Information($"Verifying source directory: {_sourceDirectory}");
            return Directory.Exists(_sourceDirectory);
        }

        private bool DestinationDirectoryExists()
        {
            Log.Information($"Verifying destination directory: {_destinationDirectory}");
            return Directory.Exists(_destinationDirectory);
        }

        private void CopyFiles()
        {
            Log.Information($"Getting files from source directory: {_sourceDirectory}");
            var sourceFiles = Directory.GetFiles(_sourceDirectory);
            Log.Information($"Found {sourceFiles.Length} file(s) in the source directory");
            if (sourceFiles != null && sourceFiles.Length > 0)
            {
                foreach (var sfile in sourceFiles)
                {
                    string destFileName = Path.GetFileName(sfile);
                    string destFullFilePath = Path.Combine(_destinationDirectory, destFileName);

                    while (File.Exists(destFullFilePath))
                    {
                        Log.Information($"Destination file name {destFullFilePath} exists");
                        var fname = Path.GetFileNameWithoutExtension(sfile);
                        var ext = Path.GetExtension(sfile);
                        var finalName = fname + "_" + RandomNumberGenerator.GetInt32(10000);
                        destFullFilePath = Path.Combine(_destinationDirectory, finalName + ext);
                    }
                    Log.Information($"Copying file {sfile} to destination {destFullFilePath}");
                    File.Copy(sfile, destFullFilePath);

                    if (_verifyFileOnCopy)
                    {
                        if (!VerifyFiles(sfile, destFullFilePath))
                            Log.Warning($"File contents do not match.");
                        else
                            Log.Information($"File contents match.");
                    }

                    if (_deleteFileOnCopy)
                    {
                        DeleteSourceDirectoryFile(sfile);
                    }
                }
            }
        }

        private static bool VerifyFiles(string sourceFilePath, string destinationFilePath)
        {
            if (!string.IsNullOrWhiteSpace(sourceFilePath) && !string.IsNullOrWhiteSpace(destinationFilePath))
            {
                if (File.Exists(destinationFilePath))
                {
                    var sourceFileContents = File.ReadAllBytes(sourceFilePath);
                    var destinationFileContents = File.ReadAllBytes(destinationFilePath);
                    return CompareFileContents(sourceFileContents, destinationFileContents);
                }
                else
                {
                    Log.Warning($"Could not find destination file: {destinationFilePath} to compare");
                }
            }
            else
            {
                Log.Warning($"Source and Destination file paths are required. Source: {sourceFilePath}, Destination: {destinationFilePath}");
            }
            return false;
        }

        private static bool CompareFileContents(ReadOnlySpan<byte> sourceFileContents, ReadOnlySpan<byte> destinationFileContents)
        {
            return sourceFileContents.SequenceEqual(destinationFileContents);
        }

        private static void DeleteSourceDirectoryFile(string sourceFilePath)
        {
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                File.Delete(sourceFilePath);
                if (File.Exists(sourceFilePath))
                    Log.Warning($"Unable to delete source file. Path: {sourceFilePath}");
                else
                    Log.Information($"Source file deleted successfully");
            }
            else
                Log.Warning($"Source file path required! Source: {sourceFilePath}");
        }
    }
}
