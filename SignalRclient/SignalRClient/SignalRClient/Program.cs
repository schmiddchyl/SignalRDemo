using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Data.Common;
using System.Threading.Channels;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a connection to the SignalR server
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5177/filehub")  // Replace with your server URL
            .Build();

        connection.On<string>("FileUploadComplete", fileName => {
            Console.WriteLine($"File {fileName} uploaded successfully!");
        });

        connection.On<string, string>("NotifyStartUpload", async (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
            string path = @"c:\app.js";
            try
            {
                await SendFile(path, connection);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return;
            }
        });
        // Set up a handler to receive messages from the server
        /*
        connection.On<string, string>("ReceiveMessage", async (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
            string path = @"c:\ex.xlsx";

            // Calling the ReadAllBytes() function 
            byte[] readText = File.ReadAllBytes(path);
            try
            {

               // await connection.InvokeAsync("UploadFile", "ex.xlsx", readText);
             
             
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return;
            }

        });
        */
        try
        {
            // Start the connection
            await connection.StartAsync();
            Console.WriteLine("Connected to the server.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }

        // Keep the client running and send messages
        while (true)
        {
            Console.Write("Enter your name: ");
            var user = Console.ReadLine();

            Console.Write("Enter a message: ");
            var message = Console.ReadLine();

            // Send a message to the server
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(message))
            {
                try
                {
                    await connection.InvokeAsync("SendMessage", user, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message: {ex.Message}");
                }

            }
        }
    }

    static async Task SendFile(string filePath, HubConnection connection)
    {
        var fileName = Path.GetFileName(filePath);
        var fileSize = new FileInfo(filePath).Length;
        var chunkSize = 1024 * 1024 * 10; // 1MB chunk size
        int chunkNumber = 0;

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);

                // Send chunk to the server
                await connection.InvokeAsync("ReceiveFileChunk", fileName, chunk, fileSize, chunkNumber);
                Console.WriteLine($"Sent chunk {chunkNumber}");
                chunkNumber++;
            }
        }
    }

    static async Task StreamFile(string filePath, HubConnection connection)
    {
        var fileSize = new FileInfo(filePath).Length;
        var fileName = Path.GetFileName(filePath);

        // Create a channel to stream the file as byte[] chunks
        var channel = Channel.CreateUnbounded<byte[]>();

        // Task to read file and write to the channel
        _ = Task.Run(async () =>
        {
            var chunkSize = 1024 * 1024; // 1MB chunk size
            byte[] buffer = new byte[chunkSize];
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var chunk = new byte[bytesRead];
                    Array.Copy(buffer, chunk, bytesRead);

                    await channel.Writer.WriteAsync(chunk);
                }
            }
            channel.Writer.Complete();
        });

        // Call the UploadFile method on the server and stream the file
        await connection.StreamAsChannelAsync<byte[]>("UploadFileStream", channel.Reader, fileName, fileSize);
    }

}
