﻿using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace PM
{
	public class GameToMigration : MonoBehaviour
	{
		[FormerlySerializedAs("Version")]
		public Version version;

		[FormerlySerializedAs("BaseOutputPath")]
		public string baseOutputPath;

		public static GameToMigration instance;

		private void Start()
		{
			if (instance == null)
			{
				instance = this;
			}
		}

		public void CreateMigrationFromJson()
		{
			GameDefinition game = Main.instance.gameDefinition;

			string basePath = baseOutputPath + "Zifro/App_Code/Persistance/Migrations/GameUpgrades/";
			string fileName = "TargetVersion_" + version.PrintWithUnderscore() + ".cs";
			string path = basePath + fileName;

			if (File.Exists(path))
			{
				throw new IOException("The file \"" + fileName + "\" already exists at \"" + basePath + "\"");
			}

			using (var tw = new StreamWriter(path))
			{
				tw.WriteLine("using System.Collections.Generic;");
				tw.WriteLine("using System.Linq;");
				tw.WriteLine("using Zifro.Models.Playground.Database;");
				tw.WriteLine("using Umbraco.Core.Logging;");
				tw.WriteLine("using Umbraco.Core.Persistence.Migrations;");
				tw.WriteLine("using Umbraco.Core.Persistence.SqlSyntax;");
				tw.WriteLine("using Zifro.Code;\n");

				tw.WriteLine("namespace Zifro.Persistance.Migrations.GameUpgrades.TargetVersion_" +
				             version.PrintWithUnderscore());
				tw.WriteLine("{");
				tw.WriteLine("	[Migration(\"" + version.PrintWithDots() +
				             "\", 1, Constants.Application.GameMigrationName)]");
				tw.WriteLine("	public class UpdatePlaygroundGameData : MigrationBase");
				tw.WriteLine("	{");
				tw.WriteLine("		public UpdatePlaygroundGameData(ISqlSyntaxProvider sqlSyntax, ILogger logger)");
				tw.WriteLine("			: base(sqlSyntax, logger)");
				tw.WriteLine("		{}\n");

				tw.WriteLine("		public override void Up()");
				tw.WriteLine("		{");
				tw.WriteLine("			using (var dbContext = new PlaygroundDbContext())");
				tw.WriteLine("			{");
				tw.WriteLine("				var game = dbContext.PlaygroundGame.Find(\"" + game.gameId + "\");\n");

				tw.WriteLine("				if (game == null)");
				tw.WriteLine("					game = dbContext.PlaygroundGame.Add(new PlaygroundGame() {GameId = \"" +
				             game.gameId + "\"});\n");

				tw.WriteLine(
					"				var levelsInDatabase = dbContext.PlaygroundLevel.Where(x => x.GameId == game.GameId);\n");

				tw.WriteLine("				var levelsToUpdate = new List<string>");
				tw.WriteLine("				{");
				foreach (ActiveLevel level in game.activeLevels)
				{
					tw.WriteLine("					\"" + level.levelId + "\",");
				}

				tw.WriteLine("				};\n");

				tw.WriteLine("				var levelsPrecode = new Dictionary<string, string>()");
				tw.WriteLine("				{");
				foreach (ActiveLevel activeLevel in game.activeLevels)
				{
					System.Collections.Generic.List<Level> levels =
						game.scenes.First(scene => scene.name == activeLevel.sceneName).levels;
					Level level = levels.First(lvl => lvl.id == activeLevel.levelId);
					string levelPrecode = "";

					if (level.levelSettings != null)
					{
						levelPrecode = level.levelSettings.precode;
					}

					if (level.cases != null && level.cases.Any() && level.cases.First().caseSettings != null)
					{
						if (!string.IsNullOrEmpty(level.cases.First().caseSettings.precode) && level.sandbox == null)
						{
							levelPrecode = level.cases.First().caseSettings.precode;
						}
					}

					if (!string.IsNullOrEmpty(levelPrecode))
					{
						tw.WriteLine("					{ \"" + activeLevel.levelId + "\", \"" + levelPrecode.Replace("\n", "\\n") +
						             "\" },");
					}
				}

				tw.WriteLine("				};\n");

				tw.WriteLine("				foreach (var levelToUpdate in levelsToUpdate)");
				tw.WriteLine("				{");
				tw.WriteLine("					var level = levelsInDatabase.FirstOrDefault(x => x.LevelId == levelToUpdate);");
				tw.WriteLine(
					"					var precode = levelsPrecode.ContainsKey(levelToUpdate) ? levelsPrecode[levelToUpdate] : null;\n");
				tw.WriteLine("					if (level == null)");
				tw.WriteLine("					{");
				tw.WriteLine("						dbContext.PlaygroundLevel.Add(new PlaygroundLevel()");
				tw.WriteLine("						{");
				tw.WriteLine("							LevelId = levelToUpdate,");
				tw.WriteLine("							GameId = game.GameId,");
				tw.WriteLine("							Precode = precode,");
				tw.WriteLine("							PlaygroundGame = game");
				tw.WriteLine("						});");
				tw.WriteLine("					}");
				tw.WriteLine("					else");
				tw.WriteLine("					{");
				tw.WriteLine("						level.Precode = precode;");
				tw.WriteLine("					}");
				tw.WriteLine("				}\n");

				tw.WriteLine("				dbContext.SaveChanges();");
				tw.WriteLine("			}");
				tw.WriteLine("		}\n");

				tw.WriteLine("		public override void Down()");
				tw.WriteLine("		{}");
				tw.WriteLine("	}");
				tw.WriteLine("}");
			}

			Debug.Log("Migration created successfully at " + path);
		}
	}

	[Serializable]
	public struct Version
	{
		public int major, minor, build;

		public Version(int major, int minor, int build)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
		}

		public string PrintWithDots()
		{
			return $"{major}.{minor}.{build}";
		}

		public string PrintWithUnderscore()
		{
			return $"{major}_{minor}_{build}";
		}
	}
}