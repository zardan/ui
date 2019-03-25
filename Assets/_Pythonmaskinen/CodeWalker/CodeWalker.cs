using UnityEngine;
using System;

namespace PM
{

	public class CodeWalker : MonoBehaviour, IPMSpeedChanged
	{

		#region time based variables
		private float baseWalkerWaitTime = 3f;
		public float BaseWalkerWaitTime
		{
			get { return baseWalkerWaitTime; }
			set
			{
				baseWalkerWaitTime = Mathf.Clamp(value, 0.01f, 1000);
				OnPMSpeedChanged(PMWrapper.speedMultiplier);
			}
		}

		float sleepTime = 3f;
		private float sleepTimer;
		#endregion
		
		public static bool IsSleeping;
		public static bool WalkerRunning = true;
		public bool IsUserPaused { get; private set; }

		public static int CurrentLineNumber;

		private static bool doEndWalker;
		private static Action<HelloCompiler.StopStatus> stopCompiler;

		// Properties needed for input connections to compiler 
		// TODO
		//public static Scope CurrentScope;
		//public static Action<string, Scope> SubmitInput;

		//This Script needs to be added to an object in the scene
		//To start the compiler simply call "ActivateWalker" Method
		#region init

		/// <summary>
		/// Activates the walker by telling the compiler to compile code and links necessary methods.
		/// </summary>
		public void ActivateWalker(Action<HelloCompiler.StopStatus> stopCompilerMeth)
		{
			// TODO
			//Compiler.SyntaxCheck.CompileCode(PMWrapper.fullCode, EndWalker, PauseWalker, LinkInputSubmitter, IDELineMarker.activateFunctionCall, IDELineMarker.SetWalkerPosition);
			stopCompiler = stopCompilerMeth;

			enabled = true;
			WalkerRunning = true;
			doEndWalker = false;
			IsUserPaused = false;

			CurrentLineNumber = 0;
		}
		#endregion


		#region CodeWalker
		// Update method runs every frame, and check if it is time to parse a line.
		// if so is the case, then we call "Runtime.CodeWalker.parseLine()" while we handle any thrown runtime exceptions that the codeWalker finds.

		private void Update()
		{
			if (IsUserPaused) return;

			if (WalkerRunning && !IsSleeping)
			{
				if (doEndWalker)
				{
					stopCompiler.Invoke(HelloCompiler.StopStatus.Finished);
					return;
				}

				try
				{
					// TODO
					//Runtime.CodeWalker.parseLine();

					CurrentLineNumber++;
				}
				catch
				{
					stopCompiler.Invoke(HelloCompiler.StopStatus.RuntimeError);
					throw;
				}
				finally
				{
					IsSleeping = true;
				}
			}

			RunSleepTimer();
		}

		private void RunSleepTimer()
		{
			sleepTimer += Time.deltaTime;
			float firstInterval = sleepTime - sleepTime / 20;
			// TODO
			if (sleepTimer > firstInterval /*&& !Runtime.CodeWalker.isWaitingForUserInput*/)
			{
				IDELineMarker.instance.SetState(IDELineMarker.State.Hidden);
				if (sleepTimer > sleepTime)
				{
					sleepTimer = 0;
					IsSleeping = false;
				}
			}
		}
		#endregion


		#region Compiler Methodes
		//Methods that the CC should be able to call.
		//We link this to the CC by passing them into the "Runtime.CodeWalker.initCodeWalker" method
		public static void EndWalker()
		{
			doEndWalker = true;
		}

		public static void PauseWalker()
		{
			WalkerRunning = false;
		}

		// TODO
		//public static void LinkInputSubmitter(Action<string, Scope> submitInput, Scope currentScope)
		public static void LinkInputSubmitter(Action<string, object> submitInput, object currentScope)
		{
			//SubmitInput = submitInput;
			//CurrentScope = currentScope;
		}
		#endregion


		#region Public Methods

		public void ResumeWalker()
		{
			sleepTimer = 0;
			IsSleeping = true;
			WalkerRunning = true;
		}

		public void StopWalker()
		{
			SetWalkerUserPaused(false);
			WalkerRunning = false;
			enabled = false;
		}

		// Called by the RunCodeButton script
		public void SetWalkerUserPaused(bool paused)
		{
			if (paused == IsUserPaused) return;

			IsUserPaused = paused;

			if (IsUserPaused)
			{
				foreach (var ev in UISingleton.FindInterfaces<IPMCompilerUserPaused>())
					ev.OnPMCompilerUserPaused();
			}
			else
			{
				foreach (var ev in UISingleton.FindInterfaces<IPMCompilerUserUnpaused>())
					ev.OnPMCompilerUserUnpaused();
			}
		}

		public void OnPMSpeedChanged(float speed)
		{
			float speedFactor = 1 - speed;
			sleepTime = baseWalkerWaitTime * speedFactor;
		}
		#endregion
	}

}