using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcp
{
	class file_server
	{
		/// <summary>
		/// The PORT
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		const int BUFSIZE = 1024;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// Opretter en socket.
		/// Venter på en connect fra en klient.
		/// Modtager filnavn
		/// Finder filstørrelsen
		/// Kalder metoden sendFile
		/// Lukker socketen og programmet
 		/// </summary>
		private file_server ()
		{
			// TO DO Your own code
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// The filename.
		/// </param>
		/// <param name='fileSize'>
		/// The filesize.
		/// </param>
		/// <param name='io'>
		/// Network stream for writing to the client.
		/// </param>
		private void sendFile (String fileName, long fileSize, NetworkStream io)
		{
            FileStream Fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length)/ Convert.ToDouble(BUFSIZE)));
            int TotalLength = (int) Fs.Length;
            int currentPacketLength, bytesSent = 0;
            byte[] SendingBuffer = null;
            for(int i = 0; i < NoOfPackets; ++i)
            {
                if ((TotalLength - bytesSent) > 1024)
                {
                    SendingBuffer = new byte[BUFSIZE];
                    Fs.Read(SendingBuffer, 0, BUFSIZE);
                    io.Write(SendingBuffer,0,(int)SendingBuffer.Length);
                    bytesSent += 1024;
                }
                else
                {
                    SendingBuffer = new byte[TotalLength-bytesSent];
                    Fs.Read(SendingBuffer, 0, SendingBuffer.Length);
                    io.Write(SendingBuffer, 0, (int)SendingBuffer.Length);
                    bytesSent += SendingBuffer.Length;
                }
            }
            Fs.Close();
        }

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			Console.WriteLine ("Server starts...");
			new file_server();
            TcpListener serverSocket = new TcpListener(PORT);
            serverSocket.Start();
            Console.WriteLine("Server socket started");
            TcpClient clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine("Server socket now accepts clients");

            while (true)
            {
                try
                {
                    file_server fileServer = new file_server();
                    NetworkStream networkStream = clientSocket.GetStream();
                    string path = LIB.readTextTCP(networkStream);
                    string fileName = LIB.extractFileName(path);
                    long fileLength = LIB.check_File_Exists(fileName);
                    if (fileLength == 0) //File not found
                    {
                        LIB.writeTextTCP(networkStream,fileLength.ToString());
                    }
                    else // File found
                    {
                        LIB.writeTextTCP(networkStream,"Filesize: " + fileLength.ToString());
                        fileServer.sendFile(path,fileLength,networkStream);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine(" >> exit");
            Console.ReadLine();

        }
	}
}
