using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Deli.Bootstrap;
using Deli.Runtime;
using UnityEngine;

namespace Deli.Setup
{
	internal delegate Coroutine CoroutineRunner(IEnumerator method);

	internal delegate void CoroutineStopper(Coroutine coroutine);

#pragma warning disable CS1591

	[BepInPlugin(Constants.Metadata.Guid, Constants.Metadata.Name, Constants.Metadata.SysVersion)]
	public class PluginEntrypoint : BaseUnityPlugin
	{
		private void Awake()
		{
			var blob = PatcherEntrypoint.Handoff();
			var manager = new GameObject(Constants.Metadata.Name);
			DontDestroyOnLoad(manager);

			var behaviours = new Dictionary<Mod, List<DeliBehaviour>>();
			var setup = new SetupStage(blob.StageData, manager, behaviours);
			var runtime = new RuntimeStage(blob.StageData, behaviours);

			// Eagerly evaluate; do not leave this to runtime to enumerate or it will be too late.
			var mods = setup.RunInternal(blob.Mods).ToList();

			StartCoroutine(runtime.Run(mods, StartCoroutine, StopCoroutine));
		}
	}

#pragma warning restore CS1591
}
