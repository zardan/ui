using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mellis.Core.Interfaces;
using Newtonsoft.Json;
using PM.Guide;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PM
{
	[Serializable]
	public class Main : MonoBehaviour, IPMCompilerStopped, IPMLevelChanged, IPMCaseSwitched
	{
		private string loadedScene;
		public string GameDataFileName;

		public GameDefinition GameDefinition;

		public Level LevelDefinition;
		public LevelAnswer LevelAnswer;

		public CaseHandler CaseHandler;

		public static Main Instance;

		private SceneSettings currentSceneSettings;
		private LevelSettings currentLevelSettings;

		// Everything should be placed in Awake() but there are some things that needs to be set in Awake() in some other script before the things currently in Start() is called
		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
		}

		private void Start()
		{
			LoadingScreen.Instance.Show();

			GameDefinition = ParseJson();

			Progress.Instance.LoadUserGameProgress();
		}

		private GameDefinition ParseJson()
		{
			TextAsset jsonAsset = Resources.Load<TextAsset>(GameDataFileName);

			if (jsonAsset == null)
			{
				throw new Exception("Could not find the file \"" + GameDataFileName +
				                    "\" that should contain game data in json format.");
			}

			string jsonString = jsonAsset.text;

			GameDefinition gameDefinition = JsonConvert.DeserializeObject<GameDefinition>(jsonString);

			return gameDefinition;
		}

		public void StartGame()
		{
			// Will create level navigation buttons
			PMWrapper.numOfLevels = GameDefinition.activeLevels.Count;

			StartLevel(0);

			LoadingScreen.Instance.Hide();
		}

		public void StartLevel(int levelIndex)
		{
			string sceneName = GameDefinition.activeLevels[levelIndex].sceneName;
			LoadScene(sceneName);

			string levelId = GameDefinition.activeLevels[levelIndex].levelId;
			LoadLevel(levelId);

			foreach (IPMLevelChanged ev in UISingleton.FindInterfaces<IPMLevelChanged>())
			{
				ev.OnPMLevelChanged();
			}
		}

		private void LoadScene(string sceneName)
		{
			if (sceneName != loadedScene)
			{
				var scenes = GameDefinition.scenes.Where(x => x.name == sceneName).ToList();

				if (scenes.Count > 1)
				{
					throw new Exception("There are more than one scene with name " + sceneName);
				}

				if (!scenes.Any())
				{
					throw new Exception("There is no scene with name " + sceneName);
				}

				Scene scene = scenes.First();

				currentSceneSettings = scene.sceneSettings;

				if (loadedScene != null)
				{
					SceneManager.UnloadSceneAsync(loadedScene);
				}

				int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene.name);
				if (sceneIndex < 0)
				{
					throw new Exception("Scene with name " + scene.name + " exists but is not added to build settings");
				}

				SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
				loadedScene = sceneName;
			}
		}

		private void LoadLevel(string levelId)
		{
			var levels = GameDefinition.scenes.First(x => x.name == loadedScene).levels.Where(x => x.id == levelId)
				.ToList();

			if (levels.Count > 1)
			{
				throw new Exception("There are more than one level with id " + levelId);
			}

			if (!levels.Any())
			{
				throw new Exception("There is no level with id " + levelId);
			}

			LevelDefinition = levels.First();

			currentLevelSettings = LevelDefinition.levelSettings;

			LevelModeButtons.Instance.CreateButtons();

			BuildGuides(LevelDefinition.guideBubbles);
			BuildCases(LevelDefinition.cases);

			if (LevelDefinition.sandbox != null)
			{
				LevelModeController.Instance.InitSandboxMode();
			}
			else
			{
				LevelModeController.Instance.InitCaseMode();
			}
		}

		public void SetSettings()
		{
			ClearSettings();
			SetSceneSettings();
			SetLevelSettings();

			if (PMWrapper.LevelMode == LevelMode.Sandbox)
			{
				SetSandboxSettings();
			}
			else if (PMWrapper.LevelMode == LevelMode.Case)
			{
				SetCaseSettings();
			}

			LoadMainCode();
		}

		private void ClearSettings()
		{
			PMWrapper.SetTaskDescription("", "");
			PMWrapper.SetCompilerFunctions();
			PMWrapper.preCode = "";
		}

		private void SetSceneSettings()
		{
			if (currentSceneSettings.walkerStepTime > 0)
			{
				PMWrapper.walkerStepTime = currentSceneSettings.walkerStepTime;
			}

			if (currentSceneSettings.gameWindowUiLightTheme)
			{
				GameWindow.Instance.SetGameWindowUiTheme(GameWindowUiTheme.light);
			}
			else
			{
				GameWindow.Instance.SetGameWindowUiTheme(GameWindowUiTheme.dark);
			}

			if (currentSceneSettings.availableFunctions != null)
			{
				List<IEmbeddedType> availableFunctions =
					CreateFunctionsFromStrings(currentSceneSettings.availableFunctions);
				PMWrapper.SetCompilerFunctions(availableFunctions);
			}
		}

		private void SetLevelSettings()
		{
			if (currentLevelSettings == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(currentLevelSettings.precode))
			{
				PMWrapper.preCode = currentLevelSettings.precode;
			}

			if (currentLevelSettings.taskDescription != null)
			{
				PMWrapper.SetTaskDescription(currentLevelSettings.taskDescription.header,
					currentLevelSettings.taskDescription.body);
			}
			else
			{
				PMWrapper.SetTaskDescription("", "");
			}

			if (currentLevelSettings.rowLimit > 0)
			{
				PMWrapper.codeRowsLimit = currentLevelSettings.rowLimit;
			}

			if (currentLevelSettings.availableFunctions != null)
			{
				List<IEmbeddedType> availableFunctions =
					CreateFunctionsFromStrings(currentLevelSettings.availableFunctions);
				PMWrapper.AddCompilerFunctions(availableFunctions);
			}
		}

		private void SetCaseSettings()
		{
			if (LevelDefinition.cases != null && LevelDefinition.cases.Any())
			{
				CaseSettings caseSettings = LevelDefinition.cases[PMWrapper.currentCase].caseSettings;

				if (caseSettings == null)
				{
					return;
				}

				if (!String.IsNullOrEmpty(caseSettings.precode))
				{
					PMWrapper.preCode = caseSettings.precode;
				}

				if (caseSettings.walkerStepTime > 0)
				{
					PMWrapper.walkerStepTime = caseSettings.walkerStepTime;
				}
			}
		}

		private void SetSandboxSettings()
		{
			if (LevelDefinition.sandbox != null)
			{
				SandboxSettings sandboxSettings = LevelDefinition.sandbox.sandboxSettings;

				if (sandboxSettings == null)
				{
					return;
				}

				if (!String.IsNullOrEmpty(sandboxSettings.precode))
				{
					PMWrapper.preCode = sandboxSettings.precode;
				}

				if (sandboxSettings.walkerStepTime > 0)
				{
					PMWrapper.walkerStepTime = sandboxSettings.walkerStepTime;
				}
			}
		}

		private static void BuildGuides(List<GuideBubble> guideBubbles)
		{
			if (guideBubbles != null && guideBubbles.Any())
			{
				var levelGuide = new LevelGuide();
				foreach (GuideBubble guideBubble in guideBubbles)
				{
					if (guideBubble.target == null || string.IsNullOrEmpty(guideBubble.text))
					{
						throw new Exception("A guide bubble for level with index " + PMWrapper.CurrentLevelIndex +
						                    " is missing target or text");
					}

					// Check if target is a number
					Match match = Regex.Match(guideBubble.target, @"^[0-9]+$");
					if (match.Success)
					{
						int.TryParse(guideBubble.target, out int lineNumber);
						levelGuide.guides.Add(new Guide.Guide(guideBubble.target, guideBubble.text, lineNumber));
					}
					else
					{
						levelGuide.guides.Add(new Guide.Guide(guideBubble.target, guideBubble.text));
					}
				}

				UISingleton.instance.guidePlayer.currentGuide = levelGuide;
			}
			else
			{
				UISingleton.instance.guideBubble.HideMessage();
				UISingleton.instance.guidePlayer.currentGuide = null;
			}
		}

		private void BuildCases(List<Case> cases)
		{
			if (cases != null && cases.Any())
			{
				CaseHandler = new CaseHandler(cases.Count);
			}
			else
			{
				if (LevelDefinition.sandbox == null)
				{
					CaseHandler = new CaseHandler(1);
				}
			}
		}

		private void LoadMainCode()
		{
			if (Progress.Instance.LevelData[PMWrapper.CurrentLevel.id].IsStarted)
			{
				PMWrapper.mainCode = Progress.Instance.LevelData[PMWrapper.CurrentLevel.id].MainCode;
			}
			else
			{
				if (currentLevelSettings != null && currentLevelSettings.startCode != null)
				{
					PMWrapper.AddCodeAtStart(currentLevelSettings.startCode);
				}
				else
				{
					PMWrapper.mainCode = string.Empty;
				}

				Progress.Instance.LevelData[PMWrapper.CurrentLevel.id].IsStarted = true;
			}
		}

		private static List<IEmbeddedType> CreateFunctionsFromStrings(IEnumerable<string> functionNames)
		{
			var functions = new List<IEmbeddedType>();

			// Use reflection to get an instance of compiler function class from string
			foreach (string functionName in functionNames)
			{
				var type = Type.GetType(functionName);

				if (type == null)
				{
					throw new Exception("Error when trying to read available functions. Function name: \"" +
					                    functionName + "\" could not be found.");
				}

				var function = (IEmbeddedType)Activator.CreateInstance(type);
				functions.Add(function);
			}

			return functions;
		}

		public void OnPMCompilerStopped(HelloCompiler.StopStatus status)
		{
			if (LevelAnswer != null)
			{
				LevelAnswer.compilerHasBeenStopped = true;
			}

			if (status == HelloCompiler.StopStatus.Finished)
			{
				if (PMWrapper.levelShouldBeAnswered && UISingleton.instance.taskDescription.isActiveAndEnabled)
				{
					PMWrapper.RaiseTaskError("Fick inget svar");
				}
			}
		}

		public void OnPMLevelChanged()
		{
			PMWrapper.StopCompiler();
			StopAllCoroutines();
		}

		public void OnPMCaseSwitched(int caseNumber)
		{
			StopAllCoroutines();
			UISingleton.instance.answerBubble.HideMessage();
		}
	}
}