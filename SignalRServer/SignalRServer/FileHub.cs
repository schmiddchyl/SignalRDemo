using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

public class FileHub : Hub
{

    // This method will be called by the client to send a message to the server.
    public async Task SendMessage(string user, string message)
    {
        // Broadcasts the message to all connected clients.
        await Clients.Caller.SendAsync("ReceiveMessage", user, message);
    }

    public async Task BroadcastStream(IAsyncEnumerable<string> stream)
    {
        await foreach (var item in stream)
        {
            Console.WriteLine($"Server received {item}");
        }
    }

    public async Task ReceiveFileChunk(string fileName, byte[] chunk, long fileSize, int chunkNumber)
    {
        // var filePath = Path.Combine("@c:\test", fileName);
        long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var filePath = @"C:\test\newfileouttest3.xlsx";

        // Append the chunk to the file
        using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
        {
            await stream.WriteAsync(chunk, 0, chunk.Length);
        }

        // Notify the client that the chunk has been received
        await Clients.Caller.SendAsync("ChunkReceived", chunkNumber);

        // Once all chunks are received, notify the client
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length >= fileSize)
        {
            await Clients.Caller.SendAsync("FileUploadComplete", fileName);
        }
    }



    public async Task UploadFile(string filename, byte[] fileArray)
    {
        Console.WriteLine("received array.");
        string path = @"C:\test\" + filename;
        try
        {
            File.WriteAllBytes(path, fileArray);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }
    }

    public async Task UploadFileStream(IAsyncEnumerable<byte[]> fileStream, string fileName, long fileSize)
    {
        var filePath = @"C:\test\newfileout2.xlsx"; ;

        // Delete existing file with the same name (optional)
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await using (var outputStream = new FileStream(filePath, FileMode.CreateNew))
        {
            await foreach (var chunk in fileStream)
            {
                // Write each chunk to the file
                await outputStream.WriteAsync(chunk, 0, chunk.Length);
            }
        }

        // Notify the client that the file upload is complete
        await Clients.Caller.SendAsync("FileUploadComplete", fileName);
    }




}