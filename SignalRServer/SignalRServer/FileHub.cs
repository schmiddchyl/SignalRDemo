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
}