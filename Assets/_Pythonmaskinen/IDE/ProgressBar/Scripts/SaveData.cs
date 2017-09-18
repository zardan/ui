﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PM {

	public class SaveData : MonoBehaviour {

		//public static bool gottenLevelTip { get; set; }

		private static Dictionary<int, string> codes = new Dictionary<int, string>();

		public void SaveAndClearMainCode() {
			if (!this.enabled) return;

			if (codes.ContainsKey(PMWrapper.currentLevel))
				PMWrapper.mainCode = codes[PMWrapper.currentLevel];
			else
				PMWrapper.mainCode = string.Empty;
		}

		public static void SaveMainCode() {
			codes[PMWrapper.currentLevel] = PMWrapper.mainCode;
		}
	}

}