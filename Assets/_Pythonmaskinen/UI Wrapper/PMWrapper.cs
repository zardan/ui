using PM;
using System;
using System.Collections.Generic;
using System.Linq;
using Mellis.Core.Interfaces;
using UnityEngine;

/// <summary>
/// PMWrapper, short for "Python Machine Wrapper".
/// <para>This class contains lots of static methods and properties for hooking into the "PythonMachine" without editing the source.</para>
/// <para>Except this class you may also use the interface events. All events interfaces starts with the prefix IPM, for example <seealso cref="IPMCompilerStarted"/></para>
/// </summary>
public static class PMWrapper
{
	/// <summary>
	/// Tells which mode the level is currently running. See <see cref="PM.LevelMode"/> for avaliable modes.
	/// </summary>
	public static LevelMode LevelMode => LevelModeController.Instance.LevelMode;

	/// <summary>
	/// Value from the speed slider. Ranges from 0 to 1, with a default of 0.5.
	/// <para>To get instant updates from the speed change, use <see cref="IPMSpeedChanged"/>.</para>
	/// </summary>
	public static float speedMultiplier
	{
		get => UISingleton.instance.speed.currentSpeed;
		set => UISingleton.instance.speed.currentSpeed = value;
	}

	/// <summary>
	/// A base factor in how long it takes for the walker to take one step in the code. 
	/// It is multiplied with <see cref="speedMultiplier"/> to calculate the actual step time.
	/// </summary>
	public static float walkerStepTime
	{
		get => UISingleton.instance.walker.sleepTime;
		set => UISingleton.instance.walker.sleepTime = value;
	}

	public static Level CurrentLevel => Main.Instance.LevelDefinition;

	/// <summary>
	/// The pre code, i.e. the un-changeable code BEFORE the main code.
	/// <para>Changing this value will automatically offset the main codes Y position.</para>
	/// </summary>
	public static string preCode
	{
		get => UISingleton.instance.textField.preCode;
		set
		{
			UISingleton.instance.textField.preCode = value;
			UISingleton.instance.textField.InitTextFields();
			UISingleton.instance.textField.ColorCodeDaCode();
		}
	}
	/// <summary>
	/// The main code, i.e. the code the user is able to change.
	/// Colorcoding is automatically applied if <seealso cref="IDETextField"/> is disabled.
	/// <para>Changing this value while the user is coding may lead to unexpected glitches. Be careful!</para>
	/// </summary>
	public static string mainCode
	{
		get => UISingleton.instance.textField.theInputField.text;
		set => UISingleton.instance.textField.InsertMainCode(value);
	}
	/// <summary>
	/// The post code, i.e. the un-changeable code AFTER the main code.
	/// <para>At the moment post code is not available and will throw an <seealso cref="NotImplementedException"/>.</para>
	/// </summary>
	/// <exception cref="NotImplementedException">IDE doesn't have full support for post code yet.</exception>
	public static string postCode
	{
		get => throw new NotImplementedException("IDE doesn't have full support for post code yet!");
		set => throw new NotImplementedException("IDE doesn't have full support for post code yet!");
	}

	/// <summary>
	/// All codes combined, i.e. <see cref="preCode"/> + <see cref="mainCode"/> + <see cref="postCode"/> (with linebreaks inbetween).
	/// <para>This is the property that <seealso cref="PM.CodeWalker"/> uses when sending the code to compile to the <seealso cref="Compiler.SyntaxCheck"/>.</para>
	/// </summary>
	public static string fullCode => (preCode.Length > 0 ? preCode + '\n' + mainCode : mainCode);

