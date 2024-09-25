using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a connection to the SignalR server
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5177/filehub")  // Replace with your server URL
            .Build();

        // Set up a handler to receive messages from the server
        connection.On<string, string>("ReceiveMessage", async (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
            string path = @"c:\ex.xlsx";

            // Calling the ReadAllBytes() function 
            byte[] readText = File.ReadAllBytes(path);
            try
            {

                await connection.InvokeAsync("UploadFile", "ex.xlsx", readText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return;
            }

        });

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
}
