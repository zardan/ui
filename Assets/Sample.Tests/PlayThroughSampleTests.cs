﻿using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using ZifroPlaygroundTests;
using ZifroPlaygroundTests.PlayMode;

namespace Sample.Tests
{
	public class PlayThroughSample : PlayThroughLevelsTests
	{
		static CaseTestData[] GetActiveCases()
		{
			return PlaygroundTestHelper.GetActiveCases("game");
		}

		static LevelTestData[] GetActiveLevels()
		{
			return PlaygroundTestHelper.GetActiveLevels("game");
		}

		protected override string testingScenePath => "Assets/Sample.Tests/MainSceneForTesting.unity";

		[UnityTest]
		public override IEnumerator TestPlayWholeGame()
		{
			return TestPlayWholeGame(GetActiveLevels());
		}

		[UnityTest]
		public override IEnumerator TestPlayGuidesInLevel([ValueSource(nameof(GetActiveLevels))] LevelTestData data)
		{
			return base.TestPlayGuidesInLevel(data);
		}

		[UnityTest]
		public override IEnumerator TestPlayCase([ValueSource(nameof(GetActiveCases))] CaseTestData data)
		{
			return base.TestPlayCase(data);
		}

		[UnityTest]
		public override IEnumerator TestPlayLevel([ValueSource(nameof(GetActiveLevels))] LevelTestData data)
		{
			return base.TestPlayLevel(data);
		}
	}
}
