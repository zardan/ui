﻿using System.Collections;
using System.Collections.Generic;
using PM;
using UnityEngine;
using UnityEngine.UI;

public class LevelModeButton : MonoBehaviour
{
	[Header("Button states")]
	public Sprite Default;
	public Sprite Active;
	public Sprite Completed;
	public Sprite Failed;

	public Image Image;

	public void SetButtonDefault()
	{
		Image.sprite = Default;
	}

	public void SetButtonActive()
	{
		Image.sprite = Active;
	}

	public void SetButtonCompleted()
	{
		Image.sprite = Completed;
	}

	public void SetButtonFailed()
	{
		Image.sprite = Failed;
	}

	public void SwitchToCase(int caseNumber)
	{
		if (PMWrapper.IsCompilerRunning || PMWrapper.IsCompilerUserPaused || PMWrapper.IsCasesRunning)
		{
			return;
		}

		LevelModeButtons.Instance.SetSandboxButtonToDefault();
		SetButtonActive();
		PMWrapper.SwitchCase(caseNumber);
	}

	public void SwitchToSandbox()
	{
		if (PMWrapper.IsCompilerRunning || PMWrapper.IsCompilerUserPaused || PMWrapper.IsCasesRunning)
		{
			return;
		}

		LevelModeController.Instance.InitSandboxMode();
		SetButtonActive();
	}
}
