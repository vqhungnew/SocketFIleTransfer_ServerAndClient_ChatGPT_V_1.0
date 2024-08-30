using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class FileServer
{
    static void Main()
    {
        //Call Server Function
        fileTransferServer();

        //Call Client Function
        fileTransferClient();
    }

    private static void fileTransferServer()
    {
        const int port = 8000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine("Server is listening on port " + port);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream networkStream = client.GetStream();
                Console.WriteLine("Client connected.");

                // Receive file name
                byte[] fileNameLengthBytes = new byte[4];
                networkStream.Read(fileNameLengthBytes, 0, 4);
                int fileNameLength = BitConverter.ToInt32(fileNameLengthBytes, 0);

                byte[] fileNameBytes = new byte[fileNameLength];
                networkStream.Read(fileNameBytes, 0, fileNameLength);
                string fileName = Encoding.UTF8.GetString(fileNameBytes);

                Console.WriteLine("Receiving file: " + fileName);

                // Receive file data
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }

                Console.WriteLine("File received successfully.");
                client.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void fileTransferClient()
    {
        const string serverAddress = "127.0.0.1";
        const int port = 8000;

        Console.Write("Enter the path of the file to send: ");
        string filePath = Console.ReadLine();

        try
        {
            using (TcpClient client = new TcpClient(serverAddress, port))
            {
                NetworkStream networkStream = client.GetStream();
                FileInfo fileInfo = new FileInfo(filePath);

                // Send file name length
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileInfo.Name);
                byte[] fileNameLengthBytes = BitConverter.GetBytes(fileNameBytes.Length);
                networkStream.Write(fileNameLengthBytes, 0, fileNameLengthBytes.Length);

                // Send file name
                networkStream.Write(fileNameBytes, 0, fileNameBytes.Length);

                // Send file data
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        networkStream.Write(buffer, 0, bytesRead);
                    }
                }

                Console.WriteLine("File sent successfully.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }

    }
}
