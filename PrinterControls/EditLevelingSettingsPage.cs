﻿/*
Copyright (c) 2018, Kevin Pope, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System.Collections.Generic;
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.ConfigurationPage.PrintLeveling;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl
{
	public class EditLevelingSettingsPage : DialogPage
	{
		public EditLevelingSettingsPage(PrinterConfig printer, ThemeConfig theme)
		{
			this.WindowTitle = "Leveling Settings".Localize();
			this.HeaderText = "Sampled Positions".Localize();

			var scrollableWidget = new ScrollableWidget()
			{
				AutoScroll = true,
				HAnchor = HAnchor.Stretch,
				VAnchor = VAnchor.Stretch,
			};
			scrollableWidget.ScrollArea.HAnchor = HAnchor.Stretch;
			contentRow.AddChild(scrollableWidget);
			var scrollArrea = new FlowLayoutWidget(FlowDirection.TopToBottom)
			{
				HAnchor = HAnchor.Stretch,
			};
			scrollableWidget.AddChild(scrollArrea);

			var positions = new List<Vector3>();

			PrintLevelingData levelingData = printer.Settings.Helpers.GetPrintLevelingData();
			for (int i = 0; i < levelingData.SampledPositions.Count; i++)
			{
				positions.Add(levelingData.SampledPositions[i]);
			}

			int tab_index = 0;
			for (int row = 0; row < positions.Count; row++)
			{
				var leftRightEdit = new FlowLayoutWidget
				{
					Padding = new BorderDouble(3),
					HAnchor = HAnchor.Stretch
				};

				var positionLabel = new TextWidget("{0} {1,-5}".FormatWith("Position".Localize(), row + 1), textColor: ActiveTheme.Instance.PrimaryTextColor);

				positionLabel.VAnchor = VAnchor.Center;
				leftRightEdit.AddChild(positionLabel);

				for (int axis = 0; axis < 3; axis++)
				{
					leftRightEdit.AddChild(new HorizontalSpacer());

					string axisName = "x";
					if (axis == 1) axisName = "y";
					else if (axis == 2) axisName = "z";

					leftRightEdit.AddChild(
						new TextWidget($"  {axisName}: ", textColor: ActiveTheme.Instance.PrimaryTextColor)
						{
							VAnchor = VAnchor.Center
						});

					int linkCompatibleRow = row;
					int linkCompatibleAxis = axis;

					MHNumberEdit valueEdit = new MHNumberEdit(positions[linkCompatibleRow][linkCompatibleAxis], allowNegatives: true, allowDecimals: true, pixelWidth: 60, tabIndex: tab_index++);
					valueEdit.ActuallNumberEdit.InternalTextEditWidget.EditComplete += (sender, e) =>
					{
						Vector3 position = positions[linkCompatibleRow];
						position[linkCompatibleAxis] = valueEdit.ActuallNumberEdit.Value;
						positions[linkCompatibleRow] = position;
					};

					valueEdit.Margin = new BorderDouble(3);
					leftRightEdit.AddChild(valueEdit);
				}

				scrollArrea.AddChild(leftRightEdit);
			}

			var savePresetsButton = theme.CreateDialogButton("Save".Localize());
			savePresetsButton.Click += (s, e) => UiThread.RunOnIdle(() =>
			{
				PrintLevelingData newLevelingData = printer.Settings.Helpers.GetPrintLevelingData();

				for (int i = 0; i < newLevelingData.SampledPositions.Count; i++)
				{
					newLevelingData.SampledPositions[i] = positions[i];
				}

				printer.Settings.Helpers.SetPrintLevelingData(newLevelingData, false);
				this.DialogWindow.Close();
			});

			this.AddPageAction(savePresetsButton);
		}
	}
}