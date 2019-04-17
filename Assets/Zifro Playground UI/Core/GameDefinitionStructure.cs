﻿using System.Collections.Generic;
using PM;

public class ActiveLevel
{
	public string sceneName { get; set; }
	public string levelId { get; set; }
}

public class SceneSettings
{
	public float walkerStepTime { get; set; }
	public bool gameWindowUiLightTheme { get; set; }
	public List<string> availableFunctions { get; set; }
}

public class GuideBubble
{
	public string target { get; set; }
	public string text { get; set; }
}

public class TaskDescription
{
	public string header { get; set; }
	public string body { get; set; }
}

public class LevelSettings
{
	public string precode { get; set; }
	public string startCode { get; set; }

	/// <summary>
	/// Example of solution code for current level. Precode is excluded.
	/// </summary>
	public string exampleSolutionCode { get; set; }

	public int rowLimit { get; set; }
	public TaskDescription taskDescription { get; set; }
	public List<string> availableFunctions { get; set; }
}

/// <summary>
/// Can be inherited to add custom properties and then registered with
/// <see cref="Main"/> static method <see cref="Main.RegisterLevelDefinitionContract{TLevelDefinition}"/>
/// </summary>
public class LevelDefinition
{
}

public class SandboxSettings
{
	public string precode { get; set; }
	public float walkerStepTime { get; set; }
}

/// <summary>
/// Can be inherited to add custom properties and then registered with
/// <see cref="Main"/> static method <see cref="Main.RegisterSandboxDefinitionContract{TSandboxDefinition}"/>
/// </summary>
public class SandboxDefinition
{
}

/// <summary>
/// If Sandbox exists the user will get to test level without the program being corrected.
/// If there are cases after Sandbox, they will correct the program.
/// Sandbox currently does not have any properties but is a object to make it easy to add properties in the future.
/// </summary>
public class Sandbox
{
	public SandboxSettings sandboxSettings { get; set; }
	public SandboxDefinition sandboxDefinition { get; set; }
}

public class CaseSettings
{
	public string precode { get; set; }
	public float walkerStepTime { get; set; }
}

/// <summary>
/// Can be inherited to add custom properties and then registered with
/// <see cref="Main"/> static method <see cref="Main.RegisterCaseDefinitionContract{TCaseDefinition}"/>
/// </summary>
public class CaseDefinition
{
}

public class Case
{
	public CaseSettings caseSettings { get; set; }
	public CaseDefinition caseDefinition { get; set; }
}

public class Level
{
	public string id { get; set; }
	public List<GuideBubble> guideBubbles { get; set; }
	public LevelSettings levelSettings { get; set; }
	public LevelDefinition levelDefinition { get; set; }
	public Sandbox sandbox { get; set; }
	public List<Case> cases { get; set; }
}

public class Scene
{
	public string name { get; set; }
	public SceneSettings sceneSettings { get; set; }
	public List<Level> levels { get; set; }
}

public class GameDefinition
{
	public string gameId { get; set; }
	public List<ActiveLevel> activeLevels { get; set; }
	public List<Scene> scenes { get; set; }
}
