﻿using System;
namespace Step3.Services
{
	public partial class WatchService
	{
		public Action<int> ValueUpdated { get; set; }
		public partial void SendValue(int value);
		public partial void Activate();
		public partial void Deactivate();
	}
}

