using System.Xml;

namespace AddinXform
{
	internal class UpdateCommand
	{
		private readonly string _addinPath;
		private readonly string? _verEqualTo;
		private readonly string? _verGreater;
		private readonly string? _verLess;
		private readonly string? _verNotEqualTo;

		public UpdateCommand(string addinPath, string? verEqualTo = null, string? verGreater = null, string? verLess = null, string? verNotEqualTo = null)
		{
			_addinPath = addinPath;
			_verEqualTo = verEqualTo;
			_verGreater = verGreater;
			_verLess = verLess;
			_verNotEqualTo = verNotEqualTo;
		}

		public async Task<int> RunAsync()
		{
			if (String.IsNullOrWhiteSpace(_addinPath)) { return 1; }
			if (!File.Exists(_addinPath)) { return 1; }

			var canProceed = false;
			if (!String.IsNullOrWhiteSpace(_verEqualTo)) { canProceed = true; }
			if (!String.IsNullOrWhiteSpace(_verGreater)) { canProceed = true; }
			if (!String.IsNullOrWhiteSpace(_verLess)) { canProceed = true; }
			if (!String.IsNullOrWhiteSpace(_verNotEqualTo)) { canProceed = true; }

			if (!canProceed)
			{
				Console.Error.WriteLine("At least one of the version options must be specified.");
				return 2;
			}

			var successful = await UpdateAddinVersionInfoAsync().ConfigureAwait(false);
			if (!successful)
			{
				Console.Error.WriteLine("Failed to update the addin file.");
				return 3;
			}

			return 0;
		}

		private async Task<bool> UpdateAddinVersionInfoAsync()
		{
			try
			{
				using var reader = new StreamReader(_addinPath);
				var content = await reader.ReadToEndAsync().ConfigureAwait(false);
				reader.Close();
				reader.Dispose();

				var doc = new XmlDocument();
				doc.LoadXml(content);

				if (!String.IsNullOrWhiteSpace(_verEqualTo)) { UpdateTagInDocument(doc, "SupportedSoftwareVersionEqualTo", _verEqualTo); }
				if (!String.IsNullOrWhiteSpace(_verGreater)) { UpdateTagInDocument(doc, "SupportedSoftwareVersionGreaterThan", _verGreater); }
				if (!String.IsNullOrWhiteSpace(_verLess)) { UpdateTagInDocument(doc, "SupportedSoftwareVersionLessThan", _verLess); }
				if (!String.IsNullOrWhiteSpace(_verNotEqualTo)) { UpdateTagInDocument(doc, "SupportedSoftwareVersionNotEqualTo", _verNotEqualTo); }

				doc.Save(_addinPath);
				return true;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				return false;
			}
		}


		private void UpdateTagInDocument(XmlDocument doc, string tag, string value)
		{
			var node = doc.SelectSingleNode($"descendant::{tag}");
			if (node == null)
			{
				var rootNode = doc.DocumentElement;
				if (rootNode == null)
				{
					throw new Exception("Invalid addin file.");
				}

				var newNode = doc.CreateElement(tag);
				newNode.InnerText = value;
				rootNode.AppendChild(newNode);
			}
			else
			{
				node.InnerText = value;
			}
		}
	}
}
