﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

using SBRL.GlidingSquirrel.Websocket;

using Nibriboard.RippleSpace;
using Nibriboard.Client;

namespace Nibriboard
{
	/// <summary>
	/// The main Nibriboard server.
	/// This class manages not only the connected clients, but also holds the master reference to the <see cref="Nibriboard.RippleSpace.RippleSpaceManager"/>.
	/// </summary>
	public class NibriboardServer
	{
		private TcpListener commandServer;
		private NibriboardApp appServer;

		private ClientSettings clientSettings;
		private RippleSpaceManager planeManager;

		public readonly int CommandPort = 31587;
		public readonly int Port = 31586;

		public NibriboardServer(string pathToRippleSpace, int inPort = 31586)
		{
			Port = inPort;

			// Load the specified packed ripple space file if it exists - otherwise save it to disk
			if(File.Exists(pathToRippleSpace)) {
				planeManager = RippleSpaceManager.FromFile(pathToRippleSpace).Result;
			} else {
				Log.WriteLine("[NibriboardServer] Couldn't find packed ripple space at {0} - creating new ripple space instead.", pathToRippleSpace);
				planeManager = new RippleSpaceManager() { SourceFilename = pathToRippleSpace };
			}

			clientSettings = new ClientSettings() {
				SecureWebSocket = false,
				WebSocketHost = "192.168.0.56",
				WebSocketPort = Port,
				WebSocketPath = "/RipplespaceLink"
			};

			// HTTP Server setup
			appServer = new NibriboardApp(new NibriboardAppStartInfo() {
				FilePrefix = "Nibriboard.obj.client_dist",
				ClientSettings = clientSettings,
				SpaceManager = planeManager
			}, IPAddress.Any, Port);
		}

		public async Task Start()
		{
			await appServer.Start();
			Log.WriteLine("[NibriboardServer] Started on port {0}", Port);

			await planeManager.StartMaintenanceMonkey();
		}

		/// <summary>
		/// Starts the command listener.
		/// The command listener is a light tcp-based command console that allows control
		/// of the Nibriboard server, since C# doesn't currently have support for signal handling.
		/// It listeners on [::1] _only_, to avoid security issues.
		/// In the future, a simple secret might be required to use it to aid security further.
		/// </summary>
		public async Task StartCommandListener()
		{
			commandServer = new TcpListener(IPAddress.IPv6Loopback, CommandPort);
			commandServer.Start();
			Log.WriteLine("[CommandConsole] Listening on {0}.", new IPEndPoint(IPAddress.IPv6Loopback, CommandPort));
			while(true)
			{
				TcpClient nextClient = await commandServer.AcceptTcpClientAsync();

				StreamReader source = new StreamReader(nextClient.GetStream());
				StreamWriter destination = new StreamWriter(nextClient.GetStream()) { AutoFlush = true };

				string rawCommand = await source.ReadLineAsync();
				string[] commandParts = rawCommand.Split(" \t".ToCharArray());
				Log.WriteLine("[CommandConsole] Client executing {0}", rawCommand);

				try
				{
					switch(commandParts[0].Trim())
					{
						case "help":
							await destination.WriteLineAsync("Nibriboard Server Command Console");
							await destination.WriteLineAsync("=================================");
							await destination.WriteLineAsync("Available commands:");
							await destination.WriteLineAsync("    help     Show this message");
							await destination.WriteLineAsync("    save     Save the ripplespace to disk");
							await destination.WriteLineAsync("    planes   List all the currently loaded planes");
							break;
						case "save":
							await destination.WriteAsync("Saving ripple space - ");
							await planeManager.Save();
							await destination.WriteLineAsync("done.");
							break;
						case "planes":
							await destination.WriteLineAsync("Planes:");
							foreach(Plane plane in planeManager.Planes)
								await destination.WriteLineAsync($"\t{plane.Name}");
							await destination.WriteLineAsync();
							await destination.WriteLineAsync($"Total {planeManager.Planes.Count}");
							break;

						default:
							await destination.WriteLineAsync($"Error: Unrecognised command {commandParts[0].Trim()}");
							break;
					}
				}
				catch(Exception error)
				{
					await destination.WriteLineAsync(error.ToString());
				}
				nextClient.Close();
			}
		}
	}
}
