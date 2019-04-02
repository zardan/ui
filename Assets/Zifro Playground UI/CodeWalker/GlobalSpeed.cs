using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace PM
{
	public class GlobalSpeed : MonoBehaviour
	{
		public Slider speedSlider;

		public float currentSpeed
		{
			get => speedSlider.value;
			set => speedSlider.value = Mathf.Clamp01(value);
		}

		// Use this for initialization
		void Start()
		{
			speedSlider.onValueChanged.AddListener(delegate { ValueChanged(); });
			ValueChanged();
		}

		// Listener
		public void ValueChanged()
		{
			foreach (IPMSpeedChanged ev in UISingleton.FindInterfaces<IPMSpeedChanged>())
			{
				ev.OnPMSpeedChanged(currentSpeed);
			}
		}
	}
}