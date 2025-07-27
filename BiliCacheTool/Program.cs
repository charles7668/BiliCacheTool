using System.CommandLine;

namespace BiliCacheTool;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("BiliCacheTool - 處理 Bilibili 緩存檔案的工具");

        var inputOption = new Option<string>("--input", "-i")
        {
            Description = "輸入路徑",
            Required = true
        };

        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "輸出路徑",
            Required = true
        };
        rootCommand.Add(inputOption);
        rootCommand.Add(outputOption);

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Any())
        {
            foreach (var error in parseResult.Errors)
            {
                await Console.Error.WriteLineAsync($"錯誤: {error.Message}");
            }

            return 1;
        }

        try
        {
            var runOptions = new RunOptions
            {
                InputPath = Path.GetFullPath(parseResult.GetRequiredValue(inputOption)),
                OutputPath = Path.GetFullPath(parseResult.GetRequiredValue(outputOption))
            };
            await ExecuteAsync(runOptions);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"錯誤: {ex.Message}");
            return 1;
        }

        return 0;
    }

    private static async Task ExecuteAsync(RunOptions options)
    {
        Console.WriteLine("Input : " + options.InputPath);
        Console.WriteLine("Output: " + options.OutputPath);
        try
        {
            if (!Directory.Exists(options.InputPath))
            {
                await Console.Error.WriteLineAsync($"錯誤: 輸入路徑不存在: {options.InputPath}");
                return;
            }

            var entryFiles = GetEntryJsonFiles(options.InputPath);

            Console.WriteLine($"\n找到 {entryFiles.Length} 個 entry.json 檔案:");

            foreach (var file in entryFiles)
            {
                var relativePath = Path.GetRelativePath(options.InputPath, file);
                Console.WriteLine($"  - {relativePath}");
            }

            await ProcessEntryFilesAsync(entryFiles, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"執行錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 從指定路徑取得所有 entry.json 檔案，包含子資料夾
    /// </summary>
    /// <param name="inputPath">輸入路徑</param>
    /// <returns>entry.json 檔案路徑陣列</returns>
    private static string[] GetEntryJsonFiles(string inputPath)
    {
        try
        {
            var entryFiles = Directory.GetFiles(
                inputPath,
                "entry.json",
                SearchOption.AllDirectories
            );

            return entryFiles;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"權限不足，無法存取某些資料夾: {ex.Message}");
            return [];
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.Error.WriteLine($"目錄不存在: {ex.Message}");
            return [];
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"搜尋檔案時發生錯誤: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// 處理找到的 entry.json 檔案
    /// </summary>
    /// <param name="entryFiles">entry.json 檔案路徑陣列</param>
    /// <param name="options">執行選項</param>
    private static async Task ProcessEntryFilesAsync(string[] entryFiles, RunOptions options)
    {
        if (entryFiles.Length == 0)
        {
            Console.WriteLine("\n沒有找到任何 entry.json 檔案");
            return;
        }

        Console.WriteLine($"\n開始處理 {entryFiles.Length} 個檔案...");

        for (int i = 0; i < entryFiles.Length; i++)
        {
            var entryFile = entryFiles[i];
            var relativePath = Path.GetRelativePath(options.InputPath, entryFile);

            Console.WriteLine($"\n[{i + 1}/{entryFiles.Length}] 處理: {relativePath}");

            try
            {
                var jsonContent = await File.ReadAllTextAsync(entryFile);

                var fileInfo = new FileInfo(entryFile);
                Console.WriteLine($"  檔案大小: {fileInfo.Length} bytes");
                Console.WriteLine($"  修改時間: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"  所在目錄: {Path.GetDirectoryName(relativePath)}");

                await ProcessSingleEntryFileAsync(entryFile, jsonContent, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ 處理失敗: {ex.Message}");
            }
        }

        Console.WriteLine($"\n✅ 完成處理 {entryFiles.Length} 個檔案");
    }

    /// <summary>
    /// 處理單個 entry.json 檔案
    /// </summary>
    /// <param name="filePath">檔案路徑</param>
    /// <param name="jsonContent">JSON 內容</param>
    /// <param name="options">執行選項</param>
    private static async Task ProcessSingleEntryFileAsync(string filePath, string jsonContent, RunOptions options)
    {
        Console.WriteLine($"  📄 JSON 內容長度: {jsonContent.Length} 字元");

        await Task.Delay(10);
        Console.WriteLine($"  ✅ 處理完成");
    }
}