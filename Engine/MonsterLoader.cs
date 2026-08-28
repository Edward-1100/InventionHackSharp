using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MegaDungeon {
	public class MonsterLoader {
		public static Dictionary<string, string> LoadAll(string monstersPath) {
			var prototypes = new Dictionary<string, string>();
			if(!Directory.Exists(monstersPath)) {return prototypes;}

			foreach(var file in Directory.GetFiles(monstersPath, "*.json").OrderBy(f => f)) {
				var monster = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file));
				foreach(var entry in monster) {
					prototypes[entry.Key] = JsonConvert.SerializeObject(entry.Value);
				}
			}

			return prototypes;
		}
	}
}
