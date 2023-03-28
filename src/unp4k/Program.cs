using CommandLineParser.Arguments;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;

namespace unp4k
{
	internal class CommandLineArguments
	{
		[SwitchArgument('s', "smelt", true, Description = "Smelt files")]
		public bool smeltFiles = true;

		[SwitchArgument("report", false, Description = "Report Exceptions to server")]
		public bool reportExceptions = false;

		[ValueArgument(typeof(string), 'i', "input", Description = "Path to SC Data.p4k")]
		public string dataPakPath = null;

		[ValueArgument(typeof(string), 'o', "output", Description = "Path to export data to")]
		public string outputPath = null;

		public List<string> filters;
		[ValueArgument(typeof(string), 'f', "filter", Description = "Comma separated string of file extensions to filter on")]
		public string FilterString
		{
			get => string.Join(",", filters.ToArray());
			set => filters = value.Split(",").Select(x => x.Trim().ToLowerInvariant()).ToList();
		}
	};

	internal class Program
	{
		private static void Main(string[] args)
		{
			var key = new Byte[] { 0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47 };

			CommandLineParser.CommandLineParser parser = new();
			CommandLineArguments arguments = new();

			try
			{
				parser.ExtractArgumentAttributes(arguments);
				parser.ParseCommandLine(args);
			}
			catch
			{
				parser.ShowUsage();
				return;
			}

			Log.Logger = new LoggerConfiguration()
    			.MinimumLevel.Verbose() // See everything
				.WriteTo.Console(
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
				)
				.WriteTo.File("Log-.txt",
					restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug,
					rollingInterval: RollingInterval.Day,
					outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
				)
				.CreateLogger();

			if (Directory.Exists(arguments.outputPath) == false)
			{
				_ = Directory.CreateDirectory(arguments.outputPath);
			}
			
			if (arguments.filters.Contains(".xml"))
			{
				// Add .dcb as this contains more XMLs for processing later
				arguments.filters.Add(".dcb");
			}

			using FileStream pakFile = File.OpenRead(arguments.dataPakPath);
			ZipFile pak = new(pakFile);
			pak.KeysRequired += (sender, args) => args.Key = key;

			byte[] buf = new byte[4096];
			foreach (ZipEntry entry in pak)
			{
				try
				{
					string crypto = entry.IsAesCrypted ? "Crypt" : "Plain";

					string extension = Path.GetExtension(entry.Name).ToLowerInvariant();

					bool shouldProcess = arguments.filters.Count == 0
						|| arguments.filters.Contains(".*")
						|| arguments.filters.Contains(extension);

					if (shouldProcess)
					{
						string targetPath = Path.Join(arguments.outputPath, entry.Name);
						FileInfo target = new(targetPath);

						if (!target.Directory.Exists)
						{
							target.Directory.Create();
						}

						if (!target.Exists)
						{
							Log.Debug($"{entry.CompressionMethod} | {crypto} | {entry.Name}");

							using Stream s = pak.GetInputStream(entry);
							using FileStream fs = File.Create(targetPath);
							StreamUtils.Copy(s, fs, buf);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error($"Exception while extracting {entry.Name}: {ex.Message}");

					if (arguments.reportExceptions)
					{
						try
						{
							using HttpClient client = new() { };
							// var server = "http://herald.holoxplor.local";
							string server = "https://herald.holoxplor.space";

							client.DefaultRequestHeaders.Add("client", "unp4k");

							using MultipartFormDataContent content = new("UPLOAD----")
							{
								{ new StringContent($"{ex.Message}\r\n\r\n{ex.StackTrace}"), "exception", entry.Name }
							};

							using HttpResponseMessage errorReport = client.PostAsync($"{server}/p4k/exception/{entry.Name}", content).Result;
							if (errorReport.StatusCode == System.Net.HttpStatusCode.OK)
							{
								Log.Information("This exception has been reported.");
							}
						}
						catch (Exception)
						{
							Log.Error("There was a problem whilst attempting to report this error.");
						}
					}
				}
			}

			Log.CloseAndFlush();
		}
	}
}
