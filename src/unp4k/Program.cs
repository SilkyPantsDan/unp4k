using ICSharpCode.SharpZipLib.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.Net.Http;
using CommandLineParser.Arguments;

namespace unp4k
{
	
			
	class CommandLineArguments
	{
		[SwitchArgument('s', "smelt", true, Description = "Smelt files")]
		public bool show;

		[ValueArgument(typeof(string), 'i', "input", Description = "Path to SC Data.p4k")]
		public string dataPakPath;

		[ValueArgument(typeof(string), 'o', "output", Description = "Path to export data to")]
		public string outputPath;

		public List<string> filters;
		[ValueArgument(typeof(string), 'f', "filter", Description = "Comma separated string of file extensions to filter on")]
		public string FilterString
		{
			get => string.Join(",", filters.ToArray());
			set { filters = value.Split(",").Select(x => x.Trim().ToLowerInvariant()).ToList(); }
		}
	};

	class Program
	{	
		static void Main(string[] args)
		{
			var key = new Byte[] { 0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47 };

			CommandLineParser.CommandLineParser parser = new(); 
			CommandLineArguments arguments = new();

			try {
				parser.ExtractArgumentAttributes(arguments);
				parser.ParseCommandLine(args);
			}
			catch {
				parser.ShowUsage();
				return;
			}

			if (Directory.Exists(arguments.outputPath) == false)
			{
				Directory.CreateDirectory(arguments.outputPath);
			}

			using (var pakFile = File.OpenRead(arguments.dataPakPath))
			{
				var pak = new ZipFile(pakFile);
				pak.KeysRequired += (sender, args) => args.Key = key;

				byte[] buf = new byte[4096];

				foreach (ZipEntry entry in pak)
				{
					try
					{
						var crypto = entry.IsAesCrypted ? "Crypt" : "Plain";

						var extension = Path.GetExtension(entry.Name).ToLowerInvariant();

						var shouldProcess = arguments.filters.Contains(extension);
						shouldProcess |= extension == ".dcb";                                                                                        // Enable *.ext format for extensions

						if (shouldProcess)
						{
							var targetPath = Path.Join(arguments.outputPath, entry.Name);
							var target = new FileInfo(targetPath);

							if (!target.Directory.Exists) 
							{
								target.Directory.Create();
							}

							if (!target.Exists)
							{
								Console.WriteLine($"{entry.CompressionMethod} | {crypto} | {entry.Name}");

								using (Stream s = pak.GetInputStream(entry))
								{
									using (FileStream fs = File.Create(targetPath))
									{
										StreamUtils.Copy(s, fs, buf);
									}
								}

								// target.Delete();
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Exception while extracting {entry.Name}: {ex.Message}");

						try
						{
							using (var client = new HttpClient { })
							{
								// var server = "http://herald.holoxplor.local";
								var server = "https://herald.holoxplor.space";

								client.DefaultRequestHeaders.Add("client", "unp4k");

								using (var content = new MultipartFormDataContent("UPLOAD----"))
								{
									content.Add(new StringContent($"{ex.Message}\r\n\r\n{ex.StackTrace}"), "exception", entry.Name);

									using (var errorReport = client.PostAsync($"{server}/p4k/exception/{entry.Name}", content).Result)
									{
										if (errorReport.StatusCode == System.Net.HttpStatusCode.OK)
										{
											Console.WriteLine("This exception has been reported.");
										}
									}
								}
							}
						}
						catch (Exception)
						{
							Console.WriteLine("There was a problem whilst attempting to report this error.");
						}
					}
				}
			}
		}
	}
}
