using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TasksLibrary
{
	public class RandomGenerator
	{
		private bool _isAvailableApi = true, _isDownload = false;
		private int _maxValue;
		private int _downloadedLength = 50;

		private readonly Queue<int> sequence;
		private readonly Random localGenerator;
		private RandomOrgApiRequest setting;

		public int RandomDigit { get { return getDigit(); }}

		public IEnumerable<int> GetDigits(int count)
		{
			for (int i = 0; i < count; i++)
				yield return getDigit();
		}

		private int getDigit()
		{
			if (!_isAvailableApi)
				return localGenerator.Next(_maxValue);
			if (sequence.Count < 20 && !_isDownload)
				DownloadNumbersAsync();
			return sequence.Count != 0 ? sequence.Dequeue() : localGenerator.Next(_maxValue);
		}

		public RandomGenerator(int maxValue)
		{
			sequence = new Queue<int>();
			localGenerator = new Random();
			setting = new RandomOrgApiRequest();
			_maxValue = maxValue;
			setting.Parameters.Length = _downloadedLength;
			setting.Parameters.MaxValue = _maxValue;
			DownloadNumbersAsync();
		}

		private async void DownloadNumbersAsync()
		{
			_isDownload = true;
			await Task.Run(async () =>
			{
				var json = JsonConvert.SerializeObject(setting);
				HttpWebRequest request = WebRequest.CreateHttp(@"https://api.random.org/json-rpc/1/invoke");
				request.ContentType = "application/json-rpc";
				request.Method = "POST";
				using (var writer = new StreamWriter(request.GetRequestStream()))
				{
					writer.Write(json);
				}
				HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
				if (response.StatusCode != HttpStatusCode.OK)
				{
					_isAvailableApi = false;
					return;
				}
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					json = reader.ReadToEnd();
				}
				dynamic result = JsonConvert.DeserializeObject(json);
				var returnArray = (JArray) result["result"]["random"]["data"];
				foreach (var a in returnArray)
				{
					sequence.Enqueue(a.ToObject<int>());
				}
				_isDownload = false;
			});
		}

		[JsonObject]
		internal class RandomOrgApiRequest
		{
			[JsonProperty(PropertyName = "jsonrpc")]
			public string JsonRPC = "2.0";
			[JsonProperty(PropertyName = "method")]
			public string ApiMethod = "generateIntegers";
			[JsonProperty(PropertyName = "params")]
			public Params Parameters;
			[JsonProperty(PropertyName = "id")] 
			public int id = 10;
			public RandomOrgApiRequest()
			{
				Parameters = new Params();
			}
		}
		[JsonObject]
		internal class Params
		{
			[JsonProperty(PropertyName = "apiKey")]
			public string ApiKey = "a1b4a88f-9d7c-4ad2-8b35-ca8f621daf9c";
			[JsonProperty(PropertyName = "n")]
			public int Length;
			[JsonProperty(PropertyName = "min")]
			public int MinValue = 0;
			[JsonProperty(PropertyName = "max")]
			public int MaxValue;
		}
	}
}
