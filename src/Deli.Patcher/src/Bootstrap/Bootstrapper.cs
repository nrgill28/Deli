using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Deli.Immediate;
using Deli.VFS.Disk;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Serialization;
using Deli.Patcher;
using static Deli.Bootstrap.Constants;

namespace Deli.Bootstrap
{
	internal class Bootstrapper
	{
		private Stage.Blob? _stageData;
		private PatcherStage? _stage;
		private List<Mod>? _mods;

		public ManualLogSource Logger => Mod.Logger;

		private Stage.Blob StageData
		{
			get
			{
				Stage.Blob Init()
				{
					var jsonReaders = new ImmediateReaderCollection(Logger);
					var serializer = JsonSerializer.Create(new JsonSerializerSettings
					{
						Formatting = Formatting.Indented,
						ContractResolver = new DefaultContractResolver
						{
							NamingStrategy = new SnakeCaseNamingStrategy()
						},
						Converters =
						{
							new DeepDictionaryJsonConverter(),
							new AssetLoaderIDJsonConverter(),
							new SemVersionJsonConverter()
						}
					});
					var sharedLoaders = new AssetLoaderCollection<ImmediateAssetLoader<Stage>>();
					var immediateReaders = Readers.DefaultCollection(Logger);
					var modModules = new Dictionary<Mod, List<DeliModule>>();

					return new Stage.Blob(Mod, jsonReaders, serializer, sharedLoaders, immediateReaders, modModules);
				}

				return _stageData ??= Init();
			}
		}

		private PatcherStage Stage => _stage ??= new PatcherStage(StageData);

		private List<Mod> Mods
		{
			get
			{
				IEnumerable<Mod> Init()
				{
					var manifestReader = Stage.ImmediateReaders.Get<Mod.Manifest>();
					var discovery = new Discovery(Logger, manifestReader);
					var sorter = new Sorter(Logger);

					var mods = new[] { Mod }.Concat(discovery.Run());
					mods = sorter.Run(mods);
					mods = Stage.RunInternal(mods);

					foreach (var mod in mods) yield return mod;
				}

				return _mods ??= Init().ToList();
			}
		}

		public Mod Mod { get;  }

		public PatcherCollection Patchers
		{
			get
			{
				var stage = Stage;
				// Ensure mods have loaded
				var _ = Mods;

				return stage.Patchers;
			}
		}

		public HandoffBlob Blob => new(StageData, Mods);

		public Bootstrapper()
		{
			var manifest = new Mod.Manifest(Metadata.Guid, Metadata.Version,Metadata.Version, Metadata.Name, Metadata.Description, Metadata.Authors, Metadata.SourceUrl);

			Directory.CreateDirectory(Filesystem.Directory);
			Mod = new Mod(manifest, new RootDirectoryHandle(Filesystem.Directory));
		}
	}
}
