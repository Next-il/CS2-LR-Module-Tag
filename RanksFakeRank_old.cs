using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using LevelsRanks.API;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CounterStrikeSharp.API.Modules.Memory;
using System.Linq;
using System;

namespace LevelsRanksModuleRanksFakeRank;

[MinimumApiVersion(80)]
public class LevelsRanksModuleRanksFakeRank : BasePlugin
{
	public override string ModuleName => "[LR] Module Fake Rank";
	public override string ModuleVersion => "1.0";
	public override string ModuleAuthor => "ShiNxz";
	public override string ModuleDescription => "...";

	private readonly PluginCapability<IPointsManager> _pointsManagerCapability = new("levelsranks");
	private IPointsManager? _pointsManager;

	public override void Load(bool hotReload)
	{
		_pointsManager = _pointsManagerCapability.Get();

		if (_pointsManager == null)
		{
			Server.PrintToConsole("Points management system is currently unavailable.");
			return;
		}

		RegisterListener<Listeners.OnMapStart>(OnMapStart);

		VirtualFunctions.CCSPlayerPawnBase_PostThinkFunc.Hook(_ =>
		{
			foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid))
			{
				int xp = _pointsManager.GetCurrentXP(player.SteamID.ToString());
				Console.WriteLine($"Player {player.PlayerName} has {xp} XP");

				if (xp == 0)
					continue;

				player.CompetitiveRankType = (sbyte)(11);
				player.CompetitiveRanking = xp;
			}
			return HookResult.Continue;
		}, HookMode.Post);
	}

	private void OnMapStart(string mapName)
	{
		foreach (var gameRulesProxy in Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules"))
		{
			var gameRules = gameRulesProxy.GameRules;
			if (gameRules == null) continue;

			gameRules.IsQueuedMatchmaking = true;
		}
	}

}
