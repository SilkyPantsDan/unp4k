namespace unforge
{
	public class Smelter
	{
		public static Smelter Instance => new();

		private Boolean _overwrite;

		public void Smelt(String path, Boolean overwrite = true)
		{
			this._overwrite = overwrite;

			try
			{
				if (File.Exists(path))
				{
					if (Path.GetExtension(path) == ".dcb")
					{
						using BinaryReader br = new(File.OpenRead(path));
						bool legacy = new FileInfo(path).Length < 0x0e2e00;

						DataForge df = new(br, legacy);

						df.Save(Path.ChangeExtension(path, "xml"));
					}
					else
					{
						if (!_overwrite)
						{
							if (!File.Exists(Path.ChangeExtension(path, "raw")))
							{
								File.Move(path, Path.ChangeExtension(path, "raw"));
								path = Path.ChangeExtension(path, "raw");
							}
						}

						var xml = CryXmlSerializer.ReadFile(path);

						if (xml != null)
						{
							xml.Save(Path.ChangeExtension(path, "xml"));
						}
						else
						{
							Console.WriteLine("{0} already in XML format", path);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error converting {0}: {1}", path, ex.Message);
			}
		}
	}
}