	/// <summary>
	/// Replacement for <see cref="IDETextField.amountOfRows"/>. Defines how many lines of code the user is allowed to enter.
	/// <para>Setting a value lower than the current number of rows user has typed will result in buggy behaviour. Be aware!</para>
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when setting this to any non-positive values.</exception>
	public static int codeRowsLimit
	{
		get => UISingleton.instance.textField.codeRowsLimit;
		set
		{
			if (value > 0)
			{
				UISingleton.instance.textField.rowLimit = value;
			}
			else throw new ArgumentOutOfRangeException("codeRowsLimit", value, "Zero and negative values are not accepted!");
		}
	}

	/// <summary>
	/// Adds code to the main code where the cursor currently is. Typically used by smart buttons.
	/// </summary>
	public static void AddCode(string code, bool smartButtony = false)
	{
		UISingleton.instance.textField.InsertText(code, smartButtony);
	}

	/// <summary>
	/// Adds code to the main code only if the main code is empty. Should be used for setting given code first time the user opens the level.
	/// </summary>
	public static void AddCodeAtStart(string code)
	{
		UISingleton.instance.textField.InsertMainCodeAtStart(code);
	}

	public static int CurrentLineNumber => CodeWalker.CurrentLineNumber;

	/// <summary>
	/// Scrolls so that the <paramref name="lineNumber"/> is visible in the IDE.
	/// </summary>
	public static void FocusOnLineNumber(int lineNumber)
	{
		UISingleton.instance.textField.theScrollLord.FocusOnLineNumber(lineNumber);
	}

	/// <summary>
	/// Scrolls so that the current selected line is visible in the IDE.
	/// </summary>
	public static void FocusOnLineNumber()
	{
		UISingleton.instance.textField.FocusCursor();
	}

	/// <summary>
	/// Boolean representing wether cases is currently running or not.
	/// </summary>
	public static bool IsCasesRunning => Main.Instance.CaseHandler.IsCasesRunning;

	/// <summary>
	/// Boolean representing wether the compiler is currently executing or not.
	/// </summary>
	public static bool IsCompilerRunning => UISingleton.instance.compiler.isRunning;

	/// <summary>
	/// Boolean representing wether the walker is currently paused by the user (via pressing the pause button).
	/// </summary>
	public static bool IsCompilerUserPaused => UISingleton.instance.walker.isUserPaused;

	/// <summary>
	/// Starts the compiler if it's not currently running. Static wrapper for <see cref="HelloCompiler.compileCode"/>
	/// </summary>
	public static void StartCompiler()
	{
		UISingleton.instance.compiler.compileCode();
	}

	/// <summary>
	/// Starts case 0 and starts compiler. Will run all test cases if possible
	/// </summary>
	public static void RunCode()
	{
		LevelModeController.Instance.RunProgram();
	}

	/// <summary>
	/// Stops the compiler if it's currently running. Static wrapper for <see cref="HelloCompiler.stopCompiler(HelloCompiler.StopStatus)"/> with the argument <seealso cref="HelloCompiler.StopStatus.CodeForced"/>
	/// </summary>
	public static void StopCompiler()
	{
		UISingleton.instance.compiler.stopCompiler(HelloCompiler.StopStatus.CodeForced);
	}

