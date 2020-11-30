using System;
using System.Collections.Generic;
using System.Text;
using ADepIn;
using BepInEx.Configuration;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;

namespace Deli
{
	public class Mod
	{
		/// <summary>
		///		Information about the mod
		/// </summary>
		public Manifest Info { get; }

		/// <summary>
		///		The assets for the mod
		/// </summary>
		public IResourceIO Resources { get; }

		/// <summary>
		///		The configuration for the mod
		/// </summary>
		public ConfigFile Config { get; }

		/// <summary>
		///		The log to be used by the mod
		/// </summary>
		public ManualLogSource Logger { get; }

		public Mod(Manifest info, IResourceIO resources, ConfigFile config, ManualLogSource logger)
		{
			Info = info;
			Resources = resources;
			Config = config;
			Logger = logger;
		}

		/// <summary>
		///		A simple printout of this mod's identity. Use <seealso cref="Info" /> in
		///		conjunction with <see cref="Manifest.ToPrettyString" /> to get a more
		///		complete printout.
		/// </summary>
		public override string ToString()
		{
			return Info.ToString();
		}

		public readonly struct Manifest
		{
			#region Critical metadata

			/// <summary>
			///		The globally unique identitifer of this mod. This cannot conflict with any
			///		other mods.
			/// </summary>
			[JsonProperty(Required = Required.Always)]
			public string Guid { get; }

			/// <summary>
			///		The current version of this mod.
			/// </summary>
			[JsonProperty(Required = Required.Always)]
			public Version Version { get; }

			/// <summary>
			///		The GUIDs and corresponding versions of mods that this mod requires.
			/// </summary>
			public Option<Dictionary<string, Version>> Dependencies { get; }

			#endregion

			#region Helpful metadata

			/// <summary>
			///		The user-friendly name for this mod.
			/// </summary>
			public Option<string> Name { get; }

			/// <summary>
			///		The creators of this mod.
			/// </summary>
			public Option<string[]> Authors { get; }

			/// <summary>
			///		The source URL of the mod.
			/// </summary>
			public Option<string> SourceUrl { get; }

			#endregion

			#region Assets

			/// <summary>
			///		The patcher asset paths and corresponding asset loaders that this mod contains.
			/// </summary>
			public Option<Dictionary<string, string>> Patcher { get; }

			/// <summary>
			///		The runtime asset paths and corresponding asset loaders that this mod contains.
			/// </summary>
			public Option<Dictionary<string, string>> Runtime { get; }

			#endregion

			[JsonConstructor]
			public Manifest(string guid, Version version, Option<Dictionary<string, Version>> dependencies, Option<string> name, Option<string[]> authors, Option<string> sourceUrl, Option<Dictionary<string, string>> patcher, Option<Dictionary<string, string>> runtime)
			{
				Guid = guid;
				Version = version;
				Dependencies = dependencies;

				Name = name;
				Authors = authors;

				Patcher = patcher;
				Runtime = runtime;

				SourceUrl = sourceUrl;
			}

			/// <summary>
			///		A pretty-printout of the mods identity. Examples:
			///		<code>deli.example @ 1.0.0.0</code>
			///		<code>Example Mod (deli.example @ 1.0.0.0)</code>
			///		<code>Example Mod (deli.example @ 1.0.0.0) by Developer A</code>
			///		<code>Example Mod (deli.example @ 1.0.0.0) by Developer A and Developer B</code>
			/// </summary>
			public string ToPrettyString()
			{
				var builder = new StringBuilder();

				var hasName = Name.MatchSome(out var name);
				if (hasName) builder.Append(name).Append(" (");

				builder.Append(Guid).Append(" @ ").Append(Version);

				if (hasName) builder.Append(')');

				if (Authors.MatchSome(out var authors))
				{
					builder.Append(' ');

					var iLast = authors.Length - 1;
					for (var i = 0; i < iLast; ++i) builder.Append(authors[i]).Append(", ");

					if (authors.Length > 1) builder.Append("and ");

					builder.Append(authors[iLast]);
				}

				return builder.ToString();
			}

			/// <summary>
			///		A simple prinout of this mod's identity. Example:
			///		<code>deli.example @ 1.0.0.0</code>
			/// </summary>
			public override string ToString()
			{
				return $"[{Name.UnwrapOr(Guid)} {Version}]";
			}
		}
	}
}
