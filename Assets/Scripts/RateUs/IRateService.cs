using System;
using UnityEngine;

namespace com.F4A.MobileThird
{
	public partial interface IRateService
	{
		void Initialize();
		void ShowReviewInApp(bool forced);
		void ShowAppstore();
	}
}
