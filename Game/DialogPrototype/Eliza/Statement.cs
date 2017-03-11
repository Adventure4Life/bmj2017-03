﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DialogPrototype
{
	public class Statement
	{
		private DialogContext context;
		private List<Message> messages = new List<Message>();

		public DialogContext Context
		{
			get { return this.context; }
		}
		public IEnumerable<Message> Messages
		{
			get { return this.messages; }
		}

		public Statement(IEnumerable<Message> messages) : this(null, messages) { }
		public Statement(DialogContext context, IEnumerable<Message> messages)
		{
			this.context = context;
			this.messages.AddRange(messages);
		}
		public float GetSimilarity(DialogContext context, Message other)
		{
			if (this.Context != null && this.Context != context)
				return 0.0f;

			float maxSimilarity = 0.0f;
			foreach (Message message in this.messages)
			{
				if (message.Context != null && message.Context != context)
					continue;

				float similarity = message.GetSimilarity(other);
				maxSimilarity = (float)Math.Max(similarity, maxSimilarity);
			}
			return maxSimilarity;
		}
	}
}
