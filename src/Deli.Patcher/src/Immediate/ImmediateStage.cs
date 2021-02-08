using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Deli.VFS;
using Deli.VFS.Disk;

namespace Deli.Immediate
{
	public abstract class ImmediateStage<TStage> : Stage<ImmediateAssetLoader<TStage>> where TStage : ImmediateStage<TStage>
	{
		protected abstract TStage GenericThis { get; }

		public ImmediateStage(Blob data) : base(data)
		{
		}

		protected abstract Dictionary<string, AssetLoaderID>? GetAssets(Mod.AssetTable table);

		private void LoadMod(Mod mod, Dictionary<string, Mod> lookup)
		{
			var table = mod.Info.Assets;
			if (table is null) return;

			var assets = GetAssets(table);
			if (assets is null) return;

			Logger.LogDebug(Locale.LoadingAssets(mod));
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset, out var loaderMod);

				foreach (var handle in Glob(mod, asset))
				{
					try
					{
						loader(GenericThis, mod, handle);
					}
					catch
					{
						Logger.LogFatal(Locale.LoaderException(asset.Value, loaderMod, mod, handle));
						throw;
					}
				}
			}
		}

		protected static byte[] BytesReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return File.ReadAllBytes(disk.PathOnDisk);
			}

			using var raw = file.OpenRead();
			using var memory = new MemoryStream();

			return memory.ToArray();
		}

		protected static Assembly AssemblyReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return Assembly.LoadFile(disk.PathOnDisk);
			}

			var raw = BytesReader(file);
			var symbols = file.WithExtension("mdb");

			return symbols is not IFileHandle symbolsFile ? Assembly.Load(raw) : Assembly.Load(raw, BytesReader(symbolsFile));
		}

		protected void AssemblyLoader(Stage stage, Mod mod, IHandle handle)
		{
			AssemblyLoader(stage, mod, AssemblyReader(AssemblyPreloader(handle)));
		}

		protected IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			PreRun();

			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				RunModules(mod);
				LoadMod(mod, lookup);

				yield return mod;
			}
		}
	}
}