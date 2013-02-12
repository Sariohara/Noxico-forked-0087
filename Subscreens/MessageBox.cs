﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;

namespace Noxico
{
	public class MessageBox
	{
		private enum BoxType { Notice, Question, List, Input };
		//private static string[] text = { };
		private static string text;
		private static BoxType type;
		private static string title;
		private static Action onYes, onNo;
		private static Dictionary<object, string> options;
		private static int option;
		private static bool allowEscape;
		public static object Answer { get; private set; }
		public static Action ScriptPauseHandler { get; set; }

		private static UIWindow win;
		private static UILabel lbl;
		private static UIList lst;
		private static UILabel key;
		private static UITextBox txt;
		private static bool fromWalkaround;

		public static void Handler()
		{
			var host = NoxicoGame.HostForm;
			var keys = NoxicoGame.KeyMap;
			if (Subscreens.FirstDraw)
			{
				Subscreens.FirstDraw = false;
				var lines = text.Split('\n').Length;
				var height = lines + 1;
				if (type == BoxType.List)
					height += 1 + options.Count;
				else if (type == BoxType.Input)
					height += 2;
				var top = 10 - (height / 2);
				if (top < 0)
					top = 0;
				if (UIManager.Elements == null || fromWalkaround)
					UIManager.Initialize();

				win = new UIWindow(type == BoxType.Question ? "Question" : title) { Left = 5, Top = top, Width = 70, Height = height, Background = Color.Black, Foreground = Color.Gray };
				UIManager.Elements.Add(win);
				lbl = new UILabel(text) { Left = 7, Top = top + 1, Width = 68, Height = lines };
				UIManager.Elements.Add(lbl);
				lst = null;
				txt = null;
				if (type == BoxType.List)
				{
					lst = new UIList("", Enter, options.Values.ToList(), 0) { Left = 6, Top = top + lines + 1, Width = 67, Height = options.Count, Background = Color.Black, Foreground = Color.Gray };
					lst.Change += (s, e) =>
						{
							option = lst.Index;
							Answer = options.Keys.ToArray()[option];
						};
					lst.Change(null, null);
					UIManager.Elements.Add(lst);
				}
				else if (type == BoxType.Input)
				{
					txt = new UITextBox((string)Answer) { Left = 7, Top = top + lines + 1, Width = 65, Height = 1, Background = Color.FromArgb(48, 48, 48), Foreground = Color.White };
					UIManager.Elements.Add(txt);
				}
				if (type == BoxType.Notice || type == BoxType.Input)
					key = new UILabel("<g2561><cWhite><g2026><cGray><g255E>") { Top = top + height - 1, Left = 70 };
				else if (type == BoxType.Question)
					key = new UILabel("<g2561><cWhite> Y/N <cGray><g255E>") { Top = top + height - 1, Left = 66 };
				else if (type == BoxType.List)
					key = new UILabel("<g2561><cWhite> <g2191>/<g2193> <cGray><g255E>") { Top = top + height - 1, Left = 66 };
				UIManager.Elements.Add(key);
				
				Subscreens.Redraw = true;
			}
			if (Subscreens.Redraw)
			{
				Subscreens.Redraw = false;
				UIManager.Draw();
			}

			if (NoxicoGame.IsKeyDown(KeyBinding.Back) || NoxicoGame.IsKeyDown(KeyBinding.Accept) || (type == BoxType.Question && (keys[(int)Keys.Y] || keys[(int)Keys.N])))
			{
				if (type == BoxType.List && NoxicoGame.IsKeyDown(KeyBinding.Back))
				{
					if (!allowEscape)
						return;
					else
						option = -1;
				}
				if (type == BoxType.Input && NoxicoGame.IsKeyDown(KeyBinding.Back))
				{
					UIManager.CheckKeys();
					return;
				}

				Enter(null, null);

				if (type == BoxType.Question)
				{
					if ((NoxicoGame.IsKeyDown(KeyBinding.Accept) || keys[(int)Keys.Y]) && onYes != null)
					{
						NoxicoGame.Sound.PlaySound("Get Item");
						NoxicoGame.ClearKeys();
						onYes();
					}
					else if ((NoxicoGame.IsKeyDown(KeyBinding.Back) || keys[(int)Keys.N]) && onNo != null)
					{
						NoxicoGame.Sound.PlaySound("Put Item");
						NoxicoGame.ClearKeys();
						onNo();
					}
				}
				else if (type == BoxType.List)
				{
					NoxicoGame.Sound.PlaySound(option == -1 ? "Put Item" : "Get Item");
					Answer = options.ElementAt(option).Key;
					onYes();
					NoxicoGame.ClearKeys();
				}
				else if (type == BoxType.Input)
				{
					NoxicoGame.Sound.PlaySound("Put Item");
					Answer = txt.Text;
					onYes();
					NoxicoGame.ClearKeys();
				}
				else
				{
					type = BoxType.Notice;
					NoxicoGame.ClearKeys();
				}
				if (ScriptPauseHandler != null)
				{
					ScriptPauseHandler();
					ScriptPauseHandler = null;
				}
			}
			else
			{
				UIManager.CheckKeys();
			}
		}

