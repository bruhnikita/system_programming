using System.Text;

public class AsyncWriter
{
    public static async Task AsyncWrteFile(string filePath, string text)
    {
        byte[] encodedText = Encoding.Unicode.GetBytes(text);

        try
        {
            using (FileStream fs = new FileStream(filePath,
            FileMode.Append, FileAccess.Write, FileShare.None,
            bufferSize: 4096, useAsync: true))
            {
                await fs.WriteAsync(encodedText, 0, encodedText.Length);
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка записи в файл: {ex.Message}");
        }
    }

    public static async Task<string> AsyncReadFile(string filePath)
    {
        try
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);

            using var reader = new StreamReader(fs, Encoding.Unicode);
            return await reader.ReadToEndAsync();
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка чтения: {ex.Message}");
            return null;
        }
    }

    public static async Task PrintData(string filePath)
    {
        string data = await AsyncReadFile(filePath);
        Console.WriteLine(data);
    }

    public static async Task Logging(string filePath, string action, DateTime time)
    {
        using FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Write, bufferSize: 4096, useAsync: true);

        using var logger = new StreamWriter(fs, Encoding.Unicode);
        await logger.WriteLineAsync(action + time);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = "example.txt";

        bool isExit = false;
        while (!isExit)
        {
            Console.WriteLine("Введите действие:\n1 - Ввод в файл\n2 - Чтение из файла\n3 - Запрос к серверу\n4 - Выйти");
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Введите текст для ввода: ");
                    string text = Console.ReadLine();

                    Task textWrite = Task.Run(() => AsyncWriter.AsyncWrteFile(filePath, text));
                    Task log = Task.Run(() => AsyncWriter.Logging("log.txt", "Текст введен", DateTime.Now));
            
                    break;

                case 2: 
                    Task fileRead = Task.Run(() => AsyncWriter.PrintData(filePath));
                    log = Task.Run(() => AsyncWriter.Logging("log.txt", "Файл считан", DateTime.Now));
                    break;

                case 3:
                    Console.WriteLine("Введите URL для запроса: ");
                    string url = Console.ReadLine();

                    try
                    {
                        HttpClient client = new HttpClient();
                        string response = await client.GetStringAsync(url);
                        Console.WriteLine(response);

                        log = Task.Run(() => AsyncWriter.Logging("log.txt", "Произведено обращение к серверу", DateTime.Now));

                        break;
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обращения к серверу: {ex.Message}");
                        break;
                    }

                case 4:
                    log = Task.Run(() => AsyncWriter.Logging("log.txt", "Работа окончена", DateTime.Now));
                    isExit = true;
                    break;
            }
        }
    }
}