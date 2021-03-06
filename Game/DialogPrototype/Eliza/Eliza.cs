﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DialogPrototype
{
	public class Eliza
	{
		private VectorDataStore vectorData = null;
		private DialogTree      dialogTree = null;
		private List<Message>   newInput   = new List<Message>();
		private float           lastInput  = 0.0f;
		private float           lastOutput = 0.0f;
		private float           waitOffset = 0.0f;
		private Random          random     = new Random();
		private Stopwatch       watch      = new Stopwatch();
		private float           timer      = 0.0f;
		private DialogContext   context    = null;

		public Eliza(VectorDataStore vectorData, DialogTree dialogTree)
		{
			this.vectorData = vectorData;
			this.dialogTree = dialogTree;
			this.watch.Start();
		}

		public void Update(bool userTyping)
		{
			// Advance the timer
			this.timer += (float)this.watch.Elapsed.TotalSeconds;
			this.watch.Reset();
			this.watch.Start();

			// Initially say hello
			if (this.lastOutput == 0.0f)
			{
				this.Say("Hello.");
			}

			// Don't answer if the user hasn't said anything new - or is still typing.
			if (this.newInput.Count > 0 && !userTyping)
			{
				// Wait a little while until answering.
				// Approximately as long as the user took writing the text.
				float userReactionTime = this.lastInput - this.lastOutput;
				float timeSinceInput = this.timer - this.lastInput;
				float waitTime = Math.Min((userReactionTime + this.waitOffset) * 0.5f, 3.0f);
				if (timeSinceInput > waitTime && userReactionTime > 0.0f)
				{
					// Pick a new random answer time offset
					this.waitOffset = 0.35f * userReactionTime * 2.0f * ((float)this.random.NextDouble() - 0.5f);

					// Think about the input and potentially say something
					this.ThinkAbout(this.newInput);

					// Flag input as processed
					this.newInput.Clear();
				}
			}

			return;
		}
		public void Listen(string input)
		{
			if (string.IsNullOrEmpty(input)) return;

			// Remove any occurrences of the bot name, so it doesn't confuse the phrase matching
			input = Regex.Replace(input, @"\b[bB]ob\b", "");

			Message inputMessage = new Message(input, this.vectorData);

			this.newInput.Add(inputMessage);
			this.lastInput = this.timer;
		}
		public void Say(string output)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("Bob: ");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(output);
			this.lastOutput = this.timer;
		}
		public void Say(Statement statement)
		{
			Message response = statement.SelectRandomMessage(this.random, this.timer);
			response.TickUsed(this.timer);
			this.Say(response.Text);
			this.context = response.Context ?? statement.Context ?? this.context;
		}
		public void ThinkAbout(IEnumerable<Message> input)
		{
			ScoredDialogNode bestMatch = default(ScoredDialogNode);
			float weight = 1.0f;
			foreach (Message message in input.Reverse())
			{
				List<ScoredDialogNode> matchList = this.dialogTree.Match(this.context, message);
				ScoredDialogNode localBestMatch = matchList.FirstOrDefault();
				if (localBestMatch.Score < 0.15f) continue;

				if (localBestMatch.Score * weight > bestMatch.Score)
				{
					weight *= 0.8f;
					bestMatch = localBestMatch;
					break;
				}
			}

			if (bestMatch.Node != null)
				this.Say(bestMatch.Node.Output);
			else
				this.Say(this.dialogTree.DontGetItNode.Output);
		}
	}
}
