using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory;
using System.Collections.Generic;
using LevelsRanks.API;
using System.Linq;
using System;

namespace LevelsRanksModuleRanksFakeRank
{
	[MinimumApiVersion(200)]
	public sealed partial class PluginLevelsRanksModuleRanksFakeRank : BasePlugin
	{
		public override string ModuleName => "[LR] Module Fake Rank";
		public override string ModuleVersion => "1.0.0";
		public override string ModuleAuthor => "K4ryuu";
		public override string ModuleDescription => "...";

		public static PluginCapability<IPointsManager> _pointsManagerCapability { get; } = new("levelsranks");
		private IPointsManager? _pointsManager;

		private Dictionary<string, int> playerRanks = new Dictionary<string, int>();

		public override void Load(bool hotReload)
		{
			_pointsManager = _pointsManagerCapability.Get();

			if (_pointsManager == null)
			{
				Server.PrintToConsole("Points management system is currently unavailable.");
				return;
			}

			RegisterListener<Listeners.OnMapStart>(OnMapStart);
			RegisterEventHandler<EventRoundStart>(OnRoundStart);
			// RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);

			VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Hook(_ =>
			{
				Utilities.GetPlayers().Where(p => p?.IsValid == true && p.PlayerPawn?.IsValid == true && !p.IsBot && !p.IsHLTV && p.SteamID.ToString().Length == 17)
					.ToList()
					.ForEach(p =>
					{
						if (playerRanks.TryGetValue(p.SteamID.ToString(), out int xp))
						{
							if (xp == 0)
								return;

							p.CompetitiveRankType = (sbyte)(11);
							p.CompetitiveRanking = xp;
						}
					});

				return HookResult.Continue;
			}, HookMode.Post);
		}

		public static CCSGameRules GetGameRules()
		{
			var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
			var gameRules = gameRulesEntities.First().GameRules;

			if (gameRules == null)
			{
				Console.WriteLine("Failed to get game rules.");
			}

			return gameRules;
		}

		private void OnMapStart(string mapName)
		{
			playerRanks.Clear();

			if (_pointsManager != null)
			{
				foreach (var p in Utilities.GetPlayers().Where(p => p?.IsValid == true && p.PlayerPawn?.IsValid == true && !p.IsBot && !p.IsHLTV && p.SteamID.ToString().Length == 17))
				{
					int xp = _pointsManager.GetCurrentXP(p.SteamID.ToString());
					playerRanks[p.SteamID.ToString()] = xp;
				}
			}

			var gameRules = GetGameRules();
			gameRules.IsValveDS = true;
			// gameRules.IsQueuedMatchmaking = true;
			gameRules.QueuedMatchmakingMode = 1;

			Console.WriteLine("Map started.");
			Console.WriteLine($"IsValveDS: {gameRules.IsValveDS}");
			// Console.WriteLine($"QueuedMatchmakingMode : {gameRules.QueuedMatchmakingMode}");
			Console.WriteLine($"IsQueuedMatchmaking: {gameRules.IsQueuedMatchmaking}");

			// foreach (var gameRulesProxy in Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules"))
			// {
			// 	var gameRules = gameRulesProxy.GameRules;
			// 	if (gameRules == null) continue;

			// 	gameRules.IsQueuedMatchmaking = true;
			// 	gameRules.IsValveDS = true;
			// }
		}

		private HookResult OnRoundStart(EventRoundStart roundStartEvent, GameEventInfo info)
		{
			var gameRules = GetGameRules();
			gameRules.IsValveDS = true;
			gameRules.QueuedMatchmakingMode = 1;
			// gameRules.IsQueuedMatchmaking = true;

			Console.WriteLine("Round started.");
			Console.WriteLine($"IsValveDS: {gameRules.IsValveDS}");
			Console.WriteLine($"QueuedMatchmakingMode: {gameRules.QueuedMatchmakingMode}");
			// Console.WriteLine($"IsQueuedMatchmaking: {gameRules.IsQueuedMatchmaking}");

			foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
			{
				if(player.team)
				
				if (player != null && player.IsValid && !player.IsBot)
				{
					if (_pointsManager == null) return HookResult.Continue;

					// get the player xp
					int xp = _pointsManager.GetCurrentXP(player.SteamID.ToString());

					if (xp == 0)
						return HookResult.Continue;

					playerRanks[player.SteamID.ToString()] = xp;
				}
			}

			return HookResult.Continue;
		}

		// private HookResult OnPlayerDeath(EventPlayerDeath deathEvent, GameEventInfo info)
		// {
		// 	foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
		// 	{
		// 		if (player != null && player.IsValid && !player.IsBot)
		// 		{
		// 			if (_pointsManager == null) return HookResult.Continue;

		// 			// get the player xp
		// 			int xp = _pointsManager.GetCurrentXP(player.SteamID.ToString());

		// 			if (xp == 0)
		// 				return HookResult.Continue;

		// 			playerRanks[player.SteamID.ToString()] = xp;
		// 		}
		// 	}

		// 	return HookResult.Continue;
		// }
	}
}
