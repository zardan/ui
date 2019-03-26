﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace PM
{
	public class CaseHandler
	{
		public bool IsCasesRunning;
		public int numberOfCases = 1;

		public bool AllCasesCompleted;
		public int CurrentCase = 0;

		public CaseHandler(int numOfCases)
		{
			numberOfCases = numOfCases;
		}

		// Is called OnLevelChanged and OnRunButtonClicked
		public void ResetHandlerAndButtons()
		{
			LevelModeButtons.Instance.SetCaseButtonsToDefault();
			SetCurrentCase(0);
		}

		public void SetCurrentCase(int caseNumber)
		{
			if (caseNumber != CurrentCase)
			{
				// currentCaseButtonUnpressed
				LevelModeButtons.Instance.SetCaseButtonsToDefault();

				CurrentCase = Mathf.Clamp(caseNumber, 0, numberOfCases);

				CaseFlash.Instance.HideFlash();
				if (numberOfCases > 1)
					CaseFlash.Instance.ShowNewCaseFlash(CurrentCase);
			}

			// currentCaseButtonPressed
			LevelModeButtons.Instance.SetCurrentCaseButtonState(LevelModeButtonState.Active);

			LevelModeController.Instance.SwitchToCaseMode();

			// Call every implemented event
			foreach (var ev in UISingleton.FindInterfaces<IPMCaseSwitched>())
				ev.OnPMCaseSwitched(CurrentCase);
		}

		public void RunCase(int caseNumber)
		{
			IsCasesRunning = true;

			CaseFlash.Instance.HideFlash();
			if (numberOfCases > 1)
				CaseFlash.Instance.ShowNewCaseFlash(CurrentCase, true);
			else
				PMWrapper.StartCompiler();

		}
		
		public void CaseCompleted()
		{
			PMWrapper.StopCompiler();

			Main.instance.StartCoroutine(ShowFeedbackAndRunNextCase());
		}

		public void CaseFailed()
		{
			IsCasesRunning = false;
			LevelModeButtons.Instance.SetCurrentCaseButtonState(LevelModeButtonState.Failed);
		}

		private IEnumerator ShowFeedbackAndRunNextCase()
		{
			string positiveMassage;
			if (numberOfCases == 1)
				positiveMassage = "Bra jobbat!";
			else
				positiveMassage = "Test " + (CurrentCase + 1) + " avklarat!";

			UISingleton.instance.taskDescription.ShowPositiveMessage(positiveMassage);

			yield return new WaitForSeconds(3 * (1 - PMWrapper.speedMultiplier));

			UISingleton.instance.answerBubble.HideMessage();
			UISingleton.instance.taskDescription.HideTaskFeedback();
			LevelModeButtons.Instance.SetCurrentCaseButtonState(LevelModeButtonState.Completed);

			CurrentCase++;

			if (CurrentCase >= numberOfCases)
			{
				IsCasesRunning = false;
				AllCasesCompleted = true;
				PMWrapper.SetLevelCompleted();
				yield break;
			}

			SetCurrentCase(CurrentCase);
			RunCase(CurrentCase);
		}
	}
}