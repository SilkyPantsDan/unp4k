namespace unforge
{
	class Program
	{
		static void Main(params String[] args)
		{
			var ci = System.Globalization.CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;


			if (args.Length == 0)
			{
				args = new String[] { "game.v4.dcb" };
				// args = new String[] { "wrld.xml" };
				// args = new String[] { "Data" };
				// args = new String[] { @"S:\Mods\BuildXPLOR\archive-3.0\661655\Data\game.dcb" };
			}

			if (args.Length < 1 || args.Length > 1)
			{
				Console.WriteLine("Usage: unforge.exe [infile]");
				Console.WriteLine();
				Console.WriteLine("Converts any Star Citizen binary file into an actual XML file.");
				Console.WriteLine("CryXml files (.xml) are saved as .raw in the original location.");
				Console.WriteLine("DataForge files (.dcb) are saved as .xml in the original location.");
				Console.WriteLine();
				Console.WriteLine("Can also convert all compatible files in a directory, and it's");
				Console.WriteLine("sub-directories. In that case, all CryXml files are saved in-place,");
				Console.WriteLine("and any DataForge files are saved to both .xml and extracted to");
				Console.WriteLine("the original component locations.");
				return;
			}

			if ((args.Length > 0) && Directory.Exists(args[0]))
			{
				foreach (var file in Directory.GetFiles(args[0], "*.*", SearchOption.AllDirectories))
				{
					if (new String[] { "ini", "txt" }.Contains(Path.GetExtension(file), StringComparer.InvariantCultureIgnoreCase)) continue;

					try
					{
						Console.WriteLine("Converting {0}", file.Replace(args[0], ""));

						Smelter.Instance.Smelt(file);
					}
					catch (Exception) { }
				}
			}
			else
			{
				Smelter.Instance.Smelt(args[0]);
			}
		}
	}
}