	/// <summary>
	/// Sets the available functions of type <see cref="IClrFunction"/> or <see cref="IClrYieldingFunction"/> in the compiler.
	/// </summary>
	public static void SetCompilerFunctions(List<IEmbeddedType> functions)
	{
		SetSmartButtons(functions.Select(function => function.FunctionName + "()").ToList());
		UISingleton.instance.compiler.addedFunctions.Clear();
		UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	/// <summary>
	/// Sets the available functions of type <see cref="IClrFunction"/> or <see cref="IClrYieldingFunction"/> in the compiler.
	/// </summary>
	public static void SetCompilerFunctions(params IEmbeddedType[] functions)
	{
		SetSmartButtons(functions.Select(function => function.FunctionName + "()").ToList());
		UISingleton.instance.compiler.addedFunctions.Clear();
		UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	/// <summary>
	/// Adds to the available list of functions of type <see cref="IClrFunction"/> or <see cref="IClrYieldingFunction"/> to the already existing list of compiler functions.
	/// </summary>
	public static void AddCompilerFunctions(List<IEmbeddedType> functions)
	{
		AddSmartButtons(functions.Select(function => function.FunctionName + "()").ToList());
		UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	/// <summary>
	/// Adds to the available list of functions of type <see cref="IClrFunction"/> or <see cref="IClrYieldingFunction"/> to the already existing list of compiler functions.
	/// </summary>
	public static void AddCompilerFunctions(params IEmbeddedType[] functions)
	{
		AddSmartButtons(functions.Select(function => function.FunctionName + "()").ToList());
		UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	/// <summary>
	/// Used to resolve a yielded function <see cref="IClrYieldingFunction"/>
	/// and uses the compilers NULL representation <see cref="IScriptTypeFactory.Null"/> as return value.
	/// <para>Replacement for <seealso cref="CodeWalker.unPauseWalker"/></para>
	/// </summary>
	public static void UnpauseWalker()
	{
		UISingleton.instance.walker.ResumeWalker();
	}

	/// <summary>
	/// Used to resolve a yielded function <see cref="IClrYieldingFunction"/>
	/// and uses parameter <paramref name="returnValue"/> as return value.
	/// <para>Replacement for <seealso cref="CodeWalker.unPauseWalker"/></para>
	/// </summary>
	public static void UnpauseWalker(IScriptType returnValue)
	{
		UISingleton.instance.walker.ResumeWalker(returnValue);
	}

	/// <summary>
	/// Set smart buttons from parameters.
	/// </summary>
	public static void SetSmartButtons(params string[] codes)
	{
		UISingleton.instance.smartButtons.ClearSmartButtons();
		for (int i = 0; i < codes.Length; i++)
		{
			UISingleton.instance.smartButtons.AddSmartButton(codes[i], codes[i]);
		}
	}

	/// <summary>
	/// Set smart buttons from list.
	/// </summary>
	public static void SetSmartButtons(List<string> buttonTexts)
	{
		UISingleton.instance.smartButtons.ClearSmartButtons();
		for (int i = 0; i < buttonTexts.Count; i++)
		{
			UISingleton.instance.smartButtons.AddSmartButton(buttonTexts[i], buttonTexts[i]);
		}
	}

	/// <summary>
	/// Add one smart button below code window.
	/// <para>Text on button</para>
	/// </summary>
	public static void AddSmartButton(string buttonText)
	{
		UISingleton.instance.smartButtons.AddSmartButton(buttonText, buttonText);
	}

	/// <summary>
	/// Add one smart button below code window.
	/// <para>Text on button</para>
	/// </summary>
	public static void AddSmartButtons(List<string> buttonTexts)
	{
		for (int i = 0; i < buttonTexts.Count; i++)
		{
			UISingleton.instance.smartButtons.AddSmartButton(buttonTexts[i], buttonTexts[i]);
		}
	}

	/// <summary>
	/// Adds smart buttons with names taken from registered functions via <see cref="SetCompilerFunctions(Compiler.Function[])"/> or <see cref="AddCompilerFunctions(Compiler.Function[])"/>
	/// </summary>
	public static void AutoSetSmartButtons()
	{
		// TODO
		//SetSmartButtons(UISingleton.instance.compiler.addedFunctions.ConvertAll(f => f.name + "()").ToArray());
	}

	/// <summary>
	/// Set the task description for current level. If passed empty string, both placeholders for task description will be deactivated.
	/// </summary>
	public static void SetTaskDescription(string header,string body)
	{
		UISingleton.instance.taskDescription.SetTaskDescription(header, body);
	}

	/// <summary>
	/// Set the correct answeres for the current case.
	/// </summary>
	public static void SetCaseAnswer(params int[] answer)
	{
		Main.Instance.LevelAnswer = new LevelAnswer(answer);
	}

	/// <summary>
	/// Set the correct answeres for the current case.
	/// </summary>
	public static void SetCaseAnswer(params string[] answer)
	{
		Main.Instance.LevelAnswer = new LevelAnswer(answer);
	}

	/// <summary>
	/// Set the correct answeres for the current case.
	/// </summary>
	public static void SetCaseAnswer(params bool[] answer)
	{
		Main.Instance.LevelAnswer = new LevelAnswer(answer);
	}

	/// <summary>
	/// Represents the current level. Setting this value will automatically setoff <see cref="IPMLevelChanged.OnPMLevelChanged(int)"/>
	/// <para>If set to a value higher than highest unlocked level then <seealso cref="unlockedLevel"/> will also be set to the same value.</para>
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if set to value outside of levels list index range, i.e. thrown if <seealso cref="CurrentLevelIndex"/>.set &lt; 0 or ≥ <seealso cref="numOfLevels"/></exception>
	public static int CurrentLevelIndex
	{
		get => UISingleton.instance.levelbar.Current;
		set
		{
			if (value < 0 || value >= numOfLevels)
				throw new ArgumentOutOfRangeException("currentLevel", value, "Level index out of range!");
			else
				UISingleton.instance.levelbar.ChangeLevel(value);
		}
	}

	/// <summary>
	/// This value determites how many levels are shown on the levelbar in the UI.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">In the case of non-positive values in setting <see cref="numOfLevels"/>.</exception>
	public static int numOfLevels
	{
		get => UISingleton.instance.levelbar.NumberOfLevels;
		set
		{
			if (value > 0) UISingleton.instance.levelbar.RecreateButtons(value, Mathf.Clamp(CurrentLevelIndex, 0, value - 1), unlockedLevel);
			else throw new ArgumentOutOfRangeException("numOfLevels", value, "Zero and negative values are not accepted!");
		}
	}

	/// <summary>
	/// Returns true if current level has defined Answer and the user is supposed to answer level.
	/// </summary>
	public static bool levelShouldBeAnswered => false;

	/// <summary>
	/// The highest level that's unlocked. Value of 0 means only first level is unlocked. Value of (<seealso cref="numOfLevels"/> - 1) means last level is unlocked, i.e. all levels.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">In the case of invalid values in setting <see cref="unlockedLevel"/></exception>
	public static int unlockedLevel
	{
		get => UISingleton.instance.levelbar.Unlocked;
		set
		{
			if (value >= 0 && value < numOfLevels) UISingleton.instance.levelbar.UpdateButtons(CurrentLevelIndex, value);
			else throw new ArgumentOutOfRangeException("unlockedLevel", value, "Level value is out of range of existing levels.");
		}
	}

	/// <summary>
	/// Returns the index of the current case. Index starts from 0.
	/// </summary>
	public static int currentCase => Main.Instance.CaseHandler.CurrentCase;

	/// <summary>
	/// Stops the compiler, shows the "Level complete!" popup, marks the current level as complete and unlocks the next level.
	/// </summary>
	public static void SetLevelCompleted()
	{
		UISingleton.instance.winScreen.SetLevelCompleted();
	}

	/// <summary>
	/// Set case completed if test case is passed. Marks current case as completed and starts next test case.
	/// </summary>
	public static void SetCaseCompleted()
	{
		Main.Instance.CaseHandler.CaseCompleted();
	}

	/// <summary>
	/// Switches to choosen case.
	/// </summary>
	/// <param name="caseNumber">The case number to switch to.</param> 
	public static void SwitchCase(int caseNumber)
	{
		Main.Instance.CaseHandler.SetCurrentCase(caseNumber);
	}

	/// <summary>
	/// Jumps to the last level. <paramref name="ignoreUnlocked"/> determines if it should respect or ignore <seealso cref="unlockedLevel"/>.
	/// </summary>
	/// <param name="ignoreUnlocked">If true, it will jump to the absolute last level. If false, it will jump to the last unlocked level.</param>
	public static void JumpToLastLevel(bool ignoreUnlocked = false)
	{
		CurrentLevelIndex = ignoreUnlocked ? numOfLevels - 1 : unlockedLevel;
	}

	/// <summary>
	/// Jumps to the first level.
	/// </summary>
	public static void JumpToFirstLevel()
	{
		CurrentLevelIndex = 0;
	}

	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the current line.
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(message);
	}
	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the target <paramref name="newLineNumber"/>.
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(int newLineNumber, string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(newLineNumber, message);
	}
	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the target <see cref="UnityEngine.UI.Selectable"/>.
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(UnityEngine.UI.Selectable targetSelectable, string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(targetSelectable, message);
	}
	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the target <see cref="RectTransform"/>.
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(RectTransform targetRectTransform, string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(targetRectTransform, message);
	}
	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the target canvas position.
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(Vector2 targetCanvasPosition, string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(targetCanvasPosition, message);
	}
	/// <summary>
	/// Stops the compiler and shows a dialog box containing the error message on the target world position.
	/// <para>NOTE: The game camera must be marked with the "Main Camera" tag for this to work.</para>
	/// </summary>
	/// <exception cref="PMRuntimeException">Is always thrown</exception>
	public static void RaiseError(Vector3 targetWorldPosition, string message)
	{
		UISingleton.instance.textField.theLineMarker.setErrorMarker(targetWorldPosition, message);
	}


	/// <summary>
	/// Stops compiler and shows feedback dialog from robot.
	/// </summary>
	public static void RaiseTaskError(string message)
	{
		UISingleton.instance.taskDescription.ShowTaskError(message);
		Main.Instance.CaseHandler.CaseFailed();
		UISingleton.instance.compiler.stopCompiler(HelloCompiler.StopStatus.TaskError);
	}

	/// <summary>
	/// Makes the IDE not destroy on load, i.e. on level change and such.
	/// <para>Equal to <see cref="UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object)"/> but on the IDE.</para>
	/// </summary>
	public static void DontDestroyIDEOnLoad()
	{
		if (UISingleton.instance.ideRoot.parent != null)
			UISingleton.instance.ideRoot.parent = null;
		UnityEngine.Object.DontDestroyOnLoad(UISingleton.instance.ideRoot);
	}

	#region OBSOLETE

	/// <summary>
	/// Sets the compiler functions avalible for the user.
	/// </summary>
	[Obsolete("New compiler. Use SetCompilerFunctions(List<IEmbeddedValue>) with IClrFunction or IClrYieldingFunction instead.", error: true)]
	public static void SetCompilerFunctions<T>(List<T> functions)
	{
		//SetSmartButtons(functions.Select(function => function.buttonText).ToList());
		//UISingleton.instance.compiler.addedFunctions = functions;
	}

	/// <summary>
	/// Adds a list of functions to the already existing list of compiler functions.
	/// </summary>
	[Obsolete("New compiler. Use AddCompilerFunctions(List<IEmbeddedValue>) with IClrFunction or IClrYieldingFunction instead.", error: true)]
	public static void AddCompilerFunctions<T>(List<T> functions)
	{
		//AddSmartButtons(functions.Select(function => function.buttonText).ToList());
		//UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	/// <summary>
	/// Adds all parameters of type <see cref="Compiler.Function"/> to the already existing list of compiler functions.
	/// </summary>
	[Obsolete("New compiler. Use AddCompilerFunctions(params IEmbeddedValue[]) with IClrFunction or IClrYieldingFunction instead.", error: true)]
	public static void AddCompilerFunctions<T>(params T[] functions)
	{
		//AddSmartButtons(functions.Select(function => function.buttonText).ToList());
		//UISingleton.instance.compiler.addedFunctions.AddRange(functions);
	}

	#endregion
}
