using System.CommandLine;

namespace AddinXform
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			var rootCommand = new RootCommand();
			InitializeDeploySubCommand(rootCommand);
			InitializeUpdateSubCommand(rootCommand);

			return await rootCommand.InvokeAsync(args);
		}

		static void InitializeDeploySubCommand(RootCommand rootCommand)
		{
			var addInPath = new Option<string>("--addinpath", "Path to the desired addin file. This is the addin file in the project/output directory.");
			addInPath.IsRequired = true;

			var destination = new Option<string>("--destination", "Location to copy the addin to. This is the location that Inventor will find and use to load the addin.");
			destination.IsRequired = true;

			var dllPath = new Option<string>("--dllpath", "Location of add in dll. Used to update the \"<Assembly>\" value in the newly copied addin file.");
			dllPath.IsRequired = true;

			var dc = new Command("deploy", "Copy addin file to specific version location. Use this to setup for debugging.")
			{
				addInPath,
				destination,
				dllPath
			};

			dc.SetHandler(async (addInPath, destination, dllPath) =>
			{
				Console.WriteLine($"addInPath: {addInPath}");
				Console.WriteLine($"destination: {destination}");
				Console.WriteLine($"dllPath: {dllPath}");

				var command = new DeployCommand(addInPath, destination, dllPath);
				var result = await command.RunAsync().ConfigureAwait(false);

				Environment.Exit(result);
			}, addInPath, destination, dllPath);

			rootCommand.AddCommand(dc);
		}

		private static void InitializeUpdateSubCommand(RootCommand rootCommand)
		{
			var addinPath = new Option<string>("--addinpath", "Path to the desired addin file. This is the addin file in the project/output directory.");
			addinPath.IsRequired = true;

			var verEqualTo = new Option<string>("--SupportedSoftwareVersionEqualTo", "This updates the version equal to node. Format is #major.#minor.#servicepack/#build. Supports multiple versions separated by columns.");
			var verGreater = new Option<string>("--SupportedSoftwareVersionGreaterThan", "This updates the version greater than node. Format is the same but does not support semi-colon delimited lists.");
			var verLess = new Option<string>("--SupportedSoftwareVersionLessThan", "This updates the version less than node. Format is the same but does not support semi-colon delimited lists.");
			var verNotEqualTo = new Option<string>("--SupportedSoftwareVersionNotEqualTo", "This updates the version not equal to node. Format is teh same and supports multiple versions.");

			var updateCommand = new Command("update", "Update an addin file with specific version information.")
			{
				addinPath,
				verEqualTo,
				verGreater,
				verLess,
				verNotEqualTo
			};

			updateCommand.SetHandler(async (addinPath, verEqualTo, verGreater, verLess, verNotEqualTo) =>
			{
				Console.WriteLine($"addinpath: {addinPath}");
				Console.WriteLine($"SupportedSoftwareVersionEqualTo: {verEqualTo}");
				Console.WriteLine($"SupportedSoftwareVersionGreaterThan: {verGreater}");
				Console.WriteLine($"SupportedSoftwareVersionLessThan: {verLess}");
				Console.WriteLine($"SupportedSoftwareVersionNotEqualTo: {verNotEqualTo}");

				var command = new UpdateCommand(addinPath, verEqualTo, verGreater, verLess, verNotEqualTo);
				var result = await command.RunAsync().ConfigureAwait(false);

				Environment.Exit(result);
			}, addinPath, verEqualTo, verGreater, verLess, verNotEqualTo);

			rootCommand.AddCommand(updateCommand);
		}
	}
}
