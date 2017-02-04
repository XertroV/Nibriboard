﻿using System;
using System.Drawing;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nibriboard.Utilities.JsonConverters
{
	/// <summary>
	/// Deserialises objects into rectangles from the System.Drawing namespace.
	/// </summary>
	public class RectangleConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);

			return new Rectangle(
				jsonObject.Value<int>("X"),
				jsonObject.Value<int>("Y"),
				jsonObject.Value<int>("Width"),
				jsonObject.Value<int>("Height")
			);
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType != typeof(Rectangle))
				return false;
			return true;
		}
	}
}

