﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PM
{
	public class LevelModeButtons : MonoBehaviour
	{
		public GameObject SandboxButton;
		public List<GameObject> CaseButtons;

		public static LevelModeButtons Instance;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
		}

		public void CreateButtons()
		{
			if (Main.instance.levelDefinition.sandbox != null)
				SandboxButton.SetActive(true);
			else
				SandboxButton.SetActive(false);

			SetCaseButtonsToDefault();
		}

		public void SetCurrentCaseButtonState(LevelModeButtonState state)
		{
			var caseNumber = Main.instance.caseHandler.CurrentCase;
			
			if (state == LevelModeButtonState.Default)
				CaseButtons[caseNumber].GetComponent<LevelModeButton>().SetButtonDefault();
			else if (state == LevelModeButtonState.Active)
				CaseButtons[caseNumber].GetComponent<LevelModeButton>().SetButtonActive();
			else if (state == LevelModeButtonState.Completed)
				CaseButtons[caseNumber].GetComponent<LevelModeButton>().SetButtonCompleted();
			else if (state == LevelModeButtonState.Failed)
				CaseButtons[caseNumber].GetComponent<LevelModeButton>().SetButtonFailed();
		}

		public void SetCaseButtonsToDefault()
		{
			int numberOfCases = 0;
			if (Main.instance.levelDefinition.cases != null && Main.instance.levelDefinition.cases.Any())
			{
				numberOfCases = Main.instance.levelDefinition.cases.Count;
			}
			else
			{
				if (Main.instance.levelDefinition.sandbox == null)
					numberOfCases = 1;
			}

			for (int i = 0; i < CaseButtons.Count; i++)
			{
				// Don't show buttons if there is only one case except if there is a sandbox before
				if (i < numberOfCases && (numberOfCases > 1 || SandboxButton.activeInHierarchy))
				{
					CaseButtons[i].SetActive(true);
					CaseButtons[i].GetComponent<LevelModeButton>().SetButtonDefault();
				}
				else
				{
					CaseButtons[i].SetActive(false);
				}
			}
		}

		public void SetSandboxButtonToDefault()
		{
			SandboxButton.GetComponent<LevelModeButton>().SetButtonDefault();
		}

		public void SetSandboxButtonState(LevelModeButtonState state)
		{
			if (state == LevelModeButtonState.Default)
				SandboxButton.GetComponent<LevelModeButton>().SetButtonDefault();
			else if (state == LevelModeButtonState.Active)
				SandboxButton.GetComponent<LevelModeButton>().SetButtonActive();
			else if (state == LevelModeButtonState.Completed)
				SandboxButton.GetComponent<LevelModeButton>().SetButtonCompleted();
			else if (state == LevelModeButtonState.Failed)
				SandboxButton.GetComponent<LevelModeButton>().SetButtonFailed();
		}
	}

	public enum LevelModeButtonState
	{
		Default,
		Active,
		Completed,
		Failed
	}
}
