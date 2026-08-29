using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using EntityComponentSystemCSharp.Systems;

namespace MegaDungeon {
	public class MonsterLoader {
		public static Dictionary<string, string> LoadAll(string monstersPath, string modsPath = null, ISystemLogger logger = null) {
			var prototypes = new Dictionary<string, string>();
			LoadInto(prototypes, monstersPath, logger);
			if(modsPath != null) {
				LoadInto(prototypes, modsPath, logger);
			}
			return prototypes;
		}

		static void LoadInto(Dictionary<string, string> prototypes, string path, ISystemLogger logger) {
			if(!Directory.Exists(path)) {return;}

			foreach(var file in Directory.GetFiles(path, "*.json").OrderBy(f => f)) {
				var monster = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file));
				foreach(var entry in monster) {
					if(prototypes.ContainsKey(entry.Key)) {
						logger?.Log($"'{entry.Key}' redefined by {Path.GetFileName(file)}");
					}
					prototypes[entry.Key] = JsonConvert.SerializeObject(entry.Value);
				}
			}
		}
	}
}
