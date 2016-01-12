﻿/******************************************************************************
    Copyright 2015 Maxime Degallaix

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
******************************************************************************/

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMonitor.WPF.ViewModels
{
	public class LogCollectionFilterViewModel<TValue> : ReactiveObject
	{
		private bool isSelected;

		public LogCollectionFilterViewModel(TValue value, string label)
			: this(value, label, false) { }

		public LogCollectionFilterViewModel(TValue value, string label, bool isSelected)
		{
			this.Value = value;
			this.Label = label;
			this.isSelected = isSelected;
		}

		public TValue Value { get; private set; }

		public string Label { get; private set; }

		public bool IsSelected
		{
			get { return this.isSelected; }
			set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
		}
	}
}