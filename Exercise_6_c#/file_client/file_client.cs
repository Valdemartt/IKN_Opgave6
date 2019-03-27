using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace tcp
{
	class file_client
	{
		/// <summary>
		/// The PORT.
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		const int BUFSIZE = 1024;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments. First ip-adress of the server. Second the filename
		/// </param>
		private file_client (string[] args)
        {
            string ipAdr = args[0];
            string fileName = args[1];
            TcpClient clientSocket = new TcpClient(ipAdr,PORT);
            Console.WriteLine("Connected to server with IP: " + ipAdr);
            NetworkStream serverStream = clientSocket.GetStream();
			Console.WriteLine("Got stream!");
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(fileName);
			Thread.Sleep(20);
			serverStream.Write(outStream, 0, outStream.Length);
			serverStream.WriteByte(0);
            
            byte[] inStream = new byte[20000];
            serverStream.Read(inStream, 0, inStream.Length);
            string returnMessage = System.Text.Encoding.ASCII.GetString(inStream);
            long fileLength;
            long.TryParse(returnMessage, out fileLength);
            if (fileLength == 0) //File not found
            {
                Console.WriteLine("File not found.");
            }
            else //File found
            {
                receiveFile(fileName,serverStream,fileLength);
            }

        }

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='io'>
		/// Network stream for reading from the server
		/// </param>
		private void receiveFile (String fileName, NetworkStream io, long fileLength)
		{
            FileStream Fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(fileLength) / Convert.ToDouble(BUFSIZE)));
            int currentPacketLength, bytesSent = 0;
            byte[] SendingBuffer = null;
            for (int i = 0; i < NoOfPackets; ++i)
            {
                if ((fileLength - bytesSent) > 1024)
                {
                    SendingBuffer = new byte[BUFSIZE];
                    io.Read(SendingBuffer, 0, (int)SendingBuffer.Length);
                    Fs.Write(SendingBuffer, 0, BUFSIZE);
                    bytesSent += BUFSIZE;
                }
                else
                {
                    SendingBuffer = new byte[fileLength - bytesSent];
                    io.Read(SendingBuffer, 0, (int)SendingBuffer.Length);
                    Fs.Write(SendingBuffer, 0, SendingBuffer.Length);
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
			Console.WriteLine ("Client starts...");
			new file_client(args);
		}
	}
}
