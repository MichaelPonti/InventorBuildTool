using System.Xml;

namespace AddinXform
{
	internal class DeployCommand
	{
		private readonly string _addinPath;
		private readonly string _destination;
		private readonly string _dllPath;

		public DeployCommand(string addinPath, string destination, string dllPath)
		{
			_addinPath = addinPath;
			_destination = destination;
			_dllPath = dllPath;
		}

		public async Task<int> RunAsync()
		{
			if (String.IsNullOrWhiteSpace(_addinPath)) { return 1; }
			if (!File.Exists(_addinPath)) { return 1; }

			if (String.IsNullOrWhiteSpace(_destination)) { return 2; }
			if (!Directory.Exists(_destination)) { return 2; }

			if (String.IsNullOrWhiteSpace(_dllPath)) { return 3; }
			if (!File.Exists(_dllPath)) { return 3; }

			var destinationPath = Path.Combine(_destination, Path.GetFileName(_addinPath));

			try
			{
				File.Copy(_addinPath, destinationPath, true);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return 4;
			}

			var successful = await UpdateAssemblyPathAsync(destinationPath);
			if (!successful)
			{
				Console.Error.WriteLine("Failed to update the addin file");
				return 5;
			}

			return 0;
		}

		private async Task<bool> UpdateAssemblyPathAsync(string destinationPath)
		{
			try
			{
				using var reader = new StreamReader(destinationPath);
				var content = await reader.ReadToEndAsync().ConfigureAwait(false);
				reader.Close();
				reader.Dispose();

				var doc = new XmlDocument();
				doc.LoadXml(content);

				var node = doc.SelectSingleNode("descendant::Assembly");
				if (node == null)
				{
					return false;
				}

				node.InnerText = _dllPath;
				doc.Save(destinationPath);

				Console.WriteLine($"{destinationPath} modified Assembly node to be {_dllPath}");

				return true;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return false;
			}
		}
	}
}