		private static void Enter(object sender, EventArgs args)
		{
			Remove();
			var host = NoxicoGame.HostForm;
			if (Subscreens.PreviousScreen.Count == 0)
			{
				UIManager.Initialize();
				NoxicoGame.Mode = UserMode.Walkabout;
				host.Noxico.CurrentBoard.Redraw();
			}
			else
			{
				NoxicoGame.Subscreen = Subscreens.PreviousScreen.Pop();
				host.Noxico.CurrentBoard.Redraw();
				host.Noxico.CurrentBoard.Draw();
				Subscreens.FirstDraw = true;
			}
		}

		private static void Remove()
		{
			UIManager.Elements.Remove(win);
			UIManager.Elements.Remove(lbl);
			if (lst != null)
				UIManager.Elements.Remove(lst);
			UIManager.Elements.Remove(key);
		}

		public static void List(string question, Dictionary<object, string> options, Action okay, bool allowEscape = false, bool doNotPush = false, string title = "")
		{
			fromWalkaround = NoxicoGame.Subscreen == null || Subscreens.PreviousScreen.Count == 0;
			if (!doNotPush)
				Subscreens.PreviousScreen.Push(NoxicoGame.Subscreen);
			NoxicoGame.Subscreen = MessageBox.Handler;
			type = BoxType.List;
			MessageBox.title = title;
			text = Toolkit.Wordwrap(question.Trim(), 68); //.Split('\n');
			option = 0;
			onYes = okay;
			MessageBox.options = options;
			MessageBox.allowEscape = allowEscape;
			NoxicoGame.Mode = UserMode.Subscreen;
			Subscreens.FirstDraw = true;
		}

		public static void Ask(string question, Action yes, Action no, bool doNotPush = false, string title = "")
		{
			fromWalkaround = NoxicoGame.Subscreen == null || Subscreens.PreviousScreen.Count == 0;
			if (!doNotPush)
				Subscreens.PreviousScreen.Push(NoxicoGame.Subscreen);
			NoxicoGame.Subscreen = MessageBox.Handler;
			type = BoxType.Question;
			MessageBox.title = title;
			text = Toolkit.Wordwrap(question.Trim(), 68); //.Split('\n');
			onYes = yes;
			onNo = no;
			NoxicoGame.Mode = UserMode.Subscreen;
			Subscreens.FirstDraw = true;
		}

		public static void Notice(string message, bool doNotPush = false, string title = "")
		{
			fromWalkaround = NoxicoGame.Subscreen == null || Subscreens.PreviousScreen.Count == 0;
			if (!doNotPush)
				Subscreens.PreviousScreen.Push(NoxicoGame.Subscreen);
			NoxicoGame.Subscreen = MessageBox.Handler;
			MessageBox.title = title;
			type = BoxType.Notice;
			text = Toolkit.Wordwrap(message.Trim(), 68); //.Split('\n');
			NoxicoGame.Mode = UserMode.Subscreen;
			Subscreens.FirstDraw = true;
		}

		public static void Input(string message, string defaultValue, Action okay, bool doNotPush = false, string title = "")
		{
			fromWalkaround = NoxicoGame.Subscreen == null || Subscreens.PreviousScreen.Count == 0;
			if (!doNotPush)
				Subscreens.PreviousScreen.Push(NoxicoGame.Subscreen);
			NoxicoGame.Subscreen = MessageBox.Handler;
			MessageBox.title = title;
			type = BoxType.Input;
			text = Toolkit.Wordwrap(message.Trim(), 68); //.Split('\n');
			Answer = defaultValue;
			onYes = okay;
			NoxicoGame.Mode = UserMode.Subscreen;
			Subscreens.FirstDraw = true;
		}
	}
}
